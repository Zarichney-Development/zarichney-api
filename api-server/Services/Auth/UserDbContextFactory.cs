using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zarichney.Services.Auth;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
  public UserDbContext CreateDbContext(string[] args)
  {
    // Define the dummy connection string for fallback (CI/CD migration script generation)
    // Make the password clearly indicate its dummy nature.
    const string designTimeDummyConnectionString =
      "Host=localhost;Database=zarichney_design_time;Username=postgres;Password=dummy_password_for_scripting_only";

    string connectionString;

    try
    {
      // --- Attempt to load configuration (for local 'database update') ---

      // Determine the base path. Assumes 'dotnet ef' runs from the project directory.
      // If you run 'dotnet ef' from the solution root, you might need to adjust this,
      // e.g., Directory.GetCurrentDirectory() + "/Zarichney.Auth" (replace with your project path)
      // Adding robust path detection:
      var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
      var basePath = Directory.GetCurrentDirectory();

      // Simple check: If appsettings.json exists here, use it. Otherwise, try going up one level (common when running from solution dir)
      var appSettingsPath = Path.Combine(basePath, "appsettings.json");
      if (!File.Exists(appSettingsPath))
      {
        var parentDir = Directory.GetParent(basePath)?.FullName;
        if (parentDir != null && File.Exists(Path.Combine(parentDir, "appsettings.json")))
        {
          // This is heuristic - adjust if your structure is different
          // Or better, navigate down into the specific project folder if running from solution level
          // For now, let's assume running from project or parent is most common
          Console.WriteLine(
            $"INFO: appsettings.json not found in {basePath}, trying parent directory {parentDir} (adjust if needed).");
          // Note: UserSecrets often still resolve correctly relative to the csproj when run via 'dotnet ef'
        }
        else
        {
          Console.WriteLine(
            $"INFO: Could not find appsettings.json in {basePath} or its parent. Configuration loading might fail.");
        }
        // If running from solution dir, you might explicitly need:
        // basePath = Path.Combine(Directory.GetCurrentDirectory(), "YourProjectSubfolder");
      }


      var configuration = new ConfigurationBuilder()
        .SetBasePath(basePath) // Use determined path
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile($"appsettings.{environment ?? "Production"}.json", optional: true) // Include environment-specific
        .AddUserSecrets<UserDbContextFactory>(optional: true) // Make optional: prevents error if no user secrets
        .AddEnvironmentVariables()
        .Build();

      connectionString = configuration.GetConnectionString(UserDbContext.UserDatabaseConnectionName)!;

      // --- Fallback Logic ---
      if (string.IsNullOrEmpty(connectionString))
      {
        Console.WriteLine(
          $"INFO: '{UserDbContext.UserDatabaseConnectionName}' not found in configuration sources. Using dummy connection string for design-time operation (expected during migration script generation).");
        connectionString = designTimeDummyConnectionString;
      }
      else
      {
        Console.WriteLine(
          $"INFO: Found '{UserDbContext.UserDatabaseConnectionName}' via configuration sources. Using it for design-time operation (expected during local database update).");
      }
    }
    catch (Exception ex)
    {
      // Catch potential errors during configuration building (e.g., path issues, file permissions)
      Console.WriteLine(
        $"WARNING: Error building configuration: {ex.Message}. Falling back to dummy connection string.");
      connectionString = designTimeDummyConnectionString;
    }


    // --- Configure DbContextOptions ---
    var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
    optionsBuilder.UseNpgsql(connectionString, options =>
        // CRUCIAL: Ensure this assembly name is where your Migrations folder resides.
        // If your DbContext and Migrations are in the same project/assembly (e.g., Zarichney.Auth),
        // use that assembly's name. If your main application assembly is "Zarichney",
        // and migrations live there, use "Zarichney". Double-check your structure.
        options.MigrationsAssembly("Zarichney") // <<< Verify this assembly name
    );

    Console.WriteLine(
      $"INFO: Creating UserDbContext instance for design-time operation using MigrationsAssembly: 'Zarichney'."); // Log assembly name used

    return new UserDbContext(optionsBuilder.Options);
  }
}
