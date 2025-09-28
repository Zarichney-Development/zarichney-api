using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

public class RoleCommandTests
{
  private readonly Mock<IRoleManager> _mockRoleManager;
  private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

  public RoleCommandTests()
  {
    _mockRoleManager = new Mock<IRoleManager>();
    var store = new Mock<IUserStore<ApplicationUser>>();
    _mockUserManager = new Mock<UserManager<ApplicationUser>>(
        store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
  }

  #region AddUserToRoleCommandHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task AddUserToRoleCommandHandler_WithValidUserAndRole_ReturnsSuccess()
  {
    // Arrange
    var userId = "test-user-123";
    var roleName = "Admin";
    var command = new AddUserToRoleCommand(userId, roleName);
    var existingRoles = new[] { "User", "Admin" };

    _mockRoleManager.Setup(x => x.AddUserToRoleAsync(userId, roleName))
        .ReturnsAsync(true);
    _mockRoleManager.Setup(x => x.GetUserRolesAsync(userId))
        .ReturnsAsync(existingRoles);

    var handler = new AddUserToRoleCommandHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("because the user was successfully added to the role");
    result.Message.Should().Be($"User added to role {roleName}");
    result.Roles.Should().BeEquivalentTo(existingRoles);

    _mockRoleManager.Verify(x => x.AddUserToRoleAsync(userId, roleName), Times.Once);
    _mockRoleManager.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task AddUserToRoleCommandHandler_WhenAddingFails_ReturnsFailure()
  {
    // Arrange
    var userId = "test-user-123";
    var roleName = "NonExistentRole";
    var command = new AddUserToRoleCommand(userId, roleName);

    _mockRoleManager.Setup(x => x.AddUserToRoleAsync(userId, roleName))
        .ReturnsAsync(false);

    var handler = new AddUserToRoleCommandHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("because the role addition failed");
    result.Message.Should().Be($"Failed to add user to role {roleName}");
    result.Roles.Should().BeEmpty();

    _mockRoleManager.Verify(x => x.AddUserToRoleAsync(userId, roleName), Times.Once);
    _mockRoleManager.Verify(x => x.GetUserRolesAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task AddUserToRoleCommandHandler_WithMultipleRoles_ReturnsAllRoles()
  {
    // Arrange
    var userId = "test-user-123";
    var roleName = "Editor";
    var command = new AddUserToRoleCommand(userId, roleName);
    var existingRoles = new[] { "User", "Admin", "Editor", "Viewer" };

    _mockRoleManager.Setup(x => x.AddUserToRoleAsync(userId, roleName))
        .ReturnsAsync(true);
    _mockRoleManager.Setup(x => x.GetUserRolesAsync(userId))
        .ReturnsAsync(existingRoles);

    var handler = new AddUserToRoleCommandHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
    result.Roles.Should().HaveCount(4, "because the user has 4 roles");
    result.Roles.Should().Contain(new[] { "User", "Admin", "Editor", "Viewer" });
  }

  #endregion

  #region RemoveUserFromRoleCommandHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RemoveUserFromRoleCommandHandler_WithValidUserAndRole_ReturnsSuccess()
  {
    // Arrange
    var userId = "test-user-123";
    var roleName = "Admin";
    var command = new RemoveUserFromRoleCommand(userId, roleName);
    var remainingRoles = new[] { "User" };

    _mockRoleManager.Setup(x => x.RemoveUserFromRoleAsync(userId, roleName))
        .ReturnsAsync(true);
    _mockRoleManager.Setup(x => x.GetUserRolesAsync(userId))
        .ReturnsAsync(remainingRoles);

    var handler = new RemoveUserFromRoleCommandHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("because the user was successfully removed from the role");
    result.Message.Should().Be($"User removed from role {roleName}");
    result.Roles.Should().BeEquivalentTo(remainingRoles);

    _mockRoleManager.Verify(x => x.RemoveUserFromRoleAsync(userId, roleName), Times.Once);
    _mockRoleManager.Verify(x => x.GetUserRolesAsync(userId), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RemoveUserFromRoleCommandHandler_WhenRemovalFails_ReturnsFailure()
  {
    // Arrange
    var userId = "test-user-123";
    var roleName = "NonExistentRole";
    var command = new RemoveUserFromRoleCommand(userId, roleName);

    _mockRoleManager.Setup(x => x.RemoveUserFromRoleAsync(userId, roleName))
        .ReturnsAsync(false);

    var handler = new RemoveUserFromRoleCommandHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("because the role removal failed");
    result.Message.Should().Be($"Failed to remove user from role {roleName}");
    result.Roles.Should().BeEmpty();

    _mockRoleManager.Verify(x => x.RemoveUserFromRoleAsync(userId, roleName), Times.Once);
    _mockRoleManager.Verify(x => x.GetUserRolesAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RemoveUserFromRoleCommandHandler_RemovingLastRole_ReturnsEmptyRolesList()
  {
    // Arrange
    var userId = "test-user-123";
    var roleName = "User";
    var command = new RemoveUserFromRoleCommand(userId, roleName);
    var remainingRoles = Array.Empty<string>();

    _mockRoleManager.Setup(x => x.RemoveUserFromRoleAsync(userId, roleName))
        .ReturnsAsync(true);
    _mockRoleManager.Setup(x => x.GetUserRolesAsync(userId))
        .ReturnsAsync(remainingRoles);

    var handler = new RemoveUserFromRoleCommandHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
    result.Roles.Should().BeEmpty("because the user has no remaining roles");
  }

  #endregion

  #region GetUserRolesQueryHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRolesQueryHandler_WithUserId_ReturnsUserRoles()
  {
    // Arrange
    var userId = "test-user-123";
    var query = new GetUserRolesQuery(userId);
    var user = new ApplicationUser
    {
      Id = userId,
      Email = "test@example.com",
      UserName = "testuser"
    };
    var roles = new List<string> { "User", "Admin" };

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(roles);

    var handler = new GetUserRolesQueryHandler(_mockUserManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("because the user was found");
    result.Message.Should().Be("Roles retrieved successfully");
    result.Roles.Should().BeEquivalentTo(roles);

    _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
    _mockUserManager.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRolesQueryHandler_WithEmail_ReturnsUserRoles()
  {
    // Arrange
    var email = "test@example.com";
    var query = new GetUserRolesQuery(email);
    var user = new ApplicationUser
    {
      Id = "test-user-123",
      Email = email,
      UserName = "testuser"
    };
    var roles = new List<string> { "Editor", "Viewer" };

    _mockUserManager.Setup(x => x.FindByEmailAsync(email))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(roles);

    var handler = new GetUserRolesQueryHandler(_mockUserManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("because the user was found by email");
    result.Message.Should().Be("Roles retrieved successfully");
    result.Roles.Should().BeEquivalentTo(roles);

    _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
    _mockUserManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRolesQueryHandler_WithNonExistentUserId_ReturnsFailure()
  {
    // Arrange
    var userId = "non-existent-user";
    var query = new GetUserRolesQuery(userId);

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync((ApplicationUser?)null);

    var handler = new GetUserRolesQueryHandler(_mockUserManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("because the user does not exist");
    result.Message.Should().Be("User not found with the provided ID.");
    result.Roles.Should().BeEmpty();

    _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
    _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRolesQueryHandler_WithNonExistentEmail_ReturnsFailure()
  {
    // Arrange
    var email = "nonexistent@example.com";
    var query = new GetUserRolesQuery(email);

    _mockUserManager.Setup(x => x.FindByEmailAsync(email))
        .ReturnsAsync((ApplicationUser?)null);

    var handler = new GetUserRolesQueryHandler(_mockUserManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("because the user does not exist");
    result.Message.Should().Be("User not found with the provided email.");
    result.Roles.Should().BeEmpty();

    _mockUserManager.Verify(x => x.FindByEmailAsync(email), Times.Once);
    _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRolesQueryHandler_WithUserHavingNoRoles_ReturnsEmptyRolesList()
  {
    // Arrange
    var userId = "test-user-123";
    var query = new GetUserRolesQuery(userId);
    var user = new ApplicationUser
    {
      Id = userId,
      Email = "test@example.com",
      UserName = "testuser"
    };
    var roles = new List<string>(); // Empty roles

    _mockUserManager.Setup(x => x.FindByIdAsync(userId))
        .ReturnsAsync(user);
    _mockUserManager.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(roles);

    var handler = new GetUserRolesQueryHandler(_mockUserManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue();
    result.Roles.Should().BeEmpty("because the user has no assigned roles");
  }

  #endregion

  #region GetUsersInRoleQueryHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUsersInRoleQueryHandler_WithValidRole_ReturnsUsersList()
  {
    // Arrange
    var roleName = "Admin";
    var query = new GetUsersInRoleQuery(roleName);
    var users = new List<ApplicationUser>
        {
            new() { Id = "user1", UserName = "admin1", Email = "admin1@example.com" },
            new() { Id = "user2", UserName = "admin2", Email = "admin2@example.com" },
            new() { Id = "user3", UserName = "superadmin", Email = "super@example.com" }
        };

    _mockRoleManager.Setup(x => x.GetUsersInRoleAsync(roleName))
        .ReturnsAsync(users);

    var handler = new GetUsersInRoleQueryHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCount(3, "because 3 users have the Admin role");

    result[0].UserId.Should().Be("user1");
    result[0].UserName.Should().Be("admin1");
    result[0].Email.Should().Be("admin1@example.com");

    result[1].UserId.Should().Be("user2");
    result[1].UserName.Should().Be("admin2");
    result[1].Email.Should().Be("admin2@example.com");

    result[2].UserId.Should().Be("user3");
    result[2].UserName.Should().Be("superadmin");
    result[2].Email.Should().Be("super@example.com");

    _mockRoleManager.Verify(x => x.GetUsersInRoleAsync(roleName), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUsersInRoleQueryHandler_WithNoUsersInRole_ReturnsEmptyList()
  {
    // Arrange
    var roleName = "EmptyRole";
    var query = new GetUsersInRoleQuery(roleName);
    var users = new List<ApplicationUser>(); // No users

    _mockRoleManager.Setup(x => x.GetUsersInRoleAsync(roleName))
        .ReturnsAsync(users);

    var handler = new GetUsersInRoleQueryHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEmpty("because no users have this role");

    _mockRoleManager.Verify(x => x.GetUsersInRoleAsync(roleName), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUsersInRoleQueryHandler_WithNullUserProperties_HandlesGracefully()
  {
    // Arrange
    var roleName = "TestRole";
    var query = new GetUsersInRoleQuery(roleName);
    var users = new List<ApplicationUser>
        {
            new() { Id = "user1", UserName = null, Email = null },
            new() { Id = "user2", UserName = "user2name", Email = null },
            new() { Id = "user3", UserName = null, Email = "user3@example.com" }
        };

    _mockRoleManager.Setup(x => x.GetUsersInRoleAsync(roleName))
        .ReturnsAsync(users);

    var handler = new GetUsersInRoleQueryHandler(_mockRoleManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCount(3);

    result[0].UserId.Should().Be("user1");
    result[0].UserName.Should().BeEmpty("because null UserName is converted to empty string");
    result[0].Email.Should().BeEmpty("because null Email is converted to empty string");

    result[1].UserName.Should().Be("user2name");
    result[1].Email.Should().BeEmpty();

    result[2].UserName.Should().BeEmpty();
    result[2].Email.Should().Be("user3@example.com");
  }

  #endregion

  #region Edge Cases and Boundary Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRolesQueryHandler_WithEmptyStringIdentifier_ReturnsFailure()
  {
    // Arrange
    var query = new GetUserRolesQuery(string.Empty);

    _mockUserManager.Setup(x => x.FindByIdAsync(string.Empty))
        .ReturnsAsync((ApplicationUser?)null);

    var handler = new GetUserRolesQueryHandler(_mockUserManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse();
    result.Message.Should().Be("User not found with the provided ID.");
    result.Roles.Should().BeEmpty();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRolesQueryHandler_WithEmailLikeStringButNotEmail_TriesToFindByEmail()
  {
    // Arrange
    var identifier = "test@notvalid";
    var query = new GetUserRolesQuery(identifier);

    _mockUserManager.Setup(x => x.FindByEmailAsync(identifier))
        .ReturnsAsync((ApplicationUser?)null);

    var handler = new GetUserRolesQueryHandler(_mockUserManager.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse();
    result.Message.Should().Be("User not found with the provided email.");

    _mockUserManager.Verify(x => x.FindByEmailAsync(identifier), Times.Once);
    _mockUserManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  #endregion
}
