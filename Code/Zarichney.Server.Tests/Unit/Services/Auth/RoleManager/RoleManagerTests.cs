using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.Auth;

namespace Zarichney.Server.Tests.Unit.Services.Auth.RoleManager;

/// <summary>
/// Comprehensive unit tests for RoleManager covering role and user role management functionality.
/// Tests role creation, user role assignment/removal, role queries, and error handling scenarios.
/// Follows framework-first approach with complete isolation and proper mocking patterns.
/// </summary>
[Trait("Category", "Unit")]
public class RoleManagerTests : IDisposable
{
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ILogger<Zarichney.Services.Auth.RoleManager>> _mockLogger;
    private readonly Zarichney.Services.Auth.RoleManager _roleManager;
    private readonly string _testUserId = "test-user-123";
    private readonly string _testRoleName = "admin";
    private readonly string _nonExistentUserId = "non-existent-user";
    private readonly string _nonExistentRoleName = "non-existent-role";

    public RoleManagerTests()
    {
        // Setup RoleManager mock
        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null!, null!, null!, null!);

        // Setup UserManager mock
        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _mockLogger = new Mock<ILogger<Zarichney.Services.Auth.RoleManager>>();

        _roleManager = new Zarichney.Services.Auth.RoleManager(
            _mockRoleManager.Object,
            _mockUserManager.Object,
            _mockLogger.Object);
    }

    public void Dispose()
    {
        // No resources to dispose in this implementation
    }

    #region EnsureRolesCreatedAsync Tests

    [Fact]
    public async Task EnsureRolesCreatedAsync_RoleAlreadyExists_DoesNotCreateRole()
    {
        // Arrange
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_testRoleName))
                       .ReturnsAsync(true);

        // Act
        await _roleManager.EnsureRolesCreatedAsync();

        // Assert
        _mockRoleManager.Verify(rm => rm.CreateAsync(It.IsAny<IdentityRole>()), Times.Never,
            "should not create role if it already exists");
    }

    [Fact]
    public async Task EnsureRolesCreatedAsync_RoleDoesNotExist_CreatesRole()
    {
        // Arrange
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_testRoleName))
                       .ReturnsAsync(false);
        _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        await _roleManager.EnsureRolesCreatedAsync();

        // Assert
        _mockRoleManager.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == _testRoleName)),
            Times.Once,
            "should create role when it does not exist");
    }

    [Fact]
    public async Task EnsureRolesCreatedAsync_CreateRoleFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var errorDescription = "Role creation failed";
        var identityError = new IdentityError { Description = errorDescription };
        var failedResult = IdentityResult.Failed(identityError);

        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_testRoleName))
                       .ReturnsAsync(false);
        _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                       .ReturnsAsync(failedResult);

        // Act
        Func<Task> act = async () => await _roleManager.EnsureRolesCreatedAsync();

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>(
            "should throw exception when role creation fails");
        exception.WithMessage($"*Failed to create role {_testRoleName}*");
    }

    [Fact]
    public async Task EnsureRolesCreatedAsync_CreatesAllDefaultRoles()
    {
        // Arrange
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
                       .ReturnsAsync(false);
        _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        await _roleManager.EnsureRolesCreatedAsync();

        // Assert
        _mockRoleManager.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == "admin")),
            Times.Once,
            "should create admin role");
    }

    #endregion

    #region IsUserInRoleAsync Tests

    [Fact]
    public async Task IsUserInRoleAsync_UserExists_ReturnsUserRoleStatus()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockUserManager.Setup(um => um.IsInRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(true);

        // Act
        var result = await _roleManager.IsUserInRoleAsync(_testUserId, _testRoleName);

        // Assert
        result.Should().BeTrue("should return true when user is in role");
        _mockUserManager.Verify(um => um.IsInRoleAsync(testUser, _testRoleName), Times.Once);
    }

    [Fact]
    public async Task IsUserInRoleAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _mockUserManager.Setup(um => um.FindByIdAsync(_nonExistentUserId))
                       .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _roleManager.IsUserInRoleAsync(_nonExistentUserId, _testRoleName);

        // Assert
        result.Should().BeFalse("should return false when user does not exist");
        _mockUserManager.Verify(um => um.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never,
            "should not check role when user does not exist");
    }

    [Fact]
    public async Task IsUserInRoleAsync_UserNotInRole_ReturnsFalse()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockUserManager.Setup(um => um.IsInRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(false);

        // Act
        var result = await _roleManager.IsUserInRoleAsync(_testUserId, _testRoleName);

        // Assert
        result.Should().BeFalse("should return false when user is not in role");
    }

    #endregion

    #region AddUserToRoleAsync Tests

    [Fact]
    public async Task AddUserToRoleAsync_ValidUserAndRole_ReturnsTrue()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_testRoleName))
                       .ReturnsAsync(true);
        _mockUserManager.Setup(um => um.AddToRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleManager.AddUserToRoleAsync(_testUserId, _testRoleName);

        // Assert
        result.Should().BeTrue("should return true when user is successfully added to role");
        _mockUserManager.Verify(um => um.AddToRoleAsync(testUser, _testRoleName), Times.Once);
    }

    [Fact]
    public async Task AddUserToRoleAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _mockUserManager.Setup(um => um.FindByIdAsync(_nonExistentUserId))
                       .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _roleManager.AddUserToRoleAsync(_nonExistentUserId, _testRoleName);

        // Assert
        result.Should().BeFalse("should return false when user does not exist");
        _mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never,
            "should not attempt to add to role when user does not exist");
    }

    [Fact]
    public async Task AddUserToRoleAsync_RoleDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_nonExistentRoleName))
                       .ReturnsAsync(false);

        // Act
        var result = await _roleManager.AddUserToRoleAsync(_testUserId, _nonExistentRoleName);

        // Assert
        result.Should().BeFalse("should return false when role does not exist");
        _mockUserManager.Verify(um => um.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never,
            "should not attempt to add to role when role does not exist");
    }

    [Fact]
    public async Task AddUserToRoleAsync_AddToRoleFails_ReturnsFalse()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        var errorDescription = "Failed to add user to role";
        var identityError = new IdentityError { Description = errorDescription };
        var failedResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_testRoleName))
                       .ReturnsAsync(true);
        _mockUserManager.Setup(um => um.AddToRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(failedResult);

        // Act
        var result = await _roleManager.AddUserToRoleAsync(_testUserId, _testRoleName);

        // Assert
        result.Should().BeFalse("should return false when AddToRoleAsync fails");
    }

    #endregion

    #region RemoveUserFromRoleAsync Tests

    [Fact]
    public async Task RemoveUserFromRoleAsync_ValidUserAndRole_ReturnsTrue()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockUserManager.Setup(um => um.RemoveFromRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _roleManager.RemoveUserFromRoleAsync(_testUserId, _testRoleName);

        // Assert
        result.Should().BeTrue("should return true when user is successfully removed from role");
        _mockUserManager.Verify(um => um.RemoveFromRoleAsync(testUser, _testRoleName), Times.Once);
    }

    [Fact]
    public async Task RemoveUserFromRoleAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        _mockUserManager.Setup(um => um.FindByIdAsync(_nonExistentUserId))
                       .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _roleManager.RemoveUserFromRoleAsync(_nonExistentUserId, _testRoleName);

        // Assert
        result.Should().BeFalse("should return false when user does not exist");
        _mockUserManager.Verify(um => um.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never,
            "should not attempt to remove from role when user does not exist");
    }

    [Fact]
    public async Task RemoveUserFromRoleAsync_RemoveFromRoleFails_ReturnsFalse()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        var errorDescription = "Failed to remove user from role";
        var identityError = new IdentityError { Description = errorDescription };
        var failedResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockUserManager.Setup(um => um.RemoveFromRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(failedResult);

        // Act
        var result = await _roleManager.RemoveUserFromRoleAsync(_testUserId, _testRoleName);

        // Assert
        result.Should().BeFalse("should return false when RemoveFromRoleAsync fails");
    }

    #endregion

    #region GetUserRolesAsync Tests

    [Fact]
    public async Task GetUserRolesAsync_ValidUser_ReturnsUserRoles()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        var expectedRoles = new List<string> { "admin", "user" };
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockUserManager.Setup(um => um.GetRolesAsync(testUser))
                       .ReturnsAsync(expectedRoles);

        // Act
        var result = await _roleManager.GetUserRolesAsync(_testUserId);

        // Assert
        result.Should().BeEquivalentTo(expectedRoles,
            "should return the roles assigned to the user");
        _mockUserManager.Verify(um => um.GetRolesAsync(testUser), Times.Once);
    }

    [Fact]
    public async Task GetUserRolesAsync_UserDoesNotExist_ReturnsEmptyList()
    {
        // Arrange
        _mockUserManager.Setup(um => um.FindByIdAsync(_nonExistentUserId))
                       .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _roleManager.GetUserRolesAsync(_nonExistentUserId);

        // Assert
        result.Should().BeEmpty("should return empty list when user does not exist");
        _mockUserManager.Verify(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()),
            Times.Never,
            "should not attempt to get roles when user does not exist");
    }

    [Fact]
    public async Task GetUserRolesAsync_UserWithNoRoles_ReturnsEmptyList()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        var emptyRoles = new List<string>();
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockUserManager.Setup(um => um.GetRolesAsync(testUser))
                       .ReturnsAsync(emptyRoles);

        // Act
        var result = await _roleManager.GetUserRolesAsync(_testUserId);

        // Assert
        result.Should().BeEmpty("should return empty list when user has no roles");
    }

    #endregion

    #region GetUsersInRoleAsync Tests

    [Fact]
    public async Task GetUsersInRoleAsync_ValidRole_ReturnsUsersInRole()
    {
        // Arrange
        var usersInRole = new List<ApplicationUser>
        {
            CreateTestUser("user1"),
            CreateTestUser("user2")
        };
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(_testRoleName))
                       .ReturnsAsync(usersInRole);

        // Act
        var result = await _roleManager.GetUsersInRoleAsync(_testRoleName);

        // Assert
        result.Should().HaveCount(2, "should return all users in the specified role");
        result.Should().BeEquivalentTo(usersInRole,
            "should return the exact users that are in the role");
        _mockUserManager.Verify(um => um.GetUsersInRoleAsync(_testRoleName), Times.Once);
    }

    [Fact]
    public async Task GetUsersInRoleAsync_RoleWithNoUsers_ReturnsEmptyList()
    {
        // Arrange
        var emptyUserList = new List<ApplicationUser>();
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(_testRoleName))
                       .ReturnsAsync(emptyUserList);

        // Act
        var result = await _roleManager.GetUsersInRoleAsync(_testRoleName);

        // Assert
        result.Should().BeEmpty("should return empty list when role has no users");
    }

    [Fact]
    public async Task GetUsersInRoleAsync_NonExistentRole_ReturnsEmptyList()
    {
        // Arrange
        var emptyUserList = new List<ApplicationUser>();
        _mockUserManager.Setup(um => um.GetUsersInRoleAsync(_nonExistentRoleName))
                       .ReturnsAsync(emptyUserList);

        // Act
        var result = await _roleManager.GetUsersInRoleAsync(_nonExistentRoleName);

        // Assert
        result.Should().BeEmpty("should return empty list for non-existent role");
    }

    #endregion

    #region Integration and Error Scenarios

    [Fact]
    public async Task AddUserToRoleAsync_LogsSuccessfulAddition()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_testRoleName))
                       .ReturnsAsync(true);
        _mockUserManager.Setup(um => um.AddToRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        await _roleManager.AddUserToRoleAsync(_testUserId, _testRoleName);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "should log successful user addition to role");
    }

    [Fact]
    public async Task RemoveUserFromRoleAsync_LogsSuccessfulRemoval()
    {
        // Arrange
        var testUser = CreateTestUser(_testUserId);
        _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
                       .ReturnsAsync(testUser);
        _mockUserManager.Setup(um => um.RemoveFromRoleAsync(testUser, _testRoleName))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        await _roleManager.RemoveUserFromRoleAsync(_testUserId, _testRoleName);

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Removed user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "should log successful user removal from role");
    }

    [Fact]
    public async Task EnsureRolesCreatedAsync_LogsRoleCreation()
    {
        // Arrange
        _mockRoleManager.Setup(rm => rm.RoleExistsAsync(_testRoleName))
                       .ReturnsAsync(false);
        _mockRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
                       .ReturnsAsync(IdentityResult.Success);

        // Act
        await _roleManager.EnsureRolesCreatedAsync();

        // Assert
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating role")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "should log role creation");
    }

    #endregion

    #region Helper Methods

    private ApplicationUser CreateTestUser(string userId)
    {
        return new ApplicationUser
        {
            Id = userId,
            UserName = $"user-{userId}@example.com",
            Email = $"user-{userId}@example.com",
            EmailConfirmed = true
        };
    }

    #endregion
}