using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Data
{
    public class UserPolicy
    {
        public UserPolicy()
        {
            UserPolicyMappings = new HashSet<UserPolicyMapping>();
        }

        [Key]
        public int Id { get; set; }
        public string PolicyName { get; set; }
        public int PolicyGroupId { get; set; }
        public virtual UserPolicyGroup? UserPolicyGroup { get; set; }
        public virtual ICollection<UserPolicyMapping>? UserPolicyMappings { get; set; }
    }
}
