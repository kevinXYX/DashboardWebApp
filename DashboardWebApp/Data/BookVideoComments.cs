namespace DashboardWebApp.Data
{
    public class BookVideoComments
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }
        public virtual User User { get; set; }
        public virtual Books Book { get; set; }
    }
}
