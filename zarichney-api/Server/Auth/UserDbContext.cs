using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Zarichney.Server.Auth;

public class ApplicationUser : IdentityUser
{
  // This class inherits all standard properties from IdentityUser:
  // - Id
  // - UserName
  // - Email
  // - EmailConfirmed
  // - PasswordHash
  // - SecurityStamp
  // - ConcurrencyStamp
  // - PhoneNumber
  // - PhoneNumberConfirmed
  // - TwoFactorEnabled
  // - LockoutEnd
  // - LockoutEnabled
  // - AccessFailedCount
        
  // Custom properties can be added here in the future
  
  // Navigation property for refresh tokens
  public virtual ICollection<RefreshToken>? RefreshTokens { get; set; }
}

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
  /* Tables inherited from IdentityDbContext - Command to list all tables in the database:
      ```
      zarichney_identity-# \dt
                       List of relations
       Schema |         Name          | Type  |  Owner
      --------+-----------------------+-------+----------
       public | AspNetRoleClaims      | table | postgres
       public | AspNetRoles           | table | postgres
       public | AspNetUserClaims      | table | postgres
       public | AspNetUserLogins      | table | postgres
       public | AspNetUserRoles       | table | postgres
       public | AspNetUserTokens      | table | postgres
       public | AspNetUsers           | table | postgres
       public | __EFMigrationsHistory | table | postgres
      ```
   */
  
  // Table for refresh tokens
  public DbSet<RefreshToken> RefreshTokens { get; init; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure refresh token table
        builder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}