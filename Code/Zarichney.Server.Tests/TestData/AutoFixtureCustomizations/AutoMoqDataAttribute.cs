using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoFixture.Kernel;
using Zarichney.Config;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Tests.TestData.Builders;
using System.Net.Http.Headers;
using Stripe;
using Stripe.Checkout;
using Microsoft.Graph;
using Moq;
using Zarichney.Services.Email;
using Zarichney.Services.Payment;
using Microsoft.Extensions.Logging;
using Zarichney.Server.Tests.Framework.TestData.AutoFixtureCustomizations;

namespace Zarichney.Server.Tests.TestData.AutoFixtureCustomizations;

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
  /// Creates a configured Fixture with all necessary customizations for payment service testing.
  /// </summary>
  private static Fixture CreateFixture()
  {
    var fixture = new Fixture();

    // Enable AutoMoq for interface mocking
    fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });

    // Add service specific customizations
    fixture.Customize(new PaymentServiceCustomization());
    fixture.Customize(new EmailServiceCustomization());
    fixture.Customize(new HttpHeadersCustomization());
    fixture.Customize(new StripeObjectCustomization());
    fixture.Customize(new WebScraperCustomization());

    // Configure AutoFixture to avoid infinite recursion with EF Core entities
    fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
        .ForEach(b => fixture.Behaviors.Remove(b));
    fixture.Behaviors.Add(new OmitOnRecursionBehavior());

    return fixture;
  }
}

/// <summary>
/// AutoFixture customization for payment service dependencies.
/// Provides proper PaymentConfig and ClientConfig instances for PaymentService testing,
/// ensuring webhook secrets and other configurations are properly available.
/// </summary>
public class PaymentServiceCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // Create PaymentConfig using the builder for consistent test data
    // Include all required configuration for payment service functionality
    fixture.Register(() => new PaymentConfigBuilder().Build());

    // Create ClientConfig with valid test data
    fixture.Register(() => new ClientConfig
    {
      BaseUrl = "https://test.example.com"
    });

    // Ensure ILogger<PaymentService> is properly handled by AutoMoq
    // This explicit registration ensures proper dependency injection for PaymentService constructor
    fixture.Customize<ILogger<PaymentService>>(composer =>
        composer.FromFactory(() => Mock.Of<ILogger<PaymentService>>()));
  }
}

/// <summary>
/// AutoFixture customization for HTTP headers to prevent creation errors.
/// </summary>
public class HttpHeadersCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // HttpResponseHeaders cannot be created directly, so we omit it
    fixture.Customize<HttpResponseHeaders>(c => c.OmitAutoProperties());

    // For StripeResponse, we'll handle it in the Stripe customization
    fixture.Register<HttpResponseHeaders>(() => null!);
  }
}

/// <summary>
/// AutoFixture customization for Stripe SDK objects.
/// </summary>
public class StripeObjectCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // Create concrete Session objects instead of trying to mock interfaces
    fixture.Register(() => new Session
    {
      Id = "cs_test_" + fixture.Create<string>()[..20],
      Status = "complete",
      CustomerEmail = fixture.Create<string>() + "@test.com",
      AmountTotal = fixture.Create<int>() % 10000 + 1000, // Between $10-$100
      Currency = "usd",
      PaymentStatus = "paid",
      Metadata = new Dictionary<string, string>
      {
        ["payment_type"] = "OrderCompletion",
        ["customer_email"] = fixture.Create<string>() + "@test.com"
      }
    });

    // Create concrete Event objects
    fixture.Register(() => new Event
    {
      Id = "evt_test_" + fixture.Create<string>()[..20],
      Type = "checkout.session.completed",
      Data = new EventData
      {
        Object = fixture.Create<Session>()
      }
    });
  }
}

/// <summary>
/// AutoFixture customization for EmailService dependencies.
/// Handles the complex GraphServiceClient mocking issue by providing proper mock instances.
/// </summary>
public class EmailServiceCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // Create EmailConfig using a builder for consistent test data
    fixture.Register(() => new EmailConfig
    {
      AzureTenantId = "test-tenant-id",
      AzureAppId = "test-app-id",
      AzureAppSecret = "test-app-secret",
      FromEmail = "test@zarichney.com",
      TemplateDirectory = "/test/templates",
      MailCheckApiKey = "test-mailcheck-api-key"
    });

    // GraphServiceClient requires complex constructor parameters and cannot be mocked directly
    // Use the same pattern as GraphServiceClientProxy - construct with a shared HttpClient
    fixture.Register<GraphServiceClient>(() =>
    {
      var httpClient = new HttpClient();
      return new GraphServiceClient(httpClient);
    });

    // ITemplateService: Let AutoMoq handle this automatically to allow test-specific setup
    // The tests can override the default behavior as needed

    // IMailCheckClient: Let AutoMoq handle this automatically to allow test-specific setup
    // The EmailValidationResponseBuilder will be available for individual tests to use
  }
}
