using DashboardWebApp.Data;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Areas.Admin.Pages.UserManagement
{
    public class UpdateModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IDbFactory _dbFactory;
        public UpdateModel(IUserService userService, IDbFactory dbFactory)
        {
            _userService = userService;
            Input = new UpdateInputModel();
            _dbFactory = dbFactory;
        }

        [BindProperty]
        public UpdateInputModel Input { get; set; }

        [BindProperty]
        public List<SelectListItem> Organizations { get; set; }

        [BindProperty]
        public List<SelectListItem> UserStatuses { get; set; }

        [BindProperty]
        public int UserId { get; set; }

        public class UpdateInputModel
        {
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Text)]
            public string Name { get; set; }
            public int? SelectedOrganizationId { get; set; }
            public int? SelectedUserStatus { get; set; }
            public bool IsAdmin { get; set; }
            public bool HasAPIPermission { get; set; }
            public bool HasDashboardPermission { get; set; }
        }

        public async Task<IActionResult> OnGet(int userId)
        {
            var user = _userService.GetUserById(userId);

            if (user == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            UserId = userId;

            var context = _dbFactory.GetDatabaseContext();

            var apiPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "APIAppPermission");
            var dashboardPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "DashboardAppPermission");
            var userPolicyMappings = context.UserPolicyMappings.ToList();

            Input.Email = user.UserName;
            Input.Name = user.Fullname;
            Input.IsAdmin = user.IsAdmin.GetValueOrDefault();
            Input.SelectedOrganizationId = user.Organization.OrganizationId;
            Input.HasAPIPermission = userPolicyMappings.Any(x => x.UserId == userId && x.PolicyId == apiPolicy.Id);
            Input.HasDashboardPermission = userPolicyMappings.Any(x => x.UserId == userId && x.PolicyId == dashboardPolicy.Id);
            Input.SelectedUserStatus = user.UserStatus;

            var organizations = context.Organizations.ToList();

            var organizationSelectList = new List<SelectListItem>();

            if (organizations != null)
            {
                organizations.ForEach(x =>
                {
                    organizationSelectList.Add(new SelectListItem { Text = x.OrganizationName, Value = x.OrganizationId.ToString() });
                });
            }

            Organizations = organizationSelectList;

            UserStatuses = new List<SelectListItem>();

            UserStatuses.Add(new SelectListItem { Text = "Pending", Value = "0" });
            UserStatuses.Add(new SelectListItem { Text = "Active", Value = "1" });
            UserStatuses.Add(new SelectListItem { Text = "Deactivated", Value = "2" });

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int userId)
        {
            try
            {
                ModelState.Remove("Input.Email");

                if (ModelState.IsValid)
                {
                    var context = _dbFactory.GetDatabaseContext();

                    var user = context.Users.SingleOrDefault(x => x.UserId == userId);

                    if (user == null)
                    {
                        return Redirect("/Identity/Account/Login");
                    }

                    var organizations = context.Organizations.ToList();

                    var organizationSelectList = new List<SelectListItem>();

                    if (organizations != null)
                    {
                        organizations.ForEach(x =>
                        {
                            organizationSelectList.Add(new SelectListItem { Text = x.OrganizationName, Value = x.OrganizationId.ToString() });
                        });
                    }

                    Organizations = organizationSelectList;

                    UserStatuses = new List<SelectListItem>();

                    UserStatuses.Add(new SelectListItem { Text = "Pending", Value = "0" });
                    UserStatuses.Add(new SelectListItem { Text = "Active", Value = "1" });
                    UserStatuses.Add(new SelectListItem { Text = "Deactivated", Value = "2" });

                    //user.UserName = Input.Email;
                    user.Fullname = Input.Name;
                    user.OrganizationId = Input.SelectedOrganizationId.GetValueOrDefault();
                    user.UserStatus = Input.SelectedUserStatus.GetValueOrDefault();
                    user.IsAdmin = Input.IsAdmin;

                    var userExistingPolicyMappings = context.UserPolicyMappings.Where(x => x.UserId == userId).ToList();
                    var apiPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "APIAppPermission");
                    var dashboardPolicy = context.UserPolicies.SingleOrDefault(x => x.PolicyName == "DashboardAppPermission");

                    if (Input.HasAPIPermission && apiPolicy != null)
                    {
                        if (!context.UserPolicyMappings.Any(x => x.UserId == user.UserId && x.PolicyId == apiPolicy.Id))
                        {
                            context.UserPolicyMappings.Add(new UserPolicyMapping
                            {
                                UserId = user.UserId,
                                UserPolicy = apiPolicy
                            });
                        }
                    }
                    else
                    {
                        var apiPolicyToRemove = userExistingPolicyMappings.SingleOrDefault(x => x.PolicyId == apiPolicy.Id);
                        if (apiPolicyToRemove != null)
                        {
                            context.Remove(apiPolicyToRemove);
                        }
                    }

                    if (Input.HasDashboardPermission && dashboardPolicy != null)
                    {
                        if (!context.UserPolicyMappings.Any(x => x.UserId == user.UserId && x.PolicyId == dashboardPolicy.Id))
                        {
                            context.UserPolicyMappings.Add(new UserPolicyMapping
                            {
                                UserId = user.UserId,
                                UserPolicy = dashboardPolicy
                            });
                        }
                    }
                    else
                    {
                        var dashboardPolicyToRemove = userExistingPolicyMappings.SingleOrDefault(x => x.PolicyId == dashboardPolicy.Id);
                        if (dashboardPolicyToRemove != null)
                        {
                            context.Remove(dashboardPolicyToRemove);
                        }
                    }

                    context.SaveChanges();

                    return LocalRedirect("/Admin/UserManagement");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occured while updating user");

                return Page();
            }

            return Page();
        }
    }
}
