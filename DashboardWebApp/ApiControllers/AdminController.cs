using DashboardWebApp.Data;
using DashboardWebApp.Service;
using KendoNET.DynamicLinq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DashboardWebApp.ApiControllers
{
    [Authorize]
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
            if (this.userService.GetCurrentUser().IsAdmin.GetValueOrDefault())
            {
                var context = this.dBFactory.GetDatabaseContext();
                var users = context.Users.Include(x => x.ApplicationUser).AsQueryable().ToDataSourceResult(requestModel);
                return users;
            }

            return null;
        }

        [HttpPost]
        [Route("api/admin/organizations")]
        public DataSourceResult GetOrganizations([FromBody] DataSourceRequest requestModel)
        {
            if (this.userService.GetCurrentUser().IsAdmin.GetValueOrDefault())
            {
                var context = this.dBFactory.GetDatabaseContext();
                var organizations = context.Organizations.AsQueryable().ToDataSourceResult(requestModel);
                return organizations;
            }

            return null;
        }

        [HttpPost]
        [Route("api/admin/userpolicies")]
        public DataSourceResult GetUserPolicies([FromBody] DataSourceRequest requestModel)
        {
            if (this.userService.GetCurrentUser().IsAdmin.GetValueOrDefault())
            {
                var context = this.dBFactory.GetDatabaseContext();
                var userPolicies = context.UserPolicies.AsQueryable().ToDataSourceResult(requestModel);
                return userPolicies;
            }

            return null;
        }
    }
}
