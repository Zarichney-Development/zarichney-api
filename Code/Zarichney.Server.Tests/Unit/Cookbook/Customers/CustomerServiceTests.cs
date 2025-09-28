using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Customers;
using Zarichney.Tests.Framework.Attributes;

namespace Zarichney.Tests.Unit.Cookbook.Customers;

/// <summary>
/// Unit tests for the CustomerService class.
/// Tests customer management functionality including creation, credit management, and repository interactions.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Component, TestCategories.Service)]
[Trait(TestCategories.Feature, TestCategories.Cookbook)]
[Trait(TestCategories.Mutability, TestCategories.ReadOnly)]
public class CustomerServiceTests
{
  private readonly Mock<ICustomerRepository> _mockCustomerRepository;
  private readonly Mock<ILogger<CustomerService>> _mockLogger;
  private readonly CustomerConfig _customerConfig;
  private readonly CustomerService _customerService;

  public CustomerServiceTests()
  {
    // Arrange - Setup mocks and configuration
    _mockCustomerRepository = new Mock<ICustomerRepository>();
    _mockLogger = new Mock<ILogger<CustomerService>>();
    _customerConfig = new CustomerConfig
    {
      InitialFreeRecipes = 20,
      OutputDirectory = "Data/Customers"
    };

    // Create service with mocked dependencies
    _customerService = new CustomerService(
        _mockCustomerRepository.Object,
        _customerConfig,
        _mockLogger.Object
    );
  }

  #region GetOrCreateCustomer Tests

  [Fact]
  public async Task GetOrCreateCustomer_WithExistingCustomer_ReturnsExistingCustomer()
  {
    // Arrange
    const string email = "existing@example.com";
    var existingCustomer = new Customer
    {
      Email = email,
      AvailableRecipes = 15,
      LifetimeRecipesUsed = 5,
      LifetimePurchasedRecipes = 0
    };

    _mockCustomerRepository
        .Setup(repo => repo.GetCustomerByEmail(email))
        .ReturnsAsync(existingCustomer);

    // Act
    var result = await _customerService.GetOrCreateCustomer(email);

    // Assert
    result.Should().BeEquivalentTo(existingCustomer,
        "because an existing customer should be returned unchanged");
    result.Email.Should().Be(email,
        "because the returned customer should have the requested email");
    result.AvailableRecipes.Should().Be(15,
        "because existing customer's available recipes should be preserved");

    _mockCustomerRepository.Verify(
        repo => repo.GetCustomerByEmail(email),
        Times.Once,
        "because the repository should be queried once for the existing customer");
    _mockCustomerRepository.Verify(
        repo => repo.SaveCustomer(It.IsAny<Customer>()),
        Times.Never,
        "because existing customers should not be saved again");
  }

  [Fact]
  public async Task GetOrCreateCustomer_WithNewCustomer_CreatesAndReturnsNewCustomer()
  {
    // Arrange
    const string email = "new@example.com";

    _mockCustomerRepository
        .Setup(repo => repo.GetCustomerByEmail(email))
        .ReturnsAsync((Customer?)null);

    Customer savedCustomer = null!;
    _mockCustomerRepository
        .Setup(repo => repo.SaveCustomer(It.IsAny<Customer>()))
        .Callback<Customer>(customer => savedCustomer = customer);

    // Act
    var result = await _customerService.GetOrCreateCustomer(email);

    // Assert
    result.Should().NotBeNull("because a new customer should be created");
    result.Email.Should().Be(email,
        "because the new customer should have the specified email");
    result.AvailableRecipes.Should().Be(_customerConfig.InitialFreeRecipes,
        "because new customers should receive the configured initial free recipes");
    result.LifetimeRecipesUsed.Should().Be(0,
        "because new customers should start with zero recipes used");
    result.LifetimePurchasedRecipes.Should().Be(0,
        "because new customers should start with zero purchased recipes");

    _mockCustomerRepository.Verify(
        repo => repo.GetCustomerByEmail(email),
        Times.Once,
        "because the repository should be queried once to check for existing customer");
    _mockCustomerRepository.Verify(
        repo => repo.SaveCustomer(It.IsAny<Customer>()),
        Times.Once,
        "because the new customer should be saved immediately");

    savedCustomer.Should().NotBeNull("because SaveCustomer should have been called");
    savedCustomer.Should().BeEquivalentTo(result,
        "because the saved customer should match the returned customer");
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public async Task GetOrCreateCustomer_WithInvalidEmail_ThrowsArgumentException(string? invalidEmail)
  {
    // Act
    var act = () => _customerService.GetOrCreateCustomer(invalidEmail!);

    // Assert
    await act.Should().ThrowAsync<ArgumentException>("because invalid email addresses should be rejected")
        .WithMessage("Email cannot be empty.*")
        .WithParameterName("email");

    _mockCustomerRepository.Verify(
        repo => repo.GetCustomerByEmail(It.IsAny<string>()),
        Times.Never,
        "because repository should not be called with invalid email");
  }

  #endregion

  #region DecrementRecipes Tests

  [Fact]
  public void DecrementRecipes_WithValidCustomerAndPositiveAmount_DecrementsCorrectly()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = 10,
      LifetimeRecipesUsed = 5
    };
    const int recipesToDecrement = 3;

    // Act
    _customerService.DecrementRecipes(customer, recipesToDecrement);

    // Assert
    customer.AvailableRecipes.Should().Be(7,
        "because 3 recipes should be subtracted from the original 10");
    customer.LifetimeRecipesUsed.Should().Be(8,
        "because lifetime used should increase by 3 from original 5");
  }

  [Fact]
  public void DecrementRecipes_WithInsufficientRecipes_ClampsToZero()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = 2,
      LifetimeRecipesUsed = 8
    };
    const int recipesToDecrement = 5;

    // Act
    _customerService.DecrementRecipes(customer, recipesToDecrement);

    // Assert
    customer.AvailableRecipes.Should().Be(0,
        "because available recipes should not go below zero");
    customer.LifetimeRecipesUsed.Should().Be(13,
        "because lifetime used should still increase by the full amount");
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  [InlineData(-5)]
  public void DecrementRecipes_WithZeroOrNegativeAmount_DoesNothing(int amount)
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = 10,
      LifetimeRecipesUsed = 5
    };
    const int originalAvailable = 10;
    const int originalUsed = 5;

    // Act
    _customerService.DecrementRecipes(customer, amount);

    // Assert
    customer.AvailableRecipes.Should().Be(originalAvailable,
        "because zero or negative amounts should not change available recipes");
    customer.LifetimeRecipesUsed.Should().Be(originalUsed,
        "because zero or negative amounts should not change lifetime used");
  }

  [Fact]
  public void DecrementRecipes_WithNullCustomer_ThrowsArgumentNullException()
  {
    // Act
    var act = () => _customerService.DecrementRecipes(null!, 5);

    // Assert
    act.Should().Throw<ArgumentNullException>("because null customer should be rejected")
        .WithParameterName("customer");
  }

  #endregion

  #region AddRecipes Tests

  [Fact]
  public async Task AddRecipes_WithValidCustomerAndPositiveAmount_AddsCorrectly()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = 5,
      LifetimePurchasedRecipes = 0
    };
    const int recipesToAdd = 10;

    // Act
    await _customerService.AddRecipes(customer, recipesToAdd);

    // Assert
    customer.AvailableRecipes.Should().Be(15,
        "because 10 recipes should be added to the original 5");
    customer.LifetimePurchasedRecipes.Should().Be(10,
        "because lifetime purchased should increase by 10");

    _mockCustomerRepository.Verify(
        repo => repo.SaveCustomer(customer),
        Times.Once,
        "because customer should be saved after adding recipes");
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  [InlineData(-5)]
  public async Task AddRecipes_WithZeroOrNegativeAmount_DoesNothing(int amount)
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = 5,
      LifetimePurchasedRecipes = 0
    };
    const int originalAvailable = 5;
    const int originalPurchased = 0;

    // Act
    await _customerService.AddRecipes(customer, amount);

    // Assert
    customer.AvailableRecipes.Should().Be(originalAvailable,
        "because zero or negative amounts should not change available recipes");
    customer.LifetimePurchasedRecipes.Should().Be(originalPurchased,
        "because zero or negative amounts should not change lifetime purchased");

    _mockCustomerRepository.Verify(
        repo => repo.SaveCustomer(It.IsAny<Customer>()),
        Times.Never,
        "because customer should not be saved when no changes are made");
  }

  [Fact]
  public async Task AddRecipes_WithNullCustomer_ThrowsArgumentNullException()
  {
    // Act
    var act = () => _customerService.AddRecipes(null!, 10);

    // Assert
    var exception = await act.Should().ThrowAsync<ArgumentNullException>();
    exception.Which.ParamName.Should().Be("customer", "because null customer should be rejected");
  }

  #endregion

  #region SaveCustomer Tests

  [Fact]
  public void SaveCustomer_WithValidCustomer_CallsRepository()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = 15
    };

    // Act
    _customerService.SaveCustomer(customer);

    // Assert
    _mockCustomerRepository.Verify(
        repo => repo.SaveCustomer(customer),
        Times.Once,
        "because the repository save method should be called exactly once");
  }

  #endregion

  #region Edge Cases and Complex Scenarios

  [Fact]
  public async Task GetOrCreateCustomer_WithDifferentCasing_TreatsAsNewCustomer()
  {
    // Arrange
    const string existingEmail = "test@example.com";
    const string upperCaseEmail = "TEST@EXAMPLE.COM";

    var existingCustomer = new Customer
    {
      Email = existingEmail,
      AvailableRecipes = 10
    };

    _mockCustomerRepository
        .Setup(repo => repo.GetCustomerByEmail(existingEmail))
        .ReturnsAsync(existingCustomer);
    _mockCustomerRepository
        .Setup(repo => repo.GetCustomerByEmail(upperCaseEmail))
        .ReturnsAsync((Customer?)null);

    // Act
    var result = await _customerService.GetOrCreateCustomer(upperCaseEmail);

    // Assert
    result.Should().NotBeNull("because a new customer should be created for different casing");
    result.Email.Should().Be(upperCaseEmail,
        "because the new customer should preserve the exact email casing provided");
    result.AvailableRecipes.Should().Be(_customerConfig.InitialFreeRecipes,
        "because this should be treated as a new customer");

    _mockCustomerRepository.Verify(
        repo => repo.GetCustomerByEmail(upperCaseEmail),
        Times.Once,
        "because the repository should be queried with the exact email provided");
  }

  [Fact]
  public void DecrementRecipes_WithLargeNumbers_HandlesCorrectly()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = int.MaxValue - 1,
      LifetimeRecipesUsed = 1000000
    };
    const int recipesToDecrement = 2000000;

    // Act
    _customerService.DecrementRecipes(customer, recipesToDecrement);

    // Assert
    customer.AvailableRecipes.Should().Be(int.MaxValue - 1 - recipesToDecrement,
        "because large number arithmetic should work correctly");
    customer.LifetimeRecipesUsed.Should().Be(1000000 + recipesToDecrement,
        "because lifetime used should handle large increments");
  }

  [Fact]
  public async Task AddRecipes_WithLargeNumbers_HandlesCorrectly()
  {
    // Arrange
    var customer = new Customer
    {
      Email = "test@example.com",
      AvailableRecipes = 1000000,
      LifetimePurchasedRecipes = 500000
    };
    const int recipesToAdd = 2000000;

    // Act
    await _customerService.AddRecipes(customer, recipesToAdd);

    // Assert
    customer.AvailableRecipes.Should().Be(1000000 + recipesToAdd,
        "because large number arithmetic should work correctly");
    customer.LifetimePurchasedRecipes.Should().Be(500000 + recipesToAdd,
        "because lifetime purchased should handle large increments");
  }

  #endregion
}
