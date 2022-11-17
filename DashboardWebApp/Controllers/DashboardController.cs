using DashboardWebApp.Data;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DashboardWebApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IUserService userService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DashboardController(IUserService userService, SignInManager<ApplicationUser> signInManager)
        {
            this.userService = userService;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var hasDashboardPermission = userService.UserHasDashboardPermission();

            if (!hasDashboardPermission)
            {
                await _signInManager.SignOutAsync();
                return LocalRedirect("/Identity/Account/Login");
            }

            return View();
        }
    }
}
