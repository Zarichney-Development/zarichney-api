using Zarichney.Models.Configuration;

namespace Zarichney.Models.Tests.Unit.Configuration;

public class ServerConfigTests
{
    [Fact]
    public void ServerConfig_WithValidUrl_ShouldPassValidation()
    {
        // Arrange
        var config = new ServerConfig { BaseUrl = "https://api.example.com" };
        var context = new ValidationContext(config);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void ServerConfig_WithEmptyUrl_ShouldFailValidation()
    {
        // Arrange
        var config = new ServerConfig { BaseUrl = "" };
        var context = new ValidationContext(config);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.ErrorMessage!.Contains("required"));
    }

    [Fact]
    public void ServerConfig_WithInvalidUrl_ShouldFailValidation()
    {
        // Arrange
        var config = new ServerConfig { BaseUrl = "not-a-valid-url" };
        var context = new ValidationContext(config);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().ContainSingle(r => r.ErrorMessage!.Contains("valid"));
    }

    [Fact]
    public void ServerConfig_JsonSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var config = new ServerConfig { BaseUrl = "https://api.example.com" };

        // Act
        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<ServerConfig>(json);

        // Assert
        json.Should().Contain("\"BaseUrl\":\"https://api.example.com\"");
        
        deserialized.Should().NotBeNull();
        deserialized!.BaseUrl.Should().Be(config.BaseUrl);
    }

    [Theory]
    [InlineData("https://api.example.com")]
    [InlineData("http://localhost:5000")]
    [InlineData("https://api.company.co.uk/v1")]
    public void ServerConfig_WithValidUrls_ShouldPassValidation(string url)
    {
        // Arrange
        var config = new ServerConfig { BaseUrl = url };
        var context = new ValidationContext(config);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    public void ServerConfig_WithInvalidUrls_ShouldFailValidation(string url)
    {
        // Arrange
        var config = new ServerConfig { BaseUrl = url };
        var context = new ValidationContext(config);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(config, context, results, true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().NotBeEmpty();
    }
}