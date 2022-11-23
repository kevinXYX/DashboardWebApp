using DashboardWebApp.Data;
using DashboardWebApp.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.Identity.Pages.Account
{
    public class VerifyEmailModel : PageModel
    {
        private readonly IDbFactory dBFactory;

        public VerifyEmailModel(IDbFactory dBFactory)
        {
            this.dBFactory = dBFactory;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string email)
        {
            if (userId == null || email == null)
            {
                return LocalRedirect("/Identity/Error");
            }

            userId = Crypto.DecryptString(userId);
            email = Crypto.DecryptString(email);

            var context = dBFactory.GetDatabaseContext();

            var user = context.Users
                .SingleOrDefault(x => x.UserId == Convert.ToInt32(userId));

            if (user == null)
            {
                return NotFound($"Unable to find user");
            }

            if (user.ChangeEmailVerified == null)
            {
                return LocalRedirect($"/");
            }

            user.ChangeEmailVerified = true;

            context.SaveChanges();

            TempData["VerifiedEmail"] = email;

            return LocalRedirect($"/Identity/Account/ProfileManagement");
        }
    }
}
