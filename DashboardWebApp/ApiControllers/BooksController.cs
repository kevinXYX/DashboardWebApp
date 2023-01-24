using DashboardWebApp.Data;
using DashboardWebApp.Models;
using DashboardWebApp.Service;
using KendoNET.DynamicLinq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace DashboardWebApp.ApiControllers
{
    [Authorize]
    public class BooksController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly ADODataLayer adoDataLayer;

        public BooksController(IUserService userService, ADODataLayer adoDataLayer)
        {
            this.userService = userService;
            this.adoDataLayer = adoDataLayer;
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

            var currentUser = this.userService.GetCurrentUser();

            var sqlParams = new SqlParameter[] {
              new SqlParameter() { ParameterName = "@ISBN", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.ISBN ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@DateCreatedFrom", SqlDbType = System.Data.SqlDbType.DateTime, Value = DateTime.Parse(videoFilterViewModel.DateTakenFrom.GetValueOrDefault().ToString(format)) },
              new SqlParameter() { ParameterName = "@DateCreatedTo", SqlDbType = System.Data.SqlDbType.DateTime, Value = DateTime.Parse(videoFilterViewModel.DateTakenTo.GetValueOrDefault().ToString(format)) },
              new SqlParameter() { ParameterName = "@DateUploadedFrom", SqlDbType = System.Data.SqlDbType.DateTime, Value = DateTime.Parse(videoFilterViewModel.DateUploadedFrom.GetValueOrDefault().ToString(format)) },
              new SqlParameter() { ParameterName = "@DateUploadedTo", SqlDbType = System.Data.SqlDbType.DateTime, Value = DateTime.Parse(videoFilterViewModel.DateUploadedTo.GetValueOrDefault().ToString(format)) },
              new SqlParameter() { ParameterName = "@UserNote", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.UserNote ?? Convert.DBNull },
              new SqlParameter() { ParameterName = "@VideoDurationFrom", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.VideoDurationFrom > 0 ? videoFilterViewModel.VideoDurationFrom : null },
              new SqlParameter() { ParameterName = "@VideoDurationTo", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.VideoDurationTo > 0 ? videoFilterViewModel.VideoDurationTo : null },
              new SqlParameter() { ParameterName = "@FileSizeFrom", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.FileSizeFrom > 0 ? videoFilterViewModel.FileSizeFrom : null },
              new SqlParameter() { ParameterName = "@FileSizeTo", SqlDbType = System.Data.SqlDbType.Int, Value = videoFilterViewModel.FileSizeTo > 0 ? videoFilterViewModel.FileSizeTo : null },
              new SqlParameter() { ParameterName = "@LabelsID", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.SelectedBookVideoLabels != null && videoFilterViewModel.SelectedBookVideoLabels.Count() > 0 ? string.Join(",", videoFilterViewModel.SelectedBookVideoLabels) : null },
              new SqlParameter() { ParameterName = "@BookTypesID", SqlDbType = System.Data.SqlDbType.VarChar, Value = videoFilterViewModel.SelectedBookTypes != null && videoFilterViewModel.SelectedBookTypes.Count() > 0 ? string.Join(",", videoFilterViewModel.SelectedBookTypes) : null },
              new SqlParameter() { ParameterName = "@HasComments", SqlDbType = System.Data.SqlDbType.Bit, Value = videoFilterViewModel.SelectedHasComments != null && videoFilterViewModel.SelectedHasComments.Count() > 0 && videoFilterViewModel.SelectedHasComments.Any(x => int.Parse(x) == 1) ? true : false },
              new SqlParameter() { ParameterName = "@SortBy", SqlDbType = System.Data.SqlDbType.Int, Value = 2 },
              new SqlParameter() { ParameterName = "@UserID", SqlDbType = System.Data.SqlDbType.Int, Value = currentUser.UserId },
              new SqlParameter() { ParameterName = "@OrganizationID", SqlDbType = System.Data.SqlDbType.Int, Value = currentUser.OrganizationId },
            };

            var searchVideosDataSet = this.adoDataLayer.GetDataSet("SearchVideos", sqlParams);

            var booksTable = searchVideosDataSet.Tables[0];
            var bookLabelsTable = searchVideosDataSet.Tables[1];
            var searchResultCount = searchVideosDataSet.Tables[2];

            var booksViewModel = new List<BooksViewModel>();
            var booksLabelViewModel = new List<BooksLabelViewModel>();

            foreach (DataRow dataRow in bookLabelsTable.Rows)
            {
                booksLabelViewModel.Add(new BooksLabelViewModel
                {
                    BookId = (int)dataRow["BookId"],
                    BookVideoLabelId = (int)dataRow["BookVideoLabelID"],
                    LabelName = dataRow["LabelName"].ToString()
                });
            }

            foreach (DataRow dataRow in booksTable.Rows)
            {
                booksViewModel.Add(new BooksViewModel
                {
                    BookId = (int)dataRow["BookId"],
                    Isbn = dataRow["ISBN"].ToString(),
                    Labels = booksLabelViewModel.Count() > 0 ? string.Join(",", booksLabelViewModel.Where(x => x.BookId == (int)dataRow["BookId"]).GroupBy(x => x.LabelName).Select(x => x.Select(x => x.LabelName).FirstOrDefault()).ToList()) : string.Empty,
                    DateTaken = dataRow["DateTaken"].ToString(),
                    TimeTaken = dataRow["TimeTaken"].ToString(),
                    DateUploaded = dataRow["DateUploaded"].ToString(),
                    TimeUploaded = dataRow["TimeUploaded"].ToString(),
                    VideoDuration = (int)dataRow["VideoDuration"],
                    FileSize = (int)dataRow["FileSize"],
                    Note = dataRow["UserNote"].ToString(),
                    User = dataRow["UserName"].ToString(),
                    BookType = dataRow["BookTypeName"].ToString(),
                    Comment = bool.Parse(dataRow["HasComments"].ToString()) ? "Yes" : "No"
                });
            }

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
        public string TimeTaken { get; set; }
        public string DateUploaded { get; set; }
        public string TimeUploaded { get; set; }
        public string Comment { get; set; }
    }

    public class BooksLabelViewModel
    {
        public int BookId { get; set; }
        public int BookVideoLabelId { get; set; }
        public string LabelName { get; set; }
    }
}
