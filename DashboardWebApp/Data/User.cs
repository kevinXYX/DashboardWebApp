using System.ComponentModel.DataAnnotations;

namespace DashboardWebApp.Data
{
    public class User
    {
        public User()
        {
            UserPolicyMappings = new HashSet<UserPolicyMapping>();
        }

        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? Password { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string? LogoFileName { get; set; }
        public int AspNetUserId { get; set; }
        public int OrganizationId { get; set; }
        public string? Fullname { get; set; }
        public string? BucketName { get; set; }
        public string? RegionName { get; set; }
        public string? StorageServiceName { get; set; }
        public int? StorageLimitKB { get; set; }
        public int? StorageUsedKB { get; set; }
        public bool? IsAdmin { get; set; }
        public int? UserStatus { get; set; }
        public DateTime? CreatedDate { get; set; }    
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual ICollection<UserPolicyMapping>? UserPolicyMappings { get; set; }
    }
}
