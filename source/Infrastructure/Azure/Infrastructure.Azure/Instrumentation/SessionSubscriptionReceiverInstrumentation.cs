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

namespace Infrastructure.Azure.Instrumentation
{
    using System.Diagnostics;

    public class SessionSubscriptionReceiverInstrumentation : SubscriptionReceiverInstrumentation, ISessionSubscriptionReceiverInstrumentation
    {
        public const string TotalSessionsCounterName = "Total sessions";

        private readonly PerformanceCounter totalSessionsCounter;

        public SessionSubscriptionReceiverInstrumentation(string instanceName, bool instrumentationEnabled)
            : base(instanceName, instrumentationEnabled)
        {
            if (this.InstrumentationEnabled)
            {
                this.totalSessionsCounter = new PerformanceCounter(Constants.ReceiversPerformanceCountersCategory, TotalSessionsCounterName, this.InstanceName, false);
            }
        }

        public void SessionStarted()
        {
            if (this.InstrumentationEnabled)
            {
                this.totalSessionsCounter.Increment();
            }
        }

        public void SessionEnded()
        {
            if (this.InstrumentationEnabled)
            {
            }
        }
    }
}
