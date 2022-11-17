using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DashboardWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserPolicy> UserPolicies { get; set; }
        public DbSet<UserPolicyGroup> UserPoliciesGroup { get; set; }
        public DbSet<UserPolicyMapping> UserPolicyMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(d => d.User)
                  .WithOne(d => d.ApplicationUser)
                  .HasForeignKey<ApplicationUser>(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_AspNetUsers_UserId");
            });

            builder.Entity<User>(entity =>
            {
                entity.HasOne(d => d.Organization)
                  .WithMany(d => d.Users)
                  .HasForeignKey(d => d.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Users_Organization_OrganizationId");
            });

            builder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organization");
            });

            builder.Entity<UserPolicy>(entity =>
            {
                entity.ToTable("UserPolicy");

                entity.HasOne(d => d.UserPolicyGroup)
                    .WithMany(d => d.UserPolicies)
                    .HasForeignKey(d => d.PolicyGroupId)
                    .HasConstraintName("FK_UserPolicy_PolicyGroup_PolicyGroupId");
            });

            builder.Entity<UserPolicyGroup>(entity =>
            {
                entity.ToTable("UserPolicyGroup");
            });

            builder.Entity<UserPolicyMapping>(entity =>
            {
                entity.ToTable("UserPolicyMapping");

                entity.HasOne(d => d.User)
                    .WithMany(d => d.UserPolicyMappings)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_UserPolicyMapping_UserId");

                entity.HasOne(d => d.UserPolicy)
                    .WithMany(d => d.UserPolicyMappings)
                    .HasForeignKey(d => d.PolicyId)
                    .HasConstraintName("FK_UserPolicyMapping_PolicyId");
            });
        }
    }
}