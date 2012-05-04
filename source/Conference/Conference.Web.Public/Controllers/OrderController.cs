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

namespace Conference.Web.Public.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using AutoMapper;
    using Infrastructure.Messaging;
    using Registration.Commands;
    using Registration.ReadModel;

    public class OrderController : ConferenceTenantController
    {
        private readonly IOrderDao orderDao;
        private readonly ICommandBus bus;

        static OrderController()
        {
            Mapper.CreateMap<OrderSeat, AssignSeat>();
        }

        public OrderController(IConferenceDao conferenceDao, IOrderDao orderDao, ICommandBus bus)
            : base(conferenceDao)
        {
            this.orderDao = orderDao;
            this.bus = bus;
        }

        [HttpGet]
        public ActionResult Display(Guid orderId)
        {
            var order = orderDao.GetPricedOrder(orderId);
            if (order == null)
                return RedirectToAction("Find", new { conferenceCode = this.ConferenceCode });

            return View(order);
        }

        [HttpGet]
        [OutputCache(Duration = 0)]
        public ActionResult AssignSeats(Guid orderId)
        {
            var assignments = this.orderDao.FindOrderSeats(orderId);
            if (assignments == null)
                return RedirectToAction("Find", new { conferenceCode = this.ConferenceCode });

            return View(assignments);
        }

        [HttpPost]
        public ActionResult AssignSeats(Guid orderId, Guid assignmentsId, List<OrderSeat> seats)
        {
            var saved = this.orderDao.FindOrderSeats(orderId);
            if (saved == null)
                return RedirectToAction("Find", new { conferenceCode = this.ConferenceCode });

            var pairs = seats
                .Select(dto => new { Saved = saved.Seats.FirstOrDefault(x => x.Position == dto.Position), New = dto })
                // Ignore posted seats that we don't have saved already: pair.Saved == null
                // This may be because the client sent mangled or incorrect data so we couldn't 
                // find a matching saved seat.
                // Also, if pair.New is null, it's because it's an invalid null entry 
                // in the list of seats, so we ignore that too.
                .Where(pair => pair.Saved != null && pair.New != null)
                // Only process those where they don't remain unassigned.
                .Where(pair => pair.Saved.Attendee.Email != null || pair.New.Attendee.Email != null)
                .ToList();

            var unassigned = pairs
                .Where(x => !string.IsNullOrWhiteSpace(x.Saved.Attendee.Email) && string.IsNullOrWhiteSpace(x.New.Attendee.Email))
                .Select(x => (ICommand)new UnassignSeat { SeatAssignmentsId = orderId, Position = x.Saved.Position });

            var changed = pairs
                .Where(x => x.Saved.Attendee != x.New.Attendee)
                .Select(x => (ICommand)Mapper.Map(x.New, new AssignSeat { SeatAssignmentsId = assignmentsId }));

            var commands = unassigned.Union(changed).ToList();
            if (commands.Count > 0)
            {
                this.bus.Send(commands);
            }

            return RedirectToAction("Display");
        }

        [HttpGet]
        public ActionResult Find()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Find(string email, string accessCode)
        {
            var orderId = orderDao.LocateOrder(email, accessCode);

            if (!orderId.HasValue)
            {
                // TODO: 404?
                return RedirectToAction("Find", new { conferenceCode = this.ConferenceCode });
            }

            return RedirectToAction("Display", new { conferenceCode = this.ConferenceCode, orderId = orderId.Value });
        }
    }
}