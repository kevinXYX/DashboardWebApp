using DashboardWebApp.Data;
using DashboardWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;

namespace DashboardWebApp.Service
{
    public class VideoService : IVideoService
    {
        private readonly ApplicationDbContext context;
        private readonly IUserService userService;
        private readonly ADODataLayer adoDataLayer;
        public VideoService(IDbFactory dbFactory, IUserService userService, ADODataLayer aDODataLayer)
        {
            context = dbFactory.GetDatabaseContext();
            this.userService = userService;
            this.adoDataLayer = aDODataLayer;
        }

        public Dictionary<string, DataTable> GetBookDetails(int bookId)
        {
            var dataSetDict = new Dictionary<string, DataTable>();

            var videoDetailsDataSet = this.adoDataLayer.GetDataSet("GetVideoToPlay", new System.Data.SqlClient.SqlParameter[]
            {
                 new SqlParameter() { ParameterName = "@BookID", SqlDbType = System.Data.SqlDbType.Int, Value = bookId },
            });

            if (videoDetailsDataSet != null)
            {
                var bookDetailsTable = videoDetailsDataSet.Tables[0];
                dataSetDict.Add("BookDetails", bookDetailsTable);

                var commentsTable = videoDetailsDataSet.Tables[1];
                dataSetDict.Add("Comments", commentsTable);

                var labelsTable = videoDetailsDataSet.Tables[2];
                dataSetDict.Add("Labels", labelsTable);

                var historyTable = videoDetailsDataSet.Tables[3];
                dataSetDict.Add("History", historyTable);

                var totalCommentsCount = videoDetailsDataSet.Tables[4];
                dataSetDict.Add("TotalComments", totalCommentsCount);
            }

            return dataSetDict;
        }

        public Dictionary<string, DataTable> GetFilterDropDowns(int userId, int organizationId)
        {
            var dataSetDict = new Dictionary<string, DataTable>();

            var filterDropDownsDataSet = this.adoDataLayer.GetDataSet("GetFiltersDropdowns", new System.Data.SqlClient.SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@UserID", SqlDbType = System.Data.SqlDbType.Int, Value = userId },
                new SqlParameter() { ParameterName = "@OrganizationID", SqlDbType = System.Data.SqlDbType.Int, Value = organizationId },
            });

            if (filterDropDownsDataSet != null)
            {
                var filtersByUsername = filterDropDownsDataSet.Tables[0];
                dataSetDict.Add("TakenByUserDropDown", filtersByUsername);

                var filtersByBookType = filterDropDownsDataSet.Tables[1];
                dataSetDict.Add("BookTypeDropDown", filtersByBookType);

                var filtersByBookVideoLabels = filterDropDownsDataSet.Tables[2];
                dataSetDict.Add("BookVideoLabels", filtersByBookVideoLabels);
            }

            return dataSetDict;
        }

        public DataSet GetUserLabelsDataSet(int bookId, int userId, int organizationId)
        {
            return this.adoDataLayer.GetDataSet("GetLabelsForUser", new System.Data.SqlClient.SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@BookID", SqlDbType = System.Data.SqlDbType.Int, Value = bookId },
                new SqlParameter() { ParameterName = "@UserID", SqlDbType = System.Data.SqlDbType.Int, Value = userId },
                new SqlParameter() { ParameterName = "@OrganizationID", SqlDbType = System.Data.SqlDbType.Int, Value = organizationId },
            });
        }

        public DataSet GetAllLabelsForOrganizationDataSet(int userId, int organizationId)
        {
            return this.adoDataLayer.GetDataSet("GetLabelsForUser", new System.Data.SqlClient.SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@UserID", SqlDbType = System.Data.SqlDbType.Int, Value = userId },
                new SqlParameter() { ParameterName = "@OrganizationID", SqlDbType = System.Data.SqlDbType.Int, Value = organizationId },
            });
        }

        public BookVideoComments AddVideoComment(int bookId, string comment)
        {
            var currentUser = this.userService.GetCurrentUser();

            if (currentUser == null)
                return null;

            var book = context.Books.SingleOrDefault(x => x.Id == bookId);

            if (book == null)
                return null;

            var result = this.adoDataLayer.ExecuteSP("AddCommentToVideo", new System.Data.SqlClient.SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@BookID", SqlDbType = System.Data.SqlDbType.Int, Value = bookId },
                new SqlParameter() { ParameterName = "@UserID", SqlDbType = System.Data.SqlDbType.Int, Value = currentUser.UserId },
                new SqlParameter() { ParameterName = "@Comment", SqlDbType = System.Data.SqlDbType.NVarChar, Value = comment },
            });

            return new BookVideoComments { BookId = bookId, Comment = comment, UserName = currentUser.Fullname, DisplayDate = $"{DateTime.UtcNow.ToString("dd/MM/yyyy")} {DateTime.UtcNow.ToString("hh:mm tt")}" };
        }

        public BookVideoLabels AddVideoLabel(int bookId, int labelId)
        {
            var currentUser = this.userService.GetCurrentUser();

            if (currentUser == null)
                return null;

            var book = context.Books.SingleOrDefault(x => x.Id == bookId);

            if (book == null)
                return null;

            var result = this.adoDataLayer.ExecuteSP("AddLabelToVideo", new System.Data.SqlClient.SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@BookID", SqlDbType = System.Data.SqlDbType.Int, Value = bookId },
                new SqlParameter() { ParameterName = "@UserID", SqlDbType = System.Data.SqlDbType.Int, Value = currentUser.UserId },
                new SqlParameter() { ParameterName = "@LabelID", SqlDbType = System.Data.SqlDbType.Int, Value = labelId },
            });

            return new BookVideoLabels { BookId = bookId, LabelId = labelId, DisplayDate = $"{DateTime.UtcNow.ToString("dd/MM/yyyy")} {DateTime.UtcNow.ToString("hh:mm tt")}" };
        }

        public BookVideoHistory AddVideoHistory(int bookId, string history)
        {
            var currentUser = this.userService.GetCurrentUser();

            if (currentUser == null)
                return null;

            var book = context.Books.SingleOrDefault(x => x.Id == bookId);

            if (book == null)
                return null;

            var result = this.adoDataLayer.ExecuteSP("AddHistoryToVideo", new System.Data.SqlClient.SqlParameter[]
            {
                new SqlParameter() { ParameterName = "@BookID", SqlDbType = System.Data.SqlDbType.Int, Value = bookId },
                new SqlParameter() { ParameterName = "@UserID", SqlDbType = System.Data.SqlDbType.Int, Value = currentUser.UserId },
                new SqlParameter() { ParameterName = "@HistoryDescription", SqlDbType = System.Data.SqlDbType.NVarChar, Value = history },
            });

            return new BookVideoHistory { BookId = bookId, History = history };
        }
    }
}
