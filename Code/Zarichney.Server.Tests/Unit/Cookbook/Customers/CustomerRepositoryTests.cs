using FluentAssertions;
using Moq;
using Xunit;
using Zarichney.Cookbook.Customers;
using Zarichney.Services.Email;
using Zarichney.Services.FileSystem;
using Zarichney.Server.Tests.Framework.Attributes;

namespace Zarichney.Tests.Unit.Cookbook.Customers;

/// <summary>
/// Unit tests for the CustomerFileRepository class.
/// Tests file-based storage operations for customer data.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Component, TestCategories.Service)]
[Trait(TestCategories.Feature, TestCategories.Cookbook)]
[Trait(TestCategories.Mutability, TestCategories.ReadOnly)]
public class CustomerRepositoryTests
{
  private readonly Mock<IFileService> _mockFileService;
  private readonly Mock<IFileWriteQueueService> _mockFileWriteQueueService;
  private readonly CustomerConfig _customerConfig;
  private readonly CustomerFileRepository _repository;

  public CustomerRepositoryTests()
  {
    // Arrange - Setup mocks and configuration
    _mockFileService = new Mock<IFileService>();
    _mockFileWriteQueueService = new Mock<IFileWriteQueueService>();
    _customerConfig = new CustomerConfig
    {
      OutputDirectory = "Data/Customers"
    };

    // Create repository with mocked dependencies
    _repository = new CustomerFileRepository(
        _mockFileService.Object,
        _mockFileWriteQueueService.Object,
        _customerConfig
    );
  }

  #region GetCustomerByEmail Tests

  [Fact]
  public async Task GetCustomerByEmail_WithExistingCustomer_ReturnsCustomer()
  {
    // Arrange
    const string email = "test@example.com";

    var expectedCustomer = new Customer
    {
      Email = email,
      AvailableRecipes = 15,
      LifetimeRecipesUsed = 5,
      LifetimePurchasedRecipes = 2
    };

    _mockFileService
        .Setup(fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
        .ReturnsAsync(expectedCustomer);

    // Act
    var result = await _repository.GetCustomerByEmail(email);

    // Assert
    result.Should().NotBeNull("because an existing customer should be returned");
    result.Should().BeEquivalentTo(expectedCustomer,
        "because the returned customer should match the stored data");

    _mockFileService.Verify(
        fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
        Times.Once,
        "because the file service should be called once to read the customer data");
  }

  [Fact]
  public async Task GetCustomerByEmail_WithNonExistentCustomer_ReturnsNull()
  {
    // Arrange
    const string email = "nonexistent@example.com";

    _mockFileService
        .Setup(fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
        .ReturnsAsync((Customer?)null);

    // Act
    var result = await _repository.GetCustomerByEmail(email);

    // Assert
    result.Should().BeNull("because a non-existent customer should return null");

    _mockFileService.Verify(
        fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
        Times.Once,
        "because the file service should be called once to check for the customer");
  }

  [Theory]
  [InlineData("user@domain.com")]
  [InlineData("test.user@example.co.uk")]
  [InlineData("user+tag@gmail.com")]
  [InlineData("user with spaces@example.com")]
  [InlineData("user:with:colons@example.com")]
  [InlineData("user;with;semicolons@example.com")]
  public async Task GetCustomerByEmail_WithSpecialCharacters_CallsFileService(string email)
  {
    // Arrange
    _mockFileService
        .Setup(fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
        .ReturnsAsync((Customer?)null);

    // Act
    await _repository.GetCustomerByEmail(email);

    // Assert
    _mockFileService.Verify(
        fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
        Times.Once,
        $"because email '{email}' should result in a file service call");
  }

  #endregion

  #region SaveCustomer Tests

  [Fact]
  public void SaveCustomer_WithValidCustomer_QueuesWrite()
  {
    // Arrange
    const string email = "test@example.com";

    var customer = new Customer
    {
      Email = email,
      AvailableRecipes = 10,
      LifetimeRecipesUsed = 3,
      LifetimePurchasedRecipes = 1
    };

    // Act
    _repository.SaveCustomer(customer);

    // Assert
    _mockFileWriteQueueService.Verify(
        fwqs => fwqs.QueueWrite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<string?>()),
        Times.Once,
        "because the file write queue service should be called once to save the customer");
  }

  [Theory]
  [InlineData("user@domain.com")]
  [InlineData("test.user@example.co.uk")]
  [InlineData("user+tag@gmail.com")]
  [InlineData("user with spaces@example.com")]
  public void SaveCustomer_WithSpecialCharacters_QueuesWrite(string email)
  {
    // Arrange
    var customer = new Customer
    {
      Email = email,
      AvailableRecipes = 5
    };

    // Act
    _repository.SaveCustomer(customer);

    // Assert
    _mockFileWriteQueueService.Verify(
        fwqs => fwqs.QueueWrite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<string?>()),
        Times.Once,
        $"because email '{email}' should result in a queue write operation");
  }

  [Fact]
  public void SaveCustomer_WithCustomerHavingZeroValues_SavesCorrectly()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "zero@example.com",
      AvailableRecipes = 0,
      LifetimeRecipesUsed = 0,
      LifetimePurchasedRecipes = 0
    };

    // Act
    _repository.SaveCustomer(customer);

    // Assert
    _mockFileWriteQueueService.Verify(
        fwqs => fwqs.QueueWrite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<string?>()),
        Times.Once,
        "because customers with zero values should be saved correctly");
  }

  [Fact]
  public void SaveCustomer_WithCustomerHavingLargeNumbers_SavesCorrectly()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "large@example.com",
      AvailableRecipes = int.MaxValue,
      LifetimeRecipesUsed = 1000000,
      LifetimePurchasedRecipes = 500000
    };

    // Act
    _repository.SaveCustomer(customer);

    // Assert
    _mockFileWriteQueueService.Verify(
        fwqs => fwqs.QueueWrite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<string?>()),
        Times.Once,
        "because customers with large numbers should be saved correctly");
  }

  #endregion

  #region Email Safe Filename Integration Tests

  [Fact]
  public void SaveCustomer_UsesEmailServiceMakeSafeFileName()
  {
    // Arrange
    const string originalEmail = "test@example.com";
    const string expectedSafeFileName = "test_at_example_dot_com";
    var customer = new Customer { Email = originalEmail, AvailableRecipes = 1 };

    // Act
    _repository.SaveCustomer(customer);

    // Assert - Validate that the safe filename transformation is used
    var actualSafeFileName = EmailService.MakeSafeFileName(originalEmail);
    actualSafeFileName.Should().Be(expectedSafeFileName,
        "because EmailService.MakeSafeFileName should convert @ to _at_ and . to _dot_");

    _mockFileWriteQueueService.Verify(
        fwqs => fwqs.QueueWrite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<string?>()),
        Times.Once,
        "because the repository should use the safe filename transformation");
  }

  [Fact]
  public async Task GetCustomerByEmail_UsesEmailServiceMakeSafeFileName()
  {
    // Arrange
    const string originalEmail = "test@example.com";
    const string expectedSafeFileName = "test_at_example_dot_com";

    _mockFileService
        .Setup(fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
        .ReturnsAsync((Customer?)null);

    // Act
    await _repository.GetCustomerByEmail(originalEmail);

    // Assert - Validate that the safe filename transformation is used
    var actualSafeFileName = EmailService.MakeSafeFileName(originalEmail);
    actualSafeFileName.Should().Be(expectedSafeFileName,
        "because EmailService.MakeSafeFileName should convert @ to _at_ and . to _dot_");

    _mockFileService.Verify(
        fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
        Times.Once,
        "because the repository should use the safe filename transformation");
  }

  #endregion

  #region Configuration Tests

  [Fact]
  public async Task GetCustomerByEmail_UsesConfiguredOutputDirectory()
  {
    // Arrange
    const string email = "test@example.com";
    const string customDirectory = "Custom/Customer/Path";

    var customConfig = new CustomerConfig { OutputDirectory = customDirectory };
    var repository = new CustomerFileRepository(
        _mockFileService.Object,
        _mockFileWriteQueueService.Object,
        customConfig
    );

    _mockFileService
        .Setup(fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
        .ReturnsAsync((Customer?)null);

    // Act
    await repository.GetCustomerByEmail(email);

    // Assert
    _mockFileService.Verify(
        fs => fs.ReadFromFile<Customer?>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()),
        Times.Once,
        "because the repository should use the configured output directory");
  }

  [Fact]
  public void SaveCustomer_UsesConfiguredOutputDirectory()
  {
    // Arrange
    const string customDirectory = "Custom/Customer/Path";
    var customer = new Customer { Email = "test@example.com", AvailableRecipes = 1 };

    var customConfig = new CustomerConfig { OutputDirectory = customDirectory };
    var repository = new CustomerFileRepository(
        _mockFileService.Object,
        _mockFileWriteQueueService.Object,
        customConfig
    );

    // Act
    repository.SaveCustomer(customer);

    // Assert
    _mockFileWriteQueueService.Verify(
        fwqs => fwqs.QueueWrite(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Customer>(), It.IsAny<string?>()),
        Times.Once,
        "because the repository should use the configured output directory");
  }

  #endregion
}
