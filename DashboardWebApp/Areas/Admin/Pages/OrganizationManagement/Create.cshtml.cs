using DashboardWebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Areas.Admin.Pages.OrganizationManagement
{
    public class CreateModel : PageModel
    {
        private readonly IDbFactory dbFactory;

        public CreateModel(IDbFactory dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        [BindProperty]
        public CreateOrganizationModel Input { get; set; }

        public class CreateOrganizationModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string OrganizationName { get; set; }
        }

        public void OnGet()
        {

        }

        public IActionResult OnPost()
        {
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
                    CreatedDate = DateTime.UtcNow
                });

                context.SaveChanges();

                return LocalRedirect("/Admin/OrganizationManagement");
            }

            return Page();
        }
    }
}
