using DashboardWebApp.Data;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Areas.Admin.Pages.OrganizationManagement
{
    public class UpdateModel : PageModel
    {
        private readonly IDbFactory dbFactory;
        private readonly IUserService _userService;
        public UpdateModel(IDbFactory dbFactory, IUserService userService)
        {
            this.dbFactory = dbFactory;
            Input = new UpdateOrganizationModel();
            _userService = userService;
        }

        [BindProperty]
        public UpdateOrganizationModel Input { get; set; }

        [BindProperty]
        public int OrganizationId { get; set; }

        public class UpdateOrganizationModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string OrganizationName { get; set; }

            [Required]
            public long UserQuota { get; set; }
        }

        public async Task<IActionResult> OnGet(int? organizationId)
        {
            if (!_userService.IsUserSuperAdmin())
            {
                return LocalRedirect("/");
            }

            var context = dbFactory.GetDatabaseContext();

            if (organizationId == null || organizationId == 0)
            {
                return LocalRedirect("/Admin/OrganizationManagement");
            }

            var organization = context.Organizations.SingleOrDefault(x => x.OrganizationId == organizationId);

            if (organization == null)
            {
                return LocalRedirect("/Admin/OrganizationManagement");
            }

            OrganizationId = organization.OrganizationId;

            Input.OrganizationName = organization.OrganizationName;
            Input.UserQuota = organization.UserQuota.GetValueOrDefault();

            return Page();
        }

        public IActionResult OnPost(int organizationId)
        {
            if (Input.UserQuota <= 0)
            {
                ModelState.AddModelError("Input.UserQuota", "User Quota can't be zero or negative in value");

                return Page();
            }

            var context = dbFactory.GetDatabaseContext();

            if (ModelState.IsValid)
            {
                var organization = context.Organizations.SingleOrDefault(x => x.OrganizationId == organizationId);

                if (organization == null)
                {
                    return LocalRedirect("/Admin/OrganizationManagement");
                }

                organization.OrganizationName = Input.OrganizationName;
                organization.UserQuota = Input.UserQuota;

                context.SaveChanges();

                return LocalRedirect("/Admin/OrganizationManagement");
            }

            return Page();
        }
    }
}
