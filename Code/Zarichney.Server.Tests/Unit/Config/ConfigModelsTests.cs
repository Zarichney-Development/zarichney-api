using FluentAssertions;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.Status;
using AutoFixture;
using System.Reflection;

namespace Zarichney.Tests.Unit.Config;

public class ConfigModelsTests
{
    private readonly Fixture _fixture = new();

    [Trait("Category", "Unit")]
    [Fact]
    public void ServerConfig_ImplementsIConfig()
    {
        // Arrange & Act
        var config = new ServerConfig();
        
        // Assert
        config.Should().BeAssignableTo<IConfig>("because ServerConfig should implement the IConfig marker interface");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ServerConfig_BaseUrl_DefaultsToEmpty()
    {
        // Arrange & Act
        var config = new ServerConfig();
        
        // Assert
        config.BaseUrl.Should().BeEmpty("because BaseUrl should default to empty string when not configured");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ServerConfig_BaseUrl_CanBeSet()
    {
        // Arrange
        var expectedUrl = _fixture.Create<string>();
        
        // Act
        var config = new ServerConfig { BaseUrl = expectedUrl };
        
        // Assert
        config.BaseUrl.Should().Be(expectedUrl, "because BaseUrl should be settable to any string value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ServerConfig_BaseUrl_HasRequiresConfigurationAttribute()
    {
        // Arrange
        var property = typeof(ServerConfig).GetProperty(nameof(ServerConfig.BaseUrl));
        
        // Act
        var attribute = property?.GetCustomAttribute<RequiresConfigurationAttribute>();
        
        // Assert
        attribute.Should().NotBeNull("because BaseUrl property should have RequiresConfigurationAttribute");
        attribute.Features.Should().Contain(ExternalServices.FrontEnd, 
            "because BaseUrl requires FrontEnd service configuration");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ClientConfig_ImplementsIConfig()
    {
        // Arrange & Act
        var config = new ClientConfig();
        
        // Assert
        config.Should().BeAssignableTo<IConfig>("because ClientConfig should implement the IConfig marker interface");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ClientConfig_BaseUrl_DefaultsToEmpty()
    {
        // Arrange & Act
        var config = new ClientConfig();
        
        // Assert
        config.BaseUrl.Should().BeEmpty("because BaseUrl should default to empty string when not configured");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ClientConfig_BaseUrl_CanBeSet()
    {
        // Arrange
        var expectedUrl = _fixture.Create<string>();
        
        // Act
        var config = new ClientConfig { BaseUrl = expectedUrl };
        
        // Assert
        config.BaseUrl.Should().Be(expectedUrl, "because BaseUrl should be settable to any string value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void ClientConfig_BaseUrl_HasRequiresConfigurationAttribute()
    {
        // Arrange
        var property = typeof(ClientConfig).GetProperty(nameof(ClientConfig.BaseUrl));
        
        // Act
        var attribute = property?.GetCustomAttribute<RequiresConfigurationAttribute>();
        
        // Assert
        attribute.Should().NotBeNull("because BaseUrl property should have RequiresConfigurationAttribute");
        attribute.Features.Should().Contain(ExternalServices.FrontEnd, 
            "because BaseUrl requires FrontEnd service configuration");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_ImplementsIConfig()
    {
        // Arrange & Act
        var config = new MockAuthConfig();
        
        // Assert
        config.Should().BeAssignableTo<IConfig>("because MockAuthConfig should implement the IConfig marker interface");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultRoles_InitializesWithUserRole()
    {
        // Arrange & Act
        var config = new MockAuthConfig();
        
        // Assert
        config.DefaultRoles.Should().NotBeNull("because DefaultRoles should be initialized");
        config.DefaultRoles.Should().HaveCount(1, "because default configuration should have one role");
        config.DefaultRoles.Should().Contain("User", "because User should be the default role");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultRoles_CanBeCustomized()
    {
        // Arrange
        var customRoles = _fixture.CreateMany<string>().ToList();
        
        // Act
        var config = new MockAuthConfig { DefaultRoles = customRoles };
        
        // Assert
        config.DefaultRoles.Should().BeEquivalentTo(customRoles, "because DefaultRoles should be settable to custom values");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultUsername_InitializesToMockUser()
    {
        // Arrange & Act
        var config = new MockAuthConfig();
        
        // Assert
        config.DefaultUsername.Should().Be("MockUser", "because DefaultUsername should initialize to the expected default value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultUsername_CanBeCustomized()
    {
        // Arrange
        var customUsername = _fixture.Create<string>();
        
        // Act
        var config = new MockAuthConfig { DefaultUsername = customUsername };
        
        // Assert
        config.DefaultUsername.Should().Be(customUsername, "because DefaultUsername should be settable to custom values");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultEmail_InitializesToMockEmail()
    {
        // Arrange & Act
        var config = new MockAuthConfig();
        
        // Assert
        config.DefaultEmail.Should().Be("mock@example.com", "because DefaultEmail should initialize to the expected default value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultEmail_CanBeCustomized()
    {
        // Arrange
        var customEmail = _fixture.Create<string>();
        
        // Act
        var config = new MockAuthConfig { DefaultEmail = customEmail };
        
        // Assert
        config.DefaultEmail.Should().Be(customEmail, "because DefaultEmail should be settable to custom values");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultUserId_InitializesToMockUserId()
    {
        // Arrange & Act
        var config = new MockAuthConfig();
        
        // Assert
        config.DefaultUserId.Should().Be("mock-user-id", "because DefaultUserId should initialize to the expected default value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void MockAuthConfig_DefaultUserId_CanBeCustomized()
    {
        // Arrange
        var customUserId = _fixture.Create<string>();
        
        // Act
        var config = new MockAuthConfig { DefaultUserId = customUserId };
        
        // Assert
        config.DefaultUserId.Should().Be(customUserId, "because DefaultUserId should be settable to custom values");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DefaultAdminUserConfig_ImplementsIConfig()
    {
        // Arrange & Act
        var config = new DefaultAdminUserConfig();
        
        // Assert
        config.Should().BeAssignableTo<IConfig>("because DefaultAdminUserConfig should implement the IConfig marker interface");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DefaultAdminUserConfig_Email_DefaultsToEmpty()
    {
        // Arrange & Act
        var config = new DefaultAdminUserConfig();
        
        // Assert
        config.Email.Should().BeEmpty("because Email should default to empty string when not configured");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DefaultAdminUserConfig_Email_CanBeSet()
    {
        // Arrange
        var expectedEmail = _fixture.Create<string>();
        
        // Act
        var config = new DefaultAdminUserConfig { Email = expectedEmail };
        
        // Assert
        config.Email.Should().Be(expectedEmail, "because Email should be settable to any string value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DefaultAdminUserConfig_UserName_DefaultsToEmpty()
    {
        // Arrange & Act
        var config = new DefaultAdminUserConfig();
        
        // Assert
        config.UserName.Should().BeEmpty("because UserName should default to empty string when not configured");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DefaultAdminUserConfig_UserName_CanBeSet()
    {
        // Arrange
        var expectedUserName = _fixture.Create<string>();
        
        // Act
        var config = new DefaultAdminUserConfig { UserName = expectedUserName };
        
        // Assert
        config.UserName.Should().Be(expectedUserName, "because UserName should be settable to any string value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DefaultAdminUserConfig_Password_DefaultsToEmpty()
    {
        // Arrange & Act
        var config = new DefaultAdminUserConfig();
        
        // Assert
        config.Password.Should().BeEmpty("because Password should default to empty string when not configured");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void DefaultAdminUserConfig_Password_CanBeSet()
    {
        // Arrange
        var expectedPassword = _fixture.Create<string>();
        
        // Act
        var config = new DefaultAdminUserConfig { Password = expectedPassword };
        
        // Assert
        config.Password.Should().Be(expectedPassword, "because Password should be settable to any string value");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void IConfig_IsMarkerInterface()
    {
        // Arrange
        var interfaceType = typeof(IConfig);
        
        // Act
        var methods = interfaceType.GetMethods();
        var properties = interfaceType.GetProperties();
        
        // Assert
        methods.Should().BeEmpty("because IConfig should be a marker interface with no methods");
        properties.Should().BeEmpty("because IConfig should be a marker interface with no properties");
        interfaceType.IsInterface.Should().BeTrue("because IConfig should be an interface type");
    }

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData(typeof(ServerConfig))]
    [InlineData(typeof(ClientConfig))]
    [InlineData(typeof(MockAuthConfig))]
    [InlineData(typeof(DefaultAdminUserConfig))]
    public void AllConfigClasses_ImplementIConfig(Type configType)
    {
        // Arrange & Act
        var implementsIConfig = typeof(IConfig).IsAssignableFrom(configType);
        
        // Assert
        implementsIConfig.Should().BeTrue($"because {configType.Name} should implement IConfig for automatic registration");
    }
}