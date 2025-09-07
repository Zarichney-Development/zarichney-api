using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Cookbook.Customers;

namespace Zarichney.Tests.Framework.Mocks;

/// <summary>
/// Mock factory for CustomerService dependencies with common configurations
/// </summary>
public static class CustomerServiceMockFactory
{
    public static Mock<ICustomerRepository> CreateRepositoryMock()
    {
        var mock = new Mock<ICustomerRepository>();
        
        // Default setup for common scenarios
        mock.Setup(r => r.GetCustomerByEmail(It.IsAny<string>()))
            .ReturnsAsync((Customer?)null);
            
        mock.Setup(r => r.SaveCustomer(It.IsAny<Customer>()))
            .Verifiable();
            
        return mock;
    }

    public static Mock<ILogger<CustomerService>> CreateLoggerMock()
    {
        var mock = new Mock<ILogger<CustomerService>>();
        return mock;
    }

    public static CustomerConfig CreateDefaultConfig()
    {
        return new CustomerConfig
        {
            InitialFreeRecipes = 20,
            OutputDirectory = "Data/Customers"
        };
    }

    public static CustomerConfig CreateConfigWithInitialRecipes(int count)
    {
        return new CustomerConfig
        {
            InitialFreeRecipes = count,
            OutputDirectory = "Data/Customers"
        };
    }

    /// <summary>
    /// Creates a complete CustomerService with default mocked dependencies
    /// </summary>
    public static (CustomerService Service, Mock<ICustomerRepository> RepositoryMock, Mock<ILogger<CustomerService>> LoggerMock) CreateWithMocks(CustomerConfig? config = null)
    {
        var repositoryMock = CreateRepositoryMock();
        var loggerMock = CreateLoggerMock();
        var customerConfig = config ?? CreateDefaultConfig();

        var service = new CustomerService(repositoryMock.Object, customerConfig, loggerMock.Object);

        return (service, repositoryMock, loggerMock);
    }
}