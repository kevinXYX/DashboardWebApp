namespace DashboardWebApp.Data
{
    public interface IDbFactory
    {
        ApplicationDbContext GetDatabaseContext();
    }
}
