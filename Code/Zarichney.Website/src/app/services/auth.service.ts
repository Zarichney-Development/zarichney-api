import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { LoggingService } from './log.service';
import {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ConfirmEmailRequest,
  ResendConfirmationRequest,
  ApiKeyResponse,
  RoleRequest,
  RoleCommandResult,
  UserRoleInfo,
  CheckAuthResponse,
  CreateApiKeyCommand
} from '../models/auth.models';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private authEndpoint = 'auth';

  constructor(
    private apiService: ApiService,
    private log: LoggingService
  ) {
    this.log.verbose('AuthService initialized', `${this.apiService.getApiUrl()}/${this.authEndpoint}`);
  }

  /**
   * Logs in a user with email and password
   * @param credentials Login credentials
   * @returns Observable with auth response
   */
  login(credentials: LoginRequest): Observable<AuthResponse> {
    this.log.verbose('AuthService.login', { email: credentials.email });
    const url = `/${this.authEndpoint}/login`;
    return this.apiService.post<AuthResponse>(
      url,
      credentials,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Login response', response))
    );
  }

  /**
   * Registers a new user
   * @param details Registration details
   * @returns Observable with auth response
   */
  register(details: RegisterRequest): Observable<AuthResponse> {
    this.log.verbose('AuthService.register', { email: details.email });
    const url = `/${this.authEndpoint}/register`;
    return this.apiService.post<AuthResponse>(
      url,
      details,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Registration response', response))
    );
  }

  /**
   * Logs the current user out by clearing authentication cookies
   * @returns Observable with auth response
   */
  logout(): Observable<AuthResponse> {
    this.log.verbose('AuthService.logout');
    const url = `/${this.authEndpoint}/logout`;
    return this.apiService.post<AuthResponse>(
      url,
      {},
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Logout response', response))
    );
  }

  /**
   * Refreshes the access token using the refresh token cookie
   * @returns Observable with auth response
   */
  refreshToken(): Observable<AuthResponse> {
    this.log.verbose('AuthService.refreshToken');
    const url = `/${this.authEndpoint}/refresh`;
    return this.apiService.post<AuthResponse>(
      url,
      {},
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Token refresh response', response))
    );
  }

  /**
   * Revokes the current refresh token associated with the session
   * @returns Observable with auth response
   */
  revokeToken(): Observable<AuthResponse> {
    this.log.verbose('AuthService.revokeToken');
    const url = `/${this.authEndpoint}/revoke`;
    return this.apiService.post<AuthResponse>(
      url,
      {},
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Token revocation response', response))
    );
  }

  /**
   * Initiates the password reset process for a given email address
   * @param req Forgot password request with email
   * @returns Observable with auth response
   */
  forgotPassword(req: ForgotPasswordRequest): Observable<AuthResponse> {
    this.log.verbose('AuthService.forgotPassword', { email: req.email });
    const url = `/${this.authEndpoint}/email-forgot-password`;
    return this.apiService.post<AuthResponse>(
      url,
      req,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Forgot password response', response))
    );
  }

  /**
   * Resets the user's password using a token received via email
   * @param req Reset password request
   * @returns Observable with auth response
   */
  resetPassword(req: ResetPasswordRequest): Observable<AuthResponse> {
    this.log.verbose('AuthService.resetPassword', { email: req.email });
    const url = `/${this.authEndpoint}/reset-password`;
    return this.apiService.post<AuthResponse>(
      url,
      req,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Password reset response', response))
    );
  }

  /**
   * Confirms a user's email address using a token sent via email
   * @param req Confirm email request with userId and token
   * @returns Observable with auth response or void (for redirects)
   */
  confirmEmail(req: ConfirmEmailRequest): Observable<AuthResponse> {
    this.log.verbose('AuthService.confirmEmail', { userId: req.userId });
    const url = `/${this.authEndpoint}/email-confirmed?userId=${req.userId}&token=${req.token}`;
    return this.apiService.get<AuthResponse>(
      url,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Email confirmation response', response))
    );
  }

  /**
   * Resends the email confirmation link to the specified email address
   * @param req Resend confirmation request with email
   * @returns Observable with auth response
   */
  resendConfirmation(req: ResendConfirmationRequest): Observable<AuthResponse> {
    this.log.verbose('AuthService.resendConfirmation', { email: req.email });
    const url = `/${this.authEndpoint}/resend-confirmation`;
    return this.apiService.post<AuthResponse>(
      url,
      req,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Resend confirmation response', response))
    );
  }

  /**
   * Checks the authentication status and basic claims of the current user
   * @returns Observable with check auth response
   */
  checkAuthentication(): Observable<CheckAuthResponse> {
    this.log.verbose('AuthService.checkAuthentication');
    const url = `/${this.authEndpoint}/check-authentication`;
    return this.apiService.get<CheckAuthResponse>(
      url,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Authentication check response', response))
    );
  }

  /**
   * Refreshes the claims included in the user's access token
   * @returns Observable with auth response
   */
  refreshUserClaims(): Observable<AuthResponse> {
    this.log.verbose('AuthService.refreshUserClaims');
    const url = `/${this.authEndpoint}/refresh-claims`;
    return this.apiService.post<AuthResponse>(
      url,
      {},
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Claims refresh response', response))
    );
  }

  // API Key management methods (admin only)

  /**
   * Creates a new API key (Admin only)
   * @param req Create API key command
   * @returns Observable with API key response
   */
  createApiKey(req: CreateApiKeyCommand): Observable<ApiKeyResponse> {
    this.log.verbose('AuthService.createApiKey', { description: req.description });
    const url = `/${this.authEndpoint}/api-keys`;
    return this.apiService.post<ApiKeyResponse>(
      url,
      req,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('API key creation response', response))
    );
  }

  /**
   * Retrieves a list of all active API keys (Admin only)
   * @returns Observable with list of API key responses
   */
  getApiKeys(): Observable<ApiKeyResponse[]> {
    this.log.verbose('AuthService.getApiKeys');
    const url = `/${this.authEndpoint}/api-keys`;
    return this.apiService.get<ApiKeyResponse[]>(
      url,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('API keys retrieval response', response))
    );
  }

  /**
   * Retrieves metadata for a specific API key by ID (Admin only)
   * @param id The unique identifier of the API key
   * @returns Observable with API key response
   */
  getApiKeyById(id: number): Observable<ApiKeyResponse> {
    this.log.verbose('AuthService.getApiKeyById', { id });
    const url = `/${this.authEndpoint}/api-keys/${id}`;
    return this.apiService.get<ApiKeyResponse>(
      url,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('API key retrieval response', response))
    );
  }

  /**
   * Revokes (deactivates) an existing API key
   * @param id The unique identifier of the API key to revoke
   * @returns Observable with success status and message
   */
  revokeApiKey(id: number): Observable<{ success: boolean; message: string }> {
    this.log.verbose('AuthService.revokeApiKey', { id });
    const url = `/${this.authEndpoint}/api-keys/${id}`;
    return this.apiService.delete<{ success: boolean; message: string }>(
      url,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('API key revocation response', response))
    );
  }

  // Role management methods (admin only)

  /**
   * Assigns a specified role to a user (Admin only)
   * @param req Role request with userId and roleName
   * @returns Observable with role command result
   */
  addUserToRole(req: RoleRequest): Observable<RoleCommandResult> {
    this.log.verbose('AuthService.addUserToRole', { userId: req.userId, roleName: req.roleName });
    const url = `/${this.authEndpoint}/roles/add`;
    return this.apiService.post<RoleCommandResult>(
      url,
      req,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Add user to role response', response))
    );
  }

  /**
   * Removes a specified role from a user (Admin only)
   * @param req Role request with userId and roleName
   * @returns Observable with role command result
   */
  removeUserFromRole(req: RoleRequest): Observable<RoleCommandResult> {
    this.log.verbose('AuthService.removeUserFromRole', { userId: req.userId, roleName: req.roleName });
    const url = `/${this.authEndpoint}/roles/remove`;
    return this.apiService.post<RoleCommandResult>(
      url,
      req,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Remove user from role response', response))
    );
  }

  /**
   * Gets all roles assigned to a specific user (Admin only)
   * @param userId The unique identifier of the user
   * @returns Observable with role command result
   */
  getUserRoles(userId: string): Observable<RoleCommandResult> {
    this.log.verbose('AuthService.getUserRoles', { userId });
    const url = `/${this.authEndpoint}/roles/user/${userId}`;
    return this.apiService.get<RoleCommandResult>(
      url,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Get user roles response', response))
    );
  }

  /**
   * Gets all users assigned to a specific role (Admin only)
   * @param roleName The name of the role
   * @returns Observable with list of user role info
   */
  getUsersInRole(roleName: string): Observable<UserRoleInfo[]> {
    this.log.verbose('AuthService.getUsersInRole', { roleName });
    const url = `/${this.authEndpoint}/roles/${roleName}/users`;
    return this.apiService.get<UserRoleInfo[]>(
      url,
      { withCredentials: true }
    ).pipe(
      tap(response => this.log.info('Get users in role response', response))
    );
  }
}