using Zarichney.Server.Services.Emails;
using Zarichney.Server.Services.FileSystem;

namespace Zarichney.Server.Cookbook.Customers;

public interface ICustomerRepository
{
  Task<Customer?> GetCustomerByEmail(string email);
  void SaveCustomer(Customer customer);
}

public class CustomerFileRepository(
  IFileService fileService,
  CustomerConfig config
  ) : ICustomerRepository
{
  public async Task<Customer?> GetCustomerByEmail(string email)
  {
    var safeFileName = EmailService.MakeSafeFileName(email);

    var customer = await fileService.ReadFromFile<Customer?>(
      Path.Combine(config.OutputDirectory),
      safeFileName
    );

    return customer;
  }

  public void SaveCustomer(Customer customer)
  {
    var safeFileName = EmailService.MakeSafeFileName(customer.Email);
    
    fileService.WriteToFileAsync(
      Path.Combine(config.OutputDirectory),
      safeFileName,
      customer
    );
  }

}