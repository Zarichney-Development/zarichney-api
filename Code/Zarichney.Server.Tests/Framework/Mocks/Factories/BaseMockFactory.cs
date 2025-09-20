using Moq;

namespace Zarichney.Server.Tests.Framework.Mocks.Factories;

/// <summary>
/// Base factory for creating mock services.
/// Provides common functionality for all mock factories.
/// </summary>
/// <typeparam name="T">The type of service to mock.</typeparam>
public abstract class BaseMockFactory<T> where T : class
{
  /// <summary>
  /// Creates a new mock of the service.
  /// </summary>
  /// <returns>A mock of the service.</returns>
  public static Mock<T> CreateMock()
  {
    return new Mock<T>();
  }

  /// <summary>
  /// Creates a new mock of the service with default setup.
  /// </summary>
  /// <returns>A mock of the service with default setup.</returns>
  protected Mock<T> CreateDefaultMock()
  {
    var mock = new Mock<T>();
    SetupDefaultMock(mock);
    return mock;
  }

  /// <summary>
  /// Sets up the default behavior for the mock.
  /// </summary>
  /// <param name="mock">The mock to set up.</param>
  protected abstract void SetupDefaultMock(Mock<T> mock);
}
