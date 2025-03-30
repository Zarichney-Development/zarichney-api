using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zarichney.Server.Auth;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
         // Build configuration manually - assumes running from project root or similar
        // Adjust base path as necessary if running 'dotnet ef' from a different directory
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Assumes 'dotnet ef' runs from project root
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<UserDbContextFactory>() // Reads user secrets for this assembly
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("IdentityConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find 'IdentityConnection' connection string.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
            options.MigrationsAssembly("Zarichney")
        );

        return new UserDbContext(optionsBuilder.Options);
    }
}