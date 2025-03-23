# PostgreSQL Authentication Maintenance Guide

This guide provides instructions for maintaining the ASP.NET Core Identity authentication system using PostgreSQL for the Zarichney API.

## Database Connection

The authentication system uses PostgreSQL with the connection string defined in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Host=localhost;Database=zarichney_identity;Username=postgres;Password=your_password"
  }
}
```

## Migrations

### Running Migrations

Migrations are automatically applied during application startup. To manually apply migrations:

```bash
# From the project directory
dotnet ef database update --context UserDbContext
```

### Creating New Migrations

If you modify the `ApplicationUser` class or need to make other schema changes:

```bash
dotnet ef migrations add MigrationName --context UserDbContext --output-dir Migrations
```

## Managing Users via Entity Framework

### Creating Admin User

```csharp
using Microsoft.AspNetCore.Identity;
using Zarichney.Auth;

// In a service or controller:
public async Task CreateAdminUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Create admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    // Create admin user
    var adminUser = new ApplicationUser
    {
        UserName = "admin@example.com",
        Email = "admin@example.com",
        EmailConfirmed = true
    };
    
    if (await userManager.FindByEmailAsync(adminUser.Email) == null)
    {
        var result = await userManager.CreateAsync(adminUser, "StrongPassword123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
```

### Resetting User Password

```csharp
public async Task ResetUserPassword(UserManager<ApplicationUser> userManager, string email, string newPassword)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user != null)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, newPassword);
        
        if (result.Succeeded)
        {
            // Password reset successful
        }
    }
}
```

## Managing Users via PostgreSQL

While it's recommended to use the API or Entity Framework for user management, here are some direct PostgreSQL queries for administrative purposes.

### View Users

```sql
SELECT "Id", "UserName", "Email", "EmailConfirmed", "LockoutEnabled" 
FROM "AspNetUsers";

SELECT u."UserName", r."Name" as "RoleName"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r ON ur."RoleId" = r."Id";
```

### View Roles

```sql
SELECT "Id", "Name", "NormalizedName"
FROM "AspNetRoles";
```

### Unlock Account

```sql
UPDATE "AspNetUsers"
SET "LockoutEnd" = NULL
WHERE "Email" = 'user@example.com';
```

## Database Backup and Restore

### Backup

```bash
pg_dump -U postgres -h localhost -d zarichney_identity > zarichney_identity_backup.sql
```

### Restore

```bash
psql -U postgres -h localhost -d zarichney_identity < zarichney_identity_backup.sql
```

## Troubleshooting

### Check Migration Status

```sql
SELECT * FROM "__EFMigrationsHistory"
WHERE "MigrationId" LIKE 'Zarichney%';
```

### Verify User Login Issues

Check failed login attempts:

```sql
SELECT "UserName", "AccessFailedCount", "LockoutEnd"
FROM "AspNetUsers"
WHERE "AccessFailedCount" > 0 OR "LockoutEnd" IS NOT NULL;
```

### Database Connection Issues

Verify connection settings in `appsettings.json` and ensure that PostgreSQL is running:

```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# or on Windows with installed services
sc query postgresql
```

## Security Best Practices

1. Ensure the PostgreSQL server is properly secured with a strong password
2. Limit network access to the database server
3. Use encrypted connections (SSL/TLS) for database communication
4. Regularly backup the identity database
5. Enable detailed logging for security audits
6. Regularly update the application and dependencies to patch security vulnerabilities

## Recommended Maintenance Schedule

- **Daily**: Check logs for unusual login patterns or failures
- **Weekly**: Backup the identity database
- **Monthly**: Check for locked accounts and review access patterns
- **Quarterly**: Review all admin accounts and roles
- **Yearly**: Full security audit including password policies