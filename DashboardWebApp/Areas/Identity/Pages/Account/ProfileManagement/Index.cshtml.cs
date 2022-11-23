using DashboardWebApp.Data;
using DashboardWebApp.Helpers;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.Encodings.Web;

namespace DashboardWebApp.Areas.Identity.Pages.Account.ProfileManagement
{
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IDbFactory _dbFactory;
        private readonly IConfiguration configuration;

        public IndexModel(IUserService userService, IDbFactory dbFactory, IConfiguration configuration)
        {
            _userService = userService;
            _dbFactory = dbFactory;
            Input = new UserViewModel();
            this.configuration = configuration;
        }

        [BindProperty]
        public UserViewModel Input { get; set; }

        [BindProperty]
        public string StatusMessage { get; set; }

        public class UserViewModel
        {
            public string Email { get; set; }
            public string Name { get; set; }
            public string OrganizationName { get; set; }
        }

        public void OnGet()
        {
            var currentUser = _userService.GetCurrentUser();

            var tempVerifiedEmail = TempData["VerifiedEmail"];

            if (tempVerifiedEmail != null)
            {
                StatusMessage = "Email " + tempVerifiedEmail.ToString() + " is verified, You can now change email address";
                Input.Email = tempVerifiedEmail.ToString();
            }
            else
            {
                Input.Email = currentUser.UserName;
            }

            Input.Name = currentUser.Fullname;
            Input.OrganizationName = currentUser?.Organization?.OrganizationName;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = "/")
        {
            returnUrl ??= Url.Content("~/");
            ModelState.Remove("ReturnUrl");
            ModelState.Remove("StatusMessage");
            ModelState.Remove("Input.OrganizationName");
            var currentUser = _userService.GetCurrentUser();

            if (ModelState.IsValid)
            {
                var context = _dbFactory.GetDatabaseContext();

                var userInDb = context.Users.Include(x => x.ApplicationUser).Include(x => x.Organization).SingleOrDefault(x => x.UserId == currentUser.UserId);

                if (userInDb.UserName != Input.Email)
                {
                    var emailExists = context.Users.Any(x => x.UserName == Input.Email);

                    if (emailExists)
                    {
                        StatusMessage = "Email already taken";

                        return Page();
                    }

                    if (userInDb.ChangeEmailVerified == null)
                    {
                        userInDb.ChangeEmailVerified = false;

                        var sendGridApiKey = configuration.GetSection("SENDGRID_API_KEY");
                        var sendGridClient = new SendGridClient(sendGridApiKey.Value);
                        var callbackUrl = Url.Page(
                            "/Account/VerifyEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = Crypto.EncryptString(userInDb.UserId.ToString()), Email = Crypto.EncryptString(Input.Email), returnUrl = returnUrl },
                            protocol: Request.Scheme);
                        var msg = new SendGridMessage()
                        {
                            From = new EmailAddress("test@dashboardapp.com", "Dashboard Admin"),
                            Subject = "Verify your email",
                            HtmlContent = $"Verify your email by clicking here <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
                        };
                        msg.AddTo(new EmailAddress(Input.Email, Input.Name));
                        var response = await sendGridClient.SendEmailAsync(msg);
                        userInDb.Fullname = Input.Name;
                        Input.OrganizationName = userInDb.Organization.OrganizationName;
                        context.SaveChanges();
                        StatusMessage = "We have sent an email confirmation to " + Input.Email + " please confirm to change email address";
                        return Page();
                    }
                    else
                    {
                        if (userInDb.ChangeEmailVerified.HasValue && userInDb.ChangeEmailVerified.Value)
                        {
                            userInDb.UserName = Input.Email;
                            userInDb.ApplicationUser.UserName = Input.Email;
                            userInDb.ApplicationUser.NormalizedUserName = Input.Email;
                            userInDb.ApplicationUser.Email = Input.Email;
                            userInDb.ApplicationUser.NormalizedEmail = Input.Email;
                        }

                        if (userInDb.ChangeEmailVerified.HasValue && !userInDb.ChangeEmailVerified.Value)
                        {
                            StatusMessage = "We have sent an email confirmation to " + Input.Email + " please confirm to change email address";
                            Input.OrganizationName = userInDb.Organization.OrganizationName;
                            userInDb.Fullname = Input.Name;
                            context.SaveChanges();
                            return Page();
                        }

                        userInDb.Fullname = Input.Name;
                        userInDb.ChangeEmailVerified = null;
                        context.SaveChanges();
                        return LocalRedirect("/Identity/Account/Logout");
                    }
                }

                userInDb.Fullname = Input.Name;
                context.SaveChanges();
                return LocalRedirect("/");
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
