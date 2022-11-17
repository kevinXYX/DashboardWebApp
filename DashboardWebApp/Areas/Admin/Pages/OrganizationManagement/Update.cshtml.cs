using DashboardWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Areas.Admin.Pages.OrganizationManagement
{
    public class UpdateModel : PageModel
    {
        private readonly IDbFactory dbFactory;

        public UpdateModel(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
            Input = new UpdateOrganizationModel();
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
        }

        public IActionResult OnGet(int? organizationId)
        {
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

            return Page();
        }

        public IActionResult OnPost(int organizationId)
        {
            var context = dbFactory.GetDatabaseContext();

            if (ModelState.IsValid)
            {
                var organization = context.Organizations.SingleOrDefault(x => x.OrganizationId == organizationId);

                if (organization == null)
                {
                    return LocalRedirect("/Admin/OrganizationManagement");
                }

                organization.OrganizationName = Input.OrganizationName;

                context.SaveChanges();

                return LocalRedirect("/Admin/OrganizationManagement");
            }

            return Page();
        }
    }
}
