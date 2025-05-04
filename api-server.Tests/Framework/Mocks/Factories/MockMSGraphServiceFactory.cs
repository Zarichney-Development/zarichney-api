using Moq;

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
    // Default implementation for sending emails
    mock.Setup(s => s.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>()))
        .ReturnsAsync(true);

    // Default implementation for sending HTML emails
    mock.Setup(s => s.SendHtmlEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>()))
        .ReturnsAsync(true);

    // Default implementation for sending emails with attachments
    mock.Setup(s => s.SendEmailWithAttachmentsAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<IEnumerable<EmailAttachment>?>()))
        .ReturnsAsync(true);
  }
}

/// <summary>
/// Interface for email service.
/// This would typically be defined in the actual service project.
/// </summary>
public interface IEmailService
{
  Task<bool> SendEmailAsync(
      string to,
      string subject,
      string body,
      string? from = null);

  Task<bool> SendHtmlEmailAsync(
      string to,
      string subject,
      string htmlBody,
      string? from = null);

  Task<bool> SendEmailWithAttachmentsAsync(
      string to,
      string subject,
      string body,
      string? from = null,
      IEnumerable<EmailAttachment>? attachments = null);
}

/// <summary>
/// Represents an email attachment.
/// </summary>
public class EmailAttachment
{
  public string Name { get; set; } = string.Empty;
  public byte[] Content { get; set; } = Array.Empty<byte>();
  public string ContentType { get; set; } = "application/octet-stream";
}
