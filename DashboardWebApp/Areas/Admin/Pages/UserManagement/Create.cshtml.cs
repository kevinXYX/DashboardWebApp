using DashboardWebApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using SendGrid;
using SendGrid.Helpers.Mail;
using DashboardWebApp.Helpers;

namespace DashboardWebApp.Areas.Admin.Views.UserManagement
{
    public class CreateModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly IDbFactory _dbFactory;
        private readonly IConfiguration configuration;

        public CreateModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            IDbFactory dbFactory,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _emailSender = emailSender;
            _dbFactory = dbFactory;
            this.configuration = configuration;
        }

        [BindProperty]
        public CreateInputModel Input { get; set; }

        [BindProperty]
        public List<SelectListItem> Organizations { get; set; }

        public class CreateInputModel
        {
            [Required]
            [EmailAddress]  
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Text)]
            public string Name { get; set; }
            public int OrganizationId { get; set; }
            public bool IsAdmin { get; set; }
            public bool HasAPIPermission { get; set; }
            public bool HasDashboardPermission { get; set; }
        }

        public string ReturnUrl { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            SetOrganizationDropdown();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = "/Admin/UserManagement")
        {
            returnUrl ??= Url.Content("~/");
            ModelState.Remove("ReturnUrl");

            if (ModelState.IsValid)
            {
                var context = _dbFactory.GetDatabaseContext();

                var isUserExists = context.Users.Any(x => x.UserName.ToLower() == Input.Email.ToLower());

                if (isUserExists)
                {
                    ModelState.AddModelError(string.Empty, "User already exists");

                    SetOrganizationDropdown();

                    return Page();
                }

                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                user.User = new User { LogoFileName = String.Empty, UserName = user.UserName, Password = String.Empty, Fullname = Input.Name, IsAdmin = Input.IsAdmin, OrganizationId = Input.OrganizationId, UserStatus = 0, CreatedDate = DateTime.UtcNow };

                var result = await _userManager.CreateAsync(user, "P" + Guid.NewGuid().ToString()); //set default password to be changed later by the user

                if (result.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user);
                    var userInDb = context.ApplicationUsers.Include(x => x.User).SingleOrDefault(x => x.Id == userId);
                    
                    if (userInDb != null)
                    {
                        userInDb.User.AspNetUserId = userInDb.UserId;

                        if (Input.HasAPIPermission)
                        {
                            var apiPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "APIAppPermission");

                            if (apiPolicy != null)
                            {
                                context.UserPolicyMappings.Add(new UserPolicyMapping
                                {
                                    User = userInDb.User,
                                    UserPolicy = apiPolicy
                                });
                            }
                        }

                        if (Input.HasDashboardPermission)
                        {
                            var dashboardPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "DashboardAppPermission");

                            if (dashboardPolicy != null)
                            {
                                context.UserPolicyMappings.Add(new UserPolicyMapping
                                {
                                    User = userInDb.User,
                                    UserPolicy = dashboardPolicy
                                });
                            }
                        }

                        context.SaveChanges();
                    }

                    var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    confirmationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationCode));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, Id = Crypto.EncryptString(Input.Email), confirmationToken = confirmationCode, returnUrl = returnUrl },
                        protocol: Request.Scheme);
                    var sendGridApiKey = configuration.GetSection("SENDGRID_API_KEY");
                    var sendGridClient = new SendGridClient(sendGridApiKey.Value);
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress("test@dashboardapp.com", "Dashboard Admin"),
                        Subject = "Setup your account",
                        HtmlContent = $"Setup your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>."
                    };
                    msg.AddTo(new EmailAddress(Input.Email, Input.Name));
                    var response = await sendGridClient.SendEmailAsync(msg);

                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private void SetOrganizationDropdown()
        {
            var organizations = _dbFactory.GetDatabaseContext().Organizations.ToList();

            var organizationSelectList = new List<SelectListItem>();

            if (organizations != null)
            {
                organizations.ForEach(x =>
                {
                    organizationSelectList.Add(new SelectListItem { Text = x.OrganizationName, Value = x.OrganizationId.ToString() });
                });
            }

            Organizations = organizationSelectList;
        }
    }
}
