using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.Admin.Views.UserManagement
{
    public class IndexModel : PageModel
    {
        public readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!_userService.IsUserAdmin() && !_userService.IsUserSuperAdmin())
            {
                return LocalRedirect("/");
            }

            return Page();
        }
    }
}
