using FluentAssertions;
using Xunit;
using Zarichney.Services.Auth;

namespace Zarichney.Tests.Unit.Services.Auth;

/// <summary>
/// Unit tests for the MiddlewareConfiguration class which handles authentication bypass logic
/// for specific routes in the application middleware pipeline.
/// </summary>
public class MiddlewareConfigurationTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithSwaggerPath_ReturnsTrue()
    {
        // Arrange
        var swaggerPath = "/api/swagger";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(swaggerPath);

        // Assert
        result.Should().BeTrue("because swagger paths should bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithSwaggerSubPath_ReturnsTrue()
    {
        // Arrange
        var swaggerSubPath = "/api/swagger/v1/swagger.json";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(swaggerSubPath);

        // Assert
        result.Should().BeTrue("because all swagger sub-paths should bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithAuthPath_ReturnsTrue()
    {
        // Arrange
        var authPath = "/api/auth";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(authPath);

        // Assert
        result.Should().BeTrue("because auth endpoint itself should bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithAuthSubPath_ReturnsTrue()
    {
        // Arrange
        var authLoginPath = "/api/auth/login";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(authLoginPath);

        // Assert
        result.Should().BeTrue("because all auth sub-paths should bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithAuthRegisterPath_ReturnsTrue()
    {
        // Arrange
        var authRegisterPath = "/api/auth/register";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(authRegisterPath);

        // Assert
        result.Should().BeTrue("because registration endpoint should bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithProtectedPath_ReturnsFalse()
    {
        // Arrange
        var protectedPath = "/api/users";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(protectedPath);

        // Assert
        result.Should().BeFalse("because user endpoints require authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithApiCookbookPath_ReturnsFalse()
    {
        // Arrange
        var cookbookPath = "/api/cookbook";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(cookbookPath);

        // Assert
        result.Should().BeFalse("because cookbook endpoints require authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithApiPaymentPath_ReturnsFalse()
    {
        // Arrange
        var paymentPath = "/api/payment";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(paymentPath);

        // Assert
        result.Should().BeFalse("because payment endpoints require authentication");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/API/SWAGGER")]
    [InlineData("/Api/Swagger")]
    [InlineData("/api/SWAGGER")]
    [InlineData("/API/swagger")]
    public void ShouldBypass_WithSwaggerPathDifferentCasing_ReturnsTrue(string path)
    {
        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(path);

        // Assert
        result.Should().BeTrue("because path comparison should be case-insensitive");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/API/AUTH")]
    [InlineData("/Api/Auth")]
    [InlineData("/api/AUTH")]
    [InlineData("/API/auth")]
    public void ShouldBypass_WithAuthPathDifferentCasing_ReturnsTrue(string path)
    {
        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(path);

        // Assert
        result.Should().BeTrue("because path comparison should be case-insensitive");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/API/AUTH/LOGIN")]
    [InlineData("/api/auth/REGISTER")]
    [InlineData("/Api/Auth/RefreshToken")]
    public void ShouldBypass_WithAuthSubPathsDifferentCasing_ReturnsTrue(string path)
    {
        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(path);

        // Assert
        result.Should().BeTrue("because auth sub-paths with any casing should bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithEmptyPath_ReturnsFalse()
    {
        // Arrange
        var emptyPath = "";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(emptyPath);

        // Assert
        result.Should().BeFalse("because empty paths should not bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithRootPath_ReturnsFalse()
    {
        // Arrange
        var rootPath = "/";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(rootPath);

        // Assert
        result.Should().BeFalse("because root path should not bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithApiOnlyPath_ReturnsFalse()
    {
        // Arrange
        var apiPath = "/api";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(apiPath);

        // Assert
        result.Should().BeFalse("because '/api' alone should not bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithPartialMatchNotAtStart_ReturnsFalse()
    {
        // Arrange
        var partialPath = "/v1/api/auth";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(partialPath);

        // Assert
        result.Should().BeFalse("because bypass paths must start with the specified prefixes");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithAuthenticationPath_ReturnsTrue()
    {
        // Arrange
        var authenticationPath = "/api/authentication";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(authenticationPath);

        // Assert
        result.Should().BeTrue("because '/api/authentication' starts with '/api/auth' prefix");
        // NOTE: This shows the current implementation bypasses any path starting with '/api/auth' regardless of the full word
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithSwaggerUiPath_ReturnsTrue()
    {
        // Arrange
        var swaggerUiPath = "/api/swagger/index.html";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(swaggerUiPath);

        // Assert
        result.Should().BeTrue("because Swagger UI paths should bypass authentication");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/api/auth/forgot-password")]
    [InlineData("/api/auth/reset-password")]
    [InlineData("/api/auth/confirm-email")]
    [InlineData("/api/auth/resend-confirmation")]
    public void ShouldBypass_WithCommonAuthEndpoints_ReturnsTrue(string path)
    {
        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(path);

        // Assert
        result.Should().BeTrue($"because {path} is an authentication-related endpoint that should bypass authentication");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/api/swagger-ui")]
    [InlineData("/api/swaggerui")]
    [InlineData("/api/swaggers")]
    public void ShouldBypass_WithSimilarButIncorrectSwaggerPath_ReturnsTrue(string path)
    {
        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(path);

        // Assert
        result.Should().BeTrue($"because {path} starts with '/api/swagger' prefix (current implementation uses StartsWith)");
        // NOTE: This means paths like /api/swaggers are also bypassed which may not be intended
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithNullPath_ThrowsNullReferenceException()
    {
        // Arrange
        string? nullPath = null;

        // Act
        var act = () => MiddlewareConfiguration.Routes.ShouldBypass(nullPath!);

        // Assert
        act.Should().Throw<NullReferenceException>("because null path handling is not implemented in production code");
        // NOTE: This test documents current behavior. Production code should be updated to handle null paths gracefully.
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/api/auth?param=value")]
    [InlineData("/api/auth#fragment")]
    [InlineData("/api/swagger?v=1.0")]
    public void ShouldBypass_WithQueryStringOrFragment_ReturnsTrue(string path)
    {
        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(path);

        // Assert
        result.Should().BeTrue($"because {path} starts with a bypass prefix regardless of query strings or fragments");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithWhitespacePaddedPath_HandlesCorrectly()
    {
        // Arrange
        var paddedPath = "  /api/auth  ";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(paddedPath);

        // Assert
        result.Should().BeFalse("because paths with leading/trailing whitespace are not normalized and should not match");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("/api/authorize")]
    [InlineData("/api/authorized")]
    [InlineData("/api/authorizations")]
    public void ShouldBypass_WithAuthPrefixButDifferentWord_ReturnsTrue(string path)
    {
        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(path);

        // Assert
        result.Should().BeTrue($"because {path} starts with '/api/auth' prefix (current implementation uses StartsWith)");
        // NOTE: This may be a security issue as it bypasses auth for paths like /api/authorize which may not be intended
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithDeepNestedAuthPath_ReturnsTrue()
    {
        // Arrange
        var deepPath = "/api/auth/v1/users/123/tokens/refresh";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(deepPath);

        // Assert
        result.Should().BeTrue("because any path starting with '/api/auth' should bypass authentication");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ShouldBypass_WithDeepNestedSwaggerPath_ReturnsTrue()
    {
        // Arrange
        var deepSwaggerPath = "/api/swagger/v1/docs/openapi.json";

        // Act
        var result = MiddlewareConfiguration.Routes.ShouldBypass(deepSwaggerPath);

        // Assert
        result.Should().BeTrue("because any path starting with '/api/swagger' should bypass authentication");
    }
}