/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItwinProjectSampleApp.Models
{
    public class Member
        {
        public string Email { get; set; }
        
        /// <summary>
        /// Role names
        /// </summary>
        public List<string> Roles { get; set; }
        }
}
