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

public class UserDbContext : IdentityDbContext<ApplicationUser>
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Additional configuration for the RefreshToken entity if needed
        builder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}