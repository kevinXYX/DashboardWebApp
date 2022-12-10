namespace DashboardWebApp.Data
{
    public class Books
    {
        public Books()
        {
            BookVideoLabels = new HashSet<BookVideoLabels>();
            BookVideoComments = new HashSet<BookVideoComments>();
        }

        public int Id { get; set; }
        public int StationId { get; set; }
        public int InstallationId { get; set; }
        public int StageId { get; set; }
        public int BookId { get; set; }
        public int? TypeId { get; set; }
        public string Isbn { get; set; }
        public string Note { get; set; }
        public string FileName { get; set; }
        public string FileCheckSum { get; set; }
        public int FileSize { get; set; }
        public int VideoDuration { get; set; }
        public int UserId { get; set; }
        public DateTime? UploadDate { get; set; }
        public DateTime Created { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<BookVideoLabels>? BookVideoLabels { get; set; }
        public virtual ICollection<BookVideoComments>? BookVideoComments { get; set; }
    }
}
