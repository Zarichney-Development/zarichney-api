using Zarichney.Services.Email;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating EmailValidationResponse test data objects.
/// Provides a fluent interface for setting up email validation response scenarios.
/// </summary>
public class EmailValidationResponseBuilder
{
    private readonly EmailValidationResponse _entity;

    public EmailValidationResponseBuilder()
    {
        _entity = new EmailValidationResponse
        {
            Domain = "example.com",
            Text = "Default text",
            Reason = "Default reason",
            MxHost = "mx.example.com",
            PossibleTypo = Array.Empty<string>(),
            MxIp = "192.168.1.1",
            MxInfo = "Default MX info"
        };
        WithDefaults();
    }

    /// <summary>
    /// Sets default values for a valid email validation response.
    /// </summary>
    public EmailValidationResponseBuilder WithDefaults()
    {
        _entity.Valid = true;
        _entity.Block = false;
        _entity.Disposable = false;
        _entity.EmailForwarder = false;
        _entity.Domain = "example.com";
        _entity.Text = "Valid email domain";
        _entity.Reason = "Valid domain with good reputation";
        _entity.Risk = 10;
        _entity.MxHost = "mx.example.com";
        _entity.PossibleTypo = Array.Empty<string>();
        _entity.MxIp = "192.168.1.1";
        _entity.MxInfo = "Valid MX record";
        _entity.LastChangedAt = DateTime.UtcNow.AddDays(-30);
        
        return this;
    }

    /// <summary>
    /// Builds and returns the configured EmailValidationResponse.
    /// </summary>
    public EmailValidationResponse Build() => _entity;

    /// <summary>
    /// Configures the response as valid email.
    /// </summary>
    public EmailValidationResponseBuilder AsValid()
    {
        _entity.Valid = true;
        _entity.Block = false;
        _entity.Disposable = false;
        _entity.Risk = 5;
        _entity.Text = "Valid email domain";
        _entity.Reason = "Valid domain";
        
        return this;
    }

    /// <summary>
    /// Configures the response as invalid email.
    /// </summary>
    public EmailValidationResponseBuilder AsInvalid(string reason = "Invalid domain")
    {
        _entity.Valid = false;
        _entity.Text = "Invalid email domain";
        _entity.Reason = reason;
        _entity.Risk = 90;
        
        return this;
    }

    /// <summary>
    /// Configures the response as blocked email.
    /// </summary>
    public EmailValidationResponseBuilder AsBlocked()
    {
        _entity.Valid = true;
        _entity.Block = true;
        _entity.Text = "Blocked domain";
        _entity.Reason = "Domain is on block list";
        _entity.Risk = 95;
        
        return this;
    }

    /// <summary>
    /// Configures the response as disposable email.
    /// </summary>
    public EmailValidationResponseBuilder AsDisposable()
    {
        _entity.Valid = true;
        _entity.Block = false;
        _entity.Disposable = true;
        _entity.Domain = "10minutemail.com";
        _entity.Text = "Disposable email provider";
        _entity.Reason = "Temporary email service";
        _entity.Risk = 80;
        
        return this;
    }

    /// <summary>
    /// Configures the response as high risk email.
    /// </summary>
    public EmailValidationResponseBuilder AsHighRisk(int riskScore = 85)
    {
        _entity.Valid = true;
        _entity.Block = false;
        _entity.Disposable = false;
        _entity.Risk = riskScore;
        _entity.Text = "High risk domain";
        _entity.Reason = "Domain has poor reputation";
        
        return this;
    }

    /// <summary>
    /// Configures the response with possible typo suggestions.
    /// </summary>
    public EmailValidationResponseBuilder WithPossibleTypos(params string[] typos)
    {
        _entity.PossibleTypo = typos;
        if (typos.Length > 0)
        {
            _entity.Reason = "Possible typo detected";
        }
        
        return this;
    }

    /// <summary>
    /// Sets the domain for the validation response.
    /// </summary>
    public EmailValidationResponseBuilder WithDomain(string domain)
    {
        _entity.Domain = domain;
        _entity.MxHost = $"mx.{domain}";
        
        return this;
    }

    /// <summary>
    /// Sets the risk score for the validation response.
    /// </summary>
    public EmailValidationResponseBuilder WithRiskScore(int risk)
    {
        _entity.Risk = risk;
        
        return this;
    }

    /// <summary>
    /// Sets the MX host information.
    /// </summary>
    public EmailValidationResponseBuilder WithMxHost(string mxHost, string mxIp = "192.168.1.1", string mxInfo = "Valid MX")
    {
        _entity.MxHost = mxHost;
        _entity.MxIp = mxIp;
        _entity.MxInfo = mxInfo;
        
        return this;
    }

    /// <summary>
    /// Sets when the domain was last changed.
    /// </summary>
    public EmailValidationResponseBuilder WithLastChanged(DateTime lastChanged)
    {
        _entity.LastChangedAt = lastChanged;
        
        return this;
    }

    /// <summary>
    /// Configures the response for syntax error scenarios.
    /// </summary>
    public EmailValidationResponseBuilder WithSyntaxError()
    {
        _entity.Valid = false;
        _entity.Text = "Invalid syntax";
        _entity.Reason = "email syntax is invalid";
        _entity.Risk = 100;
        
        return this;
    }

    /// <summary>
    /// Configures the response for domain-related issues.
    /// </summary>
    public EmailValidationResponseBuilder WithDomainIssue(string issue = "domain not found")
    {
        _entity.Valid = false;
        _entity.Text = "Domain issue";
        _entity.Reason = $"domain {issue}";
        _entity.Risk = 95;
        _entity.MxHost = "";
        _entity.MxIp = "";
        _entity.MxInfo = "";
        
        return this;
    }
}