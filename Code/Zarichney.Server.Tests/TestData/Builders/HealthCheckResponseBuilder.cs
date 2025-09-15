using Zarichney.Controllers.Responses;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for HealthCheckResponse - provides fluent API for creating test health check responses
/// </summary>
public class HealthCheckResponseBuilder
{
    private bool _success = true;
    private DateTime _time = DateTime.UtcNow;
    private string _environment = "Development";

    public HealthCheckResponseBuilder WithSuccess(bool success = true)
    {
        _success = success;
        return this;
    }

    public HealthCheckResponseBuilder WithFailure()
    {
        _success = false;
        return this;
    }

    public HealthCheckResponseBuilder WithTime(DateTime time)
    {
        _time = time;
        return this;
    }

    public HealthCheckResponseBuilder WithCurrentTime()
    {
        _time = DateTime.UtcNow;
        return this;
    }

    public HealthCheckResponseBuilder WithSpecificTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        _time = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        return this;
    }

    public HealthCheckResponseBuilder WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    public HealthCheckResponseBuilder WithDevelopmentEnvironment()
    {
        _environment = "Development";
        return this;
    }

    public HealthCheckResponseBuilder WithStagingEnvironment()
    {
        _environment = "Staging";
        return this;
    }

    public HealthCheckResponseBuilder WithProductionEnvironment()
    {
        _environment = "Production";
        return this;
    }

    public HealthCheckResponseBuilder WithTestingEnvironment()
    {
        _environment = "Testing";
        return this;
    }

    public HealthCheckResponseBuilder WithDefaults()
    {
        _success = true;
        _time = DateTime.UtcNow;
        _environment = "Development";
        return this;
    }

    public HealthCheckResponseBuilder WithHealthyProduction()
    {
        _success = true;
        _time = DateTime.UtcNow;
        _environment = "Production";
        return this;
    }

    public HealthCheckResponseBuilder WithUnhealthyProduction()
    {
        _success = false;
        _time = DateTime.UtcNow;
        _environment = "Production";
        return this;
    }

    public HealthCheckResponse Build()
    {
        return new HealthCheckResponse(_success, _time, _environment);
    }
}