// Authentication-related interfaces based on the backend AuthController models

// Request model for user registration
export interface RegisterRequest {
  email: string;
  password: string;
}

// Request model for user login
export interface LoginRequest {
  email: string;
  password: string;
}

// Standard response model for authentication operations
export interface AuthResponse {
  success: boolean;
  message?: string;
  email?: string;
}

// Request model to initiate the forgot password process
export interface ForgotPasswordRequest {
  email: string;
}

// Request model to reset the password using a token
export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

// Parameters for email confirmation from the query string of the confirmation link
export interface ConfirmEmailRequest {
  userId: string;
  token: string;
}

// Request model for resending the email confirmation link
export interface ResendConfirmationRequest {
  email: string;
}

// Response for API key operations
export interface ApiKeyResponse {
  id: number;
  key?: string; // Only present on creation, not retrieval
  description: string;
  createdAt: string;
  expiresAt?: string;
  isActive: boolean;
}

// Command to create an API key
export interface CreateApiKeyCommand {
  description: string;
  expiresAt?: string;
}

// Request model for assigning or removing user roles
export interface RoleRequest {
  userId: string;
  roleName: string;
}

// Result for role management operations
export interface RoleCommandResult {
  success: boolean;
  message?: string;
  roles?: string[];
}

// User role information
export interface UserRoleInfo {
  userId: string;
  email: string;
}

// Response from check-authentication endpoint
export interface CheckAuthResponse {
  userId: string;
  isAdmin: boolean;
  roles: string[];
  authenticationType: string;
  isAuthenticated: boolean;
}