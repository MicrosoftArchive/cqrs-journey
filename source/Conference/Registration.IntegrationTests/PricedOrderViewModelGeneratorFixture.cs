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

namespace Registration.IntegrationTests.PricedOrderViewModelGeneratorFixture
{
    using System;
    using System.Collections.Generic;
    using Infrastructure.Serialization;
    using Moq;
    using Registration.Events;
    using Registration.Handlers;
    using Registration.ReadModel;
    using Registration.ReadModel.Implementation;
    using Xunit;

    public class given_a_read_model_generator : given_a_read_model_database
    {
        private static readonly List<SeatTypeName> seatTypes = new List<SeatTypeName>
        {
            new SeatTypeName { Id = Guid.NewGuid(), Name= "General" }, 
            new SeatTypeName { Id = Guid.NewGuid(), Name= "Precon" }, 
        };

        protected PricedOrderViewModelGenerator sut;
        private IOrderDao dao;

        public given_a_read_model_generator()
        {
            var conferenceDao = new Mock<IConferenceDao>();
            conferenceDao.Setup(x => x.GetSeatTypeNames(It.IsAny<IEnumerable<Guid>>()))
                .Returns(seatTypes);

            this.sut = new PricedOrderViewModelGenerator(conferenceDao.Object, () => new ConferenceRegistrationDbContext(dbName));
            this.dao = new OrderDao(() => new ConferenceRegistrationDbContext(dbName), new MemoryBlobStorage(), new JsonTextSerializer());
        }

        public class given_a_calculated_order : given_a_read_model_generator
        {
            private static readonly Guid orderId = Guid.NewGuid();

            private PricedOrder dto;

            public given_a_calculated_order()
            {
                this.sut.Handle(new OrderTotalsCalculated
                {
                    SourceId = orderId,
                    Lines = new[]
                    {
                        new SeatOrderLine 
                        { 
                            LineTotal = 50, 
                            SeatType = seatTypes[0].Id, 
                            Quantity = 10, 
                            UnitPrice = 5 
                        },
                    },
                    Total = 50,
                    IsFreeOfCharge = true
                });

                this.dto = this.dao.FindPricedOrder(orderId);
            }

            [Fact]
            public void then_creates_model()
            {
                Assert.NotNull(dto);
            }

            [Fact]
            public void then_creates_order_lines()
            {
                Assert.Equal(1, dto.Lines.Count);
                Assert.Equal(50, dto.Lines[0].LineTotal);
                Assert.Equal(10, dto.Lines[0].Quantity);
                Assert.Equal(5, dto.Lines[0].UnitPrice);
                Assert.Equal(50, dto.Total);
            }

            [Fact]
            public void then_populates_description()
            {
                Assert.Equal("General", dto.Lines[0].Description);
            }

            [Fact]
            public void then_populates_is_free_of_charge()
            {
                Assert.Equal(true, dto.IsFreeOfCharge);
            }

            [Fact]
            public void when_recalculated_then_replaces_line()
            {
                this.sut.Handle(new OrderTotalsCalculated
                {
                    SourceId = orderId,
                    Lines = new[]
                    {
                        new SeatOrderLine 
                        { 
                            LineTotal = 20, 
                            SeatType = seatTypes[1].Id, 
                            Quantity = 2, 
                            UnitPrice = 10 
                        },
                    },
                    Total = 20,
                });

                var dto = this.dao.FindPricedOrder(orderId);

                Assert.Equal(1, dto.Lines.Count);
                Assert.Equal(20, dto.Lines[0].LineTotal);
                Assert.Equal(2, dto.Lines[0].Quantity);
                Assert.Equal(10, dto.Lines[0].UnitPrice);
                Assert.Equal(20, dto.Total);
                Assert.Equal("Precon", dto.Lines[0].Description);
            }

            [Fact]
            public void when_expired_then_deletes_priced_order()
            {
                this.sut.Handle(new OrderExpired { SourceId = orderId });

                var dto = this.dao.FindPricedOrder(orderId);

                Assert.Null(dto);
            }

            [Fact]
            public void when_seat_assignments_created_then_updates_order_with_assignments_id()
            {
                var assignmentsId = Guid.NewGuid();
                this.sut.Handle(new SeatAssignmentsCreated { SourceId = assignmentsId, OrderId = orderId });

                var dto = this.dao.FindPricedOrder(orderId);

                Assert.Equal(assignmentsId, dto.AssignmentsId);
            }
        }
    }


}
