using Zarichney.Models.Auth;

namespace Zarichney.Models.Tests.Unit.Auth;

public class AuthResultTests
{
    [Fact]
    public void AuthResult_Constructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var success = true;
        var message = "Login successful";
        var email = "test@example.com";
        var redirectUrl = "/dashboard";

        // Act
        var result = new AuthResult(success, message, email, redirectUrl);

        // Assert
        result.Success.Should().Be(success);
        result.Message.Should().Be(message);
        result.Email.Should().Be(email);
        result.RedirectUrl.Should().Be(redirectUrl);
    }

    [Fact]
    public void AuthResult_Constructor_WithOptionalParameters_ShouldSetDefaults()
    {
        // Arrange
        var success = false;
        var message = "Login failed";

        // Act
        var result = new AuthResult(success, message);

        // Assert
        result.Success.Should().Be(success);
        result.Message.Should().Be(message);
        result.Email.Should().BeNull();
        result.RedirectUrl.Should().BeNull();
    }

    [Fact]
    public void AuthResult_Fail_ShouldCreateFailedResult()
    {
        // Arrange
        var message = "Authentication failed";

        // Act
        var result = AuthResult.Fail(message);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(message);
        result.Email.Should().BeNull();
        result.RedirectUrl.Should().BeNull();
    }

    [Fact]
    public void AuthResult_Ok_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var message = "Authentication successful";

        // Act
        var result = AuthResult.Ok(message);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(message);
        result.Email.Should().BeNull();
        result.RedirectUrl.Should().BeNull();
    }

    [Fact]
    public void AuthResult_Ok_WithParameters_ShouldCreateSuccessfulResultWithData()
    {
        // Arrange
        var message = "Login successful";
        var email = "user@example.com";
        var redirectUrl = "/home";

        // Act
        var result = AuthResult.Ok(message, email, redirectUrl);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(message);
        result.Email.Should().Be(email);
        result.RedirectUrl.Should().Be(redirectUrl);
    }

    [Fact]
    public void AuthResult_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var authResult = new AuthResult(true, "Success", "test@example.com", "/dashboard");

        // Act
        var json = JsonSerializer.Serialize(authResult);
        var deserialized = JsonSerializer.Deserialize<AuthResult>(json);

        // Assert
        json.Should().Contain("\"Success\":true");
        json.Should().Contain("\"Message\":\"Success\"");
        json.Should().Contain("\"Email\":\"test@example.com\"");
        json.Should().Contain("\"RedirectUrl\":\"/dashboard\"");

        deserialized.Should().NotBeNull();
        deserialized!.Success.Should().Be(authResult.Success);
        deserialized.Message.Should().Be(authResult.Message);
        deserialized.Email.Should().Be(authResult.Email);
        deserialized.RedirectUrl.Should().Be(authResult.RedirectUrl);
    }

    [Fact]
    public void AuthResult_JsonDeserialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """{"Success":false,"Message":"Invalid credentials","Email":null,"RedirectUrl":null}""";

        // Act
        var result = JsonSerializer.Deserialize<AuthResult>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials");
        result.Email.Should().BeNull();
        result.RedirectUrl.Should().BeNull();
    }
}