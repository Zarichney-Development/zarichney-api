using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Zarichney.Config;

namespace Zarichney.Services.Email;

public class EmailConfig : IConfig
{
  [Required]
  [RequiresConfiguration("EmailConfig:AzureTenantId")]
  public required string AzureTenantId { get; init; }

  [Required]
  [RequiresConfiguration("EmailConfig:AzureAppId")]
  public required string AzureAppId { get; init; }

  [Required]
  [RequiresConfiguration("EmailConfig:AzureAppSecret")]
  public required string AzureAppSecret { get; init; }

  [Required]
  [RequiresConfiguration("EmailConfig:FromEmail")]
  public required string FromEmail { get; init; }

  public string TemplateDirectory { get; init; } = "/Services/Email/Templates";

  [Required]
  [RequiresConfiguration("EmailConfig:MailCheckApiKey")]
  public required string MailCheckApiKey { get; init; }
}
public enum InvalidEmailReason
{
  InvalidSyntax,
  PossibleTypo,
  InvalidDomain,
  DisposableEmail
}

public class EmailValidationResponse
{
  [JsonPropertyName("valid")] public bool Valid { get; set; }
  [JsonPropertyName("block")] public bool Block { get; set; }
  [JsonPropertyName("disposable")] public bool Disposable { get; set; }
  [JsonPropertyName("email_forwarder")] public bool EmailForwarder { get; set; }
  [JsonPropertyName("domain")] public required string Domain { get; set; }
  [JsonPropertyName("text")] public required string Text { get; set; }
  [JsonPropertyName("reason")] public required string Reason { get; set; }
  [JsonPropertyName("risk")] public int Risk { get; set; }
  [JsonPropertyName("mx_host")] public required string MxHost { get; set; }
  [JsonPropertyName("possible_typo")] public required string[] PossibleTypo { get; set; }
  [JsonPropertyName("mx_ip")] public required string MxIp { get; set; }
  [JsonPropertyName("mx_info")] public required string MxInfo { get; set; }
  [JsonPropertyName("last_changed_at")] public DateTime LastChangedAt { get; set; }
}

public class InvalidEmailException(string message, string email, InvalidEmailReason reason) : Exception(message)
{
  public string Email { get; } = email;
  public InvalidEmailReason Reason { get; } = reason;
}
