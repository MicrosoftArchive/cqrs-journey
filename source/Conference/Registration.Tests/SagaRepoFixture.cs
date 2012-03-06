﻿// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// Copyright (c) Microsoft Corporation and contributors http://cqrsjourney.github.com/contributors/members
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

namespace Registration.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Xunit;
	using Common;

	public class SagaRepoFixture
	{
		[Fact(Skip = "Fix for async bus")]
		public void WhenSagaPublishesEvent_ThenAnotherSagaIsRehidrated()
		{
			var repo = new MemorySagaRepository();
			var events = new MemoryEventBus(
				new RegistrationSagaUserDeactivatedHandler(repo));
			var commands = new MemoryCommandBus(
				new PlaceOrderCommandCommandHandler(repo),
				new DeactivateUserCommandHandler(events));

			var userId = Guid.NewGuid();

			commands.Send(new PlaceOrderCommand(userId));

			// Saga is created.
			Assert.Equal(1, repo.Query<RegistrationSaga>().Count());

			commands.Send(new DeactivateUserCommand(userId));

			Assert.True(events.Events.OfType<UserDeactivated>().Any());

			Assert.True(repo.Query<RegistrationSaga>().Single(x => x.UserId == userId).IsCompleted);
		}

		class PlaceOrderCommand : ICommand
		{
			public PlaceOrderCommand(Guid userId)
			{
				this.Id = Guid.NewGuid();
				this.UserId = userId;
			}

			public Guid Id { get; set; }
			public Guid UserId { get; set; }
		}

		class PlaceOrderCommandCommandHandler : ICommandHandler<PlaceOrderCommand>
		{
			private ISagaRepository repo;

			public PlaceOrderCommandCommandHandler(ISagaRepository repo)
			{
				this.repo = repo;
			}

			public void Handle(PlaceOrderCommand command)
			{
				var saga = new RegistrationSaga(command.Id, command.UserId);

				this.repo.Save(saga);
			}
		}

		class DeactivateUserCommand : ICommand
		{
			public DeactivateUserCommand(Guid userId)
			{
				this.Id = Guid.NewGuid();
				this.UserId = userId;
			}

			public Guid Id { get; private set; }
			public Guid UserId { get; set; }
		}

		class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
		{
			private IEventBus events;

			public DeactivateUserCommandHandler(IEventBus events)
			{
				this.events = events;
			}

			public void Handle(DeactivateUserCommand command)
			{
				// Invoke some biz logic in the domain.
				this.events.Publish(new UserDeactivated(command.UserId));
			}
		}

		class UserDeactivated : IEvent
		{
			public UserDeactivated(Guid userId)
			{
				this.UserId = userId;
			}

			public Guid UserId { get; set; }
		}

		class RegistrationSagaUserDeactivatedHandler : IEventHandler<UserDeactivated>
		{
			private ISagaRepository repo;

			public RegistrationSagaUserDeactivatedHandler(ISagaRepository repo)
			{
				this.repo = repo;
			}

			public void Handle(UserDeactivated @event)
			{
				// Route the event to the corresponding saga by correlation id.
				var saga = this.repo.Query<RegistrationSaga>().SingleOrDefault(x => !x.IsCompleted && x.UserId == @event.UserId);
				if (saga != null)
				{
					saga.Handle(@event);
					this.repo.Save(saga);
				}
			}
		}

		class RegistrationSaga : IAggregateRoot, IEventHandler<UserDeactivated>
		{
			public RegistrationSaga(Guid id, Guid userId)
			{
				this.Id = id;
				this.UserId = userId;
			}

			protected RegistrationSaga() { }

			// Dependencies.
			public ICommandBus Commands { get; set; }
			public IEventBus Events { get; set; }

			public Guid Id { get; private set; }
			public Guid UserId { get; private set; }
			public bool IsCompleted { get; private set; }

			public void Handle(UserDeactivated @event)
			{
				if (@event.UserId == this.UserId)
					this.IsCompleted = true;
			}
		}

		class MemorySagaRepository : ISagaRepository
		{
			private List<IAggregateRoot> aggregates = new List<IAggregateRoot>();

			public IQueryable<T> Query<T>() where T : class, IAggregateRoot
			{
				return this.aggregates.OfType<T>().AsQueryable();
			}

			public T Find<T>(Guid id) where T : class, IAggregateRoot
			{
				return this.aggregates.OfType<T>().FirstOrDefault(x => x.Id == id);
			}

			public void Save<T>(T aggregate) where T : class, IAggregateRoot
			{
				if (!this.aggregates.Contains(aggregate))
					this.aggregates.Add(aggregate);
			}
		}


	}
}
