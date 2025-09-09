using Microsoft.Graph;
using Microsoft.Graph.Models;
using Zarichney.Config;
using SendMailPostRequestBody = Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody;

namespace Zarichney.Services.Email;

public interface IEmailService
{
  Task SendEmail(string recipient, string subject, string templateName,
    Dictionary<string, object>? templateData = null, FileAttachment? attachment = null);

  Task<bool> ValidateEmail(string email);

  /// <summary>
  /// Sends an error notification email with details about an exception.
  /// </summary>
  /// <param name="stage">The processing stage where the error occurred.</param>
  /// <param name="ex">The exception that was thrown.</param>
  /// <param name="serviceName">The name of the service reporting the error.</param>
  /// <param name="additionalContext">Optional dictionary with additional contextual information about the error.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task SendErrorNotification(string stage, Exception ex, string serviceName, Dictionary<string, string>? additionalContext = null);
}

public class EmailService : IEmailService
{
  private readonly GraphServiceClient _graphClient;
  private readonly EmailConfig _config;
  private readonly ITemplateService _templateService;
  private readonly IMailCheckClient _mailCheckClient;
  private readonly ILogger<EmailService> _logger;

  public EmailService(
      GraphServiceClient graphClient,
      EmailConfig config,
      ITemplateService templateService,
      IMailCheckClient mailCheckClient,
      ILogger<EmailService> logger)
  {
    _graphClient = graphClient ?? throw new ArgumentNullException(nameof(graphClient));
    _config = config ?? throw new ArgumentNullException(nameof(config));
    _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
    _mailCheckClient = mailCheckClient ?? throw new ArgumentNullException(nameof(mailCheckClient));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }
  private const int HighRiskThreshold = 70;

  public async Task SendEmail(string recipient, string subject, string templateName,
    Dictionary<string, object>? templateData = null, FileAttachment? attachment = null)
  {
    // ServiceUnavailableException will be thrown by the proxy if Email service is unavailable
    ArgumentNullException.ThrowIfNull(_graphClient, nameof(_graphClient));

    string bodyContent;
    try
    {
      bodyContent = await _templateService.ApplyTemplate(
        templateName,
        templateData ?? new Dictionary<string, object>(),
        subject
      );

      if (string.IsNullOrEmpty(bodyContent))
      {
        throw new Exception("Email body content is empty");
      }
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Error applying email template: {TemplateName} for recipient {Recipient}", templateName,
        recipient);
      throw;
    }

    var message = new Message
    {
      Subject = subject,
      From = new Recipient { EmailAddress = new EmailAddress { Address = _config.FromEmail } },
      Body = new ItemBody { ContentType = BodyType.Html, Content = bodyContent },
      ToRecipients = [new Recipient { EmailAddress = new EmailAddress { Address = recipient } }],
      Attachments = []
    };

    if (attachment != null)
    {
      message.Attachments.Add(attachment);
    }

    var requestBody = new SendMailPostRequestBody
    {
      Message = message,
      // Only save in sent folder for non-self-sent emails (the ones as notification for debugging purposes)
      SaveToSentItems = message.ToRecipients.Any(r => r.EmailAddress!.Address != _config.FromEmail)
    };

    try
    {
      _logger.LogInformation("Attempting to send email with configuration: {@EmailDetails}", new
      {
        _config.FromEmail,
        ToEmail = recipient,
        Subject = subject,
        HasContent = !string.IsNullOrEmpty(message.Body.Content),
        ContentLength = message.Body.Content?.Length,
        AttachmentSize = attachment?.Size,
        _config.AzureAppId
      });

      var emailAccount = _graphClient.Users[_config.FromEmail];

      await emailAccount.SendMail.PostAsync(requestBody);

      _logger.LogInformation("Email sent successfully. Request details: {@RequestDetails}", new
      {
        _config.FromEmail,
        ToEmail = recipient,
        Subject = subject,
        MessageId = message.Id
      });
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Error sending email. Request details: {@RequestDetails}", new
      {
        _config.FromEmail,
        ToEmail = recipient,
        Subject = subject,
        MessageId = message.Id,
        ErrorType = e.GetType().Name,
        ErrorMessage = e.Message,
        InnerError = e.InnerException?.Message
      });
      throw;
    }
  }

  /// <summary>
  /// Uses the MailCheck API to validate an email address and checks the result against validation criteria.
  /// </summary>
  /// <param name="email">The email address to validate</param>
  /// <returns>True if the email is valid, otherwise an exception is thrown</returns>
  /// <exception cref="InvalidOperationException">Thrown when the API returns unexpected results</exception>
  /// <exception cref="ConfigurationMissingException">Thrown when the MailCheck API key is missing or invalid</exception>
  /// <exception cref="InvalidEmailException">Thrown when the email fails validation criteria</exception>
  public async Task<bool> ValidateEmail(string email)
  {
    var domain = email.Split('@').Last();

    var result = await _mailCheckClient.GetValidationData(domain);

    return ValidateWithResult(email, result);
  }

  private bool ValidateWithResult(string email, EmailValidationResponse result)
  {
    try
    {
      if (!result.Valid)
      {
        ThrowInvalidEmailException("Invalid email", email, DetermineInvalidReason(result));
      }

      if (result.Block)
      {
        ThrowInvalidEmailException("Blocked email detected", email, InvalidEmailReason.InvalidDomain);
      }

      if (result.Disposable)
      {
        ThrowInvalidEmailException("Disposable email detected", email, InvalidEmailReason.DisposableEmail);
      }

      if (result.Risk > HighRiskThreshold)
      {
        ThrowInvalidEmailException($"High risk email detected. Risk score: {result.Risk}", email,
          InvalidEmailReason.InvalidDomain);
      }
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Invalid email detected: {@Result}", result);
      throw;
    }

    return true;
  }

  private void ThrowInvalidEmailException(string message, string email, InvalidEmailReason reason)
  {
    _logger.LogWarning("{Message}: {Email} ({Reason})", message, email, reason);
    throw new InvalidEmailException(message, email, reason);
  }

  private static InvalidEmailReason DetermineInvalidReason(EmailValidationResponse result)
    => result.Reason.ToLower() switch
    {
      { } reason when reason.Contains("syntax") => InvalidEmailReason.InvalidSyntax,
      not null when result.PossibleTypo.Length > 0 => InvalidEmailReason.PossibleTypo,
      { } reason when reason.Contains("domain") => InvalidEmailReason.InvalidDomain,
      _ => InvalidEmailReason.InvalidDomain
    };

  public static string MakeSafeFileName(string email)
    => email
      .Replace("@", "_at_")
      .Replace(".", "_dot_")
      .Replace(" ", "_")
      .Replace(":", "_")
      .Replace(";", "_");

  /// <summary>
  /// Sends an error notification email with details about an exception.
  /// </summary>
  /// <param name="stage">The processing stage where the error occurred.</param>
  /// <param name="ex">The exception that was thrown.</param>
  /// <param name="serviceName">The name of the service reporting the error.</param>
  /// <param name="additionalContext">Optional dictionary with additional contextual information about the error.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  public async Task SendErrorNotification(string stage, Exception ex, string serviceName, Dictionary<string, string>? additionalContext = null)
  {
    try // Prevent email sending from crashing the calling code
    {
      var templateData = TemplateService.GetErrorTemplateData(ex);
      templateData["stage"] = stage;
      templateData["serviceName"] = serviceName;

      // Add additional context if provided
      if (additionalContext != null)
      {
        // Ensure additionalContext exists in templateData before adding to it
        if (!templateData.TryGetValue("additionalContext", out var value) ||
            value is not Dictionary<string, string> existingContext)
        {
          existingContext = new Dictionary<string, string>();
          templateData["additionalContext"] = existingContext;
        }

        // Add all provided context items
        foreach (var item in additionalContext)
        {
          ((Dictionary<string, string>)templateData["additionalContext"])[item.Key] = item.Value;
        }
      }

      await SendEmail(
        _config.FromEmail, // Send to the configured From email (self notification)
        $"{serviceName} Error - {stage}",
        "error-log", // Template name
        templateData
      );

      _logger.LogInformation("Error notification email sent for service: {ServiceName}, stage: {Stage}", serviceName, stage);
    }
    catch (Exception emailEx)
    {
      _logger.LogError(emailEx, "Failed to send error notification email for service: {ServiceName}, stage: {Stage}",
        serviceName, stage);
      // Intentionally swallow the exception to prevent it from bubbling up
    }
  }
}
