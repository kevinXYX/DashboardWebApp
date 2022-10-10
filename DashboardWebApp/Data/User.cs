namespace DashboardWebApp.Data
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime ExpiredAt { get; set; }
        public string LogoFileName { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
