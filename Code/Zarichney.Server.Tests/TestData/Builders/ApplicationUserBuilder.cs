using Microsoft.AspNetCore.Identity;
using Zarichney.Services.Auth;

namespace Zarichney.Tests.TestData.Builders;

public class ApplicationUserBuilder : BaseBuilder<ApplicationUserBuilder, ApplicationUser>
{
  private ApplicationUser _user;

  public ApplicationUserBuilder()
  {
    _user = new ApplicationUser
    {
      Id = Guid.NewGuid().ToString(),
      UserName = $"testuser_{Guid.NewGuid():N}",
      Email = $"test_{Guid.NewGuid():N}@example.com",
      EmailConfirmed = true,
      PhoneNumber = "+1234567890",
      PhoneNumberConfirmed = false,
      TwoFactorEnabled = false,
      LockoutEnabled = true,
      AccessFailedCount = 0,
      SecurityStamp = Guid.NewGuid().ToString(),
      ConcurrencyStamp = Guid.NewGuid().ToString(),
      NormalizedEmail = null,
      NormalizedUserName = null,
      PasswordHash = "AQAAAAEAACcQAAAAEH3+test+hash+value"
    };
  }

  public ApplicationUserBuilder WithId(string id)
  {
    _user.Id = id;
    return this;
  }

  public ApplicationUserBuilder WithUserName(string userName)
  {
    _user.UserName = userName;
    _user.NormalizedUserName = userName?.ToUpperInvariant();
    return this;
  }

  public ApplicationUserBuilder WithEmail(string email)
  {
    _user.Email = email;
    _user.NormalizedEmail = email?.ToUpperInvariant();
    return this;
  }

  public ApplicationUserBuilder WithEmailConfirmed(bool confirmed = true)
  {
    _user.EmailConfirmed = confirmed;
    return this;
  }

  public ApplicationUserBuilder WithPhoneNumber(string phoneNumber)
  {
    _user.PhoneNumber = phoneNumber;
    return this;
  }

  public ApplicationUserBuilder WithPhoneNumberConfirmed(bool confirmed = true)
  {
    _user.PhoneNumberConfirmed = confirmed;
    return this;
  }

  public ApplicationUserBuilder WithTwoFactorEnabled(bool enabled = true)
  {
    _user.TwoFactorEnabled = enabled;
    return this;
  }

  public ApplicationUserBuilder WithLockoutEnd(DateTimeOffset? lockoutEnd)
  {
    _user.LockoutEnd = lockoutEnd;
    return this;
  }

  public ApplicationUserBuilder WithLockoutEnabled(bool enabled = true)
  {
    _user.LockoutEnabled = enabled;
    return this;
  }

  public ApplicationUserBuilder WithAccessFailedCount(int count)
  {
    _user.AccessFailedCount = count;
    return this;
  }

  public ApplicationUserBuilder WithPasswordHash(string passwordHash)
  {
    _user.PasswordHash = passwordHash;
    return this;
  }

  public ApplicationUserBuilder WithSecurityStamp(string stamp)
  {
    _user.SecurityStamp = stamp;
    return this;
  }

  public ApplicationUserBuilder WithDefaults()
  {
    return WithUserName("defaultuser")
        .WithEmail("default@example.com")
        .WithEmailConfirmed(true);
  }

  public ApplicationUserBuilder AsLockedOut()
  {
    return WithLockoutEnd(DateTimeOffset.UtcNow.AddHours(1))
        .WithLockoutEnabled(true)
        .WithAccessFailedCount(5);
  }

  public ApplicationUserBuilder AsUnconfirmed()
  {
    return WithEmailConfirmed(false)
        .WithPhoneNumberConfirmed(false);
  }

  public new ApplicationUser Build()
  {
    return _user;
  }
}
