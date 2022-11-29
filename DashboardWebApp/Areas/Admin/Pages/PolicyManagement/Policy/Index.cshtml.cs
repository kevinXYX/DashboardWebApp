using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.Admin.Pages.PolicyManagement.Policy
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            this._userService = userService;
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
