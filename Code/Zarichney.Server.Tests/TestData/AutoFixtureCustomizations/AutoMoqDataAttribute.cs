using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoFixture.Kernel;
using Zarichney.Server.Tests.Framework.TestData.AutoFixtureCustomizations;

namespace Zarichney.Tests.TestData.AutoFixtureCustomizations;

/// <summary>
/// Custom AutoData attribute that includes AutoMoq customization and enhanced object creation for service testing.
/// This enables automatic creation of Mock{T} instances for interface dependencies and proper handling of
/// complex types like PaymentConfig, ClientConfig, EmailConfig, GraphServiceClient, and Stripe SDK objects.
/// </summary>
/// <remarks>
/// Use this attribute instead of [AutoData] when your test methods require automatic mocking of interface dependencies.
/// This is particularly useful for services with complex dependency injection requirements.
///
/// Enhanced features:
/// - Automatic mocking of all interfaces
/// - PaymentConfig creation with valid test data
/// - ClientConfig creation with valid test data
/// - EmailConfig creation with valid test data
/// - GraphServiceClient mocking for Microsoft Graph API testing
/// - ITemplateService and IMailCheckClient mocking for EmailService testing
/// - Stripe SDK object handling with concrete implementations
/// - HTTP Headers customization to avoid creation issues
///
/// Example usage:
/// <code>
/// [Theory, AutoMoqData]
/// public async Task MyTest(
///     [Frozen] Mock{IMyService} mockService,
///     PaymentService sut)
/// {
///     // Test implementation with automatically mocked dependencies
/// }
/// </code>
/// </remarks>
public class AutoMoqDataAttribute : AutoDataAttribute
{
  /// <summary>
  /// Initializes a new instance of the <see cref="AutoMoqDataAttribute"/> class
  /// with AutoMoq customization and enhanced object creation.
  /// </summary>
  public AutoMoqDataAttribute() : base(() => CreateFixture())
  {
  }

  /// <summary>
  /// Creates a configured Fixture with all necessary customizations for comprehensive service testing.
  /// Applies focused customizations for different service domains and infrastructure concerns.
  /// </summary>
  private static Fixture CreateFixture()
  {
    var fixture = new Fixture();

    // Enable AutoMoq for interface mocking
    fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });

    // Apply focused customizations for different concerns
    fixture.Customize(new ConfigurationCustomization());
    fixture.Customize(new LoggerCustomization());
    fixture.Customize(new HttpContextCustomization());
    fixture.Customize(new GraphServiceCustomization());
    fixture.Customize(new StripeObjectCustomization());
    fixture.Customize(new WebScraperCustomization());

    // Configure AutoFixture to avoid infinite recursion with EF Core entities
    var behaviorsToRemove = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList();
    foreach (var behavior in behaviorsToRemove)
    {
      fixture.Behaviors.Remove(behavior);
    }
    fixture.Behaviors.Add(new OmitOnRecursionBehavior());

    return fixture;
  }
}
