# API Key Authentication and Role-Based Authorization

This document describes the API key authentication system and role-based authorization in Zarichney API.

## Overview

The API supports two authentication methods:

1. **JWT Authentication**: Standard token-based authentication using JWT tokens stored in HTTP-only cookies.
2. **API Key Authentication**: Using an `X-Api-Key` header with a valid API key.

Both methods can be used to authenticate requests, with JWT taking precedence if both are provided.

## API Key Concepts

- API keys are associated with individual user accounts 
- Each user can have multiple API keys
- API keys can be created, listed, and revoked through the API
- API keys can optionally be set to expire at a specific time
- API keys provide the same level of access as the associated user

## Authentication Flow

### JWT Authentication Flow

1. User logs in with email/password
2. Server returns JWT and refresh tokens in HTTP-only cookies
3. Subsequent requests include these cookies automatically
4. Token is refreshed automatically when expired

### API Key Authentication Flow

1. User creates an API key through the authenticated endpoint
2. The API key is included in subsequent requests via the `X-Api-Key` header
3. The server validates the API key and identifies the associated user
4. If valid, the request proceeds as if the user was authenticated with JWT

## Managing API Keys

### Creating an API Key

```http
POST /api/auth/api-keys
Content-Type: application/json
Authorization: Bearer <jwt_token>

{
  "name": "My API Key",
  "description": "Used for automated testing",
  "expiresAt": "2025-12-31T23:59:59Z"  // Optional
}
```

Response:

```json
{
  "id": 1,
  "keyValue": "generated-api-key-value",
  "name": "My API Key",
  "createdAt": "2025-03-24T12:34:56Z",
  "expiresAt": "2025-12-31T23:59:59Z",
  "isActive": true,
  "description": "Used for automated testing"
}
```

**Important**: The full API key value is only returned once upon creation. Store it securely as it cannot be retrieved later.

### Listing API Keys

```http
GET /api/auth/api-keys
Authorization: Bearer <jwt_token>
```

Response:

```json
[
  {
    "id": 1,
    "keyValue": "generated-api-key-value",
    "name": "My API Key",
    "createdAt": "2025-03-24T12:34:56Z",
    "expiresAt": "2025-12-31T23:59:59Z",
    "isActive": true,
    "description": "Used for automated testing"
  }
]
```

### Revoking an API Key

```http
DELETE /api/auth/api-keys/{id}
Authorization: Bearer <jwt_token>
```

Response:

```json
{
  "success": true,
  "message": "API key revoked successfully"
}
```

## Using API Keys for Authentication

To authenticate a request using an API key, include the `X-Api-Key` header:

```http
GET /api/some-endpoint
X-Api-Key: your-api-key-value
```

The server will:
1. Validate the API key
2. Identify the associated user
3. Create a session linked to that user
4. Process the request as if the user was authenticated via JWT

## Security Considerations

- API keys are as powerful as user credentials - keep them secure
- Avoid including API keys in client-side code
- Use HTTPS for all API requests
- Consider setting expiration dates on API keys
- Rotate API keys periodically
- Revoke API keys when no longer needed

## Precedence Rules

If a request includes both JWT authentication (via cookies) and an API key (via header):

1. JWT authentication is validated first
2. If JWT is valid, the API key is ignored
3. If JWT is invalid or missing, API key authentication is attempted

## Example Request/Response

### Request with API Key Only

```http
GET /api/cookbook/recipes
X-Api-Key: api-key-value
```

### Request with Both JWT and API Key

```http
GET /api/cookbook/recipes
Cookie: AuthAccessToken=jwt-token
X-Api-Key: api-key-value
```

In this case, the JWT token takes precedence.

## Troubleshooting API Key Authentication

If you encounter authentication issues when using API keys, check the following:

### Common Errors

1. **"Authorization failed. These requirements were not met: DenyAnonymousAuthorizationRequirement: Requires an authenticated user"**
   - This error indicates that the API key authentication was not properly processed.
   - Ensure you're using a valid API key in the `X-Api-Key` header.
   - Verify that the API key belongs to a valid user in the database.
   - Check that the user associated with the API key has not been deleted or disabled.
   - The ClaimsIdentity might not be properly authenticated - the authentication type must be non-null.

### Testing API Key Authentication Status

Use the `/api/test-auth` endpoint to check your API key authentication status:

```http
GET /api/test-auth
X-Api-Key: your-api-key-value
```

This will return detailed information about the authentication status:

```json
{
  "userId": "the-user-id",
  "authType": "ApiKey",
  "isAuthenticated": true,
  "isAdmin": false,
  "roles": ["role1", "role2"],
  "isApiKeyAuth": true,
  "apiKeyInfo": {
    "keyId": "your-api-key-value"
  },
  "message": "Authentication successful!"
}
```

Check the following fields to debug authentication issues:
- `isAuthenticated`: Should be `true` if the API key is valid
- `authType`: Should be `"ApiKey"` when using API key authentication
- `roles`: Shows all roles assigned to the user, should include "admin" for admin users
- `isApiKeyAuth`: Should be `true` when using an API key

2. **"Invalid API key" or "API key or JWT authentication is required"**
   - The API key provided is not found in the database.
   - The API key has been revoked (`IsActive` = false).
   - The API key has expired (if an expiration date was set).
   
3. **JWT Authentication works but API Key doesn't**
   - Verify that the API key authentication middleware is registered properly in Program.cs.
   - Ensure that the API key header name is correct (`X-Api-Key`).
   
### Testing API Key Authentication

To test API key authentication:

1. Create an API key using the authenticated admin endpoint:
   ```http
   POST /api/auth/api-keys
   Content-Type: application/json
   Authorization: Bearer <jwt_token>
   
   {
     "name": "Test API Key",
     "description": "For testing authentication"
   }
   ```

2. Use the API key to access a protected endpoint:
   ```http
   GET /api/auth/check-authentication
   X-Api-Key: your-api-key-value
   ```

3. Verify the response includes authentication details like user ID and roles.

### Implementation Details

The API key authentication system:
1. Verifies that the key exists and is valid
2. Fetches the associated user and their roles
3. Creates an authenticated ClaimsPrincipal with the user's identity
4. Sets the identity with the appropriate authentication type
5. Adds necessary claims (user ID, email, roles)

This ensures that endpoints with `[Authorize]` attribute recognize API key authentication as valid, similar to JWT authentication.

## Role-Based Authorization

The API implements role-based authorization using ASP.NET Identity roles. Currently, the system has one predefined role:

- **admin**: Users with administrative privileges who can manage API keys for all users

### Role Protection for API Key Management

The following API key management endpoints are restricted to users with the "admin" role:

- `POST /api/auth/api-keys`: Create new API keys
- `GET /api/auth/api-keys`: List all API keys
- `GET /api/auth/api-keys/{id}`: Get details of a specific API key

The endpoint for revoking an API key is accessible to both administrators and the key owner:

- `DELETE /api/auth/api-keys/{id}`: Revoke a specific API key

### Role Management Endpoints

The following endpoints are available for role management (admin access only):

- `POST /api/auth/roles/add`: Add a user to a role
- `POST /api/auth/roles/remove`: Remove a user from a role
- `GET /api/auth/roles/user/{userId}`: Get all roles for a specific user
- `GET /api/auth/roles/{roleName}/users`: Get all users in a specific role

### Development Setup Endpoint

During initial development/setup, the following endpoint can be used to designate a user as an admin:

- `POST /api/auth/setup-admin`: Add a user to the admin role (development environment only)

### Authentication Requirements

Role-based protected endpoints require JWT authentication. API key authentication cannot be used to access role-protected endpoints, as roles are associated with the JWT claims identity.

### Becoming an Administrator

For security reasons, users cannot self-designate as administrators. Administrator designation must be done through:

1. The development setup endpoint (in development environments only)
2. Direct database manipulation by a system administrator
3. Using the role management API by an existing administrator