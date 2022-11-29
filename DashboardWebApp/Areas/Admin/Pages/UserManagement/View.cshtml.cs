using DashboardWebApp.Data;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.Admin.Pages.UserManagement
{
    public class ViewModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IDbFactory _dbFactory;
        public ViewModel(IUserService userService, IDbFactory dbFactory)
        {
            _userService = userService;
            _dbFactory = dbFactory;
            Input = new UserViewModel();
        }

        [BindProperty]
        public UserViewModel Input { get; set; }

        public class UserViewModel
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string OrganizationName { get; set; }
            public string UserStatus { get; set; }
            public bool IsAdmin { get; set; }
            public bool HasAPIPermission { get; set; }
            public bool HasDashboardPermission { get; set; }
        }

        public async Task<IActionResult> OnGet(int userId)
        {
            if (!_userService.IsUserAdmin() && !_userService.IsUserSuperAdmin())
            {
                return LocalRedirect("/");
            }

            var user = _userService.GetUserById(userId);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var context = _dbFactory.GetDatabaseContext();

            Input.Email = user.UserName;
            Input.Name = user.Fullname;
            Input.IsAdmin = user.IsAdmin.GetValueOrDefault();
            Input.OrganizationName = user.Organization.OrganizationName;

            if (user.UserStatus == 0)
            {
                Input.UserStatus = "Pending";
            }

            if (user.UserStatus == 1)
            {
                Input.UserStatus = "Active";
            }

            if (user.UserStatus == 2)
            {
                Input.UserStatus = "Deactivated";
            }

            var apiPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "APIAppPermission");
            var dashboardPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "DashboardAppPermission");
            var userPolicyMappings = context.UserPolicyMappings.ToList();

            Input.HasAPIPermission = userPolicyMappings.Any(x => x.UserId == userId && x.PolicyId == apiPolicy.Id);
            Input.HasDashboardPermission = userPolicyMappings.Any(x => x.UserId == userId && x.PolicyId == dashboardPolicy.Id);

            return Page();
        }
    }
}
