namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Defines common infrastructure dependencies required for tests.
/// These represent system-level or environmental dependencies like Docker or Database
/// that are required for certain integration tests to run properly.
/// </summary>
public enum InfrastructureDependency
{
    /// <summary>
    /// Database infrastructure is required for the test.
    /// This typically means a PostgreSQL container running via Testcontainers.
    /// </summary>
    Database,
    
    /// <summary>
    /// Docker runtime is required for the test.
    /// This is a fundamental dependency for any test that uses containers.
    /// </summary>
    Docker
}