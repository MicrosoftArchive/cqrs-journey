// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// �2012 Microsoft. All rights reserved. Certain content used with permission from contributors
// http://cqrsjourney.github.com/contributors/members
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

namespace Registration.ReadModel.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ConferenceDao : IConferenceDao
    {
        private readonly Func<ConferenceRegistrationDbContext> contextFactory;

        public ConferenceDao(Func<ConferenceRegistrationDbContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public ConferenceDetails GetConferenceDetails(string conferenceCode)
        {
            using (var repository = this.contextFactory.Invoke())
            {
                return repository
                    .Query<Conference>()
                    .Where(dto => dto.Code == conferenceCode)
                    .Select(x => new ConferenceDetails { Id = x.Id, Code = x.Code, Name = x.Name, Description = x.Description, StartDate = x.StartDate })
                    .FirstOrDefault();
            }
        }

        public ConferenceAlias GetConferenceAlias(string conferenceCode)
        {
            using (var repository = this.contextFactory.Invoke())
            {
                return repository
                    .Query<Conference>()
                    .Where(dto => dto.Code == conferenceCode)
                    .Select(x => new ConferenceAlias { Id = x.Id, Code = x.Code, Name = x.Name })
                    .FirstOrDefault();
            }
        }

        public IList<ConferenceAlias> GetPublishedConferences()
        {
            using (var repository = this.contextFactory.Invoke())
            {
                return repository
                    .Query<Conference>()
                    .Where(dto => dto.IsPublished)
                    .Select(x => new ConferenceAlias { Id = x.Id, Code = x.Code, Name = x.Name })
                    .ToList();
            }
        }

        public IList<SeatType> GetPublishedSeatTypes(Guid conferenceId)
        {
            using (var repository = this.contextFactory.Invoke())
            {
                return repository.Query<SeatType>()
                    .Where(c => c.ConferenceId == conferenceId)
                    .ToList();
            }
        }

        public IList<SeatTypeName> GetSeatTypeNames(Guid conferenceId)
        {
            using (var repository = this.contextFactory.Invoke())
            {
                return repository.Query<SeatType>()
                    .Where(c => c.ConferenceId == conferenceId)
                    .Select(s => new SeatTypeName { Id = s.Id, Name = s.Name })
                    .ToList();
            }
        }
    }
}