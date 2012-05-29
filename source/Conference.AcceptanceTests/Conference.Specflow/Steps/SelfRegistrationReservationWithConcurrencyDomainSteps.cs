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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conference.Specflow.Support;
using Infrastructure.Messaging;
using Registration;
using Registration.Commands;
using Registration.Events;
using TechTalk.SpecFlow;
using Xunit;

namespace Conference.Specflow.Steps
{
    [Binding]
    [Scope(Tag = "@SelfRegistrationReservationWithConcurrencyDomain")]
    public class SelfRegistrationReservationWithConcurrencyDomainSteps
    {
        private List<string> orderIds;
        private readonly ICommandBus commandBus;

        public SelfRegistrationReservationWithConcurrencyDomainSteps()
        {
            commandBus = ConferenceHelper.BuildCommandBus();
        }

        [When(@"(.*) Registrants selects these Order Items")]
        public void WhenManyRegistrantsSelectsTheseOrderItems(int registrants, Table table)
        {
            var conferenceInfo = ScenarioContext.Current.Get<ConferenceInfo>();

            var seats = table.Rows.Select(
                row =>
                new SeatQuantity(conferenceInfo.Seats.Single(s => s.Name == row["seat type"]).Id,
                                 Int32.Parse(row["quantity"])));

            Task<string>[] tasks = Enumerable.Range(0, registrants).
                Select(i => Task.Factory.StartNew(() => CreateAndSendRegisterToConference(conferenceInfo.Id, seats))).
                ToArray();

            Task.WaitAll(tasks);

            orderIds = tasks.Select(t => t.Result).ToList();
        }

        [Then(@"only (.*) events for completing the Order reservation are emitted")]
        public void ThenOnlySomeEventsForCompletingTheOrderReservationAreEmitted(int eventCount)
        {
            CollectEvents<OrderReservationCompleted>(eventCount);
        }

        [Then(@"(.*) events for partially completing the order are emitted")]
        public void ThenSomeEventsForPartiallyCompletingTheOrderAreEmitted(int eventCount)
        {
            CollectEvents<OrderPartiallyReserved>(eventCount);
        }

        private void CollectEvents<T>(int count) where T : IEvent
        {
            var timeout = DateTime.Now.Add(Constants.UI.WaitTimeout);
            while (MessageLogHelper.GetEvents<T>(orderIds).Count() != count)
            {
                Assert.True(DateTime.Now < timeout, "Events not collected within the specified timeframe.");
                Thread.Sleep(100);
            }
            // If we get here then we exit the loop with the expected event count
        }

        private string CreateAndSendRegisterToConference(Guid conferenceId, IEnumerable<SeatQuantity> seats)
        {
            var registration = new RegisterToConference {ConferenceId = conferenceId, OrderId = Guid.NewGuid()};
            registration.Seats.AddRange(seats);

            commandBus.Send(registration);

            return registration.OrderId.ToString();
        }
    }

    //This steps only executes on DebugLocal (Sql bus)
    [Binding]
    [Scope(Tag = "@SelfRegistrationReservationWithConcurrencyDomainDebugLocalOnly")]
    public class SelfRegistrationReservationWithConcurrencyDomainStepsDebugLocal
    {
#if LOCAL
        private readonly SelfRegistrationReservationWithConcurrencyDomainSteps steps;
#endif

        public SelfRegistrationReservationWithConcurrencyDomainStepsDebugLocal()
        {
#if LOCAL
            steps = new SelfRegistrationReservationWithConcurrencyDomainSteps();
#endif
        }

        [Given(@"the list of the available Order Items for the CQRS summit 2012 conference")]
        public void GivenTheListOfTheAvailableOrderItemsForTheCqrsSummit2012Conference(Table table)
        {
#if LOCAL
            CommonSteps common = new CommonSteps();
            common.GivenTheListOfTheAvailableOrderItemsForTheCqrsSummit2012Conference(table);
#endif
        }

        [When(@"(.*) Registrants selects these Order Items")]
        public void WhenManyRegistrantsSelectsTheseOrderItems(int registrants, Table table)
        {
#if LOCAL
            steps.WhenManyRegistrantsSelectsTheseOrderItems(registrants, table);
#endif
        }

        [Then(@"only (.*) events for completing the Order reservation are emitted")]
        public void ThenOnlySomeEventsForCompletingTheOrderReservationAreEmitted(int eventCount)
        {
#if LOCAL
            steps.ThenOnlySomeEventsForCompletingTheOrderReservationAreEmitted(eventCount);
#endif
        }

        [Then(@"(.*) events for partially completing the order are emitted")]
        public void ThenSomeEventsForPartiallyCompletingTheOrderAreEmitted(int eventCount)
        {
#if LOCAL
            steps.ThenSomeEventsForPartiallyCompletingTheOrderAreEmitted(eventCount);
#endif
        }
    }
}
