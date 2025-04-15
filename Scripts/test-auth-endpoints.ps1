#!/usr/bin/env pwsh
# AuthController Endpoint Testing Script
# This script tests all authentication-related endpoints and edge cases

# Configuration
$baseUrl = "http://localhost:5173/api/auth"
$testEmail = "placeholder"
$testPassword = "placeholder"
$headers = @{
    "Content-Type" = "application/json"
}

# Helper Functions
function Invoke-Request {
    param (
        [string]$endpoint,
        [hashtable]$body,
        [string]$method = "POST"
    )
    
    $url = "$baseUrl/$endpoint"
    $bodyJson = $body | ConvertTo-Json
    
    Write-Host "`n=== Testing: $method $url ===`n" -ForegroundColor Cyan
    Write-Host "Request:" -ForegroundColor Yellow
    Write-Host "$bodyJson`n"
    
    try {
        $response = Invoke-RestMethod -Uri $url -Method $method -Body $bodyJson -Headers $headers -ErrorVariable errResponse
        Write-Host "Response:" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 5
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Error (Status: $statusCode):" -ForegroundColor Red
        
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $reader.BaseStream.Position = 0
            $reader.DiscardBufferedData()
            $errBody = $reader.ReadToEnd() | ConvertFrom-Json
            $errBody | ConvertTo-Json -Depth 5
        }
        catch {
            Write-Host "Could not parse error response: $_"
        }
        
        return $null
    }
}

function Write-TestHeader {
    param (
        [string]$title
    )
    
    Write-Host "`n`n==============================================" -ForegroundColor Blue
    Write-Host "   $title" -ForegroundColor Blue
    Write-Host "==============================================" -ForegroundColor Blue
}

# Global variables to store tokens
$Global:accessToken = $null
$Global:refreshToken = $null
$Global:confirmationToken = $null
$Global:userId = $null
$Global:resetToken = $null

# ==== Registration Tests ====
Write-TestHeader "REGISTRATION TESTS"

# Test: Register with valid credentials
$registerResult = Invoke-Request -endpoint "register" -body @{
    email = $testEmail
    password = $testPassword
}

if ($registerResult -and $registerResult.Success) {
    $Global:confirmationToken = $registerResult.Token
    $Global:userId = $registerResult.UserId  # If returned, otherwise we'll need to get it later
    Write-Host "Successfully registered user and got confirmation token" -ForegroundColor Green
}

# Test: Register with the same email (should fail)
Invoke-Request -endpoint "register" -body @{
    email = $testEmail
    password = $testPassword
}

# Test: Register with invalid data
Invoke-Request -endpoint "register" -body @{
    email = ""
    password = $testPassword
}

Invoke-Request -endpoint "register" -body @{
    email = $testEmail
    password = ""
}

# ==== Email Confirmation Tests ====
Write-TestHeader "EMAIL CONFIRMATION TESTS"

# Test: Try to login without confirming email (should fail)
Invoke-Request -endpoint "login" -body @{
    email = $testEmail
    password = $testPassword
}

# If we didn't get the userId from the register response, we could get it here (would require login without email verification)
if (-not $Global:userId) {
    Write-Host "Note: UserId was not returned in registration. In a real scenario, you'd extract it from the confirmation URL in the email." -ForegroundColor Yellow
    # For this script, we'll assume you would extract the userId from the URL in the email
}

# Test: Confirm email with valid token
if ($Global:confirmationToken) {
    $confirmResult = Invoke-Request -endpoint "confirm-email" -body @{
        userId = $Global:userId
        token = $Global:confirmationToken
    }
}
else {
    Write-Host "Skipping email confirmation test since we don't have a confirmation token" -ForegroundColor Yellow
}

# Test: Confirm email with invalid token
Invoke-Request -endpoint "confirm-email" -body @{
    userId = $Global:userId ?? "invalid-user-id"
    token = "invalid-token"
}

# Test: Resend confirmation email
$resendResult = Invoke-Request -endpoint "resend-confirmation" -body @{
    email = $testEmail
}

# If the first confirmation was successful, this should say "already confirmed"
if ($resendResult -and $resendResult.Token) {
    $Global:confirmationToken = $resendResult.Token
    Write-Host "Got new confirmation token from resend endpoint" -ForegroundColor Green
    
    # Test: Confirm email with the new token
    Invoke-Request -endpoint "confirm-email" -body @{
        userId = $Global:userId
        token = $Global:confirmationToken
    }
}

# ==== Login Tests ====
Write-TestHeader "LOGIN TESTS"

# Test: Login with valid credentials after confirmation
$loginResult = Invoke-Request -endpoint "login" -body @{
    email = $testEmail
    password = $testPassword
}

if ($loginResult -and $loginResult.Success) {
    $Global:accessToken = $loginResult.Token
    $Global:refreshToken = $loginResult.RefreshToken
    Write-Host "Successfully logged in and got tokens" -ForegroundColor Green
}

# Test: Login with invalid password
Invoke-Request -endpoint "login" -body @{
    email = $testEmail
    password = "wrong-password"
}

# Test: Login with invalid email
Invoke-Request -endpoint "login" -body @{
    email = "nonexistent@example.com"
    password = $testPassword
}

# ==== Token Refresh Tests ====
Write-TestHeader "TOKEN REFRESH TESTS"

# Test: Refresh token
if ($Global:refreshToken) {
    $refreshResult = Invoke-Request -endpoint "refresh" -body @{
        refreshToken = $Global:refreshToken
    }
    
    if ($refreshResult -and $refreshResult.Success) {
        $Global:accessToken = $refreshResult.Token
        $Global:refreshToken = $refreshResult.RefreshToken
        Write-Host "Successfully refreshed tokens" -ForegroundColor Green
    }
}
else {
    Write-Host "Skipping token refresh test since we don't have a refresh token" -ForegroundColor Yellow
}

# Test: Refresh with invalid token
Invoke-Request -endpoint "refresh" -body @{
    refreshToken = "invalid-refresh-token"
}

# ==== Revoke Token Tests ====
Write-TestHeader "REVOKE TOKEN TESTS"

# Test: Revoke token (requires authorization header)
if ($Global:refreshToken -and $Global:accessToken) {
    $authHeaders = $headers.Clone()
    $authHeaders.Add("Authorization", "Bearer $Global:accessToken")
    
    $url = "$baseUrl/revoke"
    $body = @{ refreshToken = $Global:refreshToken } | ConvertTo-Json
    
    Write-Host "`n=== Testing: POST $url (with auth) ===`n" -ForegroundColor Cyan
    Write-Host "Request:" -ForegroundColor Yellow
    Write-Host "$body`n"
    
    try {
        $response = Invoke-RestMethod -Uri $url -Method "POST" -Body $body -Headers $authHeaders -ErrorVariable errResponse
        Write-Host "Response:" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 5
        $Global:refreshToken = $null # Token has been revoked
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "Error (Status: $statusCode):" -ForegroundColor Red
        
        try {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $reader.BaseStream.Position = 0
            $reader.DiscardBufferedData()
            $errBody = $reader.ReadToEnd() | ConvertFrom-Json
            $errBody | ConvertTo-Json -Depth 5
        }
        catch {
            Write-Host "Could not parse error response: $_"
        }
    }
    
    # Test: Try to refresh with the revoked token (should fail)
    Invoke-Request -endpoint "refresh" -body @{
        refreshToken = $Global:refreshToken ?? "revoked-token"
    }
}
else {
    Write-Host "Skipping token revocation test since we don't have tokens" -ForegroundColor Yellow
}

# ==== Password Reset Tests ====
Write-TestHeader "PASSWORD RESET TESTS"

# Test: Forgot password
$forgotResult = Invoke-Request -endpoint "forgot-password" -body @{
    email = $testEmail
}

if ($forgotResult -and $forgotResult.Success) {
    $Global:resetToken = $forgotResult.Token
    Write-Host "Successfully requested password reset and got token" -ForegroundColor Green
}

# Test: Forgot password for non-existent user
Invoke-Request -endpoint "forgot-password" -body @{
    email = "nonexistent@example.com"
}

# Test: Reset password with valid token
if ($Global:resetToken) {
    $newPassword = "new-placeholder-password"
    
    $resetResult = Invoke-Request -endpoint "reset-password" -body @{
        email = $testEmail
        token = $Global:resetToken
        newPassword = $newPassword
    }
    
    if ($resetResult -and $resetResult.Success) {
        Write-Host "Successfully reset password" -ForegroundColor Green
        
        # Test: Login with new password
        $loginResult = Invoke-Request -endpoint "login" -body @{
            email = $testEmail
            password = $newPassword
        }
        
        if ($loginResult -and $loginResult.Success) {
            Write-Host "Successfully logged in with new password" -ForegroundColor Green
            $Global:accessToken = $loginResult.Token
            $Global:refreshToken = $loginResult.RefreshToken
        }
        
        # Test: Login with old password (should fail)
        Invoke-Request -endpoint "login" -body @{
            email = $testEmail
            password = $testPassword
        }
    }
}
else {
    Write-Host "Skipping password reset test since we don't have a reset token" -ForegroundColor Yellow
}

# Test: Reset password with invalid token
Invoke-Request -endpoint "reset-password" -body @{
    email = $testEmail
    token = "invalid-reset-token"
    newPassword = "another-new-password"
}

Write-Host "`n`nAll tests completed!" -ForegroundColor Green