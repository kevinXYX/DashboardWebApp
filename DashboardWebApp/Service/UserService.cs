using DashboardWebApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DashboardWebApp.Service
{
    public class UserService : IUserService
    {
        private readonly ClaimsPrincipal _user;
        private readonly IDbFactory _dbFactory;
        public UserService(ClaimsPrincipal user, IDbFactory dbFactory)
        {
            _user = user;
            _dbFactory = dbFactory;
        }

        public User GetUserById(int userId)
        {
            var user = _dbFactory.GetDatabaseContext().Users.Include(x => x.ApplicationUser).Include(x => x.Organization).SingleOrDefault(x => x.UserId == userId);
            return user;
        }

        public User GetCurrentUser()
        {
            var userEmail = _user.FindFirst(ClaimTypes.Email)?.Value;
            var user = _dbFactory.GetDatabaseContext().Users.Include(x => x.ApplicationUser).Include(x => x.Organization).SingleOrDefault(x => x.UserName == userEmail);
            return user;
        }

        public Organization GetCurrentUserOrganization()
        {
            var userEmail = _user.FindFirst(ClaimTypes.Email)?.Value;
            var user = _dbFactory.GetDatabaseContext().Users.Include(x => x.ApplicationUser).SingleOrDefault(x => x.UserName == userEmail);
            var organization = _dbFactory.GetDatabaseContext().Organizations.SingleOrDefault(x => x.OrganizationId == user.OrganizationId);
            return organization;
        }

        public bool IsUserAdmin()
        {
            var userEmail = _user.FindFirst(ClaimTypes.Email)?.Value;
            var isUserAdmin = _dbFactory.GetDatabaseContext().Users.SingleOrDefault(x => x.UserName == userEmail)?.IsAdmin;
            return isUserAdmin.GetValueOrDefault();
        }

        public bool IsUserSuperAdmin()
        {
            var userEmail = _user.FindFirst(ClaimTypes.Email)?.Value;
            var isUserAdmin = _dbFactory.GetDatabaseContext().Users.SingleOrDefault(x => x.UserName == userEmail)?.IsSuperAdmin;
            return isUserAdmin.GetValueOrDefault();
        }

        public bool UserHasDashboardPermission()
        {
            var userEmail = _user.FindFirst(ClaimTypes.Email)?.Value;
            var context = _dbFactory.GetDatabaseContext();
            var user = context.Users.SingleOrDefault(x => x.UserName == userEmail);

            if (user == null)
            {
                return false;
            }

            var dashboardPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "DashboardAppPermission");
            var userMapping = context.UserPolicyMappings.SingleOrDefault(x => x.UserId == user.UserId && x.PolicyId == dashboardPolicy.Id);
            return userMapping != null;
        }

        public bool IsUserDeactivated()
        {
            var userEmail = _user.FindFirst(ClaimTypes.Email)?.Value;
            var context = _dbFactory.GetDatabaseContext();
            var user = context.Users.SingleOrDefault(x => x.UserName == userEmail);

            if (user == null)
            {
                return false;
            }

            return user.UserStatus == 3;
        }
    }
}
