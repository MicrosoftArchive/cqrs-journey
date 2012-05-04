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
using System.Configuration;

namespace Conference.Specflow.Support
{
    static class Constants
    {
        public static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);
        public const string NoWatiN = "NoWatiN";

        public static class UI
        {
            public const string NextStepButtonId = "Next";
            public const string ReservationSuccessfull = "Seats information";
            public const string ReservationUnsuccessfull = "Could not reserve all requested seats.";
            public const string FindOrderSuccessfull = "Purchase details";
            public const string RegistrationSuccessfull = "Thank you";
            public const string AcceptPaymentInputValue = "accepted";
            public const string RejectPaymentInputValue = "rejected";
            public static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(20); // Wait > 5 secs, longer than page retry 
        }

        public static string RegistrationPage(string conferenceSlug)
        {
            return string.Format(ConfigurationManager.AppSettings["testConferenceUrl"], conferenceSlug, "register");
        }

        public static string FindOrderPage(string conferenceSlug)
        {
            return string.Format(ConfigurationManager.AppSettings["testConferenceUrl"], conferenceSlug, "order/find");
        }
    }
}
