# PostgreSQL Database Maintenance Guide (Zarichney API)

This guide provides instructions for maintaining the PostgreSQL database used by the Zarichney API, focusing on the ASP.NET Core Identity system and related features like refresh tokens and API keys.

## Database Connection

The application connects to the PostgreSQL database (`zarichney_identity`) using the user `zarichney_user`.

**Production:**
* The connection string details (host, database name, username, password) are typically injected via environment variables or fetched securely at runtime from **AWS Secrets Manager** (Secret ID: `cookbook-factory-secrets`, Key: `DbPassword`) or potentially **AWS Systems Manager Parameter Store**, configured within the EC2 instance's environment or application startup. Do **not** rely on `appsettings.Production.json` for sensitive credentials like the password.

**Local Development:**
* Connection details are usually configured via `appsettings.Development.json` or .NET User Secrets. Example:
    ```json
    {
      "ConnectionStrings": {
        "IdentityConnection": "Host=localhost;Database=zarichney_identity;Username=postgres;Password=your_local_dev_password"
      }
    }
    ```

## Migrations (EF Core)

Database schema changes are managed using Entity Framework Core migrations.

### Production Deployment

* Migrations are **applied automatically** during the deployment pipeline (GitHub Actions).
* The pipeline generates a single, idempotent SQL script (`ApplyAllMigrations.sql`) containing all necessary schema changes.
* This script is copied to the EC2 instance (`/opt/cookbook-api/migrations/`).
* The deployment process executes the `/opt/cookbook-api/Server/Auth/Migrations/ApplyMigrations.sh` script on the EC2 instance.
* `ApplyMigrations.sh` uses `psql` to run the `ApplyAllMigrations.sql` script against the production database (`zarichney_identity`). The script ensures only pending migrations are applied.

### Local Development: Applying Migrations

Apply pending migrations directly against your local database:

```bash
# From the directory containing the .csproj file
dotnet ef database update --context UserDbContext
```

### Local Development: Creating New Migrations

When you change your C# entities or `DbContext`:

1.  Ensure your local database is up-to-date (`dotnet ef database update`).
2.  Add a new migration:
    ```bash
    # From the directory containing the .csproj file
    dotnet ef migrations add YourMigrationName --context UserDbContext --output-dir Server/Auth/Migrations
    ```
3.  This creates a new `.cs` file in `Server/Auth/Migrations` and updates `UserDbContextModelSnapshot.cs`. **Commit both files** to source control.

## Managing Users via Entity Framework (Recommended)

Use your application's API endpoints or internal services that leverage `UserManager<ApplicationUser>` and `RoleManager<IdentityRole>`.

### Creating Admin User Example

```csharp
// In a service or controller:
public async Task CreateAdminUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Create admin role if it doesn't exist
    string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // Create admin user
    var adminUser = new ApplicationUser { /* ... set properties ... */ };
    string adminPassword = "YourStrongPassword!"; // Use a secure password

    if (await userManager.FindByEmailAsync(adminUser.Email) == null)
    {
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
        // Handle errors if result.Succeeded is false
    }
}
```

### Resetting User Password Example

```csharp
// In a service or controller:
public async Task<IdentityResult> ResetUserPassword(UserManager<ApplicationUser> userManager, string email, string newPassword)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user != null)
    {
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return await userManager.ResetPasswordAsync(user, token, newPassword);
    }
    return IdentityResult.Failed(new IdentityError { Description = "User not found." });
}
```

## Managing Database via PostgreSQL Directly (Use with Caution)

Connect using `psql` (e.g., `sudo -i -u postgres psql -d zarichney_identity -U zarichney_user` or `psql -h <host> -U <user> -d zarichney_identity -W`).

### View Users

```sql
SELECT "Id", "UserName", "Email", "EmailConfirmed", "LockoutEnabled"
FROM "AspNetUsers";

-- View Users and Roles
SELECT u."UserName", r."Name" as "RoleName"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r ON ur."RoleId" = r."Id";
```

### View Roles

```sql
SELECT "Id", "Name" FROM "AspNetRoles";
```

### Unlock Account

```sql
UPDATE "AspNetUsers"
SET "LockoutEnd" = NULL, "AccessFailedCount" = 0
WHERE "Email" = '[email address removed]';
```

## Managing Refresh Tokens (Direct SQL)

```sql
-- View Recent Tokens (Limit Output)
SELECT "Id", "UserId", substring("Token" from 1 for 10) as "TokenPreview",
       "CreatedAt", "ExpiresAt", "IsUsed", "IsRevoked", "DeviceName"
FROM "RefreshTokens"
ORDER BY "CreatedAt" DESC LIMIT 50;

-- View Specific User's Active Tokens
SELECT r."Id", substring(r."Token" from 1 for 10) as "TokenPreview",
       r."CreatedAt", r."ExpiresAt", r."IsUsed", r."IsRevoked", r."DeviceName"
FROM "RefreshTokens" r
JOIN "AspNetUsers" u ON r."UserId" = u."Id"
WHERE u."Email" = 'user@example.com'
  AND r."ExpiresAt" >= NOW()
  AND r."IsUsed" = false
  AND r."IsRevoked" = false;

-- Manually Revoke All Tokens for a User
UPDATE "RefreshTokens"
SET "IsRevoked" = true
WHERE "UserId" = (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = '[email address removed]');

-- Manually Remove Expired/Used/Revoked Tokens (Note: Application should handle this)
DELETE FROM "RefreshTokens"
WHERE "ExpiresAt" < NOW() OR "IsUsed" = true OR "IsRevoked" = true;

-- Token Statistics
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
# Run from a machine with psql tools and access to the DB
pg_dump -U zarichney_user -h <db_host_or_ip> -d zarichney_identity -F c -b -v -f zarichney_identity_backup.dump
# (Will prompt for password, or use PGPASSWORD env var)
```
* `-F c`: Custom format (compressed, recommended).
* `-b`: Include large objects.
* `-v`: Verbose.

### Restore

```bash
# Run from a machine with psql tools and access to the DB
# Ensure database exists and is empty first if needed
# createdb -U zarichney_user -h <db_host_or_ip> zarichney_identity_restored (if restoring to new DB)
pg_restore -U zarichney_user -h <db_host_or_ip> -d <target_database_name> -v zarichney_identity_backup.dump
# (Will prompt for password, or use PGPASSWORD env var)
```
* `-d`: Target database (must exist).

## PostgreSQL DBA Cheatsheet

This section provides common PostgreSQL commands for database administration and maintenance.

### Connection Commands

```bash
# Connect to PostgreSQL server
psql -U postgres -h localhost

# Connect to specific database
psql -U postgres -h localhost -d zarichney_identity

# Connect with password prompt
psql -U postgres -h localhost -W
```

### Database Management

```sql
-- List all databases
\l

-- Create a new database
CREATE DATABASE new_database_name;

-- Switch to another database
\c database_name

-- Show current database
SELECT current_database();

-- Drop database (be careful!)
DROP DATABASE database_name;

-- Show database size
SELECT pg_size_pretty(pg_database_size('zarichney_identity'));
```

### Table Management

```sql
-- List all tables in current database
\dt

-- List all tables with more details
\dt+

-- List all tables in a specific schema
\dt schema_name.*

-- Describe table structure
\d table_name

-- Describe table structure with more details
\d+ table_name

-- Get table size
SELECT pg_size_pretty(pg_total_relation_size('table_name'));

-- List tables with their sizes
SELECT
    table_name,
    pg_size_pretty(pg_total_relation_size(quote_ident(table_name))) as size
FROM
    information_schema.tables
WHERE
    table_schema = 'public'
ORDER BY
    pg_total_relation_size(quote_ident(table_name)) DESC;
```

### Query Commands

```sql
-- Basic SELECT
SELECT * FROM table_name LIMIT 10;

-- Count rows in table
SELECT COUNT(*) FROM table_name;

-- Select with conditions
SELECT column1, column2 FROM table_name WHERE condition;

-- Group by with count
SELECT column1, COUNT(*) FROM table_name GROUP BY column1;
```

### Index Management

```sql
-- List all indexes on a table
\d table_name

-- Create an index
CREATE INDEX index_name ON table_name (column_name);

-- Create a unique index
CREATE UNIQUE INDEX index_name ON table_name (column_name);

-- Drop an index
DROP INDEX index_name;

-- List unused indexes
SELECT
    s.schemaname,
    s.relname AS tablename,
    s.indexrelname AS indexname,
    pg_size_pretty(pg_relation_size(s.indexrelid)) AS index_size,
    idx_scan AS index_scans
FROM
    pg_stat_user_indexes s
JOIN
    pg_index i ON i.indexrelid = s.indexrelid
WHERE
    s.idx_scan = 0      -- has never been scanned
    AND 0 <>ALL(i.indkey)  -- no index column is an expression
    AND i.indisunique IS FALSE -- is not a UNIQUE index
ORDER BY
    pg_relation_size(s.indexrelid) DESC;
```

### User/Role Management

```sql
-- List all roles/users
\du

-- Create new user
CREATE USER username WITH PASSWORD 'password';

-- Create user with specific privileges
CREATE USER username WITH PASSWORD 'password' CREATEDB;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE database_name TO username;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO username;

-- Revoke privileges
REVOKE ALL PRIVILEGES ON DATABASE database_name FROM username;

-- Alter user password
ALTER USER username WITH PASSWORD 'new_password';

-- Drop user
DROP USER username;
```

### Database Maintenance

```sql
-- Analyze database for statistics collection
ANALYZE;

-- Analyze specific table
ANALYZE table_name;

-- Vacuum to reclaim storage and update statistics
VACUUM;

-- Full vacuum (requires exclusive table lock)
VACUUM FULL;

-- Vacuum and analyze
VACUUM ANALYZE;

-- Reindex database
REINDEX DATABASE database_name;

-- Reindex specific table
REINDEX TABLE table_name;
```

### Performance Monitoring

```sql
-- Show active queries
SELECT pid, age(clock_timestamp(), query_start), usename, query
FROM pg_stat_activity
WHERE query != '<IDLE>' AND query NOT ILIKE '%pg_stat_activity%'
ORDER BY query_start desc;

-- Kill a specific query
SELECT pg_cancel_backend(pid);

-- Force kill a specific query
SELECT pg_terminate_backend(pid);

-- Table access statistics
SELECT schemaname, relname, seq_scan, seq_tup_read, idx_scan, idx_tup_fetch
FROM pg_stat_user_tables
ORDER BY seq_scan DESC;

-- Cache hit ratio
SELECT 
    sum(heap_blks_read) as heap_read,
    sum(heap_blks_hit)  as heap_hit,
    sum(heap_blks_hit) / (sum(heap_blks_hit) + sum(heap_blks_read)) as ratio
FROM 
    pg_statio_user_tables;
```

### Locks and Blocking

```sql
-- View locks
SELECT * FROM pg_locks;

-- View blocking locks
SELECT blocked_locks.pid     AS blocked_pid,
       blocked_activity.usename  AS blocked_user,
       blocking_locks.pid     AS blocking_pid,
       blocking_activity.usename AS blocking_user,
       blocked_activity.query    AS blocked_statement,
       blocking_activity.query   AS current_statement_in_blocking_process
FROM  pg_catalog.pg_locks         blocked_locks
JOIN pg_catalog.pg_stat_activity blocked_activity  ON blocked_activity.pid = blocked_locks.pid
JOIN pg_catalog.pg_locks         blocking_locks 
    ON blocking_locks.locktype = blocked_locks.locktype
    AND blocking_locks.database IS NOT DISTINCT FROM blocked_locks.database
    AND blocking_locks.relation IS NOT DISTINCT FROM blocked_locks.relation
    AND blocking_locks.page IS NOT DISTINCT FROM blocked_locks.page
    AND blocking_locks.tuple IS NOT DISTINCT FROM blocked_locks.tuple
    AND blocking_locks.virtualxid IS NOT DISTINCT FROM blocked_locks.virtualxid
    AND blocking_locks.transactionid IS NOT DISTINCT FROM blocked_locks.transactionid
    AND blocking_locks.classid IS NOT DISTINCT FROM blocked_locks.classid
    AND blocking_locks.objid IS NOT DISTINCT FROM blocked_locks.objid
    AND blocking_locks.objsubid IS NOT DISTINCT FROM blocked_locks.objsubid
    AND blocking_locks.pid != blocked_locks.pid
JOIN pg_catalog.pg_stat_activity blocking_activity ON blocking_activity.pid = blocking_locks.pid
WHERE NOT blocked_locks.granted;
```

### psql Meta-Commands

```
\q              -- Quit psql
\h              -- Help on SQL commands
\?              -- Help on psql commands
\l              -- List databases
\c DBNAME       -- Connect to database
\dt             -- List tables
\d TABLE        -- Describe table
\df             -- List functions
\dv             -- List views
\du             -- List users
\dn             -- List schemas
\timing         -- Toggle query execution time display
\x              -- Toggle expanded display
\e              -- Edit command in external editor
\i FILENAME     -- Execute commands from file
\o FILENAME     -- Send results to file
\copy           -- Import/export data to/from file
```

## Troubleshooting

### Check Migration Status

```sql
-- Connect to the database
-- \c zarichney_identity

-- View applied migrations
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId" DESC;
```

### Authentication / Connection Issues

* **Password Authentication Failed (`psql`/Application):**
    * Verify the password stored in AWS Secrets Manager (Key: `DbPassword`) is correct for the `zarichney_user`.
    * Ensure the `pg_hba.conf` file on the PostgreSQL server allows connections from the client's IP address (e.g., `127.0.0.1/32` for localhost from EC2) using the `scram-sha-256` or `md5` method for the `zarichney_user` and `zarichney_identity` database. Reload PostgreSQL after changes (`sudo systemctl reload postgresql`).
* **Ident Authentication Failed (`psql`):**
    * Usually occurs when connecting locally (`localhost`) as `ec2-user` but trying to authenticate as `zarichney_user`.
    * Fix: Modify `pg_hba.conf` to use `scram-sha-256` instead of `ident` for `host` connections from `127.0.0.1/32` for `zarichney_user`.
    * Alternatively, connect using `sudo -i -u postgres psql ...` for administrative tasks leveraging `peer` authentication if configured.
* **Permission Denied for Schema Public (`psql` during migration):**
    * The database user (`zarichney_user`) needs privileges to create objects in the schema.
    * Fix: Connect as a superuser (e.g., `postgres`) and run:
        ```sql
        GRANT USAGE, CREATE ON SCHEMA public TO zarichney_user;
        -- Optionally grant default privileges too:
        -- ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO zarichney_user;
        -- ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO zarichney_user;
        ```

### Verify User Login Issues (Direct SQL)

```sql
SELECT "UserName", "AccessFailedCount", "LockoutEnd"
FROM "AspNetUsers"
WHERE "AccessFailedCount" > 0 OR "LockoutEnd" IS NOT NULL;
```

## Security Best Practices

1.  Use strong, unique passwords for PostgreSQL users (`postgres`, `zarichney_user`).
2.  Store database passwords and sensitive configuration (like JWT secrets) securely using **AWS Secrets Manager** or **AWS Systems Manager Parameter Store (SecureString)**. Grant EC2 instance IAM roles minimal necessary permissions to access these secrets/parameters.
3.  Configure `pg_hba.conf` to restrict access. Only allow connections from specific IPs/networks needed (e.g., the EC2 instance's private IP or `localhost`). Use `scram-sha-256` authentication method.
4.  Use encrypted connections (SSL/TLS) between the application and the database, especially if the database is on a separate host (like RDS).
5.  Regularly backup the `zarichney_identity` database.
6.  Keep PostgreSQL, the operating system, and application dependencies updated.
7.  Keep JWT access tokens short-lived (e.g., 15-60 minutes).
8.  Ensure the refresh token cleanup mechanism (built into your app or a separate job) runs regularly.
9.  Enable detailed logging in PostgreSQL for security audits if required.

## Recommended Maintenance Schedule

- **Daily**: Check logs for unusual login patterns or failures
- **Daily**: Monitor refresh token usage patterns for suspicious activity
- **Weekly**: Backup the identity database
- **Weekly**: Clean up expired refresh tokens if automatic cleanup fails
- **Monthly**: Check for locked accounts and review access patterns
- **Quarterly**: Review all admin accounts and roles
- **Quarterly**: Analyze refresh token usage patterns and adjust token lifetimes if needed
- **Yearly**: Full security audit including password policies and JWT implementation