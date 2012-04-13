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

namespace Conference.Web.Controllers
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Web.Mvc;

    public class ConferenceController : Controller
    {
        private DomainContext db = new DomainContext();

        public ActionResult Locate()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Locate(string email, string accessCode)
        {
            var conference = db.Conferences.FirstOrDefault(c => c.OwnerEmail == email && c.AccessCode == accessCode);
            if (conference == null)
            {
                ViewBag.NotFound = true;
                // Preserve input so the user doesn't have to type email again.
                ViewBag.Email = email;

                return Locate();
            }

            // TODO: not very secure ;).
            return RedirectToAction("Details", new { id = conference.Id });
        }

        public ActionResult Details(Guid id)
        {
            var conference = db.Conferences.Find(id);
            if (conference == null)
            {
                return HttpNotFound();
            }
            return View(conference);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ConferenceInfo conference)
        {
            if (ModelState.IsValid)
            {
                var existingSlug = db.Conferences
                    .Where(c => c.Slug == conference.Slug)
                    .Select(c => c.Slug)
                    .Any();

                if (existingSlug)
                {
                    ModelState.AddModelError("Slug", "The chosen conference slug is already taken.");
                    return View(conference);
                }

                conference.Id = Guid.NewGuid();
                db.Conferences.Add(conference);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = conference.Id });
            }

            return View(conference);
        }

        public ActionResult Edit(Guid id)
        {
            var conference = db.Conferences.Find(id);
            if (conference == null)
            {
                return HttpNotFound();
            }
            return View(conference);
        }

        [HttpPost]
        public ActionResult Edit(ConferenceInfo conference)
        {
            if (ModelState.IsValid)
            {
                db.Entry(conference).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = conference.Id });
            }

            return View(conference);
        }

        public ActionResult Delete(Guid id)
        {
            var conference = db.Conferences.Find(id);
            if (conference == null)
            {
                return HttpNotFound();
            }
            return View(conference);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            var conference = db.Conferences.Find(id);
            db.Conferences.Remove(conference);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Publish(Guid id)
        {
            return SetPublished(id, true);
        }

        [HttpPost]
        public ActionResult Unpublish(Guid id)
        {
            return SetPublished(id, false);
        }

        private ActionResult SetPublished(Guid id, bool isPublished)
        {
            var conference = db.Conferences.Find(id);
            if (conference == null)
            {
                return HttpNotFound();
            }

            conference.IsPublished = isPublished;
            db.SaveChanges();

            // TODO: not very secure ;).
            return RedirectToAction("Details", new { id = id });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}