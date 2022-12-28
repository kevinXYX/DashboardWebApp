using DashboardWebApp.Data;
using DashboardWebApp.Models;

namespace DashboardWebApp.Service
{
    public interface IVideoService
    {
        BookDetailsViewModel GetBookDetails(int bookId);
        List<VideoCommentsViewModel> GetVideoComments(int bookId);
        List<VideoHistoryViewModel> GetVideoHistory(int bookId);
        List<VideoLabelsViewModel> GetVideoLabels(int bookId);
        BookVideoComments AddVideoComment(int bookId, string comment);
        BookVideoLabels AddVideoLabel(int bookId, string label);
        int GetTotalCommentsCount(int bookId);
        List<string> GetTagsSuggestion(int bookId);
    }
}
