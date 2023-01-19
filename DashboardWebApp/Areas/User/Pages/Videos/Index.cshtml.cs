using DashboardWebApp.ApiControllers;
using DashboardWebApp.Data;
using DashboardWebApp.Models;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DashboardWebApp.Areas.UserArea.Pages.Videos
{
    public class IndexModel : PageModel
    {
        private readonly IVideoService videoService;
        private readonly IUserService _userService;

        public IndexModel(IUserService userService, IVideoService videoService)
        {
            _userService = userService;
            this.videoService = videoService;
        }

        [BindProperty]
        public VideoFilterViewModel Input { get; set; } = new VideoFilterViewModel();

        [BindProperty]
        public List<SelectListItem> TakenByUserDropDown { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public List<SelectListItem> BookTypeDropDown { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public List<SelectListItem> BookVideoLabels { get; set; } = new List<SelectListItem>();

        public IActionResult OnGet()
        {
            var currentUser = _userService.GetCurrentUser();

            var filtersDict = this.videoService.GetFilterDropDowns(currentUser.UserId, currentUser.OrganizationId);

            foreach (DataRow dataRow in filtersDict["TakenByUserDropDown"].Rows)
            {
                TakenByUserDropDown.Add(new SelectListItem
                {
                    Value = dataRow["UserID"].ToString(),
                    Text = dataRow["UserName"].ToString()
                });
            }

            foreach (DataRow dataRow in filtersDict["BookTypeDropDown"].Rows)
            {
                BookTypeDropDown.Add(new SelectListItem
                {
                    Value = dataRow["BookTypeID"].ToString(),
                    Text = dataRow["BookTypeName"].ToString()
                });
            }

            foreach (DataRow dataRow in filtersDict["BookVideoLabels"].Rows)
            {
                BookVideoLabels.Add(new SelectListItem
                {
                    Value = dataRow["LabelID"].ToString(),
                    Text = dataRow["LabelName"].ToString()
                });
            }

            return Page();
        }
    }
}
