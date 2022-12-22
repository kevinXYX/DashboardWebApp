using DashboardWebApp.Data;
using DashboardWebApp.Models;
using DashboardWebApp.Service;
using KendoNET.DynamicLinq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DashboardWebApp.ApiControllers
{
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IDbFactory dbFactory;
        private readonly IUserService userService;
        public BooksController(IDbFactory dbFactory, IUserService userService)
        {
            this.dbFactory = dbFactory;
            this.userService = userService;
        }

        [HttpPost]
        [Route("api/books/getBooks")]
        public DataSourceResult GetBooks([FromBody] DataSourceRequest requestModel)
        {
            var videoFilterViewModelSessionString = HttpContext.Session.GetString(nameof(VideoFilterViewModel));
            var format = "yyyy-MM-dd";
            var videoFilterViewModel = new VideoFilterViewModel() { DateUploadedFrom = DateTime.Parse(DateTime.UtcNow.AddMonths(-3).ToString(format)), DateUploadedTo = DateTime.UtcNow, DateTakenFrom = DateTime.Parse(DateTime.UtcNow.AddMonths(-3).ToString(format)), DateTakenTo = DateTime.UtcNow, VideoDurationFrom = 0, VideoDurationTo = 0, FileSizeFrom = 0, FileSizeTo = 0 };

            if (!string.IsNullOrEmpty(videoFilterViewModelSessionString))
            {
                videoFilterViewModel = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoFilterViewModel>(videoFilterViewModelSessionString);
            }

            var context = dbFactory.GetDatabaseContext();
            var organization = this.userService.GetCurrentUserOrganization();
            var usersWithinOrganization = context.Users.Where(x => x.OrganizationId == organization.OrganizationId);
            var userIds = usersWithinOrganization.Select(x => x.UserId).ToList();

            if (userService.IsUserSuperAdmin())
            {
                userIds = context.Users.Select(x => x.UserId).ToList();
            }

            var bookVideoLabels = context.BookVideoLabels.Where(x => userIds.Contains(x.UserId)).ToList();
            var bookTypes = context.BookTypes.ToList();
            var bookVideos = context.BookVideos.ToList();
            var bookVideoComments = context.BookVideoComments.Where(x => userIds.Contains(x.UserId)).ToList();

            var sqlParams = new SqlParameter[] {
              new SqlParameter() { ParameterName = "@ISBN", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.ISBN ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@DateTakenFrom", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.DateTakenFrom.GetValueOrDefault().ToString("yyyy-MM-dd") ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@DateTakenTo", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.DateTakenTo.GetValueOrDefault().ToString("yyyy-MM-dd") ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@DateUploadedFrom", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.DateUploadedFrom.GetValueOrDefault().ToString("yyyy-MM-dd") ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@DateUploadedTo", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.DateUploadedTo.GetValueOrDefault().ToString("yyyy-MM-dd") ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@UserNote", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.UserNote ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@VideoDurationFrom", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.VideoDurationFrom ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@VideoDurationTo", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.VideoDurationTo ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@FileSizeFrom", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.FileSizeFrom ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@FileSizeTo", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.FileSizeTo ?? Convert.DBNull },
            };

            var books = context.Books.FromSqlRaw<Books>("EXEC [dbo].[GetBooks] @ISBN, @DateTakenFrom, @DateTakenTo, @DateUploadedFrom, " +
                "@DateUploadedTo, @UserNote, @VideoDurationFrom, " +
                "@VideoDurationTo, @FileSizeFrom, @FileSizeTo", sqlParams).ToList();

            books = books.Where(x => userIds.Contains(x.UserId)).ToList();

            if (videoFilterViewModel.SelectedTakenByUsers != null && videoFilterViewModel.SelectedTakenByUsers.Length > 0)
            {
                var selectedTakenByUsers = new List<int>();

                videoFilterViewModel.SelectedTakenByUsers.ToList().ForEach(x => selectedTakenByUsers.Add(int.Parse(x)));

                books = books.Where(x => selectedTakenByUsers.Contains(x.UserId)).ToList();
            }

            if (videoFilterViewModel.SelectedBookTypes != null && videoFilterViewModel.SelectedBookTypes.Length > 0)
            {
                var selectedBookTypes = new List<int>();

                videoFilterViewModel.SelectedBookTypes.ToList().ForEach(x => selectedBookTypes.Add(int.Parse(x)));

                books = books.Where(x => selectedBookTypes.Contains(x.TypeId.GetValueOrDefault())).ToList();
            }

            if (videoFilterViewModel.SelectedBookVideoLabels != null && videoFilterViewModel.SelectedBookVideoLabels.Length > 0)
            {
                var selectedBookVideoLabels = new List<int>();

                videoFilterViewModel.SelectedBookVideoLabels.ToList().ForEach(x => selectedBookVideoLabels.Add(int.Parse(x)));

                var filteredBookVideoLabelBookIds = bookVideoLabels.Where(x => selectedBookVideoLabels.Contains(x.Id)).Select(x => x.BookId);

                books = books.Where(x => filteredBookVideoLabelBookIds.Contains(x.Id)).ToList();
            }

            if (videoFilterViewModel.SelectedHasComments != null && videoFilterViewModel.SelectedHasComments.Length > 0)
            {
                var selectedHasComments = new List<int>();

                videoFilterViewModel.SelectedHasComments.ToList().ForEach(x => selectedHasComments.Add(int.Parse(x)));
                var bookVideoCommentsBookIds = bookVideoComments.Select(x => x.BookId).ToList();

                if (selectedHasComments.Count() == 1 && selectedHasComments.Any(x => x == 0))
                {
                    books = books.Where(x => !bookVideoCommentsBookIds.Contains(x.Id)).ToList();
                }
                else if (selectedHasComments.Count() == 1 && selectedHasComments.Any(x => x == 1))
                {
                    books = books.Where(x => bookVideoCommentsBookIds.Contains(x.Id)).ToList();
                }
                else
                {
                    if (selectedHasComments.Count() == 2)
                    {
                        books = books.Where(x => bookVideoCommentsBookIds.Contains(x.Id) || !bookVideoCommentsBookIds.Contains(x.Id)).ToList();
                    }
                }
            }

            if (videoFilterViewModel.ShowWithUserNotesOnly.HasValue && videoFilterViewModel.ShowWithUserNotesOnly.Value)
            {
                books = books.Where(x => !string.IsNullOrEmpty(x.Note)).ToList();
            }

            var booksViewModel = new List<BooksViewModel>();

            books = books.GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList();

            books.ForEach(x =>
            {
                var user = usersWithinOrganization?.SingleOrDefault(b => b.UserId == x.UserId)?.Fullname ?? usersWithinOrganization?.SingleOrDefault(b => b.UserId == x.UserId)?.UserName;
                var bookType = bookTypes.SingleOrDefault(b => b.Id == x.TypeId.GetValueOrDefault())?.Name;
                var bookVideo = bookVideos.SingleOrDefault(b => b.FileName == x.FileName && b.UserId == x.UserId);
                booksViewModel.Add(new BooksViewModel
                {
                    Id = x.Id,
                    BookId = x.BookId,
                    Isbn = x.Isbn,
                    VideoUrl = bookVideo?.VideoUri ?? string.Empty,
                    Labels = string.Join(",", bookVideoLabels.Where(b => b.BookId == x.Id).Select(x => x.Label)),
                    User = user,
                    BookType = bookType,
                    Note = x.Note,
                    VideoDuration = x.VideoDuration,
                    FileSize = x.FileSize,
                    DateTaken = x.Created.ToString("dd MMM yyyy hh:mm tt"),
                    DateUploaded = x.UploadDate.GetValueOrDefault().ToString("dd MMM yyyy hh:mm tt"),
                    Comment = bookVideoComments.Any(b => b.BookId == x.Id) ? "Yes" : "No"
                });
            });

            return booksViewModel.AsQueryable().ToDataSourceResult(requestModel);
        }

        [HttpPost]
        [Route("api/books/filterBooks")]
        public IActionResult FilterBooks(VideoFilterViewModel videoFilterViewModel)
        {
            HttpContext.Session.SetString(nameof(VideoFilterViewModel), Newtonsoft.Json.JsonConvert.SerializeObject(videoFilterViewModel));
            return Ok();
        }

        [HttpPost]
        [Route("api/books/resetFilters")]
        public IActionResult ResetFilters()
        {
            HttpContext.Session.Remove(nameof(VideoFilterViewModel));
            return Ok();
        }
    }

    public class BooksViewModel : Books
    {
        public string VideoUrl { get; set; }
        public string Labels { get; set; }
        public string User { get; set; }
        public string BookType { get; set; }
        public string DateTaken { get; set; }
        public string DateUploaded { get; set; }
        public string Comment { get; set; }
    }
}
