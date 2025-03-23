# Auth Endpoint Testing Scripts

This directory contains scripts for testing the authentication endpoints in the Zarichney API.

## Available Scripts

1. **test-auth-endpoints.ps1** - PowerShell script for testing auth endpoints
2. **test-auth-endpoints.sh** - Bash script for testing auth endpoints
3. **TestAuthEndpoints.cs** - C# script for testing auth endpoints

## Prerequisites

- Ensure your API is running (typically at http://localhost:5173)
- Modify the base URL in the scripts if your API is running on a different port

## Test Scenarios

The scripts test the following scenarios:

1. **Registration**
   - Successful registration
   - Registration with duplicate email
   - Registration with invalid data

2. **Email Confirmation**
   - Login attempt without email confirmation
   - Email confirmation with valid token
   - Email confirmation with invalid token
   - Resending confirmation email

3. **Login**
   - Login with valid credentials
   - Login with wrong password
   - Login with non-existent email

4. **Token Management**
   - Refreshing access token with valid refresh token
   - Refreshing with invalid refresh token
   - Revoking a refresh token
   - Attempting to use a revoked token

5. **Password Reset**
   - Requesting password reset
   - Resetting password with valid token
   - Resetting password with invalid token
   - Login with new password
   - Login with old password after reset

## Running the Scripts

### PowerShell Script

```
./test-auth-endpoints.ps1
```

### Bash Script

```
chmod +x test-auth-endpoints.sh
./test-auth-endpoints.sh
```

Note: The bash script requires `jq` for JSON parsing. Install it with:
```
sudo apt install jq   # Ubuntu/Debian
brew install jq       # macOS with Homebrew
```

### C# Script

```
dotnet script TestAuthEndpoints.cs
```

Note: You may need to install the `dotnet-script` tool first:
```
dotnet tool install -g dotnet-script
```

## Test User

The scripts use a hardcoded test user:
- Email: placeholder
- Password: placeholder

You can modify these values in the scripts if needed.

## Notes

- These scripts are primarily for development and testing purposes
- In development mode, the API returns tokens in responses that would normally only be sent by email
- For security, these tokens would not be returned in production responses