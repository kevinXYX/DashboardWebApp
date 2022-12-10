namespace DashboardWebApp.Data
{
    public class BookVideos
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string VideoUri { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
