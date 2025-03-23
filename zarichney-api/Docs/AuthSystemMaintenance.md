# Authentication System Maintenance Guide

This document provides guidelines for maintaining and troubleshooting the Zarichney API authentication system, including user management, token handling, and email verification.

## System Overview

The authentication system consists of:

1. **User Management** - Registration, login, and profile management
2. **Token Management** - JWT access tokens and refresh tokens
3. **Email Verification** - Email confirmation for new accounts
4. **Password Reset** - Secure password reset functionality

## Configuration Settings

Authentication settings are defined in `appsettings.json`:

```json
{
  "AppUrl": "http://localhost:5173",
  "ConnectionStrings": {
    "IdentityConnection": "connection-string-here"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "ZarichneyApi",
    "Audience": "ZarichneyClients",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30,
    "TokenCleanupIntervalHours": 24
  },
  "EmailConfig": {
    "FromEmail": "notifications@example.com",
    "AzureTenantId": "tenant-id",
    "AzureAppId": "app-id",
    "AzureAppSecret": "app-secret",
    "MailCheckApiKey": "mailcheck-api-key",
    "TemplateDirectory": "EmailTemplates"
  }
}
```

### Key Configuration Options

| Setting | Description | Recommendations |
|---------|-------------|----------------|
| `AppUrl` | Base URL for the application, used in email links | Must match your frontend deployment URL |
| `JwtSettings:SecretKey` | Secret key for signing JWTs | Use at least 32 random characters; rotate periodically |
| `JwtSettings:ExpiryMinutes` | JWT token lifetime | 15-60 minutes is typical; shorter is more secure |
| `JwtSettings:RefreshTokenExpiryDays` | Refresh token lifetime | 7-30 days is typical; adjust based on security needs |
| `EmailConfig` | Email service configuration | Required for email verification |

## Token Management

### JWT Access Tokens

- **Purpose**: Short-lived tokens for API authentication
- **Storage**: Client-side only (typically in memory or secure HTTP-only cookies)
- **Expiration**: Configured via `JwtSettings:ExpiryMinutes`
- **Claims**: User ID, email, and other essential information

### Refresh Tokens

- **Purpose**: Long-lived tokens to obtain new access tokens
- **Storage**: Server-side in database with client reference
- **Expiration**: Configured via `JwtSettings:RefreshTokenExpiryDays`
- **Security Features**:
  - One-time use (marked as used after first use)
  - Can be revoked by user or admin
  - Automatically cleaned up by background service

## Email Verification

The system implements email verification for new accounts:

1. When a user registers, a confirmation token is generated
2. An email with a verification link is sent to the user
3. The user must click the link to verify their email address
4. Until verified, the user cannot log in

### Email Templates

Email templates are stored in the `EmailTemplates` directory:

- `base.html` - Base template for all emails
- `email-verification.html` - Email verification template
- `password-reset.html` - Password reset template
- `password-reset-confirmation.html` - Password reset confirmation

To modify templates:
1. Edit the HTML files in `EmailTemplates` directory
2. Test changes using the email preview functionality
3. Deploy updated templates with your application

## Regular Maintenance Tasks

### Token Cleanup

Expired refresh tokens are automatically cleaned up by `RefreshTokenCleanupService`. Verify this service is running correctly by checking the logs.

### User Account Auditing

Periodically review:
- Inactive accounts (no login for extended periods)
- Unverified accounts (registered but never confirmed email)
- Failed login attempts (potential security threats)

### Email Deliverability

- Monitor email delivery rates and bounce rates
- Test email templates periodically for rendering issues
- Verify SPF, DKIM, and DMARC records if using custom domains

## Troubleshooting Guide

### User Cannot Log In

1. Verify the user exists in the database
2. Check if email is confirmed (`EmailConfirmed = true`)
3. Ensure password hash is valid
4. Check for account lockout due to failed attempts

### Token Validation Failures

1. Verify `JwtSettings:SecretKey` matches across all instances
2. Check token expiration times
3. Validate proper token format
4. Confirm JWT signature algorithm matches server configuration

### Email Verification Issues

1. Check email service configuration and credentials
2. Verify user's email is correctly stored
3. Test email template rendering
4. Check for token expiration issues

### Database Maintenance

The auth system uses the `UserDbContext` with the following key tables:

- `AspNetUsers` - User accounts
- `AspNetUserClaims` - User claims
- `AspNetUserLogins` - External login providers
- `AspNetUserTokens` - User tokens (including password reset)
- `RefreshTokens` - Refresh tokens

Regular database maintenance:
```sql
-- Find unverified accounts older than 30 days
SELECT * FROM AspNetUsers 
WHERE EmailConfirmed = 0 
AND CreatedAt < DATEADD(DAY, -30, GETDATE());

-- Find expired but not cleaned up refresh tokens
SELECT * FROM RefreshTokens 
WHERE ExpiresAt < GETDATE() 
AND IsRevoked = 0;
```

## Security Best Practices

1. **Rotate JWT Secret Keys**: Change the JWT secret key periodically
2. **Monitor Login Patterns**: Watch for unusual login activity or patterns
3. **Rate Limiting**: Implement rate limiting on auth endpoints to prevent brute force
4. **IP Logging**: Consider logging IP addresses for sensitive operations
5. **Audit Logs**: Maintain detailed logs of authentication events

## Testing

Test scripts are available in the `Scripts` directory:
- `test-auth-endpoints.ps1` (PowerShell)
- `test-auth-endpoints.sh` (Bash)
- `TestAuthEndpoints.cs` (C#)

Run these scripts periodically to verify all authentication endpoints are working correctly.

## Deployment Considerations

When deploying updates to the authentication system:

1. **Never** deploy with credentials in source code
2. Use environment variables or secure storage for secrets
3. Test all auth flows in staging before production
4. Consider phased rollout for major changes
5. Monitor error rates closely after deployment

## Resource Cleanup

To prevent database bloat:

1. Implement a retention policy for inactive users
2. Regularly clean up expired tokens if automatic cleanup fails
3. Archive or delete old audit logs

## Disaster Recovery

In case of authentication system failure:

1. Have a backup of user database
2. Document manual account recovery procedures
3. Prepare communication templates for users

## Future Enhancements

Potential improvements to consider:

1. **Multi-factor Authentication** (MFA)
2. **Social Login** integration
3. **Role-based Access Control** expansion
4. **Session Management** improvements
5. **Account Lockout** policies

## Related Documentation

- [JWT Authentication](https://jwt.io/introduction)
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Token-based Authentication Best Practices](https://auth0.com/blog/refresh-tokens-what-are-they-and-when-to-use-them/)