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
    var safeFileName = MakeSafeFileName(email);

    var customer = await fileService.ReadFromFile<Customer?>(
      Path.Combine(config.OutputDirectory),
      safeFileName
    );

    return customer;
  }

  public void SaveCustomer(Customer customer)
  {
    var safeFileName = MakeSafeFileName(customer.Email);
    
    fileService.WriteToFileAsync(
      Path.Combine(config.OutputDirectory),
      safeFileName,
      customer
    );
  }

  private static string MakeSafeFileName(string email)
  {
    // convert "somebody@gmail.com" => "somebody_gmail_com"
    return email.Replace("@", "_at_").Replace(".", "_dot_");
  }
}