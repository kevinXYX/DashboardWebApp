using Microsoft.AspNetCore.Identity;

namespace DashboardWebApp.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
