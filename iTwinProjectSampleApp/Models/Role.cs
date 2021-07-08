/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace ItwinProjectSampleApp.Models
{
    public class Role
        {
        public Role()
            {
            DisplayName = "Project Administrator";
            Description = "Project Administrator";
            }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; }
        }
}
