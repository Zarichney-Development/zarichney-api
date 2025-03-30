using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
// Remove IConfiguration and System.IO if only using hardcoded string

namespace Zarichney.Server.Auth; // Use the correct namespace

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        // Use a hardcoded, syntactically valid connection string for design-time operations.
        // The database doesn't need to exist or be reachable for script generation.
        // IMPORTANT: Replace password if you have security policies against any hardcoded credentials,
        // even dummy ones, though this isn't used for actual connections here.
        var designTimeString = "Host=localhost;Database=zarichney_design_time;Username=postgres;Password=dummy_password";

        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();

        optionsBuilder.UseNpgsql(designTimeString, options =>
            // Crucial: Point to the assembly containing your Migrations folder.
            // Your runtime code uses "Zarichney", so we match that.
            options.MigrationsAssembly("Zarichney")
        );

        return new UserDbContext(optionsBuilder.Options);
    }
}