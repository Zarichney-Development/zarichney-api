using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Tests.TestData.Builders;

public class RefreshTokenBuilder : BaseBuilder<RefreshTokenBuilder, RefreshToken>
{
  private string _userId = Guid.NewGuid().ToString();
  private string _token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
  private DateTime _createdAt = DateTime.UtcNow;
  private DateTime _expiresAt = DateTime.UtcNow.AddDays(30);
  private bool _isUsed = false;
  private bool _isRevoked = false;
  private string? _deviceName = "Test Device";
  private string? _deviceIp = "127.0.0.1";
  private string? _userAgent = "Mozilla/5.0 (Test)";
  private DateTime? _lastUsedAt = DateTime.UtcNow;
  private ApplicationUser? _user = null;

  public RefreshTokenBuilder WithUserId(string userId)
  {
    _userId = userId;
    return this;
  }

  public RefreshTokenBuilder WithToken(string token)
  {
    _token = token;
    return this;
  }

  public RefreshTokenBuilder WithCreatedAt(DateTime createdAt)
  {
    _createdAt = createdAt;
    return this;
  }

  public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
  {
    _expiresAt = expiresAt;
    return this;
  }

  public RefreshTokenBuilder AsUsed()
  {
    _isUsed = true;
    return this;
  }

  public RefreshTokenBuilder AsRevoked()
  {
    _isRevoked = true;
    return this;
  }

  public RefreshTokenBuilder WithDeviceName(string? deviceName)
  {
    _deviceName = deviceName;
    return this;
  }

  public RefreshTokenBuilder WithDeviceIp(string? deviceIp)
  {
    _deviceIp = deviceIp;
    return this;
  }

  public RefreshTokenBuilder WithUserAgent(string? userAgent)
  {
    _userAgent = userAgent;
    return this;
  }

  public RefreshTokenBuilder WithLastUsedAt(DateTime? lastUsedAt)
  {
    _lastUsedAt = lastUsedAt;
    return this;
  }

  public RefreshTokenBuilder WithUser(ApplicationUser? user)
  {
    _user = user;
    if (user != null)
    {
      _userId = user.Id;
    }
    return this;
  }

  public RefreshTokenBuilder AsExpired()
  {
    _expiresAt = DateTime.UtcNow.AddDays(-1);
    return this;
  }

  public RefreshTokenBuilder AsValid()
  {
    _isUsed = false;
    _isRevoked = false;
    _expiresAt = DateTime.UtcNow.AddDays(30);
    return this;
  }

  public RefreshTokenBuilder WithDefaults()
  {
    return AsValid()
        .WithDeviceName("Default Test Device")
        .WithDeviceIp("192.168.1.1")
        .WithUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
  }

  public new RefreshToken Build()
  {
    return new RefreshToken
    {
      UserId = _userId,
      Token = _token,
      CreatedAt = _createdAt,
      ExpiresAt = _expiresAt,
      IsUsed = _isUsed,
      IsRevoked = _isRevoked,
      DeviceName = _deviceName,
      DeviceIp = _deviceIp,
      UserAgent = _userAgent,
      LastUsedAt = _lastUsedAt,
      User = _user
    };
  }
}
