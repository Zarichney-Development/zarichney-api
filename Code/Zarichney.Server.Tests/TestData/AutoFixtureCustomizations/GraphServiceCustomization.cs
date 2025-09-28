using AutoFixture;
using Microsoft.Graph;

namespace Zarichney.Tests.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for Microsoft Graph SDK objects.
/// Handles GraphServiceClient creation for EmailService testing scenarios.
/// </summary>
public class GraphServiceCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // GraphServiceClient requires complex constructor parameters and cannot be mocked directly
    // Use the same pattern as GraphServiceClientProxy - construct with a shared HttpClient
    fixture.Register<GraphServiceClient>(() =>
    {
      var httpClient = new HttpClient();
      return new GraphServiceClient(httpClient);
    });

    // Note: ITemplateService and IMailCheckClient are left to AutoMoq for automatic handling
    // This allows tests to override the default behavior as needed using [Frozen] Mock<T> parameters
    // The EmailValidationResponseBuilder will be available for individual tests to use
  }
}
