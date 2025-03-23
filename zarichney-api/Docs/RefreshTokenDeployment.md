# Refresh Token Deployment Guide for AWS EC2

This guide provides comprehensive instructions for deploying the refresh token implementation to your AWS EC2 production environment. It covers PostgreSQL setup, configuration, migration steps, and verification.

## 1. Prerequisites

Before beginning the deployment, ensure you have:

* SSH access to your AWS EC2 instance
* Administrative (sudo) privileges on the EC2 instance
* Basic knowledge of PostgreSQL, .NET, and systemd services

## 2. PostgreSQL Installation and Setup

If PostgreSQL is not already installed on your EC2 instance, follow these steps:

```bash
# Update package information
sudo apt update

# Install PostgreSQL (for Ubuntu/Debian based EC2 instances)
sudo apt install postgresql postgresql-contrib -y

# For Amazon Linux 2
# sudo amazon-linux-extras install postgresql13
# sudo yum install postgresql postgresql-server postgresql-devel postgresql-contrib -y
# sudo postgresql-setup initdb

# Start PostgreSQL service
sudo systemctl enable postgresql
sudo systemctl start postgresql

# Verify PostgreSQL is running
sudo systemctl status postgresql
```

### Configure PostgreSQL for Remote Access (Optional)

If you need to access the PostgreSQL server remotely:

```bash
# Edit PostgreSQL configuration
sudo nano /etc/postgresql/*/main/postgresql.conf

# Uncomment and modify the listen_addresses line
# listen_addresses = '*'

# Edit client authentication configuration
sudo nano /etc/postgresql/*/main/pg_hba.conf

# Add the following line to allow connections from your IP
# host    all             all             your_ip/32            md5

# Restart PostgreSQL
sudo systemctl restart postgresql
```

### Create Database and User

```bash
# Switch to postgres user
sudo -i -u postgres

# Create database
createdb zarichney_identity

# Create user with password
psql -c "CREATE USER zarichney_user WITH PASSWORD 'strong_password';"

# Grant privileges
psql -c "GRANT ALL PRIVILEGES ON DATABASE zarichney_identity TO zarichney_user;"

# Exit postgres user shell
exit
```

## 3. Configure Application Settings

### Update Connection String in AWS Parameter Store

For enhanced security, store your connection string in AWS Parameter Store:

```bash
# Using AWS CLI
aws ssm put-parameter \
    --name "/cookbook-api/identity-connection" \
    --value "Host=localhost;Database=zarichney_identity;Username=zarichney_user;Password=strong_password" \
    --type SecureString \
    --overwrite
```

### Update Application Configuration

On the EC2 instance, update the `appsettings.Production.json` file:

```bash
# Navigate to the app directory
cd /opt/cookbook-api

# Edit appsettings.Production.json
sudo nano appsettings.Production.json
```

Add or update the relevant settings:

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "Host=localhost;Database=zarichney_identity;Username=zarichney_user;Password=strong_password"
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

For AWS Systems Manager integration (recommended):

```json
{
  "ConnectionStrings": {
    "IdentityConnection": "${ssm:/cookbook-api/identity-connection}"
  },
  "JwtSettings": {
    "SecretKey": "${ssm:/cookbook-api/jwt-secret-key}",
    "Issuer": "ZarichneyApi",
    "Audience": "ZarichneyClients",
    "ExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30,
    "TokenCleanupIntervalHours": 24
  }
}
```

Then add the JWT secret key to AWS Parameter Store:

```bash
aws ssm put-parameter \
    --name "/cookbook-api/jwt-secret-key" \
    --value "your_strong_secret_key_at_least_32_characters_long" \
    --type SecureString \
    --overwrite
```

## 4. Deploy Updated Application

### Stop the Service

```bash
sudo systemctl stop cookbook-api
```

### Deploy New Files

Upload the updated application files to your EC2 instance using SCP or your preferred method:

```bash
# From your local development machine
scp -r ./zarichney-api/* zarichney-api:/opt/cookbook-api/
```

Or if you use a CI/CD pipeline, ensure it deploys the updated files to `/opt/cookbook-api/`.

### Set Correct Permissions

```bash
sudo chown -R ec2-user:ec2-user /opt/cookbook-api/
sudo chmod +x /opt/cookbook-api/start-cookbook.sh
```

## 5. Apply Database Migrations

### Manual Migration

For your first deployment, it's recommended to apply the migration manually to ensure it works correctly:

```bash
# Navigate to the application directory
cd /opt/cookbook-api

# Run the migration script
dotnet ef database update AddRefreshTokenSchema --context UserDbContext
```

### Using the Migration Script

Alternatively, use the provided migration script:

```bash
# Make the script executable
chmod +x /opt/cookbook-api/Server/Auth/Migrations/ApplyRefreshTokenMigration.sh

# Run the script
cd /opt/cookbook-api/Server/Auth/Migrations/
./ApplyRefreshTokenMigration.sh
```

### Verify Migration Success

Check that the migration was applied successfully:

```bash
# Connect to PostgreSQL
sudo -i -u postgres psql -d zarichney_identity

# Check migration history
SELECT * FROM "__EFMigrationsHistory";

# Verify RefreshTokens table exists
\dt

# Check RefreshTokens table structure
\d "RefreshTokens"

# Exit PostgreSQL
\q
```

## 6. Start and Monitor the Service

### Start the Service

```bash
sudo systemctl start cookbook-api
```

### Monitor Logs for Issues

```bash
# Check service status
sudo systemctl status cookbook-api

# Monitor logs in real-time
sudo journalctl -u cookbook-api -f
```

## 7. Test the Refresh Token Implementation

### Test Login Endpoint

Use curl or Postman to test the login endpoint:

```bash
curl -X POST https://zarichney.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"YourPassword"}'
```

The response should include both a JWT token and a refresh token:

```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR...",
  "refreshToken": "JBn6QvRtprVCwuuVXKQSEKgEfPfZZzt9...",
  "email": "user@example.com"
}
```

### Test Refresh Endpoint

Test the refresh token endpoint:

```bash
curl -X POST https://zarichney.com/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"JBn6QvRtprVCwuuVXKQSEKgEfPfZZzt9..."}'
```

Verify that it returns a new JWT token and refresh token:

```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "token": "eyJhbGciOiJIUzI1NiIsInR...",
  "refreshToken": "7PBZft54PQwuuVmtGEfPfZSEKgEQSvRJz...",
  "email": "user@example.com"
}
```

## 8. Troubleshooting

### Common Issues

#### Database Connection Errors

```bash
# Check PostgreSQL is running
sudo systemctl status postgresql

# Check logs for connection issues
sudo journalctl -u cookbook-api | grep "IdentityConnection"
```

#### Migrations Failed

If migrations fail to apply:

```bash
# Check migration history
sudo -i -u postgres
psql -d zarichney_identity -c "SELECT * FROM \"__EFMigrationsHistory\""

# Try applying migration manually with verbose output
dotnet ef database update AddRefreshTokenSchema --context UserDbContext --verbose
```

#### Refresh Token Not Working

Check the database for token issues:

```bash
# Connect to PostgreSQL
sudo -i -u postgres psql -d zarichney_identity

# Check tokens in database
SELECT id, "UserId", substring("Token" from 1 for 10) as "TokenPreview", "CreatedAt", "ExpiresAt", "IsUsed", "IsRevoked" FROM "RefreshTokens";
```

## 9. Maintenance and Monitoring

### Monitor Token Usage

Create a script to monitor refresh token usage and detect suspicious patterns:

```bash
# Check for unusual token usage patterns
sudo -i -u postgres
psql -d zarichney_identity -c "SELECT COUNT(*) FROM \"RefreshTokens\" WHERE \"CreatedAt\" > NOW() - INTERVAL '24 HOURS'"
```

### Cleanup Script

The built-in cleanup service will automatically remove expired, used, or revoked tokens. Monitor its effectiveness:

```bash
# Check token counts
sudo -i -u postgres
psql -d zarichney_identity -c "SELECT COUNT(*) as total, COUNT(*) FILTER (WHERE \"ExpiresAt\" < NOW()) as expired, COUNT(*) FILTER (WHERE \"IsUsed\" = true) as used, COUNT(*) FILTER (WHERE \"IsRevoked\" = true) as revoked FROM \"RefreshTokens\""
```

### Backup Refresh Tokens

Include refresh tokens in your backup strategy:

```bash
# Backup the identity database including refresh tokens
pg_dump -U postgres -h localhost -d zarichney_identity > /backup/zarichney_identity_$(date +%Y%m%d).sql
```

## 10. Security Considerations

1. **JWT Secret Key**: Ensure your JWT secret key is strong (at least 32 characters) and stored securely
2. **Token Lifetimes**: Keep access tokens short-lived (15-30 minutes) and refresh tokens reasonably long-lived (7-30 days)
3. **Database Security**: Ensure PostgreSQL is properly secured with strong passwords and limited network access
4. **HTTPS**: Always use HTTPS for all API endpoints
5. **Rate Limiting**: Consider implementing rate limiting for authentication endpoints to prevent brute force attacks
6. **Revocation**: Use the provided `api/auth/revoke` endpoint to revoke refresh tokens when users log out

## 11. Additional Resources

- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [AWS Systems Manager Parameter Store](https://docs.aws.amazon.com/systems-manager/latest/userguide/systems-manager-parameter-store.html)

## Conclusion

This deployment guide covers all aspects of deploying the refresh token implementation to your AWS EC2 environment. If you encounter any issues not covered in this guide, refer to the troubleshooting section or review the application logs for more details.