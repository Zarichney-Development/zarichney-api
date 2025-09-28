using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;
using Zarichney.Config;

namespace Zarichney.Tests.Unit.Config;

public class ConfigurationExtensionsTests
{
  [Trait("Category", "Unit")]
  [Fact]
  public void GetService_WithRegisteredService_ReturnsService()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddSingleton<ITestService, TestService>();

    // Act
    var result = services.GetService<ITestService>();

    // Assert
    result.Should().NotBeNull("because the service should be resolved from the service collection");
    result.Should().BeOfType<TestService>("because TestService was registered as the implementation");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void GetService_WithUnregisteredService_ThrowsInvalidOperationException()
  {
    // Arrange
    var services = new ServiceCollection();

    // Act
    Action act = () => services.GetService<IUnregisteredService>();

    // Assert
    act.Should().Throw<InvalidOperationException>("because the service is not registered in the service collection")
        .WithMessage("*IUnregisteredService*", "because the exception should indicate which service could not be resolved");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void GetService_WithMultipleRegistrations_ReturnsLastRegistration()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddSingleton<ITestService, TestService>();
    services.AddSingleton<ITestService, AlternateTestService>();

    // Act
    var result = services.GetService<ITestService>();

    // Assert
    result.Should().NotBeNull("because the service should be resolved from the service collection");
    result.Should().BeOfType<AlternateTestService>("because the last registration should take precedence");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void GetService_WithTransientService_ReturnsNewInstance()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddTransient<ITestService, TestService>();

    // Act
    var result1 = services.GetService<ITestService>();
    var result2 = services.GetService<ITestService>();

    // Assert
    result1.Should().NotBeNull("because the service should be resolved");
    result2.Should().NotBeNull("because the service should be resolved");
    result1.Should().NotBeSameAs(result2, "because transient services should return new instances each time");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void GetService_WithSingletonService_CreatesSeparateServiceProviders()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddSingleton<ITestService, TestService>();

    // Act
    var result1 = services.GetService<ITestService>();
    var result2 = services.GetService<ITestService>();

    // Assert
    result1.Should().NotBeNull("because the service should be resolved");
    result2.Should().NotBeNull("because the service should be resolved");
    result1.Should().NotBeSameAs(result2, "because GetService creates a new service provider each time, even for singletons");
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void GetService_WithScopedService_CreatesSeparateServiceProviders()
  {
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<ITestService, TestService>();

    // Act
    var result1 = services.GetService<ITestService>();
    var result2 = services.GetService<ITestService>();

    // Assert
    result1.Should().NotBeNull("because the service should be resolved");
    result2.Should().NotBeNull("because the service should be resolved");
    result1.Should().NotBeSameAs(result2, "because GetService creates a new service provider each time, creating separate scopes");
  }

  // Test helper interfaces and classes
  private interface ITestService { }
  private interface IUnregisteredService { }

  private class TestService : ITestService { }
  private class AlternateTestService : ITestService { }
}
