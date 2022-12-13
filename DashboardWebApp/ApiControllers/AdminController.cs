using DashboardWebApp.Attributes;
using DashboardWebApp.Data;
using DashboardWebApp.Service;
using KendoNET.DynamicLinq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardWebApp.ApiControllers
{
    [AdminAuthorize]
    public class AdminController : ControllerBase
    {
        private readonly IDbFactory dBFactory;
        private readonly IUserService userService;

        public AdminController(IDbFactory dBFactory, IUserService userService)
        {
            this.dBFactory = dBFactory;
            this.userService = userService;
        }

        [HttpPost]
        [Route("api/admin/users")]
        public DataSourceResult GetUsers([FromBody] DataSourceRequest requestModel)
        {
            var currentUser = this.userService.GetCurrentUser();
            var context = this.dBFactory.GetDatabaseContext();
            var userOrganization = userService.GetCurrentUserOrganization();
            var users = context.Users.Include(x => x.ApplicationUser)
                .Include(x => x.Organization).AsEnumerable();
            if (!this.userService.IsUserSuperAdmin())
            {
                users = users.Where(x => x.OrganizationId == userOrganization.OrganizationId);
            }
            var result = users.AsQueryable().ToDataSourceResult(requestModel);
            return result;
        }

        [HttpPost]
        [Route("api/admin/organizations")]
        public DataSourceResult GetOrganizations([FromBody] DataSourceRequest requestModel)
        {
            var context = this.dBFactory.GetDatabaseContext();
            var organizations = context.Organizations.AsQueryable().ToDataSourceResult(requestModel);
            return organizations;
        }

        [HttpPost]
        [Route("api/admin/userpolicies")]
        public DataSourceResult GetUserPolicies([FromBody] DataSourceRequest requestModel)
        {
            var context = this.dBFactory.GetDatabaseContext();
            var userPolicies = context.UserPolicies.AsQueryable().ToDataSourceResult(requestModel);
            return userPolicies;
        }
    }
}
