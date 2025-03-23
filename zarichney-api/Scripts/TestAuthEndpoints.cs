using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Zarichney.Scripts
{
    // Models for requests and responses
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ConfirmEmailRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }

    public class ResendConfirmationRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? Email { get; set; }
        public string? UserId { get; set; } // May not be present in actual API
    }

    /// <summary>
    /// Test script for Auth endpoints
    /// </summary>
    /// <remarks>
    /// To run this script:
    /// 1. Ensure your API is running
    /// 2. Use the command: dotnet script TestAuthEndpoints.cs
    /// 
    /// Note: You may need to install dotnet-script:
    /// dotnet tool install -g dotnet-script
    /// </remarks>
    public class TestAuthEndpoints
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly string _baseUrl = "http://localhost:5173/api/auth";
        private static readonly string _testEmail = "placeholder";
        private static readonly string _testPassword = "placeholder";

        // Global state
        private static string? _accessToken;
        private static string? _refreshToken;
        private static string? _confirmationToken;
        private static string? _userId;
        private static string? _resetToken;

        public static async Task Main()
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Registration Tests
            await PrintTestHeader("REGISTRATION TESTS");
            await TestRegistration();
            
            // Email Confirmation Tests
            await PrintTestHeader("EMAIL CONFIRMATION TESTS");
            await TestEmailConfirmation();
            
            // Login Tests
            await PrintTestHeader("LOGIN TESTS");
            await TestLogin();
            
            // Token Refresh Tests
            await PrintTestHeader("TOKEN REFRESH TESTS");
            await TestTokenRefresh();
            
            // Revoke Token Tests
            await PrintTestHeader("REVOKE TOKEN TESTS");
            await TestRevokeToken();
            
            // Password Reset Tests
            await PrintTestHeader("PASSWORD RESET TESTS");
            await TestPasswordReset();
            
            Console.WriteLine("\nAll tests completed!");
        }

        private static async Task TestRegistration()
        {
            // Test: Register with valid credentials
            var registerResult = await MakeRequest<AuthResponse, RegisterRequest>("register", new RegisterRequest
            {
                Email = _testEmail,
                Password = _testPassword
            });
            
            if (registerResult?.Success == true)
            {
                _confirmationToken = registerResult.Token;
                _userId = registerResult.UserId;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Successfully registered user and got confirmation token");
                Console.ResetColor();
            }
            
            // Test: Register with the same email (should fail)
            await MakeRequest<AuthResponse, RegisterRequest>("register", new RegisterRequest
            {
                Email = _testEmail,
                Password = _testPassword
            });
            
            // Test: Register with invalid data
            await MakeRequest<AuthResponse, RegisterRequest>("register", new RegisterRequest
            {
                Email = "",
                Password = _testPassword
            });
            
            await MakeRequest<AuthResponse, RegisterRequest>("register", new RegisterRequest
            {
                Email = _testEmail,
                Password = ""
            });
        }

        private static async Task TestEmailConfirmation()
        {
            // Test: Try to login without confirming email (should fail)
            await MakeRequest<AuthResponse, LoginRequest>("login", new LoginRequest
            {
                Email = _testEmail,
                Password = _testPassword
            });
            
            // If we didn't get the userId from the register response
            if (string.IsNullOrEmpty(_userId))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Note: UserId was not returned in registration. In a real scenario, you'd extract it from the confirmation URL in the email.");
                Console.ResetColor();
                
                // For testing, you might need to set this manually
                // _userId = "some-user-id";
            }
            
            // Test: Confirm email with valid token
            if (!string.IsNullOrEmpty(_confirmationToken) && !string.IsNullOrEmpty(_userId))
            {
                var confirmResult = await MakeRequest<AuthResponse, ConfirmEmailRequest>("confirm-email", new ConfirmEmailRequest
                {
                    UserId = _userId,
                    Token = _confirmationToken
                });
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Skipping email confirmation test since we don't have a confirmation token or user ID");
                Console.ResetColor();
            }
            
            // Test: Confirm email with invalid token
            await MakeRequest<AuthResponse, ConfirmEmailRequest>("confirm-email", new ConfirmEmailRequest
            {
                UserId = _userId ?? "invalid-user-id",
                Token = "invalid-token"
            });
            
            // Test: Resend confirmation email
            var resendResult = await MakeRequest<AuthResponse, ResendConfirmationRequest>("resend-confirmation", new ResendConfirmationRequest
            {
                Email = _testEmail
            });
            
            // If the first confirmation was successful, this should say "already confirmed"
            if (resendResult?.Success == true && !string.IsNullOrEmpty(resendResult.Token))
            {
                _confirmationToken = resendResult.Token;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Got new confirmation token from resend endpoint");
                Console.ResetColor();
                
                // Test: Confirm email with the new token
                if (!string.IsNullOrEmpty(_userId))
                {
                    await MakeRequest<AuthResponse, ConfirmEmailRequest>("confirm-email", new ConfirmEmailRequest
                    {
                        UserId = _userId,
                        Token = _confirmationToken
                    });
                }
            }
        }

        private static async Task TestLogin()
        {
            // Test: Login with valid credentials after confirmation
            var loginResult = await MakeRequest<AuthResponse, LoginRequest>("login", new LoginRequest
            {
                Email = _testEmail,
                Password = _testPassword
            });
            
            if (loginResult?.Success == true)
            {
                _accessToken = loginResult.Token;
                _refreshToken = loginResult.RefreshToken;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Successfully logged in and got tokens");
                Console.ResetColor();
            }
            
            // Test: Login with invalid password
            await MakeRequest<AuthResponse, LoginRequest>("login", new LoginRequest
            {
                Email = _testEmail,
                Password = "wrong-password"
            });
            
            // Test: Login with invalid email
            await MakeRequest<AuthResponse, LoginRequest>("login", new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = _testPassword
            });
        }

        private static async Task TestTokenRefresh()
        {
            // Test: Refresh token
            if (!string.IsNullOrEmpty(_refreshToken))
            {
                var refreshResult = await MakeRequest<AuthResponse, RefreshTokenRequest>("refresh", new RefreshTokenRequest
                {
                    RefreshToken = _refreshToken
                });
                
                if (refreshResult?.Success == true)
                {
                    _accessToken = refreshResult.Token;
                    _refreshToken = refreshResult.RefreshToken;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Successfully refreshed tokens");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Skipping token refresh test since we don't have a refresh token");
                Console.ResetColor();
            }
            
            // Test: Refresh with invalid token
            await MakeRequest<AuthResponse, RefreshTokenRequest>("refresh", new RefreshTokenRequest
            {
                RefreshToken = "invalid-refresh-token"
            });
        }

        private static async Task TestRevokeToken()
        {
            // Test: Revoke token (requires authorization header)
            if (!string.IsNullOrEmpty(_refreshToken) && !string.IsNullOrEmpty(_accessToken))
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/revoke");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                
                var content = new RefreshTokenRequest
                {
                    RefreshToken = _refreshToken
                };
                
                var json = JsonSerializer.Serialize(content);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"\n=== Testing: POST {_baseUrl}/revoke (with auth) ===\n");
                Console.WriteLine("Request:");
                Console.WriteLine(json);
                Console.WriteLine();
                
                var response = await _client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"Response (Status: {(int)response.StatusCode}):");
                try
                {
                    var formattedJson = JsonSerializer.Serialize(
                        JsonSerializer.Deserialize<object>(responseBody),
                        new JsonSerializerOptions { WriteIndented = true }
                    );
                    Console.WriteLine(formattedJson);
                }
                catch
                {
                    Console.WriteLine(responseBody);
                }
                
                try
                {
                    var result = JsonSerializer.Deserialize<AuthResponse>(responseBody);
                    
                    if (result?.Success == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Successfully revoked refresh token");
                        Console.ResetColor();
                        _refreshToken = null;
                    }
                }
                catch
                {
                    // Ignore deserialization errors
                }
                
                // Test: Try to refresh with the revoked token (should fail)
                await MakeRequest<AuthResponse, RefreshTokenRequest>("refresh", new RefreshTokenRequest
                {
                    RefreshToken = _refreshToken ?? "revoked-token"
                });
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Skipping token revocation test since we don't have tokens");
                Console.ResetColor();
            }
        }

        private static async Task TestPasswordReset()
        {
            // Test: Forgot password
            var forgotResult = await MakeRequest<AuthResponse, ForgotPasswordRequest>("forgot-password", new ForgotPasswordRequest
            {
                Email = _testEmail
            });
            
            if (forgotResult?.Success == true)
            {
                _resetToken = forgotResult.Token;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Successfully requested password reset and got token");
                Console.ResetColor();
            }
            
            // Test: Forgot password for non-existent user
            await MakeRequest<AuthResponse, ForgotPasswordRequest>("forgot-password", new ForgotPasswordRequest
            {
                Email = "nonexistent@example.com"
            });
            
            // Test: Reset password with valid token
            if (!string.IsNullOrEmpty(_resetToken))
            {
                var newPassword = "new-placeholder-password";
                
                var resetResult = await MakeRequest<AuthResponse, ResetPasswordRequest>("reset-password", new ResetPasswordRequest
                {
                    Email = _testEmail,
                    Token = _resetToken,
                    NewPassword = newPassword
                });
                
                if (resetResult?.Success == true)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Successfully reset password");
                    Console.ResetColor();
                    
                    // Test: Login with new password
                    var loginResult = await MakeRequest<AuthResponse, LoginRequest>("login", new LoginRequest
                    {
                        Email = _testEmail,
                        Password = newPassword
                    });
                    
                    if (loginResult?.Success == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Successfully logged in with new password");
                        Console.ResetColor();
                        _accessToken = loginResult.Token;
                        _refreshToken = loginResult.RefreshToken;
                    }
                    
                    // Test: Login with old password (should fail)
                    await MakeRequest<AuthResponse, LoginRequest>("login", new LoginRequest
                    {
                        Email = _testEmail,
                        Password = _testPassword
                    });
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Skipping password reset test since we don't have a reset token");
                Console.ResetColor();
            }
            
            // Test: Reset password with invalid token
            await MakeRequest<AuthResponse, ResetPasswordRequest>("reset-password", new ResetPasswordRequest
            {
                Email = _testEmail,
                Token = "invalid-reset-token",
                NewPassword = "another-new-password"
            });
        }

        private static async Task<TResponse?> MakeRequest<TResponse, TRequest>(string endpoint, TRequest requestBody)
            where TResponse : class
            where TRequest : class
        {
            var url = $"{_baseUrl}/{endpoint}";
            
            Console.WriteLine($"\n=== Testing: POST {url} ===\n");
            
            var json = JsonSerializer.Serialize(requestBody);
            Console.WriteLine("Request:");
            Console.WriteLine(json);
            Console.WriteLine();
            
            HttpResponseMessage response;
            
            try
            {
                response = await _client.PostAsJsonAsync(url, requestBody);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error sending request: {ex.Message}");
                Console.ResetColor();
                return null;
            }
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Response (Status: {(int)response.StatusCode}):");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error (Status: {(int)response.StatusCode}):");
                Console.ResetColor();
            }
            
            try
            {
                var formattedJson = JsonSerializer.Serialize(
                    JsonSerializer.Deserialize<object>(responseBody),
                    new JsonSerializerOptions { WriteIndented = true }
                );
                Console.WriteLine(formattedJson);
            }
            catch
            {
                Console.WriteLine(responseBody);
            }
            
            try
            {
                return JsonSerializer.Deserialize<TResponse>(responseBody);
            }
            catch
            {
                return null;
            }
        }

        private static Task PrintTestHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n\n==============================================");
            Console.WriteLine($"   {title}");
            Console.WriteLine("==============================================");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
    }
}