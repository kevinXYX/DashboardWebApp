using DashboardWebApp.Data;
using DashboardWebApp.Models;
using DashboardWebApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace DashboardWebApp.Areas.User.Pages.Videos
{
    public class PlayVideoModel : PageModel
    {
        private readonly IVideoService videoService;
        private readonly IUserService userService;
        private readonly IDbFactory dbFactory;

        [BindProperty]
        public BookDetailsViewModel BookDetailsViewModel { get; set; } = new BookDetailsViewModel();

        [BindProperty]
        public List<VideoHistoryViewModel> VideoHistoryViewModel { get; set; } = new List<VideoHistoryViewModel>();

        [BindProperty]
        public List<VideoLabelsViewModel> VideoLabelsViewModel { get; set; } = new List<VideoLabelsViewModel>();

        [BindProperty]
        public List<VideoCommentsViewModel> VideoCommentsViewModel { get; set; } = new List<VideoCommentsViewModel>();

        [BindProperty]
        public List<string> TagsSuggestion { get; set; } = new List<string>();

        [BindProperty]
        public List<SelectListItem> BookVideoLabels { get; set; } = new List<SelectListItem>();

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
                .Select(x => new { x.UserId, x.OrganizationId })
                .ToList();

            var currentUser = userService.GetCurrentUser();

            var booksBelongToUsers = context.Books.Where(x => usersInThisOrganization.Select(d => d.UserId).Contains(x.UserId)).ToList();

            if (userService.IsUserSuperAdmin())
            {
                booksBelongToUsers = context.Books.ToList();
            }

            if (!booksBelongToUsers.Any(x => x.Id == bookId))
                return LocalRedirect("/");

            var bookDetailsDict = this.videoService.GetBookDetails(bookId);

            DataRow booksDetailsRow = bookDetailsDict["BookDetails"].Rows[0];

            var bookDetailsViewModel = new BookDetailsViewModel()
            {
                BookId = (int)booksDetailsRow["BookId"],
                FileName = booksDetailsRow["FileName"].ToString(),
                BookVideoUrl = booksDetailsRow["BookVideoURL"].ToString(),
                CreatedBy = booksDetailsRow["CreatedBy"].ToString(),
                DateTaken = booksDetailsRow["DateTaken"].ToString(),
                DateUploaded = booksDetailsRow["DateUploaded"].ToString(),
                BookType = booksDetailsRow["BookType"].ToString(),
                VideoDuration = booksDetailsRow["VideoDuration"].ToString(),
                VideoFileSize = booksDetailsRow["VideoFileSize"].ToString(),
                UserNote = booksDetailsRow["UserNote"].ToString()
            };

            BookDetailsViewModel = bookDetailsViewModel;

            foreach (DataRow dataRow in bookDetailsDict["Comments"].Rows)
            {
                VideoCommentsViewModel.Add(new Models.VideoCommentsViewModel
                {
                    Comment = dataRow["Comment"].ToString(),
                    CommentByUser = dataRow["CommentByUser"].ToString(),
                    CommentDate = dataRow["CommentDate"].ToString()
                });
            }

            foreach (DataRow dataRow in bookDetailsDict["Labels"].Rows)
            {
                VideoLabelsViewModel.Add(new Models.VideoLabelsViewModel
                {
                    BookVideoLabelId = (int)dataRow["BookVideoLabelID"],
                    Label = dataRow["LabelName"].ToString()
                });
            }

            foreach (DataRow dataRow in bookDetailsDict["History"].Rows)
            {
                VideoHistoryViewModel.Add(new Models.VideoHistoryViewModel
                {
                    History = dataRow["History"].ToString(),
                    HistoryDate = dataRow["HistoryDate"].ToString(),
                    FullName = dataRow["FullName"].ToString()
                });
            }

            DataRow totalCommentsRow = bookDetailsDict["TotalComments"].Rows[0];

            TotalCommentsCount = (int)totalCommentsRow["TotalCommentsCount"];

            usersInThisOrganization.ForEach(x =>
            {
                var userLabels = this.videoService.GetUserLabelsDataSet(bookId, x.UserId, x.OrganizationId);

                foreach (DataRow userLabel in userLabels.Tables[0].Rows)
                {
                    TagsSuggestion.Add(userLabel["LabelName"].ToString());
                }
            });

            var allLabelsForOrganization = this.videoService.GetAllLabelsForOrganizationDataSet(currentUser.UserId, currentUser.OrganizationId);

            if (allLabelsForOrganization != null)
            {
                foreach (DataRow dataRow in allLabelsForOrganization.Tables[0].Rows)
                {
                    BookVideoLabels.Add(new SelectListItem
                    {
                        Value = dataRow["LabelId"].ToString(),
                        Text = dataRow["LabelName"].ToString()
                    });
                }
            }

            if (BookDetailsViewModel == null)
                return LocalRedirect("/");

            return Page();
        }
    }
}
