using DashboardWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.Admin.Pages.OrganizationManagement
{
    public class ViewModel : PageModel
    {
        private readonly IDbFactory dBFactory;

        public ViewModel(IDbFactory dBFactory)
        {
            this.dBFactory = dBFactory;
            Input = new CreateOrganizationModel();
        }


        [BindProperty]
        public CreateOrganizationModel Input { get; set; }

        public class CreateOrganizationModel
        {
            public string OrganizationName { get; set; }
        }

        public IActionResult OnGet(int organizationId)
        {
            var organization = dBFactory.GetDatabaseContext().Organizations.SingleOrDefault(x => x.OrganizationId == organizationId);

            if (organization == null)
            {
                return LocalRedirect("/Admin/OrganizationManagement");
            }

            Input.OrganizationName = organization.OrganizationName;

            return Page();
        }
    }
}
