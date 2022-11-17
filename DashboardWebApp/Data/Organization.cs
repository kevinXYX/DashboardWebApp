using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Data
{
    public class Organization
    {
        public Organization()
        {
            Users = new HashSet<User>();
        }

        [Key]
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
