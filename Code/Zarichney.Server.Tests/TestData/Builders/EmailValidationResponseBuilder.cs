using Zarichney.Services.Email;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating EmailValidationResponse test data.
/// Provides fluent interface for creating valid and invalid email validation scenarios.
/// </summary>
public class EmailValidationResponseBuilder
{
  private readonly EmailValidationResponse _entity;

  public EmailValidationResponseBuilder()
  {
    // Create instance with required members
    _entity = new EmailValidationResponse
    {
      Domain = "example.com",
      Text = "Email is valid",
      Reason = "valid",
      MxHost = "mail.example.com",
      PossibleTypo = Array.Empty<string>(),
      MxIp = "192.168.1.1",
      MxInfo = "Valid MX record"
    };

    // Set up valid defaults
    WithValidDefaults();
  }

  /// <summary>
  /// Creates a valid email validation response with default safe values.
  /// </summary>
  public EmailValidationResponseBuilder WithValidDefaults()
  {
    _entity.Valid = true;
    _entity.Block = false;
    _entity.Disposable = false;
    _entity.EmailForwarder = false;
    _entity.Domain = "example.com";
    _entity.Text = "Email is valid";
    _entity.Reason = "valid";
    _entity.Risk = 10; // Low risk
    _entity.MxHost = "mail.example.com";
    _entity.PossibleTypo = Array.Empty<string>();
    _entity.MxIp = "192.168.1.1";
    _entity.MxInfo = "Valid MX record";
    _entity.LastChangedAt = DateTime.UtcNow.AddDays(-30);
    return this;
  }

  // Aliases for fluent readability - more intention-revealing method names
  public EmailValidationResponseBuilder AsValidEmail() => WithValidDefaults();
  public EmailValidationResponseBuilder Valid() => WithValidDefaults(); // Backward compatibility

  /// <summary>
  /// Creates an invalid email validation response.
  /// </summary>
  public EmailValidationResponseBuilder WithInvalidEmail()
  {
    _entity.Valid = false;
    _entity.Reason = "syntax error";
    _entity.Text = "Invalid email syntax";
    _entity.Risk = 95;
    return this;
  }

  public EmailValidationResponseBuilder AsInvalidEmail() => WithInvalidEmail();
  public EmailValidationResponseBuilder Invalid() => WithInvalidEmail(); // Backward compatibility

  /// <summary>
  /// Creates a blocked email validation response.
  /// </summary>
  public EmailValidationResponseBuilder WithBlockedEmail()
  {
    _entity.Valid = true;
    _entity.Block = true;
    _entity.Reason = "blocked domain";
    _entity.Text = "Domain is blocked";
    _entity.Risk = 90;
    return this;
  }

  public EmailValidationResponseBuilder AsBlockedEmail() => WithBlockedEmail();
  public EmailValidationResponseBuilder Blocked() => WithBlockedEmail(); // Backward compatibility

  /// <summary>
  /// Creates a disposable email validation response.
  /// </summary>
  public EmailValidationResponseBuilder WithDisposableEmail()
  {
    _entity.Valid = true;
    _entity.Disposable = true;
    _entity.Domain = "tempmail.org";
    _entity.Reason = "disposable email";
    _entity.Text = "Disposable email detected";
    _entity.Risk = 85;
    return this;
  }

  public EmailValidationResponseBuilder AsDisposableEmail() => WithDisposableEmail();
  public EmailValidationResponseBuilder Disposable() => WithDisposableEmail(); // Backward compatibility

  /// <summary>
  /// Creates a high-risk email validation response.
  /// </summary>
  public EmailValidationResponseBuilder WithHighRiskEmail(int riskScore = 80)
  {
    _entity.Valid = true;
    _entity.Risk = riskScore;
    _entity.Reason = "high risk domain";
    _entity.Text = $"High risk email (score: {riskScore})";
    return this;
  }

  public EmailValidationResponseBuilder AsHighRiskEmail(int riskScore = 80) => WithHighRiskEmail(riskScore);
  public EmailValidationResponseBuilder HighRisk(int riskScore = 80) => WithHighRiskEmail(riskScore); // Backward compatibility

  /// <summary>
  /// Creates an email with possible typo suggestions.
  /// </summary>
  public EmailValidationResponseBuilder WithPossibleTypo(params string[] typoSuggestions)
  {
    _entity.Valid = false;
    _entity.PossibleTypo = typoSuggestions;
    _entity.Reason = "possible typo detected";
    _entity.Text = "Possible typo in email";
    _entity.Risk = 60;
    return this;
  }

  public EmailValidationResponseBuilder WithTypoSuggestions(params string[] typoSuggestions) => WithPossibleTypo(typoSuggestions);
  public EmailValidationResponseBuilder WithTypos(params string[] typoSuggestions) => WithPossibleTypo(typoSuggestions); // Backward compatibility

  public EmailValidationResponseBuilder WithDomain(string domain)
  {
    _entity.Domain = domain;
    return this;
  }

  public EmailValidationResponseBuilder WithRisk(int risk)
  {
    _entity.Risk = risk;
    return this;
  }

  public EmailValidationResponseBuilder WithReason(string reason)
  {
    _entity.Reason = reason;
    return this;
  }

  public EmailValidationResponseBuilder WithMxHost(string mxHost)
  {
    _entity.MxHost = mxHost;
    return this;
  }

  /// <summary>
  /// Builds and returns the configured EmailValidationResponse.
  /// </summary>
  public EmailValidationResponse Build() => _entity;
}
