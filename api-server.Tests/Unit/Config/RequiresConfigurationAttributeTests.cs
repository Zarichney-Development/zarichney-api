using FluentAssertions;
using Xunit;
using Zarichney.Config;

namespace Zarichney.Tests.Unit.Config;

public class RequiresConfigurationAttributeTests
{
    [Trait("Category", "Unit")]
    [Fact]
    public void Constructor_WithConfigurationKey_SetsConfigurationKeyProperty()
    {
        // Arrange
        var configKey = "Section:Key";

        // Act
        var attribute = new RequiresConfigurationAttribute(configKey);

        // Assert
        attribute.ConfigurationKey.Should().Be(configKey);
    }

    [Trait("Category", "Unit")]
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    // Make the parameter nullable to resolve the xUnit warning
    public void Constructor_WithInvalidConfigurationKey_ThrowsArgumentException(string? invalidKey)
    {
        // Arrange & Act
        // Add null-forgiving operator as the test expects non-null invalid strings too,
        // and the constructor checks for null/whitespace.
        var action = () => new RequiresConfigurationAttribute(invalidKey!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Configuration key cannot be null or whitespace*")
            .WithParameterName("configurationKey");
    }

    [Trait("Category", "Unit")]
    [Fact]
    public void AttributeUsage_ShouldBeApplicableOnProperties_WithInheritance()
    {
        // Arrange & Act
        var attributeUsage = typeof(RequiresConfigurationAttribute).GetCustomAttributes(typeof(AttributeUsageAttribute), true)
            .OfType<AttributeUsageAttribute>()
            .FirstOrDefault();

        // Assert
        attributeUsage.Should().NotBeNull();
        attributeUsage.ValidOn.Should().Be(AttributeTargets.Property);
        attributeUsage.Inherited.Should().BeTrue();
        attributeUsage.AllowMultiple.Should().BeFalse();
    }
}
