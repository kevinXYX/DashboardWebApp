using DashboardWebApp.Data;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Areas.Admin.Pages.OrganizationManagement
{
    public class CreateModel : PageModel
    {
        private readonly IDbFactory dbFactory;
        private readonly IUserService _userService;
        public CreateModel(IDbFactory dbFactory, IUserService userService)
        {
            this.dbFactory = dbFactory;
            _userService = userService;
        }

        [BindProperty]
        public CreateOrganizationModel Input { get; set; }

        public class CreateOrganizationModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string OrganizationName { get; set; }

            [Required]
            public int UserQuota { get; set; }
        }

        public async Task<IActionResult> OnGet()
        {
            if (!_userService.IsUserSuperAdmin())
            {
                return LocalRedirect("/");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (Input.UserQuota <= 0)
            {
                ModelState.AddModelError("Input.UserQuota", "User Quota can't be zero or negative in value");

                return Page();
            }

            if (ModelState.IsValid)
            {
                var context = dbFactory.GetDatabaseContext();

                var existingOrganization = context.Organizations.SingleOrDefault(x => x.OrganizationName.ToLower() == Input.OrganizationName);

                if (existingOrganization != null)
                {
                    ModelState.AddModelError(string.Empty, "Organization already exists");

                    return Page();
                } 

                context.Organizations.Add(new Organization
                {
                    OrganizationName = Input.OrganizationName,
                    UserQuota = Input.UserQuota,
                    CreatedDate = DateTime.UtcNow
                });

                context.SaveChanges();

                return LocalRedirect("/Admin/OrganizationManagement");
            }

            return Page();
        }
    }
}
