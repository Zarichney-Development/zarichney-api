using Moq;
using Zarichney.Services.Email;
using Microsoft.Graph.Models;

namespace Zarichney.Tests.Framework.Mocks.Factories;

/// <summary>
/// Factory for creating mock MS Graph service for email functionality.
/// </summary>
public class MockMSGraphServiceFactory : BaseMockFactory<IEmailService>
{
  /// <summary>
  /// Creates a mock MS Graph service with default implementations.
  /// </summary>
  /// <returns>A Mock of the IEmailService.</returns>
  public new static Mock<IEmailService> CreateMock()
  {
    var factory = new MockMSGraphServiceFactory();
    return factory.CreateDefaultMock();
  }

  /// <summary>
  /// Sets up default behaviors for the mock MS Graph service.
  /// </summary>
  /// <param name="mock">The mock to set up.</param>
  protected override void SetupDefaultMock(Mock<IEmailService> mock)
  {
    // Setup for SendEmail method
    mock.Setup(s => s.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Setup for ValidateEmail method
    mock.Setup(s => s.ValidateEmail(It.IsAny<string>()))
        .ReturnsAsync(true);

    // Setup for SendErrorNotification method
    mock.Setup(s => s.SendErrorNotification(
            It.IsAny<string>(),
            It.IsAny<Exception>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.CompletedTask);
  }
}
