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

namespace Registration
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using Infrastructure.Messaging;
    using Infrastructure.Processes;
    using Payments.Contracts.Events;
    using Registration.Commands;
    using Registration.Events;

    public class RegistrationProcess : IProcess
    {
        private static readonly TimeSpan BufferTimeBeforeReleasingSeatsAfterExpiration = TimeSpan.FromMinutes(14);

        public enum ProcessState
        {
            NotStarted = 0,
            AwaitingReservationConfirmation = 1,
            ReservationConfirmationReceived = 2,
            PaymentConfirmationReceived = 3,
        }

        private readonly List<Envelope<ICommand>> commands = new List<Envelope<ICommand>>();

        public RegistrationProcess()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; private set; }
        public bool Completed { get; private set; }
        public Guid ConferenceId { get; set; }
        public Guid OrderId { get; internal set; }
        public Guid ReservationId { get; internal set; }

        // feels akward and possibly disrupting to store these properties here. Would it be better if instead of using 
        // current state values, we use event sourcing?
        public DateTime? ReservationAutoExpiration { get; internal set; }
        public Guid ExpirationCommandId { get; set; }

        public int StateValue { get; private set; }
        [NotMapped]
        public ProcessState State
        {
            get { return (ProcessState)this.StateValue; }
            internal set { this.StateValue = (int)value; }
        }

        public IEnumerable<Envelope<ICommand>> Commands
        {
            get { return this.commands; }
        }

        public void Handle(OrderPlaced message)
        {
            if (this.State == ProcessState.NotStarted)
            {
                this.ConferenceId = message.ConferenceId;
                this.OrderId = message.SourceId;
                this.ReservationId = Guid.NewGuid();
                this.ReservationAutoExpiration = message.ReservationAutoExpiration;
                this.State = ProcessState.AwaitingReservationConfirmation;

                this.AddCommand(
                    new MakeSeatReservation
                    {
                        ConferenceId = message.ConferenceId,
                        ReservationId = this.ReservationId,
                        Seats = message.Seats.ToList()
                    });
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Handle(OrderUpdated message)
        {
            if (this.State == ProcessState.AwaitingReservationConfirmation
                || this.State == ProcessState.ReservationConfirmationReceived)
            {
                this.State = ProcessState.AwaitingReservationConfirmation;

                this.AddCommand(
                    new MakeSeatReservation
                    {
                        ConferenceId = this.ConferenceId,
                        ReservationId = this.ReservationId,
                        Seats = message.Seats.ToList()
                    });
            }
            else
            {
                throw new InvalidOperationException("The order cannot be updated at this stage.");
            }
        }

        public void Handle(SeatsReserved message)
        {
            if (this.State == ProcessState.AwaitingReservationConfirmation)
            {
                var expirationTime = this.ReservationAutoExpiration.Value;
                this.State = ProcessState.ReservationConfirmationReceived;

                if (this.ExpirationCommandId == Guid.Empty)
                {
                    var expirationCommand = new ExpireRegistrationProcess { ProcessId = this.Id };
                    this.ExpirationCommandId = expirationCommand.Id;

                    this.AddCommand(new Envelope<ICommand>(expirationCommand)
                    {
                        Delay = expirationTime.Subtract(DateTime.UtcNow).Add(BufferTimeBeforeReleasingSeatsAfterExpiration),
                    });
                }

                this.AddCommand(new MarkSeatsAsReserved
                {
                    OrderId = this.OrderId,
                    Seats = message.ReservationDetails.ToList(),
                    Expiration = expirationTime,
                });
            }
            else
            {
                throw new InvalidOperationException("Cannot handle seat reservation at this stage.");
            }
        }

        public void Handle(PaymentCompleted @event)
        {
            if (this.State == ProcessState.ReservationConfirmationReceived)
            {
                this.State = ProcessState.PaymentConfirmationReceived;
                this.AddCommand(new ConfirmOrder { OrderId = this.OrderId });
            }
            else
            {
                throw new InvalidOperationException("Cannot handle payment confirmation at this stage.");
            }
        }

        public void Handle(OrderConfirmed @event)
        {
            if (this.State == ProcessState.ReservationConfirmationReceived || this.State == ProcessState.PaymentConfirmationReceived)
            {
                this.ExpirationCommandId = Guid.Empty;
                this.Completed = true;

                this.AddCommand(new CommitSeatReservation
                {
                    ReservationId = this.ReservationId,
                    ConferenceId = this.ConferenceId
                });
            }
            else
            {
                throw new InvalidOperationException("Cannot handle order confirmation at this stage.");
            }
        }

        public void Handle(ExpireRegistrationProcess command)
        {
            if (this.ExpirationCommandId == command.Id)
            {
                this.Completed = true;

                this.AddCommand(new CancelSeatReservation
                {
                    ConferenceId = this.ConferenceId,
                    ReservationId = this.ReservationId,
                });
                this.AddCommand(new RejectOrder { OrderId = this.OrderId });

                // TODO cancel payment if any
            }

            // else ignore the message as it is no longer relevant (but not invalid)
        }

        private void AddCommand<T>(T command)
            where T : ICommand
        {
            this.commands.Add(Envelope.Create<ICommand>(command));
        }

        private void AddCommand(Envelope<ICommand> envelope)
        {
            this.commands.Add(envelope);
        }
    }
}
