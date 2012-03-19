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

namespace Registration.Handlers
{
    using System;
    using System.Linq;
    using Common;
    using Registration.Commands;

    public class RegistrationCommandHandler
        : ICommandHandler<RegisterToConference>,
        ICommandHandler<MarkOrderAsBooked>,
        ICommandHandler<RejectOrder>
    {
        private Func<IRepository> repositoryFactory;

        public RegistrationCommandHandler(Func<IRepository> repositoryFactory)
        {
            this.repositoryFactory = repositoryFactory;
        }

        public void Handle(RegisterToConference command)
        {
            var repository = this.repositoryFactory();

            using (repository as IDisposable)
            {
                var tickets = command.Seats.Select(t => new OrderItem(t.SeatTypeId, t.Quantity)).ToList();

                var order = new Order(command.OrderId, Guid.NewGuid(), command.ConferenceId, tickets);

                repository.Save(order);
            }
        }

        public void Handle(MarkOrderAsBooked command)
        {
            var repository = this.repositoryFactory();

            using (repository as IDisposable)
            {
                var order = repository.Find<Order>(command.OrderId);

                if (order != null)
                {
                    order.MarkAsBooked();
                    repository.Save(order);
                }
            }
        }

        public void Handle(RejectOrder command)
        {
            var repository = this.repositoryFactory();

            using (repository as IDisposable)
            {
                var order = repository.Find<Order>(command.OrderId);

                if (order != null)
                {
                    order.Reject();
                    repository.Save(order);
                }
            }
        }
    }
}
