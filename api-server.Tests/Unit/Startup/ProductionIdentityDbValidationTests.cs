using System.Reflection;
using FluentAssertions;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Startup;

namespace Zarichney.Tests.Unit.Startup;

/// <summary>
/// Tests the validation of the Identity Database connection string in the Production environment.
/// </summary>
[Trait("Category", "Unit")]
public class ProductionIdentityDbValidationTests
{
    /// <summary>
    /// This test verifies that the required UserDatabaseConnectionName constant exists in UserDbContext
    /// and has the expected value, as this is used in our validation logic.
    /// </summary>
    [Fact]
    public void UserDbContext_ShouldHave_UserDatabaseConnectionName_Constant()
    {
        // Arrange & Act
        var field = typeof(UserDbContext).GetField(
            "UserDatabaseConnectionName", 
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        
        // Assert
        field.Should().NotBeNull("UserDbContext should have a UserDatabaseConnectionName constant");
        
        var value = field!.GetValue(null) as string;
        value.Should().NotBeNull("UserDatabaseConnectionName should have a non-null value");
        value.Should().Be("UserDatabase", "UserDatabaseConnectionName should have the expected value");
    }
    
    /// <summary>
    /// This test verifies that ValidateStartup has the ValidateProductionConfiguration method
    /// that is used to validate the UserDatabase connection string in Production.
    /// </summary>
    [Fact]
    public void ValidateStartup_ShouldHave_ValidateProductionConfiguration_Method()
    {
        // Arrange & Act
        var method = typeof(ValidateStartup).GetMethod(
            "ValidateProductionConfiguration", 
            BindingFlags.Public | BindingFlags.Static);
        
        // Assert
        method.Should().NotBeNull("ValidateStartup should have a ValidateProductionConfiguration method");
        
        // The method should take a WebApplicationBuilder parameter
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1, "The method should have exactly one parameter");
        parameters[0].ParameterType.Name.Should().Be("WebApplicationBuilder", 
            "The parameter should be of type WebApplicationBuilder");
    }
}