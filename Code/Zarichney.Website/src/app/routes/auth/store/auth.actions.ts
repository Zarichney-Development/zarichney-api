import { createAction, props } from '@ngrx/store';
import {
  LoginRequest,
  RegisterRequest,
  CheckAuthResponse,
  AuthResponse
} from '../../../models/auth.models';

// Check Authentication
export const checkAuth = createAction('[Auth] Check Authentication');
export const checkAuthSuccess = createAction(
  '[Auth] Check Authentication Success',
  props<{ response: CheckAuthResponse }>()
);
export const checkAuthFailure = createAction(
  '[Auth] Check Authentication Failure',
  props<{ error: string }>()
);

// Login
export const login = createAction(
  '[Auth] Login',
  props<{ credentials: LoginRequest; returnUrl?: string | null }>()
);
export const loginSuccess = createAction(
  '[Auth] Login Success',
  props<{ email: string; returnUrl?: string | null }>()
);
export const loginFailure = createAction(
  '[Auth] Login Failure',
  props<{ error: string }>()
);

// Logout
export const logout = createAction('[Auth] Logout');
export const logoutSuccess = createAction('[Auth] Logout Success');
export const logoutFailure = createAction(
  '[Auth] Logout Failure',
  props<{ error: string }>()
);

// Refresh Token
export const refreshToken = createAction('[Auth] Refresh Token');
export const refreshTokenSuccess = createAction('[Auth] Refresh Token Success');
export const refreshTokenFailure = createAction(
  '[Auth] Refresh Token Failure',
  props<{ error: string }>()
);

// Registration
export const register = createAction(
  '[Auth] Register',
  props<{ details: RegisterRequest }>()
);
export const registerSuccess = createAction(
  '[Auth] Register Success',
  props<{ response: AuthResponse }>()
);
export const registerFailure = createAction(
  '[Auth] Register Failure',
  props<{ error: string }>()
);

// UI Flow Navigation
export const uiFlowComplete = createAction(
  '[Auth] UI Flow Complete',
  props<{
    source: 'login' | 'email-confirmation' | 'password-reset';
    returnUrl?: string | null
  }>()
);

// Keep these for backward compatibility
export const emailConfirmationUiComplete = createAction(
  '[Auth] Email Confirmation UI Complete'
);

export const passwordResetUiComplete = createAction(
  '[Auth] Password Reset UI Complete'
);