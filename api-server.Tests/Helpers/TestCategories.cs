namespace Zarichney.Tests.Helpers;

/// <summary>
/// Constants for test categories and traits.
/// These are used to categorize tests and declare dependencies.
/// 
/// Usage:
/// - Use [Trait(TestCategories.Category, TestCategories.Unit)] to mark a test as a unit test
/// - Use [Trait(TestCategories.Feature, TestCategories.Auth)] to associate with a feature area
/// - Use [Trait(TestCategories.Dependency, TestCategories.Database)] to declare dependencies
/// 
/// For dependency-aware tests, use:
/// - [DependencyFact] instead of [Fact] - this will properly skip when dependencies are missing
/// </summary>
public static class TestCategories
{
    // Trait names
    public const string Category = "Category";
    public const string Feature = "Feature";
    public const string Dependency = "Dependency";
    
    // Categories
    public const string Unit = "Unit";
    public const string Integration = "Integration";
    public const string E2E = "E2E";
    public const string Smoke = "Smoke";
    public const string Performance = "Performance";
    public const string Load = "Load";
    public const string MinimalFunctionality = "MinimalFunctionality";
    public const string Controller = "Controller";
    public const string Component = "Component";
    public const string Service = "Service";
    
    // Features
    public const string Auth = "Auth";
    public const string Cookbook = "Cookbook";
    public const string Payment = "Payment";
    public const string Email = "Email";
    public const string AI = "AI";
    
    // Dependencies
    public const string Database = "Database";
    public const string ExternalStripe = "ExternalStripe";
    public const string ExternalOpenAI = "ExternalOpenAI";
    public const string ExternalGitHub = "ExternalGitHub";
    public const string ExternalMSGraph = "ExternalMSGraph";
}