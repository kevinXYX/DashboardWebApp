using DashboardWebApp.Data;
using DashboardWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DashboardWebApp.Service
{
    public class VideoService : IVideoService
    {
        private readonly ApplicationDbContext context;
        private readonly IUserService userService;
        private readonly IDataAccessLayer dataAccessLayer;
        public VideoService(IDbFactory dbFactory, IUserService userService, IDataAccessLayer dataAccessLayer)
        {
            context = dbFactory.GetDatabaseContext();
            this.userService = userService;
            this.dataAccessLayer = dataAccessLayer;
        }

        public Dictionary<string, DataTable> GetBookDetails(int bookId)
        {
            var dataSetDict = new Dictionary<string, DataTable>();

            var videoDetailsDataSet = this.dataAccessLayer.GetDataSet("GetVideoToPlay", false, new object[] { 0, bookId });

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

            var filterDropDownsDataSet = this.dataAccessLayer.GetDataSet("GetFiltersDropdowns", false, new object[] { 0, userId, organizationId });

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
            return this.dataAccessLayer.GetDataSet("GetLabelsForUser", false, new object[] { 0, userId, organizationId, bookId });
        }

        public DataSet GetAllLabelsForOrganizationDataSet(int userId, int organizationId)
        {
            return this.dataAccessLayer.GetDataSet("GetLabelsForUser", false, new object[] { 0, userId, organizationId });
        }

        public BookVideoComments AddVideoComment(int bookId, string comment)
        {
            var currentUser = this.userService.GetCurrentUser();

            if (currentUser == null)
                return null;

            var book = context.Books.SingleOrDefault(x => x.Id == bookId);

            if (book == null)
                return null;

            var result = this.dataAccessLayer.ExecuteCommand("AddCommentToVideo", false, new object[] { 0, currentUser.UserId, bookId, comment });

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

            var result = this.dataAccessLayer.ExecuteCommand("AddLabelToVideo", false, new object[] { 0, currentUser.UserId, bookId, labelId });

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

            var result = this.dataAccessLayer.ExecuteCommand("AddHistoryToVideo", false, new object[] { 0, currentUser.UserId, bookId, history });

            return new BookVideoHistory { BookId = bookId, History = history };
        }
    }
}
