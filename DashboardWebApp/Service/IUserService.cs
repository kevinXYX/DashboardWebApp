using DashboardWebApp.Data;

namespace DashboardWebApp.Service
{
    public interface IUserService
    {
        User GetUserById(int userId);
        User GetCurrentUser();
        Organization GetCurrentUserOrganization();
        bool IsUserAdmin();
        bool IsUserSuperAdmin();
        bool UserHasDashboardPermission();
        bool IsUserDeactivated();
    }
}
