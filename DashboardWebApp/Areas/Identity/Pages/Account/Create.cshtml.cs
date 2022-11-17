using DashboardWebApp.Data;
using DashboardWebApp.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace DashboardWebApp.Areas.Identity.Pages.Account
{
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDbFactory dbFactory;

        [BindProperty]
        public InputModel Input { get; set; }

        [ValidateNever]
        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; }

            [ValidateNever]
            public string EmailDisplay { get; set; }
        }

        public CreateModel(UserManager<ApplicationUser> userManager, IDbFactory dbFactory)
        {
            Input = new InputModel();
            _userManager = userManager;
            this.dbFactory = dbFactory;
        }

        public IActionResult OnGetAsync(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
               return LocalRedirect("/Identity/Error");
            }

            var context = dbFactory.GetDatabaseContext();

            Input.EmailDisplay = Crypto.DecryptString(Id);

            var user = context.Users.SingleOrDefault(x => x.UserName == Input.EmailDisplay);

            if (user == null)
            {
                return LocalRedirect("/Identity/Error");
            }

            if (user.UserStatus == 1)
            {
                return LocalRedirect("/Identity/Account/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string email)
        {
            ModelState.Remove("ReturnUrl");

            if (Input.Password != Input.ConfirmPassword)
            {
                ModelState.AddModelError("Input.Password", "Passwords do not match");
                ModelState.AddModelError("Input.ConfirmPassword", "Passwords do not match");
            }

            if (ModelState.IsValid)
            {
                var ctx = dbFactory.GetDatabaseContext();

                var user = ctx.Users.Include(x => x.ApplicationUser).SingleOrDefault(x => x.UserName == email);

                if (user == null)
                {
                    return LocalRedirect("/Identity/Error");
                }

                var passwordHasher = new PasswordHasher<string>();

                user.ApplicationUser.PasswordHash = passwordHasher.HashPassword(user.UserName, Input.ConfirmPassword);

                user.UserStatus = 1;

                ctx.SaveChanges();

                //var result = await _userManager.ResetPasswordAsync(user, AccountCode, Input.ConfirmPassword);

                //foreach (var error in result.Errors)
                //{
                //    ModelState.AddModelError(string.Empty, error.Description);
                //}

                //if (result.Errors != null && result.Errors.Count() > 0)
                //{
                //    return Page();
                //}

                return LocalRedirect("/");
            }

            return Page();
        }
    }
}
