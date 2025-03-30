# PostgreSQL Authentication Maintenance Guide

This guide provides instructions for maintaining the ASP.NET Core Identity authentication system using PostgreSQL for the Zarichney API, including the refresh token functionality.

## Database Connection

The authentication system uses PostgreSQL with the connection string defined in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Host=localhost;Database=zarichney_identity;Username=postgres;Password=your_password"
  },
  "JwtSettings": {
    "SecretKey": "your_strong_secret_key_at_least_32_characters_long",
    "Issuer": "ZarichneyApi",
    "Audience": "ZarichneyClients",
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30,
    "TokenCleanupIntervalHours": 24
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
dotnet ef migrations add MigrationName --context UserDbContext --output-dir ./zarichney-api/Server/Auth/Migrations
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

## Managing Refresh Tokens

The system uses refresh tokens to enhance security and improve user experience. Here are some useful queries for managing refresh tokens.

### View All Refresh Tokens

```sql
SELECT id, "UserId", substring("Token" from 1 for 15) as "TokenPreview", 
       "CreatedAt", "ExpiresAt", "IsUsed", "IsRevoked" 
FROM "RefreshTokens";
```

### View User's Refresh Tokens

```sql
SELECT r.id, substring(r."Token" from 1 for 15) as "TokenPreview", 
       r."CreatedAt", r."ExpiresAt", r."IsUsed", r."IsRevoked"
FROM "RefreshTokens" r
JOIN "AspNetUsers" u ON r."UserId" = u."Id"
WHERE u."Email" = 'user@example.com';
```

### Revoke All Tokens for a User

```sql
UPDATE "RefreshTokens"
SET "IsRevoked" = true
WHERE "UserId" = (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'user@example.com');
```

### Remove Expired or Used Tokens

```sql
DELETE FROM "RefreshTokens"
WHERE "ExpiresAt" < NOW() OR "IsUsed" = true OR "IsRevoked" = true;
```

### Check Token Statistics

```sql
SELECT 
  COUNT(*) as total_tokens,
  COUNT(*) FILTER (WHERE "ExpiresAt" < NOW()) as expired_tokens,
  COUNT(*) FILTER (WHERE "IsUsed" = true) as used_tokens,
  COUNT(*) FILTER (WHERE "IsRevoked" = true) as revoked_tokens,
  COUNT(*) FILTER (WHERE "ExpiresAt" >= NOW() AND "IsUsed" = false AND "IsRevoked" = false) as active_tokens
FROM "RefreshTokens";
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

### Check Refresh Token Issues

Identify potential issues with refresh tokens:

```sql
-- Find users with no active refresh tokens
SELECT u."Email"
FROM "AspNetUsers" u
LEFT JOIN (
  SELECT "UserId" 
  FROM "RefreshTokens" 
  WHERE "ExpiresAt" >= NOW() AND "IsUsed" = false AND "IsRevoked" = false
) r ON u."Id" = r."UserId"
WHERE r."UserId" IS NULL;

-- Check for suspicious token activity (multiple tokens created in short time)
SELECT u."Email", COUNT(r."Id") as token_count, MIN(r."CreatedAt") as first_token, MAX(r."CreatedAt") as last_token
FROM "RefreshTokens" r
JOIN "AspNetUsers" u ON r."UserId" = u."Id"
WHERE r."CreatedAt" > NOW() - INTERVAL '24 HOURS'
GROUP BY u."Email"
HAVING COUNT(r."Id") > 10;
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
7. Keep JWT tokens short-lived (15-30 minutes) to reduce risk if compromised
8. Use the refresh token cleanup service to automatically remove expired tokens
9. Store JWT secret keys securely using AWS Parameter Store or similar services
10. Enable token revocation on logout to prevent token reuse

## Recommended Maintenance Schedule

- **Daily**: Check logs for unusual login patterns or failures
- **Daily**: Monitor refresh token usage patterns for suspicious activity
- **Weekly**: Backup the identity database
- **Weekly**: Clean up expired refresh tokens if automatic cleanup fails
- **Monthly**: Check for locked accounts and review access patterns
- **Quarterly**: Review all admin accounts and roles
- **Quarterly**: Analyze refresh token usage patterns and adjust token lifetimes if needed
- **Yearly**: Full security audit including password policies and JWT implementation