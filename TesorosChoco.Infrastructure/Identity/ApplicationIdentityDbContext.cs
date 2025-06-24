using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TesorosChoco.Infrastructure.Identity;

/// <summary>
/// Identity DbContext for authentication and authorization
/// Separate from main application context for security and separation of concerns
/// </summary>
public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Phone)
                .HasMaxLength(20);

            entity.Property(u => u.Address)
                .HasMaxLength(500);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.HasIndex(u => u.DomainUserId)
                .IsUnique()
                .HasFilter("[DomainUserId] IS NOT NULL");
        });

        // Customize Identity table names and constraints
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
    }
}
