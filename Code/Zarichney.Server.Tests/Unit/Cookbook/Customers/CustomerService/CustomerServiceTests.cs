using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Customers;
using Zarichney.Tests.Framework.Mocks;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Cookbook.Customers.CustomerService;

/// <summary>
/// Unit tests for CustomerService focusing on isolated business logic validation
/// </summary>
[Trait("Category", "Unit")]
public class CustomerServiceTests
{
    #region GetOrCreateCustomer Tests

    [Fact]
    public async Task GetOrCreateCustomer_WithValidEmail_WhenCustomerExists_ReturnsExistingCustomer()
    {
        // Arrange
        const string email = "existing@test.com";
        var existingCustomer = new CustomerBuilder()
            .WithExistingCustomer()
            .WithEmail(email)
            .Build();

        var (sut, repositoryMock, _) = CustomerServiceMockFactory.CreateWithMocks();
        repositoryMock.Setup(r => r.GetCustomerByEmail(email))
            .ReturnsAsync(existingCustomer);

        // Act
        var result = await sut.GetOrCreateCustomer(email);

        // Assert
        result.Should().BeSameAs(existingCustomer, "because the existing customer should be returned without modification");
        result.Email.Should().Be(email, "because the email should match the requested email");
        
        repositoryMock.Verify(r => r.GetCustomerByEmail(email), Times.Once, "because the repository should be queried once");
        repositoryMock.Verify(r => r.SaveCustomer(It.IsAny<Customer>()), Times.Never, "because no new customer should be saved when one exists");
    }

    [Fact]
    public async Task GetOrCreateCustomer_WithValidEmail_WhenCustomerDoesNotExist_CreatesAndReturnsNewCustomer()
    {
        // Arrange
        const string email = "new@test.com";
        const int expectedInitialRecipes = 25;
        var config = CustomerServiceMockFactory.CreateConfigWithInitialRecipes(expectedInitialRecipes);
        var (sut, repositoryMock, loggerMock) = CustomerServiceMockFactory.CreateWithMocks(config);

        repositoryMock.Setup(r => r.GetCustomerByEmail(email))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await sut.GetOrCreateCustomer(email);

        // Assert
        result.Should().NotBeNull("because a new customer should be created");
        result.Email.Should().Be(email, "because the new customer should have the provided email");
        result.AvailableRecipes.Should().Be(expectedInitialRecipes, "because new customers should receive the configured initial recipe count");
        result.LifetimeRecipesUsed.Should().Be(0, "because new customers haven't used any recipes yet");
        result.LifetimePurchasedRecipes.Should().Be(0, "because new customers haven't purchased any recipes yet");

        repositoryMock.Verify(r => r.GetCustomerByEmail(email), Times.Once, "because the repository should be queried to check for existing customer");
        repositoryMock.Verify(r => r.SaveCustomer(It.Is<Customer>(c => c.Email == email)), Times.Once, "because the new customer should be saved immediately");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetOrCreateCustomer_WithInvalidEmail_ThrowsArgumentException(string invalidEmail)
    {
        // Arrange
        var (sut, _, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        var act = async () => await sut.GetOrCreateCustomer(invalidEmail);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email cannot be empty.*")
            .Where(ex => ex.ParamName == "email", "because the parameter name should be specified for the invalid email argument");
    }

    [Fact]
    public async Task GetOrCreateCustomer_WithNullEmail_ThrowsArgumentException()
    {
        // Arrange
        var (sut, _, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        var act = async () => await sut.GetOrCreateCustomer(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email cannot be empty.*")
            .Where(ex => ex.ParamName == "email", "because the parameter name should be specified for the null email argument");
    }

    [Fact]
    public async Task GetOrCreateCustomer_WithValidEmail_WhenCustomerDoesNotExist_LogsCustomerCreation()
    {
        // Arrange
        const string email = "logger@test.com";
        const int initialRecipes = 15;
        var config = CustomerServiceMockFactory.CreateConfigWithInitialRecipes(initialRecipes);
        var (sut, repositoryMock, loggerMock) = CustomerServiceMockFactory.CreateWithMocks(config);

        repositoryMock.Setup(r => r.GetCustomerByEmail(email))
            .ReturnsAsync((Customer?)null);

        // Act
        await sut.GetOrCreateCustomer(email);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Creating new Customer record for {email}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because customer creation should be logged for audit purposes");
    }

    #endregion

    #region DecrementRecipes Tests

    [Fact]
    public void DecrementRecipes_WithValidCustomerAndPositiveAmount_DecrementsAvailableRecipes()
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithAvailableRecipes(20)
            .WithLifetimeRecipesUsed(5)
            .Build();
        const int recipesToDecrement = 8;
        var (sut, _, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        sut.DecrementRecipes(customer, recipesToDecrement);

        // Assert
        customer.AvailableRecipes.Should().Be(12, "because 20 - 8 = 12 available recipes should remain");
        customer.LifetimeRecipesUsed.Should().Be(13, "because 5 + 8 = 13 recipes should be the new lifetime total");
    }

    [Fact]
    public void DecrementRecipes_WhenDecrementWouldGoNegative_ClampsAvailableRecipesToZero()
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithAvailableRecipes(5)
            .WithLifetimeRecipesUsed(10)
            .Build();
        const int recipesToDecrement = 10;
        var (sut, _, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        sut.DecrementRecipes(customer, recipesToDecrement);

        // Assert
        customer.AvailableRecipes.Should().Be(0, "because available recipes should never go below zero");
        customer.LifetimeRecipesUsed.Should().Be(20, "because lifetime used should still increment by the full amount");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void DecrementRecipes_WithNonPositiveAmount_DoesNothing(int nonPositiveAmount)
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithAvailableRecipes(15)
            .WithLifetimeRecipesUsed(8)
            .Build();
        var originalAvailable = customer.AvailableRecipes;
        var originalLifetime = customer.LifetimeRecipesUsed;
        var (sut, _, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        sut.DecrementRecipes(customer, nonPositiveAmount);

        // Assert
        customer.AvailableRecipes.Should().Be(originalAvailable, "because non-positive amounts should not change available recipes");
        customer.LifetimeRecipesUsed.Should().Be(originalLifetime, "because non-positive amounts should not change lifetime usage");
    }

    [Fact]
    public void DecrementRecipes_WithNullCustomer_ThrowsArgumentNullException()
    {
        // Arrange
        var (sut, _, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        var act = () => sut.DecrementRecipes(null!, 5);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("customer", "because null customer should not be allowed");
    }

    [Fact]
    public void DecrementRecipes_WithValidOperation_LogsRecipeDecrement()
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithEmail("logging@test.com")
            .WithAvailableRecipes(25)
            .WithLifetimeRecipesUsed(3)
            .Build();
        const int recipesToDecrement = 7;
        var (sut, _, loggerMock) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        sut.DecrementRecipes(customer, recipesToDecrement);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Customer {customer.Email} used {recipesToDecrement} recipes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because recipe usage should be logged for audit and monitoring purposes");
    }

    #endregion

    #region AddRecipes Tests

    [Fact]
    public async Task AddRecipes_WithValidCustomerAndPositiveAmount_AddsToAvailableRecipes()
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithAvailableRecipes(10)
            .WithLifetimePurchasedRecipes(5)
            .Build();
        const int recipesToAdd = 15;
        var (sut, repositoryMock, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        await sut.AddRecipes(customer, recipesToAdd);

        // Assert
        customer.AvailableRecipes.Should().Be(25, "because 10 + 15 = 25 available recipes");
        customer.LifetimePurchasedRecipes.Should().Be(20, "because 5 + 15 = 20 lifetime purchased recipes");
        repositoryMock.Verify(r => r.SaveCustomer(customer), Times.Once, "because the customer should be saved after adding recipes");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public async Task AddRecipes_WithNonPositiveAmount_DoesNothingAndReturnsCompletedTask(int nonPositiveAmount)
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithAvailableRecipes(12)
            .WithLifetimePurchasedRecipes(8)
            .Build();
        var originalAvailable = customer.AvailableRecipes;
        var originalPurchased = customer.LifetimePurchasedRecipes;
        var (sut, repositoryMock, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        var task = sut.AddRecipes(customer, nonPositiveAmount);
        await task;

        // Assert
        task.IsCompleted.Should().BeTrue("because the method should return a completed task");
        customer.AvailableRecipes.Should().Be(originalAvailable, "because non-positive amounts should not change available recipes");
        customer.LifetimePurchasedRecipes.Should().Be(originalPurchased, "because non-positive amounts should not change lifetime purchased");
        repositoryMock.Verify(r => r.SaveCustomer(It.IsAny<Customer>()), Times.Never, "because no save should occur for non-positive amounts");
    }

    [Fact]
    public async Task AddRecipes_WithNullCustomer_ThrowsArgumentNullException()
    {
        // Arrange
        var (sut, _, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        var act = async () => await sut.AddRecipes(null!, 10);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("customer", "because null customer should not be allowed");
    }

    [Fact]
    public async Task AddRecipes_WithValidOperation_LogsRecipeAddition()
    {
        // Arrange
        var customer = new CustomerBuilder()
            .WithEmail("purchase@test.com")
            .WithAvailableRecipes(5)
            .WithLifetimePurchasedRecipes(12)
            .Build();
        const int recipesToAdd = 20;
        var (sut, _, loggerMock) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        await sut.AddRecipes(customer, recipesToAdd);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Customer {customer.Email} purchased {recipesToAdd} recipes")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because recipe purchases should be logged for business intelligence and audit purposes");
    }

    #endregion

    #region SaveCustomer Tests

    [Fact]
    public void SaveCustomer_WithValidCustomer_CallsRepositorySave()
    {
        // Arrange
        var customer = new CustomerBuilder().WithDefaultValues().Build();
        var (sut, repositoryMock, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        sut.SaveCustomer(customer);

        // Assert
        repositoryMock.Verify(r => r.SaveCustomer(customer), Times.Once, "because SaveCustomer should delegate to the repository");
    }

    [Fact]
    public void SaveCustomer_WithNullCustomer_CallsRepositoryWithNull()
    {
        // Arrange
        var (sut, repositoryMock, _) = CustomerServiceMockFactory.CreateWithMocks();

        // Act
        sut.SaveCustomer(null!);

        // Assert
        repositoryMock.Verify(r => r.SaveCustomer(null!), Times.Once, "because the method should delegate to repository even with null input");
    }

    #endregion

    #region CustomerConfig Tests

    [Fact]
    public void CustomerConfig_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new CustomerConfig();

        // Assert
        config.InitialFreeRecipes.Should().Be(20, "because new customers should start with 20 free recipes by default");
        config.OutputDirectory.Should().Be("Data/Customers", "because customer data should be stored in the default directory");
    }

    [Fact]
    public void CustomerConfig_InitialFreeRecipes_CanBeSet()
    {
        // Arrange & Act
        var config = new CustomerConfig { InitialFreeRecipes = 50 };

        // Assert
        config.InitialFreeRecipes.Should().Be(50, "because InitialFreeRecipes should be configurable");
    }

    [Fact]
    public void CustomerConfig_OutputDirectory_CanBeInitialized()
    {
        // Arrange & Act
        var config = new CustomerConfig { OutputDirectory = "Custom/Path" };

        // Assert
        config.OutputDirectory.Should().Be("Custom/Path", "because OutputDirectory should be configurable during initialization");
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public async Task CustomerWorkflow_CompleteLifecycle_WorksCorrectly()
    {
        // Arrange - Simulate complete customer lifecycle
        const string email = "lifecycle@test.com";
        const int initialFreeRecipes = 30;
        var config = CustomerServiceMockFactory.CreateConfigWithInitialRecipes(initialFreeRecipes);
        var (sut, repositoryMock, _) = CustomerServiceMockFactory.CreateWithMocks(config);

        repositoryMock.Setup(r => r.GetCustomerByEmail(email))
            .ReturnsAsync((Customer?)null);

        // Act & Assert - Step 1: Create new customer
        var customer = await sut.GetOrCreateCustomer(email);
        customer.AvailableRecipes.Should().Be(initialFreeRecipes, "because new customer should start with configured free recipes");

        // Act & Assert - Step 2: Use some free recipes
        sut.DecrementRecipes(customer, 12);
        customer.AvailableRecipes.Should().Be(18, "because 12 recipes were used from the initial 30");
        customer.LifetimeRecipesUsed.Should().Be(12, "because 12 recipes have been used total");

        // Act & Assert - Step 3: Purchase additional recipes
        await sut.AddRecipes(customer, 25);
        customer.AvailableRecipes.Should().Be(43, "because 25 recipes were added to the remaining 18");
        customer.LifetimePurchasedRecipes.Should().Be(25, "because 25 recipes were purchased");

        // Act & Assert - Step 4: Use more recipes than available (test clamping)
        sut.DecrementRecipes(customer, 50);
        customer.AvailableRecipes.Should().Be(0, "because usage should clamp available recipes to zero");
        customer.LifetimeRecipesUsed.Should().Be(62, "because lifetime usage should track all 50 attempted uses");

        // Verify repository interactions
        repositoryMock.Verify(r => r.SaveCustomer(It.IsAny<Customer>()), Times.Exactly(2), 
            "because customer should be saved when created and when recipes are added");
    }

    #endregion
}