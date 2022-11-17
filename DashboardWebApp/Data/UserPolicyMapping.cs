using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Data
{
    public class UserPolicyMapping
    {
        [Key]
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public int UserId { get; set; }
        public virtual UserPolicy? UserPolicy { get; set; }
        public virtual User? User { get; set; }
    }
}
