/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace ItwinProjectSampleApp
    {
    class Program
        {
        static async Task Main (string[] args)          
            {         
            DisplayMainIndex();

            // Retrieve the token using the TryIt button. https://developer.bentley.com/api-groups/administration/apis/projects/operations/create-project/
            Console.WriteLine("\n\nCopy and paste the Authorization header from the 'Try It' sample in the APIM front-end:  ");
            string authorizationHeader = Console.ReadLine();
            Console.Clear();

            DisplayMainIndex();

            await using var projectMgr = new ProjectManager(authorizationHeader);

            // Execute Project workflow. This will create/update/query an iTwin project
            await projectMgr.ProjectManagementWorkflow();

            // Execute User Management workflow. This will create an iTwin project, create a project role and add a user to the project
            // with that role. The user must be a valid Bentley user so we we get it from the token to be sure. You can change this to another user.
            var projectUserEmail = RetrieveEmailFromAuthHeader(authorizationHeader); 
            await projectMgr.ProjectUserManagementWorkflow(projectUserEmail);
            }

        #region Private Methods
        private static void DisplayMainIndex()
            {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.WriteLine("*****************************************************************************************");
            Console.WriteLine("*           iTwin Platform Sample App                                                   *");
            Console.WriteLine("*****************************************************************************************\n");
            }

        private static string RetrieveEmailFromAuthHeader(string authorizationHeader)
            {
            var jwt = authorizationHeader?.Split(" ")[1]?.Trim();
            if (string.IsNullOrWhiteSpace(jwt))
                throw new ApplicationException("The jwt token is incorrect.  Ensure that 'Bearer ' precedes the token in the header.");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);
            var email = token.Claims?.FirstOrDefault(x => x.Type.Equals("Email", StringComparison.OrdinalIgnoreCase))?.Value;
            return email;
            }
        #endregion
        }
    }
