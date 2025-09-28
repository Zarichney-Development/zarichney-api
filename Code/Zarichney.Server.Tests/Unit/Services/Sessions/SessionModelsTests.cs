using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Concurrent;
using Xunit;
using Zarichney.Cookbook.Orders;
using Zarichney.Services.AI;
using Zarichney.Services.Sessions;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Sessions;

/// <summary>
/// Unit tests for Session model classes including Session, SessionConfig,
/// ScopeContainer, and related components.
/// </summary>
public class SessionModelsTests
{
  #region Session Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void Session_DefaultConstruction_InitializesCollectionsCorrectly()
  {
    // Arrange & Act
    var session = new Session();

    // Assert
    session.Scopes.Should().NotBeNull("Scopes dictionary should be initialized");
    session.Scopes.Should().BeEmpty("Scopes should be empty initially");
    session.Conversations.Should().NotBeNull("Conversations dictionary should be initialized");
    session.Conversations.Should().BeEmpty("Conversations should be empty initially");
    session.UserId.Should().BeNull("UserId should be null by default");
    session.ApiKeyValue.Should().BeNull("ApiKeyValue should be null by default");
    session.Order.Should().BeNull("Order should be null by default");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Session_InitOnlyProperties_CanBeSetViaInitializer()
  {
    // Arrange
    var expectedId = Guid.NewGuid();
    var expectedCreatedAt = DateTime.UtcNow.AddHours(-1);

    // Act
    var session = new Session
    {
      Id = expectedId,
      CreatedAt = expectedCreatedAt
    };

    // Assert
    session.Id.Should().Be(expectedId, "Id should be set via object initializer");
    session.CreatedAt.Should().Be(expectedCreatedAt, "CreatedAt should be set via object initializer");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Session_MutableProperties_CanBeModified()
  {
    // Arrange
    var session = new Session();
    const string userId = "user-123";
    const string apiKey = "api-key-456";
    var lastAccessed = DateTime.UtcNow;
    var expiresAt = DateTime.UtcNow.AddMinutes(30);
    var duration = TimeSpan.FromMinutes(30);

    // Act
    session.UserId = userId;
    session.ApiKeyValue = apiKey;
    session.LastAccessedAt = lastAccessed;
    session.ExpiresAt = expiresAt;
    session.Duration = duration;
    session.ExpiresImmediately = true;

    // Assert
    session.UserId.Should().Be(userId);
    session.ApiKeyValue.Should().Be(apiKey);
    session.LastAccessedAt.Should().Be(lastAccessed);
    session.ExpiresAt.Should().Be(expiresAt);
    session.Duration.Should().Be(duration);
    session.ExpiresImmediately.Should().BeTrue();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Session_ScopesCollection_SupportsThreadSafeConcurrentOperations()
  {
    // Arrange
    var session = new Session();
    var scopeIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();

    // Act - Parallel adds
    Parallel.ForEach(scopeIds, scopeId =>
    {
      session.Scopes.TryAdd(scopeId, 0);
    });

    // Assert
    session.Scopes.Count.Should().Be(100, "All scopes should be added");
    scopeIds.All(id => session.Scopes.ContainsKey(id)).Should().BeTrue("All scope IDs should be present");

    // Act - Parallel removes
    Parallel.ForEach(scopeIds.Take(50), scopeId =>
    {
      session.Scopes.TryRemove(scopeId, out _);
    });

    // Assert
    session.Scopes.Count.Should().Be(50, "Half of scopes should remain after removal");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Session_ConversationsCollection_SupportsMultipleConversations()
  {
    // Arrange
    var session = new Session();
    var conversation1 = new LlmConversation { Id = "conv-1" };
    var conversation2 = new LlmConversation { Id = "conv-2" };

    // Act
    session.Conversations.TryAdd("conv-1", conversation1);
    session.Conversations.TryAdd("conv-2", conversation2);

    // Assert
    session.Conversations.Count.Should().Be(2);
    session.Conversations["conv-1"].Should().BeSameAs(conversation1);
    session.Conversations["conv-2"].Should().BeSameAs(conversation2);
  }

  #endregion

  #region SessionConfig Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void SessionConfig_DefaultValues_AreSetCorrectly()
  {
    // Arrange & Act
    var config = new SessionConfig();

    // Assert
    config.CleanupIntervalMins.Should().Be(1, "Default cleanup interval should be 1 minute");
    config.DefaultDurationMins.Should().Be(15, "Default session duration should be 15 minutes");
    config.MaxConcurrentCleanup.Should().Be(10, "Default max concurrent cleanup should be 10");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void SessionConfig_Properties_CanBeModified()
  {
    // Arrange
    var config = new SessionConfig();

    // Act
    config.CleanupIntervalMins = 5;
    config.DefaultDurationMins = 30;
    config.MaxConcurrentCleanup = 20;

    // Assert
    config.CleanupIntervalMins.Should().Be(5);
    config.DefaultDurationMins.Should().Be(30);
    config.MaxConcurrentCleanup.Should().Be(20);
  }

  #endregion

  #region ScopeContainer Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void ScopeContainer_ConstructionWithoutParent_GeneratesNewId()
  {
    // Arrange
    var serviceProvider = new Mock<IServiceProvider>().Object;

    // Act
    var scope1 = new ScopeContainer(serviceProvider);
    var scope2 = new ScopeContainer(serviceProvider);

    // Assert
    scope1.Id.Should().NotBeEmpty("Should generate a new ID");
    scope2.Id.Should().NotBeEmpty("Should generate a new ID");
    scope1.Id.Should().NotBe(scope2.Id, "Each scope should have a unique ID");
    scope1.SessionId.Should().BeNull("SessionId should be null initially");
    scope1.ServiceProvider.Should().BeSameAs(serviceProvider);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ScopeContainer_ConstructionWithParent_InheritsParentProperties()
  {
    // Arrange
    var serviceProvider = new Mock<IServiceProvider>().Object;
    var parentId = Guid.NewGuid();
    var parentSessionId = Guid.NewGuid();

    var parentScope = new Mock<IScopeContainer>();
    parentScope.Setup(x => x.Id).Returns(parentId);
    parentScope.Setup(x => x.SessionId).Returns(parentSessionId);

    // Act
    var childScope = new ScopeContainer(serviceProvider, parentScope.Object);

    // Assert
    childScope.Id.Should().Be(parentId, "Should inherit parent's ID");
    childScope.SessionId.Should().Be(parentSessionId, "Should inherit parent's SessionId");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ScopeContainer_GetService_ResolvesFromServiceProvider()
  {
    // Arrange
    var serviceProvider = new Mock<IServiceProvider>();
    var expectedService = new Mock<ISessionManager>().Object;
    serviceProvider.Setup(x => x.GetService(typeof(ISessionManager)))
        .Returns(expectedService);

    var scope = new ScopeContainer(serviceProvider.Object);

    // Act
    var resolvedService = scope.GetService<ISessionManager>();

    // Assert
    resolvedService.Should().BeSameAs(expectedService,
        "Should resolve service from the service provider");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ScopeContainer_Dispose_CanBeCalledMultipleTimes()
  {
    // Arrange
    var serviceProvider = new Mock<IServiceProvider>().Object;
    var scope = new ScopeContainer(serviceProvider);

    // Act & Assert - Should not throw
    scope.Dispose();
    scope.Dispose(); // Second call should be safe

    // No exception means test passed
  }

  #endregion

  #region DisposableScopeContainer Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void DisposableScopeContainer_Dispose_DisposesUnderlyingScope()
  {
    // Arrange
    var serviceScope = new Mock<IServiceScope>();
    var serviceProvider = new Mock<IServiceProvider>().Object;
    serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider);

    var container = new DisposableScopeContainer(serviceScope.Object);

    // Act
    container.Dispose();

    // Assert
    serviceScope.Verify(x => x.Dispose(), Times.Once,
        "Should dispose the underlying service scope");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void DisposableScopeContainer_MultipleDispose_OnlyDisposesOnce()
  {
    // Arrange
    var serviceScope = new Mock<IServiceScope>();
    var serviceProvider = new Mock<IServiceProvider>().Object;
    serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider);

    var container = new DisposableScopeContainer(serviceScope.Object);

    // Act
    container.Dispose();
    container.Dispose();
    container.Dispose();

    // Assert
    serviceScope.Verify(x => x.Dispose(), Times.Once,
        "Should only dispose the underlying scope once");
  }

  #endregion

  #region ScopeFactory Tests

  // Note: ScopeFactory tests removed as they require mocking extension methods which is not supported by Moq.
  // These would need to be tested as integration tests.

  #endregion

  #region ScopeHolder Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void ScopeHolder_CurrentScope_CanBeSetAndRetrieved()
  {
    // Arrange
    var scope = new Mock<IScopeContainer>().Object;

    // Act
    ScopeHolder.CurrentScope = scope;
    var retrievedScope = ScopeHolder.CurrentScope;

    // Assert
    retrievedScope.Should().BeSameAs(scope,
        "Should retrieve the same scope that was set");

    // Cleanup
    ScopeHolder.CurrentScope = null;
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ScopeHolder_CurrentScope_IsAsyncLocalAndThreadSpecific()
  {
    // Arrange
    IScopeContainer? scope1 = null;
    IScopeContainer? scope2 = null;
    var scopeInstance1 = new Mock<IScopeContainer>().Object;
    var scopeInstance2 = new Mock<IScopeContainer>().Object;

    // Act
    var task1 = Task.Run(() =>
    {
      ScopeHolder.CurrentScope = scopeInstance1;
      Thread.Sleep(100); // Give time for task2 to set its value
      scope1 = ScopeHolder.CurrentScope;
    });

    var task2 = Task.Run(() =>
    {
      ScopeHolder.CurrentScope = scopeInstance2;
      Thread.Sleep(100); // Give time for task1 to set its value
      scope2 = ScopeHolder.CurrentScope;
    });

    await Task.WhenAll(task1, task2);

    // Assert
    scope1.Should().BeSameAs(scopeInstance1,
        "Task 1 should retrieve its own scope");
    scope2.Should().BeSameAs(scopeInstance2,
        "Task 2 should retrieve its own scope");

    // Cleanup
    ScopeHolder.CurrentScope = null;
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ScopeHolder_CurrentScope_DefaultsToNull()
  {
    // Act
    var scope = ScopeHolder.CurrentScope;

    // Assert
    scope.Should().BeNull("CurrentScope should be null by default");
  }

  #endregion

  #region Integration Scenarios

  [Fact]
  [Trait("Category", "Unit")]
  public void Session_CompleteLifecycle_WorksAsExpected()
  {
    // Arrange
    var session = new SessionBuilder()
        .WithUserId("test-user")
        .WithApiKey("test-key")
        .WithDuration(TimeSpan.FromMinutes(20))
        .Build();

    var order = new CookbookOrder();
    var orderProperty = typeof(Session).GetProperty(nameof(Session.Order));
    orderProperty?.SetValue(session, order);

    var conversation = new LlmConversationBuilder().Build();

    // Act - Simulate session lifecycle
    session.Conversations.TryAdd("conv-1", conversation);
    session.Scopes.TryAdd(Guid.NewGuid(), 0);
    session.LastAccessedAt = DateTime.UtcNow;

    // Assert
    session.UserId.Should().Be("test-user");
    session.ApiKeyValue.Should().Be("test-key");
    session.Duration.Should().Be(TimeSpan.FromMinutes(20));
    session.Order.Should().BeSameAs(order);
    session.Conversations.Should().ContainKey("conv-1");
    session.Scopes.Should().NotBeEmpty();
    session.ExpiresImmediately.Should().BeFalse();
  }

  #endregion
}
