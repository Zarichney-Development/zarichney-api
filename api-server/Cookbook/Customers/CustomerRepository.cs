using Zarichney.Services.Email;
using Zarichney.Services.FileSystem;

namespace Zarichney.Cookbook.Customers;

public interface ICustomerRepository
{
  Task<Customer?> GetCustomerByEmail(string email);
  void SaveCustomer(Customer customer);
}

public class CustomerFileRepository(
  IFileService fileService,
  IFileWriteQueueService fileWriteQueueService, // Inject IFileWriteQueueService
  CustomerConfig config
  ) : ICustomerRepository
{
  public async Task<Customer?> GetCustomerByEmail(string email)
  {
    var safeFileName = EmailService.MakeSafeFileName(email);

    // Use fileService for reading
    var customer = await fileService.ReadFromFile<Customer?>(
      Path.Combine(config.OutputDirectory),
      safeFileName
    );

    return customer;
  }

  public void SaveCustomer(Customer customer)
  {
    var safeFileName = EmailService.MakeSafeFileName(customer.Email);

    // Use fileWriteQueueService for writing
    fileWriteQueueService.QueueWrite(
      Path.Combine(config.OutputDirectory),
      safeFileName,
      customer
    );
  }

}
