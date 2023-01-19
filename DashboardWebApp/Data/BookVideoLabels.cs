using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DashboardWebApp.Data
{
    public class BookVideoLabels
    {
        public int Id { get; set; }
        public int LabelId { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }

        [NotMapped]
        public string DisplayDate { get; set; }
        public virtual User User { get; set; }
        public virtual Books Book { get; set; }
    }
}
