using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Zarichney.Services.Auth;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.Framework.Mocks.Factories;

public static class UserManagerMockFactory
{
  public static Mock<UserManager<ApplicationUser>> CreateDefault()
  {
    var store = new Mock<IUserStore<ApplicationUser>>();
    var options = new Mock<IOptions<IdentityOptions>>();
    var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
    var userValidators = new List<IUserValidator<ApplicationUser>> { new Mock<IUserValidator<ApplicationUser>>().Object };
    var passwordValidators = new List<IPasswordValidator<ApplicationUser>> { new Mock<IPasswordValidator<ApplicationUser>>().Object };
    var keyNormalizer = new Mock<ILookupNormalizer>();
    var errors = new Mock<IdentityErrorDescriber>();
    var services = new Mock<IServiceProvider>();
    var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

    var userManager = new Mock<UserManager<ApplicationUser>>(
        store.Object,
        options.Object,
        passwordHasher.Object,
        userValidators,
        passwordValidators,
        keyNormalizer.Object,
        errors.Object,
        services.Object,
        logger.Object);

    userManager.Setup(x => x.SupportsUserEmail).Returns(true);

    return userManager;
  }

  public static Mock<UserManager<ApplicationUser>> CreateWithUser(ApplicationUser user)
  {
    var mock = CreateDefault();

    mock.Setup(x => x.FindByEmailAsync(user.Email!))
        .ReturnsAsync(user);

    mock.Setup(x => x.FindByIdAsync(user.Id))
        .ReturnsAsync(user);

    mock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
        .ReturnsAsync(true);

    return mock;
  }

  public static Mock<UserManager<ApplicationUser>> CreateWithSuccessfulRegistration()
  {
    var mock = CreateDefault();

    mock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((ApplicationUser)null!);

    mock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Success);

    mock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
        .ReturnsAsync("confirmation-token");

    return mock;
  }

  public static Mock<UserManager<ApplicationUser>> CreateWithFailedRegistration(params string[] errors)
  {
    var mock = CreateDefault();

    mock.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync((ApplicationUser)null!);

    var identityErrors = errors.Select(e => new IdentityError { Description = e }).ToArray();
    mock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Failed(identityErrors));

    return mock;
  }

  public static Mock<UserManager<ApplicationUser>> CreateWithExistingUser(string email)
  {
    var mock = CreateDefault();
    var user = new ApplicationUserBuilder().WithEmail(email).Build();

    mock.Setup(x => x.FindByEmailAsync(email))
        .ReturnsAsync(user);

    return mock;
  }

  public static Mock<UserManager<ApplicationUser>> CreateWithPasswordValidation(ApplicationUser user, bool isValid)
  {
    var mock = CreateDefault();

    mock.Setup(x => x.FindByEmailAsync(user.Email!))
        .ReturnsAsync(user);

    mock.Setup(x => x.CheckPasswordAsync(user, It.IsAny<string>()))
        .ReturnsAsync(isValid);

    return mock;
  }

  public static Mock<UserManager<ApplicationUser>> CreateWithRefreshTokenSupport(ApplicationUser user)
  {
    var mock = CreateWithUser(user);

    mock.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
        .ReturnsAsync(user);

    mock.Setup(x => x.GetRolesAsync(user))
        .ReturnsAsync(new List<string> { "User" });

    return mock;
  }
}
