using Microsoft.EntityFrameworkCore;

namespace DashboardWebApp.Data
{
    public class DbFactory : IDbFactory
    {
        private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;
        public DbFactory(DbContextOptions<ApplicationDbContext> dbContextOptions)
        {
            this.dbContextOptions = dbContextOptions;
        }
        public ApplicationDbContext GetDatabaseContext()
        {
            return new ApplicationDbContext(this.dbContextOptions);
        }
    }
}
