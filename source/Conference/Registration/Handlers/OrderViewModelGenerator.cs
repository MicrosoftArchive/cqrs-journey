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
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Infrastructure.Messaging.Handling;
    using Registration.Events;
    using Registration.ReadModel;
    using Registration.ReadModel.Implementation;

    public class OrderViewModelGenerator :
        IEventHandler<OrderPlaced>, IEventHandler<OrderUpdated>,
        IEventHandler<OrderPartiallyReserved>, IEventHandler<OrderReservationCompleted>,
        IEventHandler<OrderRegistrantAssigned>
    {
        private readonly Func<ConferenceRegistrationDbContext> contextFactory;

        public OrderViewModelGenerator(Func<ConferenceRegistrationDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public void Handle(OrderPlaced @event)
        {
            using (var context = this.contextFactory.Invoke())
            {
                var dto = new OrderDTO(@event.SourceId, OrderDTO.States.PendingReservation, @event.Version)
                {
                    AccessCode = @event.AccessCode,
                };
                dto.Lines.AddRange(@event.Seats.Select(seat => new OrderItemDTO(seat.SeatType, seat.Quantity)));

                context.Save(dto);
            }
        }

        public void Handle(OrderRegistrantAssigned @event)
        {
            using (var context = this.contextFactory.Invoke())
            {
                var dto = context.Find<OrderDTO>(@event.SourceId);
                dto.RegistrantEmail = @event.Email;

                dto.OrderVersion = @event.Version;

                context.Save(dto);
            }
        }

        public void Handle(OrderUpdated @event)
        {
            using (var context = this.contextFactory.Invoke())
            {
                var dto = context.Set<OrderDTO>().Include(o => o.Lines).First(o => o.OrderId == @event.SourceId);

                var linesSet = context.Set<OrderItemDTO>();
                foreach (var line in linesSet.ToList())
                {
                    linesSet.Remove(line);
                }

                dto.Lines.AddRange(@event.Seats.Select(seat => new OrderItemDTO(seat.SeatType, seat.Quantity)));

                dto.State = OrderDTO.States.PendingReservation;
                dto.OrderVersion = @event.Version;

                context.Save(dto);
            }
        }

        public void Handle(OrderPartiallyReserved @event)
        {
            this.UpdateReserved(@event.SourceId, @event.ReservationExpiration, OrderDTO.States.PartiallyReserved, @event.Version, @event.Seats);
        }

        public void Handle(OrderReservationCompleted @event)
        {
            this.UpdateReserved(@event.SourceId, @event.ReservationExpiration, OrderDTO.States.ReservationCompleted, @event.Version, @event.Seats);
        }

        private void UpdateReserved(Guid orderId, DateTime reservationExpiration, OrderDTO.States state, int orderVersion, IEnumerable<SeatQuantity> seats)
        {
            using (var context = this.contextFactory.Invoke())
            {
                var dto = context.Set<OrderDTO>().Include(x => x.Lines).First(x => x.OrderId == orderId);
                foreach (var seat in seats)
                {
                    var item = dto.Lines.Single(x => x.SeatType == seat.SeatType);
                    item.ReservedSeats = seat.Quantity;
                }

                dto.State = state;
                dto.ReservationExpirationDate = reservationExpiration;

                dto.OrderVersion = orderVersion;

                context.Save(dto);
            }
        }
    }
}
