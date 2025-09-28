using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Models;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Auth.AuthService;

public class AuthServiceTests : IDisposable
{
  private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
  private readonly UserDbContext _dbContext;
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Zarichney.Services.Auth.AuthService _sut;
  private readonly Fixture _fixture;
  private readonly JwtSettings _jwtSettings;

  public AuthServiceTests()
  {
    _fixture = new Fixture();

    // Create JWT settings
    // ⚠️ SECURITY WARNING: This hardcoded JWT secret is for testing only!
    // NEVER use hardcoded secrets in production code - use secure configuration
    // management (Azure KeyVault, environment variables, etc.)
    _jwtSettings = new JwtSettings
    {
      SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123456789", // TEST ONLY - DO NOT COPY TO PRODUCTION
      Issuer = "TestIssuer",
      Audience = "TestAudience",
      ExpiryMinutes = 60,
      RefreshTokenExpiryDays = 30
    };

    // Setup mocks
    _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
    _jwtSettingsMock.Setup(x => x.Value).Returns(_jwtSettings);

    // Setup in-memory database
    var dbContextOptions = new DbContextOptionsBuilder<UserDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;
    _dbContext = new UserDbContext(dbContextOptions);

    _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

    var userStore = new Mock<IUserStore<ApplicationUser>>();
    _userManagerMock = new Mock<UserManager<ApplicationUser>>(
        userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

    _sut = new Zarichney.Services.Auth.AuthService(
        _jwtSettingsMock.Object,
        _dbContext,
        _httpContextAccessorMock.Object,
        _userManagerMock.Object);
  }

  public void Dispose()
  {
    _dbContext?.Dispose();
  }

  #region GenerateJwtTokenAsync Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GenerateJwtTokenAsync_ValidUser_ReturnsValidToken()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .Build();

    var roles = new[] { "User", "Admin" };
    _userManagerMock.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(roles);

    // Act
    var token = await _sut.GenerateJwtTokenAsync(user);

    // Assert
    token.Should().NotBeNullOrEmpty("because a valid token should be generated");

    // Verify token structure
    var handler = new JwtSecurityTokenHandler();
    var jsonToken = handler.ReadJwtToken(token);

    jsonToken.Issuer.Should().Be(_jwtSettings.Issuer, "because the issuer should match settings");
    jsonToken.Audiences.Should().Contain(_jwtSettings.Audience, "because the audience should match settings");

    // Verify claims
    var claims = jsonToken.Claims.ToList();
    claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id,
        "because the token should contain the user ID as subject");
    claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email,
        "because the token should contain the user email");
    claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User",
        "because the token should contain user roles");
    claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin",
        "because the token should contain all user roles");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GenerateJwtTokenAsync_UserWithNoRoles_ReturnsTokenWithoutRoles()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .Build();

    _userManagerMock.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(Array.Empty<string>());

    // Act
    var token = await _sut.GenerateJwtTokenAsync(user);

    // Assert
    token.Should().NotBeNullOrEmpty("because a valid token should be generated even without roles");

    var handler = new JwtSecurityTokenHandler();
    var jsonToken = handler.ReadJwtToken(token);

    jsonToken.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role,
        "because the token should not contain role claims when user has no roles");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GenerateJwtTokenAsync_ValidUser_TokenExpiresCorrectly()
  {
    // Arrange
    var user = new ApplicationUserBuilder().Build();
    _userManagerMock.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(Array.Empty<string>());

    var beforeGeneration = DateTime.UtcNow;

    // Act
    var token = await _sut.GenerateJwtTokenAsync(user);

    // Assert
    var handler = new JwtSecurityTokenHandler();
    var jsonToken = handler.ReadJwtToken(token);

    var expectedExpiry = beforeGeneration.AddMinutes(_jwtSettings.ExpiryMinutes);
    jsonToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5),
        "because the token should expire after the configured minutes");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GenerateJwtTokenAsync_ValidUser_ContainsUniqueJti()
  {
    // Arrange
    var user = new ApplicationUserBuilder().Build();
    _userManagerMock.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(Array.Empty<string>());

    // Act
    var token1 = await _sut.GenerateJwtTokenAsync(user);
    var token2 = await _sut.GenerateJwtTokenAsync(user);

    // Assert
    var handler = new JwtSecurityTokenHandler();
    var jsonToken1 = handler.ReadJwtToken(token1);
    var jsonToken2 = handler.ReadJwtToken(token2);

    var jti1 = jsonToken1.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
    var jti2 = jsonToken2.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

    jti1.Should().NotBeNullOrEmpty("because JTI should be present");
    jti2.Should().NotBeNullOrEmpty("because JTI should be present");
    jti1.Should().NotBe(jti2, "because each token should have a unique JTI");
  }

  #endregion

  #region GenerateRefreshToken Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void GenerateRefreshToken_Always_ReturnsBase64String()
  {
    // Act
    var token = _sut.GenerateRefreshToken();

    // Assert
    token.Should().NotBeNullOrEmpty("because a refresh token should be generated");

    // Verify it's valid Base64
    Action decode = () => Convert.FromBase64String(token);
    decode.Should().NotThrow("because the token should be valid Base64");

    // Verify decoded length is 64 bytes
    var bytes = Convert.FromBase64String(token);
    bytes.Length.Should().Be(64, "because the token should be 64 bytes");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void GenerateRefreshToken_MultipleCalls_ReturnsUniqueTokens()
  {
    // Act
    var tokens = new HashSet<string>();
    for (int i = 0; i < 100; i++)
    {
      tokens.Add(_sut.GenerateRefreshToken());
    }

    // Assert
    tokens.Count.Should().Be(100, "because each generated token should be unique");
  }

  #endregion

  #region SaveRefreshTokenAsync Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SaveRefreshTokenAsync_ValidInput_SavesTokenWithCorrectProperties()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("user-123")
        .Build();

    var refreshToken = "test-refresh-token";
    var deviceName = "iPhone 12";
    var deviceIp = "192.168.1.100";
    var userAgent = "Mozilla/5.0 iPhone";

    var beforeSave = DateTime.UtcNow;

    // Act
    var result = await _sut.SaveRefreshTokenAsync(user, refreshToken, deviceName, deviceIp, userAgent);

    // Assert
    var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
    savedToken.Should().NotBeNull("because the token should be saved to the database");

    savedToken.UserId.Should().Be(user.Id, "because the token should be associated with the user");
    savedToken.Token.Should().Be(refreshToken, "because the token value should match");
    savedToken.DeviceName.Should().Be(deviceName, "because device name should be saved");
    savedToken.DeviceIp.Should().Be(deviceIp, "because device IP should be saved");
    savedToken.UserAgent.Should().Be(userAgent, "because user agent should be saved");
    savedToken.IsUsed.Should().BeFalse("because new tokens are not used");
    savedToken.IsRevoked.Should().BeFalse("because new tokens are not revoked");

    savedToken.CreatedAt.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(1),
        "because creation time should be current UTC time");
    savedToken.ExpiresAt.Should().BeCloseTo(beforeSave.AddDays(_jwtSettings.RefreshTokenExpiryDays),
        TimeSpan.FromSeconds(1), "because expiry should be calculated from settings");
    savedToken.LastUsedAt.Should().BeCloseTo(beforeSave, TimeSpan.FromSeconds(1),
        "because last used should be set to creation time");

    // Verify it was actually saved
    _dbContext.RefreshTokens.Count().Should().Be(1, "because exactly one token should be saved");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SaveRefreshTokenAsync_NoHttpContext_UsesProvidedValues()
  {
    // Arrange
    var user = new ApplicationUserBuilder().Build();
    var refreshToken = "test-token";

    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

    // Act
    await _sut.SaveRefreshTokenAsync(user, refreshToken, "Device", "1.2.3.4", "Agent");

    // Assert
    var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
    savedToken.Should().NotBeNull("because the token should be saved");
    savedToken.DeviceName.Should().Be("Device", "because provided value should be used");
    savedToken.DeviceIp.Should().Be("1.2.3.4", "because provided value should be used");
    savedToken.UserAgent.Should().Be("Agent", "because provided value should be used");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SaveRefreshTokenAsync_HttpContextAvailable_UsesHttpContextValues()
  {
    // Arrange
    var user = new ApplicationUserBuilder().Build();
    var refreshToken = "test-token";

    var httpContext = new DefaultHttpContext();
    httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");
    httpContext.Request.Headers.UserAgent = "HttpContext User Agent";

    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    // Act
    await _sut.SaveRefreshTokenAsync(user, refreshToken);

    // Assert
    var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
    savedToken.Should().NotBeNull("because the token should be saved");
    savedToken.DeviceIp.Should().Be("10.0.0.1", "because HttpContext IP should be used when not provided");
    savedToken.UserAgent.Should().Be("HttpContext User Agent",
        "because HttpContext user agent should be used when not provided");
  }

  #endregion

  #region FindRefreshTokenAsync Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task FindRefreshTokenAsync_TokenExists_ReturnsTokenWithUser()
  {
    // Arrange
    var tokenValue = "existing-token";
    var user = new ApplicationUserBuilder().Build();
    var expectedToken = new RefreshTokenBuilder()
        .WithToken(tokenValue)
        .WithUser(user)
        .Build();

    // Add token to database
    _dbContext.RefreshTokens.Add(expectedToken);
    await _dbContext.SaveChangesAsync();

    // Act
    var result = await _sut.FindRefreshTokenAsync(tokenValue);

    // Assert
    result.Should().NotBeNull("because the token exists in the database");
    result!.Token.Should().Be(tokenValue, "because it should return the matching token");
    result.User.Should().Be(user, "because the user should be included");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task FindRefreshTokenAsync_TokenDoesNotExist_ReturnsNull()
  {
    // Arrange
    // Act - no tokens in database
    var result = await _sut.FindRefreshTokenAsync("non-existent-token");

    // Assert
    result.Should().BeNull("because the token does not exist in the database");
  }

  [Fact(Skip = "EF Core InMemory database doesn't properly handle Include operations")]
  [Trait("Category", "Unit")]
  public async Task FindRefreshTokenAsync_MultipleTokens_ReturnsCorrectOne()
  {
    // Arrange
    var targetToken = "target-token";
    var targetUserId = "target-user";

    // Create and add the target token
    var targetRefreshToken = new RefreshTokenBuilder()
        .WithToken(targetToken)
        .WithUserId(targetUserId)
        .Build();

    // Add multiple tokens to the database
    await _dbContext.RefreshTokens.AddAsync(new RefreshTokenBuilder().WithToken("token1").Build());
    await _dbContext.RefreshTokens.AddAsync(targetRefreshToken);
    await _dbContext.RefreshTokens.AddAsync(new RefreshTokenBuilder().WithToken("token3").Build());

    await _dbContext.SaveChangesAsync();

    // Verify it was saved correctly
    var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == targetToken);
    savedToken.Should().NotBeNull("because the token was saved");
    savedToken!.UserId.Should().Be(targetUserId, "because the user ID was saved");

    // Act
    var result = await _sut.FindRefreshTokenAsync(targetToken);

    // Assert
    result.Should().NotBeNull("because the token exists");
    result!.Token.Should().Be(targetToken, "because it should find the correct token");
    result.UserId.Should().Be(targetUserId, "because it should return the correct token with user ID");
  }

  #endregion

  #region MarkRefreshTokenAsUsedAsync Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task MarkRefreshTokenAsUsedAsync_ValidToken_UpdatesTokenProperties()
  {
    // Arrange
    var token = new RefreshTokenBuilder()
        .WithToken("test-token")
        .AsValid()
        .Build();

    token.IsUsed.Should().BeFalse("precondition: token should not be used");

    // Add token to database
    _dbContext.RefreshTokens.Add(token);
    await _dbContext.SaveChangesAsync();

    var beforeUpdate = DateTime.UtcNow;

    // Act
    await _sut.MarkRefreshTokenAsUsedAsync(token);

    // Assert
    token.IsUsed.Should().BeTrue("because the token should be marked as used");
    token.LastUsedAt.Should().NotBeNull("because last used time should be set");
    token.LastUsedAt.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(1),
        "because last used time should be current UTC time");

    // Verify the change was persisted
    var updatedToken = await _dbContext.RefreshTokens.FindAsync(token.Id);
    updatedToken.Should().NotBeNull();
    updatedToken!.IsUsed.Should().BeTrue("because the token should be persisted as used");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task MarkRefreshTokenAsUsedAsync_AlreadyUsedToken_StillUpdatesLastUsedAt()
  {
    // Arrange
    var originalLastUsed = DateTime.UtcNow.AddDays(-1);
    var token = new RefreshTokenBuilder()
        .AsUsed()
        .WithLastUsedAt(originalLastUsed)
        .Build();

    // Add token to database
    _dbContext.RefreshTokens.Add(token);
    await _dbContext.SaveChangesAsync();

    // Act
    await _sut.MarkRefreshTokenAsUsedAsync(token);

    // Assert
    token.IsUsed.Should().BeTrue("because the token remains used");
    token.LastUsedAt.Should().BeAfter(originalLastUsed,
        "because last used time should be updated even for already used tokens");
  }

  #endregion

  #region RevokeRefreshTokenAsync Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeRefreshTokenAsync_ValidToken_MarksAsRevoked()
  {
    // Arrange
    var token = new RefreshTokenBuilder()
        .AsValid()
        .Build();

    token.IsRevoked.Should().BeFalse("precondition: token should not be revoked");

    // Add token to database
    _dbContext.RefreshTokens.Add(token);
    await _dbContext.SaveChangesAsync();

    // Act
    await _sut.RevokeRefreshTokenAsync(token);

    // Assert
    token.IsRevoked.Should().BeTrue("because the token should be marked as revoked");

    // Verify the change was persisted
    var updatedToken = await _dbContext.RefreshTokens.FindAsync(token.Id);
    updatedToken.Should().NotBeNull();
    updatedToken!.IsRevoked.Should().BeTrue("because the token should be persisted as revoked");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeRefreshTokenAsync_AlreadyRevokedToken_RemainsRevoked()
  {
    // Arrange
    var token = new RefreshTokenBuilder()
        .AsRevoked()
        .Build();

    // Add token to database
    _dbContext.RefreshTokens.Add(token);
    await _dbContext.SaveChangesAsync();

    // Act
    await _sut.RevokeRefreshTokenAsync(token);

    // Assert
    token.IsRevoked.Should().BeTrue("because the token remains revoked");

    // Verify the change was persisted
    var updatedToken = await _dbContext.RefreshTokens.FindAsync(token.Id);
    updatedToken.Should().NotBeNull();
    updatedToken!.IsRevoked.Should().BeTrue("because the token should remain revoked");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeRefreshTokenAsync_UsedToken_CanBeRevoked()
  {
    // Arrange
    var token = new RefreshTokenBuilder()
        .AsUsed()
        .Build();

    token.IsRevoked.Should().BeFalse("precondition: used token should not be revoked");

    // Add token to database
    _dbContext.RefreshTokens.Add(token);
    await _dbContext.SaveChangesAsync();

    // Act
    await _sut.RevokeRefreshTokenAsync(token);

    // Assert
    token.IsUsed.Should().BeTrue("because used status should not change");
    token.IsRevoked.Should().BeTrue("because used tokens can still be revoked");
  }

  #endregion
}
