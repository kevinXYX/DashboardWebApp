using DashboardWebApp.ApiControllers;
using DashboardWebApp.Data;
using DashboardWebApp.Models;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DashboardWebApp.Areas.UserArea.Pages.Videos
{
    public class IndexModel : PageModel
    {
        private readonly IDbFactory _dbFactory;
        private readonly IUserService _userService;

        public IndexModel(IDbFactory dbFactory, IUserService userService)
        {
            _dbFactory = dbFactory;
            _userService = userService;
            Input = new VideoFilterViewModel();
        }

        [BindProperty]
        public VideoFilterViewModel Input { get; set; }

        [BindProperty]
        public List<SelectListItem> TakenByUserDropDown { get; set; }

        [BindProperty]
        public List<SelectListItem> BookTypeDropDown { get; set; }

        [BindProperty]
        public List<SelectListItem> BookVideoLabels { get; set; }

        public IActionResult OnGet()
        {
            var context = _dbFactory.GetDatabaseContext();
            var userCurrentOrganization = _userService.GetCurrentUserOrganization();
            var userIds = context.Users.Where(x => x.OrganizationId == userCurrentOrganization.OrganizationId).Select(x => x.UserId);
            var booksTakenByUsers = context.Books.Include(x => x.User).Where(x => userIds.Contains(x.UserId)).GroupBy(x => x.UserId).Select(x => new SelectListItem { Value = x.FirstOrDefault().UserId.ToString(), Text = x.FirstOrDefault().User.UserName }).ToList();
            var bookTypes = context.BookTypes.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
            var bookLabels = context.BookVideoLabels.Where(x => userIds.Contains(x.UserId)).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Label }).ToList();
            TakenByUserDropDown = booksTakenByUsers;
            BookTypeDropDown = bookTypes;
            BookVideoLabels = bookLabels;

            return Page();
        }
    }
}
