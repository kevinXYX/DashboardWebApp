using DashboardWebApp.Data;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.Admin.Pages.OrganizationManagement
{
    public class ViewModel : PageModel
    {
        private readonly IDbFactory dBFactory;
        private readonly IUserService _userService;
        public ViewModel(IDbFactory dBFactory, IUserService userService)
        {
            this.dBFactory = dBFactory;
            Input = new CreateOrganizationModel();
            _userService = userService;
        }

        [BindProperty]
        public CreateOrganizationModel Input { get; set; }

        public class CreateOrganizationModel
        {
            public string OrganizationName { get; set; }
            public long UserQuota { get; set; }
        }

        public async Task<IActionResult> OnGet(int organizationId)
        {
            if (!_userService.IsUserSuperAdmin())
            {
                return LocalRedirect("/");
            }

            var organization = dBFactory.GetDatabaseContext().Organizations.SingleOrDefault(x => x.OrganizationId == organizationId);

            if (organization == null)
            {
                return LocalRedirect("/Admin/OrganizationManagement");
            }

            Input.OrganizationName = organization.OrganizationName;
            Input.UserQuota = organization.UserQuota.GetValueOrDefault();

            return Page();
        }
    }
}
