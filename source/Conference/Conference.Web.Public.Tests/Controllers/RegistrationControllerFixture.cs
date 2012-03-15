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

namespace Conference.Web.Public.Tests.Controllers.RegistrationControllerFixture
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Common;
    using Conference.Web.Public.Controllers;
    using Conference.Web.Public.Models;
    using Moq;
    using Registration;
    using Registration.Commands;
    using Registration.ReadModel;
    using Xunit;

    public class given_controller
    {
        protected readonly RegistrationController sut;
        protected readonly ICommandBus bus;
        protected readonly IViewRepository viewRepository;

        public given_controller()
        {
            this.bus = Mock.Of<ICommandBus>();
            this.viewRepository = Mock.Of<IViewRepository>();

            this.sut = new RegistrationController(this.bus, () => this.viewRepository);
        }

        [Fact]
        public void when_starting_registration_then_returns_view_with_registration_for_conference()
        {
            var conferenceId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var conferenceDTO =
                new ConferenceDTO(conferenceId, "conference", "Test Conference", "", new[] { new ConferenceSeatDTO(seatId, "Test Seat", 10d) });

            // Arrange
            Mock.Get<IViewRepository>(this.viewRepository)
                .Setup(r => r.Query<ConferenceDTO>())
                .Returns(new ConferenceDTO[] { conferenceDTO }.AsQueryable());

            // Act
            var result = (ViewResult)this.sut.StartRegistration("conference");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("", result.ViewName);

            var resultRegistration = result.Model as global::Conference.Web.Public.Models.OrderViewModel;
            Assert.NotNull(resultRegistration);
            Assert.Equal("conference", resultRegistration.ConferenceCode);
            Assert.Equal("Test Conference", resultRegistration.ConferenceName);
            Assert.Equal(1, resultRegistration.Items.Count);
            Assert.Equal("Test Seat", resultRegistration.Items[0].SeatTypeDescription);
        }

        [Fact]
        public void when_specifying_seats_for_a_valid_registration_then_places_registration_and_redirects_to_action()
        {
            var conferenceId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var conferenceDTO =
                new ConferenceDTO(conferenceId, "conference", "Test Conference", "", new[] { new ConferenceSeatDTO(seatId, "Test Seat", 10d) });

            // Arrange
            Mock.Get<IViewRepository>(this.viewRepository)
                .Setup(r => r.Query<ConferenceDTO>())
                .Returns(new ConferenceDTO[] { conferenceDTO }.AsQueryable());

            var registrationId = Guid.NewGuid();

            Mock.Get<IViewRepository>(this.viewRepository)
                .Setup(r => r.Find<OrderDTO>(registrationId))
                .Returns(new OrderDTO(registrationId, Order.States.Booked));

            var registration =
                new global::Conference.Web.Public.Models.OrderViewModel
                {
                    Id = registrationId,
                    ConferenceCode = "conference",
                    Items = new[] { new OrderItemViewModel { Quantity = 10, SeatTypeId = seatId } }
                };

            // Act
            var result = (RedirectToRouteResult)this.sut.StartRegistration("conference", registration);

            // Assert

            Assert.Equal(null, result.RouteValues["controller"]);
            Assert.Equal("SpecifyPaymentDetails", result.RouteValues["action"]);
            Assert.Equal("conference", result.RouteValues["conferenceCode"]);
            Assert.Equal(registrationId, result.RouteValues["registrationId"]);

            Mock.Get<ICommandBus>(this.bus)
                .Verify(
                    b =>
                        b.Send(It.Is<Envelope<ICommand>>(e =>
                            ((RegisterToConference)e.Body).ConferenceId == conferenceId
                                && ((RegisterToConference)e.Body).RegistrationId == registrationId
                                && ((RegisterToConference)e.Body).Tickets.Count == 1
                                && ((RegisterToConference)e.Body).Tickets.ElementAt(0).Quantity == 10
                                && ((RegisterToConference)e.Body).Tickets.ElementAt(0).TicketTypeId == seatId)),
                    Times.Once());
        }

        //[Fact]
        //public void when_specifying_more_seats_than_available_then_goes_to_notification_page()
        //{
        //    var conferenceId = Guid.NewGuid();
        //    var seatId = Guid.NewGuid();
        //    var conferenceDTO =
        //        new ConferenceDTO(conferenceId, "conference", "Test Conference", "", new[] { new ConferenceSeatDTO(seatId, "Test Seat", 10d) });

        //    // Arrange
        //    Mock.Get<IViewRepository>(this.viewRepository)
        //        .Setup(r => r.Query<ConferenceDTO>())
        //        .Returns(new ConferenceDTO[] { conferenceDTO }.AsQueryable());

        //    var registrationId = Guid.NewGuid();

        //    Mock.Get<IViewRepository>(this.viewRepository)
        //        .Setup(r => r.Find<OrderDTO>(registrationId))
        //        .Returns(new OrderDTO(registrationId, Order.States.Rejected));

        //    var registration =
        //        new global::Conference.Web.Public.Models.Registration
        //        {
        //            Id = registrationId,
        //            ConferenceCode = "conference",
        //            Seats = new[] { new Seat { Quantity = 10, SeatId = seatId } }
        //        };

        //    // Act
        //    var result = (ViewResult)this.sut.StartRegistration("conference", registration);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal("ReservationRejected", result.ViewName);
        //}

        //[Fact]
        //public void when_confirming_a_registration_then_sends_completion_command()
        //{
        //    var conferenceId = Guid.NewGuid();
        //    var seatId = Guid.NewGuid();

        //    var registrationId = Guid.NewGuid();

        //    var registration =
        //        new global::Conference.Web.Public.Models.Registration
        //        {
        //            Id = registrationId,
        //            ConferenceCode = "conference",
        //            Seats = new[] { new Seat { Quantity = 10, SeatId = seatId } }
        //        };

        //    // Act
        //    var result = (ViewResult)this.sut.ConfirmRegistration("conference", registration);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal("RegistrationConfirmed", result.ViewName);
        //}
    }
}
