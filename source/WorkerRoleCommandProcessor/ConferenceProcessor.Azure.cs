﻿// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// ©2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://cqrsjourney.github.com/contributors/members
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

namespace WorkerRoleCommandProcessor
{
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading;
    using Infrastructure;
    using Infrastructure.Azure;
    using Infrastructure.Azure.BlobStorage;
    using Infrastructure.Azure.EventSourcing;
    using Infrastructure.Azure.Instrumentation;
    using Infrastructure.Azure.MessageLog;
    using Infrastructure.Azure.Messaging;
    using Infrastructure.Azure.Messaging.Handling;
    using Infrastructure.BlobStorage;
    using Infrastructure.EventSourcing;
    using Infrastructure.Messaging;
    using Infrastructure.Messaging.Handling;
    using Infrastructure.Serialization;
    using Microsoft.Practices.Unity;
    using Microsoft.WindowsAzure;
    using Registration;
    using Registration.Handlers;

    /// <summary>
    /// Azure-side of the processor, which is included for compilation conditionally 
    /// at the csproj level.
    /// </summary>
    /// <devdoc>
    /// NOTE: this file is only compiled on non-DebugLocal configurations. In DebugLocal 
    /// you will not see full syntax coloring, intellisense, etc.. But it is still 
    /// much more readable and usable than a grayed-out piece of code inside an #if
    /// </devdoc>
    partial class ConferenceProcessor
    {
        private InfrastructureSettings azureSettings;
        private ServiceBusConfig busConfig;

        partial void OnCreating()
        {
            this.azureSettings = InfrastructureSettings.Read("Settings.xml");
            this.busConfig = new ServiceBusConfig(this.azureSettings.ServiceBus);

            busConfig.Initialize();
        }

        partial void OnCreateContainer(UnityContainer container)
        {
            var metadata = container.Resolve<IMetadataProvider>();
            var serializer = container.Resolve<ITextSerializer>();

            // blob
            var blobStorageAccount = CloudStorageAccount.Parse(azureSettings.BlobStorage.ConnectionString);
            container.RegisterInstance<IBlobStorage>(new CloudBlobStorage(blobStorageAccount, azureSettings.BlobStorage.RootContainerName));

            var commandBus = new CommandBus(new TopicSender(azureSettings.ServiceBus, Topics.Commands.Path), metadata, serializer);
            var topicSender = new TopicSender(azureSettings.ServiceBus, Topics.Events.Path);
            container.RegisterInstance<IMessageSender>(topicSender);
            var eventBus = new EventBus(topicSender, metadata, serializer);

            var sessionlessCommandProcessor =
                new CommandProcessor(new SubscriptionReceiver(azureSettings.ServiceBus, Topics.Commands.Path, Topics.Commands.Subscriptions.Sessionless, false, new SubscriptionReceiverInstrumentation(Topics.Commands.Subscriptions.Sessionless, this.instrumentationEnabled)), serializer);
            var seatsAvailabilityCommandProcessor =
                new CommandProcessor(new SessionSubscriptionReceiver(azureSettings.ServiceBus, Topics.Commands.Path, Topics.Commands.Subscriptions.Seatsavailability, false, new SessionSubscriptionReceiverInstrumentation(Topics.Commands.Subscriptions.Seatsavailability, this.instrumentationEnabled)), serializer);

            var synchronousCommandBus = new SynchronousCommandBusDecorator(commandBus);
            container.RegisterInstance<ICommandBus>(synchronousCommandBus);

            container.RegisterInstance<IEventBus>(eventBus);
            container.RegisterInstance<IProcessor>("SessionlessCommandProcessor", sessionlessCommandProcessor);
            container.RegisterInstance<IProcessor>("SeatsAvailabilityCommandProcessor", seatsAvailabilityCommandProcessor);

            RegisterRepository(container);
            RegisterEventProcessors(container);
            RegisterCommandHandlers(container, sessionlessCommandProcessor, seatsAvailabilityCommandProcessor);

            // handle order commands inline, as they do not have competition.
            synchronousCommandBus.Register(container.Resolve<ICommandHandler>("OrderCommandHandler"));

            // message log
            var messageLogAccount = CloudStorageAccount.Parse(azureSettings.MessageLog.ConnectionString);

            container.RegisterInstance<IProcessor>("EventLogger", new AzureMessageLogListener(
                new AzureMessageLogWriter(messageLogAccount, azureSettings.MessageLog.TableName),
                new SubscriptionReceiver(azureSettings.ServiceBus, Topics.Events.Path, Topics.Events.Subscriptions.Log)));

            container.RegisterInstance<IProcessor>("CommandLogger", new AzureMessageLogListener(
                new AzureMessageLogWriter(messageLogAccount, azureSettings.MessageLog.TableName),
                new SubscriptionReceiver(azureSettings.ServiceBus, Topics.Commands.Path, Topics.Commands.Subscriptions.Log)));
        }

        private void RegisterEventProcessors(UnityContainer container)
        {
            container.RegisterType<RegistrationProcessManagerRouter>(new ContainerControlledLifetimeManager());

            container.RegisterEventProcessor<RegistrationProcessManagerRouter>(this.busConfig, Topics.Events.Subscriptions.RegistrationPMOrderPlaced, this.instrumentationEnabled);
            container.RegisterEventProcessor<RegistrationProcessManagerRouter>(this.busConfig, Topics.Events.Subscriptions.RegistrationPMNextSteps, this.instrumentationEnabled);
            container.RegisterEventProcessor<DraftOrderViewModelGenerator>(this.busConfig, Topics.Events.Subscriptions.OrderViewModelGeneratorV3, this.instrumentationEnabled);
            container.RegisterEventProcessor<PricedOrderViewModelGenerator>(this.busConfig, Topics.Events.Subscriptions.PricedOrderViewModelGeneratorV3, this.instrumentationEnabled);
            container.RegisterEventProcessor<ConferenceViewModelGenerator>(this.busConfig, Topics.Events.Subscriptions.ConferenceViewModelGenerator, this.instrumentationEnabled);
            container.RegisterEventProcessor<SeatAssignmentsViewModelGenerator>(this.busConfig, Topics.Events.Subscriptions.SeatAssignmentsViewModelGenerator, this.instrumentationEnabled);
            container.RegisterEventProcessor<SeatAssignmentsHandler>(this.busConfig, Topics.Events.Subscriptions.SeatAssignmentsHandler, this.instrumentationEnabled);
            container.RegisterEventProcessor<global::Conference.OrderEventHandler>(this.busConfig, Topics.Events.Subscriptions.OrderEventHandler, this.instrumentationEnabled);
        }

        private static void RegisterCommandHandlers(IUnityContainer unityContainer, ICommandHandlerRegistry sessionlessRegistry, ICommandHandlerRegistry seatsAvailabilityRegistry)
        {
            var commandHandlers = unityContainer.ResolveAll<ICommandHandler>().ToList();
            var seatsAvailabilityHandler = commandHandlers.First(x => x.GetType().IsAssignableFrom(typeof(SeatsAvailabilityHandler)));

            seatsAvailabilityRegistry.Register(seatsAvailabilityHandler);
            foreach (var commandHandler in commandHandlers.Where(x => x != seatsAvailabilityHandler))
            {
                sessionlessRegistry.Register(commandHandler);
            }
        }

        private void RegisterRepository(UnityContainer container)
        {
            // repository
            var eventSourcingAccount = CloudStorageAccount.Parse(this.azureSettings.EventSourcing.ConnectionString);
            var eventStore = new EventStore(eventSourcingAccount, this.azureSettings.EventSourcing.TableName);

            container.RegisterInstance<IEventStore>(eventStore);
            container.RegisterInstance<IPendingEventsQueue>(eventStore);
            container.RegisterInstance<IEventStoreBusPublisherInstrumentation>(new EventStoreBusPublisherInstrumentation("worker", this.instrumentationEnabled));
            container.RegisterType<IEventStoreBusPublisher, EventStoreBusPublisher>(new ContainerControlledLifetimeManager());
            var cache = new MemoryCache("RepositoryCache");
            container.RegisterType(
                typeof(IEventSourcedRepository<>),
                typeof(AzureEventSourcedRepository<>),
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(typeof(IEventStore), typeof(IEventStoreBusPublisher), typeof(ITextSerializer), typeof(IMetadataProvider), cache));

            // to satisfy the IProcessor requirements.
            container.RegisterInstance<IProcessor>("EventStoreBusPublisher", new PublisherProcessorAdapter(
                container.Resolve<IEventStoreBusPublisher>(), this.cancellationTokenSource.Token));
        }

        // to satisfy the IProcessor requirements.
        // TODO: we should unify and probaly use token-based Start only processors.
        private class PublisherProcessorAdapter : IProcessor
        {
            private IEventStoreBusPublisher publisher;
            private CancellationToken token;

            public PublisherProcessorAdapter(IEventStoreBusPublisher publisher, CancellationToken token)
            {
                this.publisher = publisher;
                this.token = token;
            }

            public void Start()
            {
                this.publisher.Start(this.token);
            }

            public void Stop()
            {
                // Do nothing. The cancelled token will stop the process anyway.
            }
        }
    }
}
