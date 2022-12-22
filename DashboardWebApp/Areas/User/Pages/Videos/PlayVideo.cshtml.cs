using DashboardWebApp.Models;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashboardWebApp.Areas.User.Pages.Videos
{
    public class PlayVideoModel : PageModel
    {
        private readonly IVideoService videoService;

        [BindProperty]
        public BookDetailsViewModel BookDetailsViewModel { get; set; }

        [BindProperty]
        public List<VideoHistoryViewModel> VideoHistoryViewModel { get; set; }

        [BindProperty]
        public List<VideoLabelsViewModel> VideoLabelsViewModel { get; set; }

        [BindProperty]
        public List<VideoCommentsViewModel> VideoCommentsViewModel { get; set; }

        public PlayVideoModel(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        public IActionResult OnGet(int bookId)
        {
            if (bookId == null)
                return LocalRedirect("/");

            BookDetailsViewModel = this.videoService.GetBookDetails(bookId);
            VideoCommentsViewModel = this.videoService.GetVideoComments(bookId);
            VideoLabelsViewModel = this.videoService.GetVideoLabels(bookId);
            VideoHistoryViewModel = this.videoService.GetVideoHistory(bookId);

            if (BookDetailsViewModel == null)
                return LocalRedirect("/");

            return Page();
        }
    }
}
