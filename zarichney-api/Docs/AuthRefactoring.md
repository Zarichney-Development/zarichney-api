# Auth System Refactoring

This document outlines the changes made to the authentication system to improve security, maintainability, and add new features.

## Key Changes

### 1. CQRS + MediatR Implementation
- Refactored monolithic `AuthService` into separate Command and Query handlers using MediatR
- Each authentication operation (login, register, etc.) is now its own isolated handler
- Controllers are now thin and delegate all business logic to Command/Query handlers

### 2. HTTP-Only Cookies for Token Storage
- Access and refresh tokens are now stored in HTTP-only cookies instead of being returned in the response body
- This provides protection against XSS attacks as JavaScript cannot access these cookies
- Cookie configuration includes:
  - `HttpOnly = true` - Cannot be accessed by JavaScript
  - `Secure = true` - Only sent over HTTPS
  - `SameSite = Lax` - Protection against CSRF attacks

### 3. Multiple Refresh Tokens per User
- Each user can have multiple refresh tokens (one per device/session)
- RefreshToken model extended with device information:
  - DeviceName - Optional friendly name for the device
  - DeviceIp - IP address when the token was issued
  - UserAgent - Browser/client user agent
  - LastUsedAt - Time when the token was last used

### 4. Sliding Token Expiration
- When a refresh token is used, a new one is issued with a reset expiration time
- The old token is marked as used and cannot be used again
- This provides a "sliding window" authentication experience for users

### 5. Email Confirmation Flow Improved
- Email confirmation no longer sends a JWT in the URL query parameter
- After email confirmation, user receives auth tokens in HTTP-only cookies
- Client redirects no longer contain sensitive token information

## Implementation Details

### New Folder Structure
```
/Server/Auth/
├── Commands/           # Command handlers (write operations)
│   ├── LoginCommand.cs
│   ├── RegisterCommand.cs
│   ├── RefreshTokenCommand.cs
│   └── ...
├── Queries/            # Query handlers (read operations)
├── Common/             # Shared models and utilities
│   └── AuthResult.cs   # Unified result type
├── CookieAuthManager.cs # Cookie management
└── ...
```

### New Database Schema
The RefreshToken table was extended with the following columns:
- DeviceName (string, nullable)
- DeviceIp (string, nullable)
- UserAgent (string, nullable)
- LastUsedAt (DateTime, nullable)

### Migration
To apply the database migration:
1. For Linux/MacOS: Run `Server/Auth/Migrations/ApplyDeviceInfoMigration.sh`
2. For Windows: Run `Server/Auth/Migrations/ApplyDeviceInfoMigration.ps1`

### Token Flow
- **Login**: User credentials → Access token + Refresh token in HTTP-only cookies
- **API Requests**: Access token automatically included from cookie
- **Token Refresh**: Refresh token from cookie → New access + refresh tokens in cookies
- **Logout**: Clear both cookies

## Integration Notes for Frontend

### Frontend Changes Required
1. Remove any code that extracts tokens from response bodies
2. Remove any code that manually adds tokens to request headers
3. Ensure cookies are included with requests by using `credentials: 'include'` in fetch calls

### Example API Calls

**Login:**
```javascript
// Before
fetch('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify(credentials),
  headers: { 'Content-Type': 'application/json' }
})
.then(r => r.json())
.then(data => {
  // Store tokens from response body
  localStorage.setItem('token', data.token);
});

// After
fetch('/api/auth/login', {
  method: 'POST',
  body: JSON.stringify(credentials),
  headers: { 'Content-Type': 'application/json' },
  credentials: 'include'  // This is important!
})
.then(r => r.json())
.then(data => {
  // No need to store tokens, they're in HTTP-only cookies
});
```

**API calls:**
```javascript
// Before
fetch('/api/protected-resource', {
  headers: { 
    'Authorization': `Bearer ${localStorage.getItem('token')}` 
  }
});

// After
fetch('/api/protected-resource', {
  credentials: 'include'  // Cookies are sent automatically
});
```

**Refresh token:**
```javascript
// Before
fetch('/api/auth/refresh', {
  method: 'POST',
  body: JSON.stringify({ refreshToken: localStorage.getItem('refreshToken') }),
  headers: { 'Content-Type': 'application/json' }
})
.then(r => r.json())
.then(data => {
  localStorage.setItem('token', data.token);
  localStorage.setItem('refreshToken', data.refreshToken);
});

// After
fetch('/api/auth/refresh', {
  method: 'POST',
  credentials: 'include'  // Sends the refresh token cookie automatically
})
.then(r => r.json());
// New tokens are automatically stored in cookies
```

**Logout:**
```javascript
// Before
fetch('/api/auth/logout', { method: 'POST' })
.then(() => {
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
});

// After
fetch('/api/auth/logout', {
  method: 'POST',
  credentials: 'include'
})
.then(() => {
  // Cookies are cleared automatically by the server
});
```