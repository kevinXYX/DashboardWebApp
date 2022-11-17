using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Data
{
    public class UserPolicyGroup
    {
        public UserPolicyGroup()
        {
            UserPolicies = new HashSet<UserPolicy>();
        }

        [Key]
        public int Id { get; set; }
        public string PolicyGroupName { get; set; }
        public virtual ICollection<UserPolicy>? UserPolicies { get; set; }
    }
}
