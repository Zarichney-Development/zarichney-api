using FluentAssertions;
using Xunit;
using Zarichney.TestingFramework.Attributes;

namespace Zarichney.TestingFramework.Tests.Unit.Attributes;

/// <summary>
/// Unit tests for TestCategories constants class.
/// Ensures consistency and availability of test category constants.
/// </summary>
public class TestCategoriesTests
{
    [Fact]
    public void TraitNames_ShouldHaveExpectedValues()
    {
        // Assert
        TestCategories.Category.Should().Be("Category");
        TestCategories.Feature.Should().Be("Feature");
        TestCategories.Dependency.Should().Be("Dependency");
        TestCategories.Mutability.Should().Be("Mutability");
    }

    [Fact]
    public void Categories_ShouldHaveExpectedValues()
    {
        // Assert
        TestCategories.Unit.Should().Be("Unit");
        TestCategories.Integration.Should().Be("Integration");
        TestCategories.SlowIntegration.Should().Be("SlowIntegration");
        TestCategories.E2E.Should().Be("E2E");
        TestCategories.Smoke.Should().Be("Smoke");
        TestCategories.Performance.Should().Be("Performance");
        TestCategories.Load.Should().Be("Load");
        TestCategories.MinimalFunctionality.Should().Be("MinimalFunctionality");
        TestCategories.Controller.Should().Be("Controller");
        TestCategories.Component.Should().Be("Component");
        TestCategories.Service.Should().Be("Service");
    }

    [Fact]
    public void Features_ShouldHaveExpectedValues()
    {
        // Assert
        TestCategories.Auth.Should().Be("Auth");
        TestCategories.Cookbook.Should().Be("Cookbook");
        TestCategories.Payment.Should().Be("Payment");
        TestCategories.Email.Should().Be("Email");
        TestCategories.AI.Should().Be("AI");
        TestCategories.Swagger.Should().Be("Swagger");
    }

    [Fact]
    public void Dependencies_ShouldHaveExpectedValues()
    {
        // Assert
        TestCategories.Database.Should().Be("Database");
        TestCategories.Docker.Should().Be("Docker");
        TestCategories.ExternalStripe.Should().Be("ExternalStripe");
        TestCategories.ExternalOpenAI.Should().Be("ExternalOpenAI");
        TestCategories.ExternalGitHub.Should().Be("ExternalGitHub");
        TestCategories.ExternalMSGraph.Should().Be("ExternalMSGraph");
        TestCategories.NoExternalDependencies.Should().Be("NoExternalDependencies");
    }

    [Fact]
    public void Mutability_ShouldHaveExpectedValues()
    {
        // Assert
        TestCategories.ReadOnly.Should().Be("ReadOnly");
        TestCategories.DataMutating.Should().Be("DataMutating");
    }

    [Fact]
    public void AllConstants_ShouldBeNonEmptyStrings()
    {
        // Get all public static string fields from TestCategories
        var constantFields = typeof(TestCategories)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string));

        // Assert
        foreach (var field in constantFields)
        {
            var value = field.GetValue(null) as string;
            value.Should().NotBeNullOrWhiteSpace($"Constant {field.Name} should not be null or whitespace");
        }
    }

    [Fact]
    public void Constants_ShouldNotContainDuplicateValues()
    {
        // Get all public static string fields from TestCategories
        var constantFields = typeof(TestCategories)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string));

        var values = constantFields.Select(f => f.GetValue(null) as string).ToList();

        // Assert - While some values might intentionally be the same (like trait names),
        // this test helps detect unintentional duplicates
        values.Should().OnlyHaveUniqueItems();
    }
}