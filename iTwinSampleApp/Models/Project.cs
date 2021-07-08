/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using System;

namespace ItwinProjectSampleApp.Models
    {
    public class Project
        {
        public Project()
            {
            DisplayName = $"iTwin Sample Name {Guid.NewGuid()}";
            ProjectNumber = $"iTwin Sample Number {Guid.NewGuid()}";
            GeographicLocation = "Vilnius, Lithuania";
            Latitude = "54.687157";
            Longitude = "25.279652";
            TimeZone = "EEST";
            BillingCountry = "LT";
            AllowExternalTeamMembers = true;
            }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string ProjectNumber { get; set; }
        public string RegistrationDateTime { get; set; }
        public string GeographicLocation { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string TimeZone { get; set; }
        public string DataCenterLocation { get; set; }
        public string BillingCountry { get; set; }
        public string Status { get; set; }
        public bool AllowExternalTeamMembers { get; set; }
        }
    }
