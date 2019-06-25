using SevSharks.Identity.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SevSharks.Identity.DataAccess
{
    public class Context : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public Context(DbContextOptions<Context> options): base(options)
        {
        }

        public DbSet<UserExternalLogin> UserExternalLogins { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
            builder.Entity<UserExternalLogin>().ToTable("UserExternalLogins");

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Claims)
                .WithOne()
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.ExternalLogins)
                .WithOne(o => o.User)
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserExternalLogin>()
                .HasIndex(x => new { x.ExternalUserName, x.ExternalSystemName })
                .IsUnique();

            builder.Entity<ApplicationRole>()
                .HasMany(r => r.Claims)
                .WithOne()
                .HasForeignKey(c => c.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationRole>()
                .HasMany(r => r.Users)
                .WithOne()
                .HasForeignKey(r => r.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
