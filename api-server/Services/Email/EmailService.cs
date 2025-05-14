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
}

public class EmailService(
  GraphServiceClient graphClient,
  EmailConfig config,
  ITemplateService templateService,
  IMailCheckClient mailCheckClient,
  ILogger<EmailService> logger)
  : IEmailService
{
  private const int HighRiskThreshold = 70;
  
  public async Task SendEmail(string recipient, string subject, string templateName,
    Dictionary<string, object>? templateData = null, FileAttachment? attachment = null)
  {
    // ServiceUnavailableException will be thrown by the proxy if Email service is unavailable
    ArgumentNullException.ThrowIfNull(graphClient, nameof(graphClient));

    string bodyContent;
    try
    {
      bodyContent = await templateService.ApplyTemplate(
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
      logger.LogError(e, "Error applying email template: {TemplateName} for recipient {Recipient}", templateName,
        recipient);
      throw;
    }

    var message = new Message
    {
      Subject = subject,
      From = new Recipient { EmailAddress = new EmailAddress { Address = config.FromEmail } },
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
      SaveToSentItems = message.ToRecipients.Any(r => r.EmailAddress!.Address != config.FromEmail)
    };

    try
    {
      logger.LogInformation("Attempting to send email with configuration: {@EmailDetails}", new
      {
        config.FromEmail,
        ToEmail = recipient,
        Subject = subject,
        HasContent = !string.IsNullOrEmpty(message.Body.Content),
        ContentLength = message.Body.Content?.Length,
        AttachmentSize = attachment?.Size,
        config.AzureAppId
      });

      var emailAccount = graphClient.Users[config.FromEmail];

      await emailAccount.SendMail.PostAsync(requestBody);

      logger.LogInformation("Email sent successfully. Request details: {@RequestDetails}", new
      {
        config.FromEmail,
        ToEmail = recipient,
        Subject = subject,
        MessageId = message.Id
      });
    }
    catch (Exception e)
    {
      logger.LogError(e, "Error sending email. Request details: {@RequestDetails}", new
      {
        config.FromEmail,
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
    
    var result = await mailCheckClient.GetValidationData(domain);
    
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
      logger.LogError(e, "Invalid email detected: {@Result}", result);
      throw;
    }

    return true;
  }

  private void ThrowInvalidEmailException(string message, string email, InvalidEmailReason reason)
  {
    logger.LogWarning("{Message}: {Email} ({Reason})", message, email, reason);
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
}
