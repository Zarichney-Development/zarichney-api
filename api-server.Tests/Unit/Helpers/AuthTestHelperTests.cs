using System.Security.Claims;
using FluentAssertions;
using Xunit;
using Zarichney.Tests.Helpers;

namespace Zarichney.Tests.Unit.Helpers;

[Trait("Category", "Unit")]
[Trait("Component", "Helper")]
public class AuthTestHelperTests
{
    [Fact]
    public void GenerateTestToken_ValidInputs_ReturnsNonEmptyString()
    {
        // Arrange
        var userId = "test123";
        var roles = new[] { "user", "admin" };
        
        // Act
        var token = AuthTestHelper.GenerateTestToken(userId, roles);
        
        // Assert
        token.Should().NotBeNullOrEmpty("because a valid token should be generated for valid inputs");
    }

    [Fact]
    public void GenerateTestToken_NoRoles_ReturnsValidToken()
    {
        // Arrange
        var userId = "test123";
        var roles = Array.Empty<string>();
        
        // Act
        var token = AuthTestHelper.GenerateTestToken(userId, roles);
        
        // Assert
        token.Should().NotBeNullOrEmpty("because a token should be generated even without roles");
        var claims = AuthTestHelper.ParseTestToken(token);
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId,
            "because user ID claim should be present");
        claims.Should().NotContain(c => c.Type == ClaimTypes.Role,
            "because no role claims should be present when no roles are provided");
    }

    [Fact]
    public void GenerateTestToken_WithRoles_ContainsExpectedClaims()
    {
        // Arrange
        var userId = "test123";
        var roles = new[] { "user", "admin" };
        
        // Act
        var token = AuthTestHelper.GenerateTestToken(userId, roles);
        var claims = AuthTestHelper.ParseTestToken(token);
        
        // Assert
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId,
            "because the user ID claim should be present");
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == $"TestUser_{userId}",
            "because the name claim should be present with expected format");
        foreach (var role in roles)
        {
            claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role,
                $"because the {role} role claim should be present");
        }
    }

    [Fact]
    public void ParseTestToken_ValidToken_ReturnsClaims()
    {
        // Arrange
        var userId = "test123";
        var roles = new[] { "user", "admin" };
        var token = AuthTestHelper.GenerateTestToken(userId, roles);
        
        // Act
        var claims = AuthTestHelper.ParseTestToken(token);
        
        // Assert
        claims.Should().NotBeEmpty("because a valid token should contain claims");
        claims.Count().Should().Be(4, "because we expect userId, name, and two role claims");
        claims.Count(c => c.Type == ClaimTypes.Role).Should().Be(2, "because we provided two roles");
    }

    [Fact]
    public void ParseTestToken_InvalidBase64_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidToken = "not-base64-encoded";
        
        // Act & Assert
        var action = () => AuthTestHelper.ParseTestToken(invalidToken);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to parse test token: *")
            .WithInnerException<FormatException>("because invalid base64 should cause a format exception");
    }

    [Fact]
    public void ParseTestToken_InvalidJsonContent_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidJson = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("not-valid-json"));
        
        // Act & Assert
        var action = () => AuthTestHelper.ParseTestToken(invalidJson);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Failed to parse test token: *")
            .WithInnerException<System.Text.Json.JsonException>("because invalid JSON content should cause a JSON exception");
    }

    [Fact]
    public void CreateTestClaims_ValidInputs_ReturnsExpectedClaims()
    {
        // Arrange
        var userId = "test123";
        var roles = new[] { "user", "admin" };
        
        // Act
        var claims = AuthTestHelper.CreateTestClaims(userId, roles);
        
        // Assert
        claims.Should().SatisfyRespectively(
            first => {
                first.Type.Should().Be(ClaimTypes.NameIdentifier, "because first claim should be user ID");
                first.Value.Should().Be(userId);
            },
            second => {
                second.Type.Should().Be(ClaimTypes.Name, "because second claim should be name");
                second.Value.Should().Be($"TestUser_{userId}");
            },
            third => {
                third.Type.Should().Be(ClaimTypes.Role, "because third claim should be first role");
                third.Value.Should().Be(roles[0]);
            },
            fourth => {
                fourth.Type.Should().Be(ClaimTypes.Role, "because fourth claim should be second role");
                fourth.Value.Should().Be(roles[1]);
            }
        );
    }

    [Fact]
    public void CreateTestClaims_NoRoles_ReturnsOnlyUserClaims()
    {
        // Arrange
        var userId = "test123";
        var roles = Array.Empty<string>();
        
        // Act
        var claims = AuthTestHelper.CreateTestClaims(userId, roles);
        
        // Assert
        claims.Should().HaveCount(2, "because only userId and name claims should be present without roles");
        claims.Should().SatisfyRespectively(
            first => {
                first.Type.Should().Be(ClaimTypes.NameIdentifier);
                first.Value.Should().Be(userId);
            },
            second => {
                second.Type.Should().Be(ClaimTypes.Name);
                second.Value.Should().Be($"TestUser_{userId}");
            }
        );
    }
}