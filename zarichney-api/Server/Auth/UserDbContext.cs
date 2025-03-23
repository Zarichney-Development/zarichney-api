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
}

public class UserDbContext(DbContextOptions<UserDbContext> options) : IdentityDbContext<ApplicationUser>(options);