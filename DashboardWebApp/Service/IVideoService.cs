using DashboardWebApp.Data;
using DashboardWebApp.Models;
using System.Data;

namespace DashboardWebApp.Service
{
    public interface IVideoService
    {
        Dictionary<string, DataTable> GetBookDetails(int bookId);
        Dictionary<string, DataTable> GetFilterDropDowns(int userId, int organizationId);
        DataSet GetUserLabelsDataSet(int bookId, int userId, int organizationId);
        DataSet GetAllLabelsForOrganizationDataSet(int userId, int organizationId);
        BookVideoComments AddVideoComment(int bookId, string comment);
        BookVideoLabels AddVideoLabel(int bookId, int labelId);
        BookVideoHistory AddVideoHistory(int bookId, string history);
    }
}
