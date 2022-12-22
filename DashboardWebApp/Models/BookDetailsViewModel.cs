namespace DashboardWebApp.Models
{
    public class BookDetailsViewModel
    {
        public int BookId { get; set; }
        public string FileName { get; set; }
        public string BookVideoUrl { get; set; }
        public string CreatedBy { get; set; }
        public string DateTaken { get; set; }
        public string DateUploaded { get; set; }
        public string BookType { get; set; }
        public string VideoDuration { get; set; }
        public string VideoFileSize { get; set; }
        public string UserNote { get; set; }
    }
}
