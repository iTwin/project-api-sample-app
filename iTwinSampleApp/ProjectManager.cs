/*--------------------------------------------------------------------------------------+
|
| Copyright (c) Bentley Systems, Incorporated. All rights reserved.
| See LICENSE.md in the project root for license terms and full copyright notice.
|
+--------------------------------------------------------------------------------------*/

using ItwinProjectSampleApp.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ItwinProjectSampleApp
    {
    internal class ProjectManager : IAsyncDisposable
        {
        private EndpointManager _endpointMgr;
        private List<Project> _projects; // Projects that will be deleted in DisposeAsync

        #region Constructors
        internal ProjectManager(string token)
            {
            _endpointMgr = new EndpointManager(token);
            _projects = new List<Project>();
            }

        public async ValueTask DisposeAsync()
            {
            Console.Write($"\n\n- Deleting any projects that were created");

            foreach (var p in _projects)
                await DeleteProject(p.Id);

            Console.Write(" (SUCCESS)\n\n");
            }

        #endregion

        #region GET
        /// <summary> 
        /// Get my projects - This will return projects that the user can access.  It is not using paging so it will only return
        /// the top 100 projects by default.
        /// </summary>
        /// <param name="projectNumber"></param>
        /// 
        /// Filter by projectNumber. It should return 1 project with the specified projectNumber. 
        /// This is the SQL equivalent of projectNumber = '{sampleProject.ProjectNumber}'
        /// 
        /// <param name="search"></param>
        /// 
        /// Get my projects - Wildcard Search. It should return any project with "iTwin Sample" in the projectNumber or displayName.
        /// This is the SQL equivalent of (projectNumber like '%iTwin Sample%' OR displayName like '%iTwin Sample%')
        /// Only use the wildcard search if the projectNumber or displayName filters are not sufficient. The wildcard search is slower. 
        /// 
        /// <returns></returns>
        internal async Task<List<Project>> GetMyProjects (string projectNumber = null, string search = null)
            { 
            var showAllPropertiesHeader = new Dictionary<string, string>
                {
                    { "Prefer", "return=representation" }
                };

            string queryString = string.Empty;
            if (!string.IsNullOrWhiteSpace(projectNumber))
                {
                Console.Write($"\n\n- Getting List of My Projects with projectNumber={projectNumber}");
                queryString = $"?projectNumber={projectNumber}";
                }
            else if (!string.IsNullOrWhiteSpace(search))
                {
                Console.Write($"\n\n- Getting List of My Projects with $search={search}");
                queryString = $"?$search={search}";
                }
            else
                Console.Write("\n\n- Getting List of My Projects");

            var responseMsg = await _endpointMgr.MakeGetCall<Project>($"/projects{queryString}", showAllPropertiesHeader);
            if ( responseMsg.Status != HttpStatusCode.OK )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances?.Count} Projects] (SUCCESS)");

            return responseMsg.Instances;
            }

        /// <summary>
        /// Get single project using the specified project id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task<Project> GetProject (string id)
            {
            Console.Write($"\n\n- Getting Project with id {id}");

            var showAllPropertiesHeader = new Dictionary<string, string>
                {
                    { "Prefer", "return=representation" }
                };
            var responseMsg = await _endpointMgr.MakeGetSingleCall<Project>($"/projects/{id}", showAllPropertiesHeader);
            if ( responseMsg.Status != HttpStatusCode.OK )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write(" (SUCCESS)");

            return responseMsg.Instance;
            }

        /// <summary>
        ///  Get my favorite projects (GET)
        /// </summary>
        /// <returns></returns>
        internal async Task<List<Project>> GetMyFavoriteProjects ()
            {
            Console.Write("\n\n- Getting List of My Project Favorites");

            var showAllPropertiesHeader = new Dictionary<string, string>
                {
                    { "Prefer", "return=representation" }
                };
            var responseMsg = await _endpointMgr.MakeGetCall<Project>($"/projects/favorites", showAllPropertiesHeader);
            if ( responseMsg.Status != HttpStatusCode.OK )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Count}] (SUCCESS)");

            return responseMsg.Instances;
            }

        /// <summary>
        /// Get my recently used projects (GET). 
        /// </summary>
        /// <returns></returns>
        internal async Task<List<Project>> GetMyRecentlyUsedProjects ()
            { 
            Console.Write("\n\n- Getting List of My Recent Projects");

            var showAllPropertiesHeader = new Dictionary<string, string>
                {
                { "Prefer", "return=representation" }
                };
            var responseMsg = await _endpointMgr.MakeGetCall<Project>($"/projects/recents", showAllPropertiesHeader);
            if ( responseMsg.Status != HttpStatusCode.OK )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Count}] (SUCCESS)");

            return responseMsg.Instances;
            }
        
        /// <summary>
        /// Get list of project roles
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        internal async Task<List<Role>> GetProjectRoles(string projectId)
            {
            Console.Write("\n\n- Getting List of Project Roles");
             
            var responseMsg = await _endpointMgr.MakeGetCall<Role>($"/projects/{projectId}/roles");
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Count}] (SUCCESS)");

            return responseMsg.Instances;
            }

        internal async Task<List<Member>> GetProjectMembers(string projectId)
            {
            Console.Write("\n\n- Getting List of Project Members");

            var responseMsg = await _endpointMgr.MakeGetCall<Member>($"/projects/{projectId}/members");
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" [Retrieved {responseMsg.Instances.Count}] (SUCCESS)");

            return responseMsg.Instances;
            }



        #endregion

        #region POST
        /// <summary>
        /// Create project (POST)
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        internal async Task<Project> CreateProject (Project project = null)
            {
            if(project == null)
                project = new Project();

            Console.Write("\n\n- Creating a Project");

            var responseMsg = await _endpointMgr.MakePostCall("/projects", project);
            if ( responseMsg.Status != HttpStatusCode.Created )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write(" (SUCCESS)");

            _projects.Add(responseMsg.NewInstance);

            return responseMsg.NewInstance;
            }

        /// <summary>
        /// Add project to my recents. There is a current max of 25 recents per user but this could change in the future.
        /// If you add a 26th project then the oldest recent will be removed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task AddProjectToMyRecents (string id)
            {
            Console.Write($"\n\n- Adding Project {id} to My Recents");

            var responseMsg = await _endpointMgr.MakePostCall<Project>($"/projects/recents/{id}");
            if ( responseMsg.Status != HttpStatusCode.OK )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write(" (SUCCESS)"); 
            }

        /// <summary>
        /// Add project to my favorites. Currently, there is no max number of favorites but this could change in the future.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task AddProjectToMyFavorites (string id)
            {
            Console.Write($"\n\n- Adding project {id} to My Favorites");

            var responseMsg = await _endpointMgr.MakePostCall<Project>($"/projects/favorites/{id}");
            if ( responseMsg.Status != HttpStatusCode.OK )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write(" (SUCCESS)"); 
            }


        internal async Task<Role> CreateProjectRole(string projectId, Role role = null)
            {
            if (role == null)
                role = new Role();

            Console.Write("\n\n- Creating Project Role");

            var responseMsg = await _endpointMgr.MakePostCall($"/projects/{projectId}/roles", role);
            if (responseMsg.Status != HttpStatusCode.Created)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write(" (SUCCESS)");
             
            return responseMsg.NewInstance;  
            }

        internal async Task AddProjectTeamMember(string projectId, string email, string roleName)
            {
            Console.Write("\n\n- Adding User to Project");

            var member = new { Email = email, roleNames = new[] { roleName } };

            var responseMsg = await _endpointMgr.MakePostCall($"/projects/{projectId}/members", member);
            if (responseMsg.Status != HttpStatusCode.Created)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write(" (SUCCESS)");
            }


        #endregion

        #region PATCH
        /// <summary>
        /// Update project name (PATCH). This method could be modified to update any project property or multiple properties.
        /// You only need to specify the properties that you want to update. In this case, it is only updating the DisplayName property.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        internal async Task UpdateProject (Project project, string newName)
            {
            Console.Write($"\n\n- Updating Project Name for ({project.Id})");

            var responseMsg = await _endpointMgr.MakePatchCall<Project>($"/projects/{project.Id}", new
                {
                DisplayName = newName
                });
            if ( responseMsg.Status != HttpStatusCode.OK )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" (SUCCESS)"); 
            }

       /// <summary>
       /// Update DisplayName or Description of the role. Assign/Modify permissions assigned to the role.
       /// </summary>
       /// <param name="projectId"></param>
       /// <param name="role"></param>
       /// <returns></returns>
        internal async Task UpdateRole(string projectId, Role role)
            {
            Console.Write($"\n\n- Updating Role ({role.Id})");

            var responseMsg = await _endpointMgr.MakePatchCall<Role>($"/projects/{projectId}/roles/{role.Id}", role);
            if (responseMsg.Status != HttpStatusCode.OK)
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}");

            Console.Write($" (SUCCESS)"); 
            }

        #endregion

        #region DELETE
        /// <summary>
        /// Delete project (DELETE) - If you are writing tests for your code, always delete projects when they are no longer needed.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task DeleteProject (string id)
            { 
            var responseMsg = await _endpointMgr.MakeDeleteCall<Project>($"/projects/{id}");
            if ( responseMsg.Status != HttpStatusCode.NoContent )
                throw new Exception($"{responseMsg.Status}: {responseMsg.ErrorDetails?.Code} - {responseMsg.ErrorDetails?.Message}"); 
            }
        #endregion

        /// <summary>
        /// Demonstrates project administration functionality.
        /// </summary>
        /// <returns></returns>
        internal async Task ProjectManagementWorkflow()
            {
            #region Manage Projects 
            var createdProject = await CreateProject(); 

            // Get the project created above
            var retrievedSingleProject = await GetProject(createdProject.Id);

            // Get All Projects including project created above
            var retrievedProjects = await GetMyProjects();

            // Get project using Project Number. 
            var projectsFilterByNumber = await GetMyProjects(projectNumber: createdProject.ProjectNumber);

            // Get projects using a wildcard search.
            var projectsSearch = await GetMyProjects(search: "iTwin Sample");
            
            // Update the project name
            await UpdateProject(createdProject, createdProject.DisplayName + " Updated");
            #endregion

            #region Manage Recent/Favorite Projects
            // Add project to recent and favorites
            await AddProjectToMyRecents(createdProject.Id);
            await AddProjectToMyFavorites(createdProject.Id);

            // Get recent projects and favorite projects
            var listOfFaves = await GetMyFavoriteProjects();
            var listOfRecents = await GetMyRecentlyUsedProjects();
            #endregion

            // Any projects that were created as part of this sample will be deleted in DisposeAsync
            }


        /// <summary>
        /// Demonstrates project user management functionality
        /// </summary>
        /// <returns></returns>
        internal async Task ProjectUserManagementWorkflow(string projectUserEmail)
            {
            var project = await CreateProject();

            var role =  await CreateProjectRole(project.Id);

            role.Permissions = new List<string>() { "administration_invite_member", "administration_manage_roles", "administration_remove_member" };

            // Assign permissions to the role
            await UpdateRole(project.Id, role);

            // GET all project roles
            var roles = await GetProjectRoles(project.Id);

            // Invite user to the project
            await AddProjectTeamMember(project.Id, projectUserEmail, role.DisplayName);

            // GET list of users invited to the project
            var members = await GetProjectMembers(project.Id);

            // Any projects that were created as part of this sample will be deleted in DisposeAsync
            }

        }
    }
