using System.ComponentModel.DataAnnotations.Schema;

namespace DashboardWebApp.Data
{
    public class BookVideoHistory
    {
        public int Id { get; set; }
        public string History { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }

        [NotMapped]
        public string DisplayDate { get; set; }
        public virtual User User { get; set; }
        public virtual Books Book { get; set; }
    }
}
