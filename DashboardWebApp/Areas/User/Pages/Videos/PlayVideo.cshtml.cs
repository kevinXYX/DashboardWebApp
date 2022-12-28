using DashboardWebApp.Data;
using DashboardWebApp.Models;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.User.Pages.Videos
{
    public class PlayVideoModel : PageModel
    {
        private readonly IVideoService videoService;
        private readonly IUserService userService;
        private readonly IDbFactory dbFactory;

        [BindProperty]
        public BookDetailsViewModel BookDetailsViewModel { get; set; }

        [BindProperty]
        public List<VideoHistoryViewModel> VideoHistoryViewModel { get; set; }

        [BindProperty]
        public List<VideoLabelsViewModel> VideoLabelsViewModel { get; set; }

        [BindProperty]
        public List<VideoCommentsViewModel> VideoCommentsViewModel { get; set; }

        [BindProperty]
        public List<string> TagsSuggestion { get; set; }

        [BindProperty]
        public int TotalCommentsCount { get; set; }

        public PlayVideoModel(IVideoService videoService, IUserService userService, IDbFactory dbFactory)
        {
            this.videoService = videoService;
            this.userService = userService;
            this.dbFactory = dbFactory;
        }

        public IActionResult OnGet(int bookId)
        {
            if (bookId == null)
                return LocalRedirect("/");

            var context = this.dbFactory.GetDatabaseContext();

            var userOrganization = this.userService.GetCurrentUserOrganization();

            var usersInThisOrganization = context.Users
                .Where(x => x.OrganizationId == userOrganization.OrganizationId)
                .Select(x => x.UserId)
                .ToList();

            var booksBelongToUsers = context.Books.Where(x => usersInThisOrganization.Contains(x.UserId)).ToList();

            if (userService.IsUserSuperAdmin())
            {
                booksBelongToUsers = context.Books.ToList();
            }

            if (!booksBelongToUsers.Any(x => x.Id == bookId))
                return LocalRedirect("/");

            BookDetailsViewModel = this.videoService.GetBookDetails(bookId);
            VideoCommentsViewModel = this.videoService.GetVideoComments(bookId);
            VideoLabelsViewModel = this.videoService.GetVideoLabels(bookId);
            VideoHistoryViewModel = this.videoService.GetVideoHistory(bookId);
            TotalCommentsCount = this.videoService.GetTotalCommentsCount(bookId);
            TagsSuggestion = this.videoService.GetTagsSuggestion(bookId);

            if (BookDetailsViewModel == null)
                return LocalRedirect("/");

            return Page();
        }
    }
}
