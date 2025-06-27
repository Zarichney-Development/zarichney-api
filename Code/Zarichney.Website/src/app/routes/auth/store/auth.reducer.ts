import { createReducer, on } from '@ngrx/store';
import * as AuthActions from './auth.actions';
import { AuthState, initialAuthState } from './auth.state';

export const authReducer = createReducer(
  initialAuthState,

  // Check Authentication
  on(AuthActions.checkAuth, (state): AuthState => ({
    ...state,
    loading: true,
    error: null,
    status: 'loading',
    isAuthenticated: false,
    user: null
  })),

  on(AuthActions.checkAuthSuccess, (state, { response }): AuthState => {
    console.log('checkAuthSuccess reducer processing response:', response);

    return {
      ...state,
      isAuthenticated: response.isAuthenticated === true, // Force boolean evaluation
      user: response.isAuthenticated === true ? {
        userId: response.userId,
        email: response.userId, // Email might not be directly available in the response
        roles: response.roles || []
      } : null,
      loading: false,
      error: null,
      status: 'success'
    };
  }),

  on(AuthActions.checkAuthFailure, (state, { error }): AuthState => ({
    ...state,
    isAuthenticated: false,
    user: null,
    loading: false,
    error,
    status: 'error'
  })),

  // Login
  on(AuthActions.login, (state): AuthState => ({
    ...state,
    loading: true,
    error: null,
    status: 'loading'
  })),

  on(AuthActions.loginSuccess, (state, { email }): AuthState => ({
    ...state,
    isAuthenticated: true,
    loading: false,
    error: null,
    status: 'success'
    // Note: We don't have full user details here, 
    // checkAuth should be dispatched after login success to get complete details
  })),

  on(AuthActions.loginFailure, (state, { error }): AuthState => ({
    ...state,
    isAuthenticated: false,
    user: null,
    loading: false,
    error,
    status: 'error'
  })),

  // Logout
  on(AuthActions.logout, (state): AuthState => ({
    ...state,
    loading: true,
    error: null,
    status: 'loading'
  })),

  on(AuthActions.logoutSuccess, (state): AuthState => ({
    ...state,
    isAuthenticated: false,
    user: null,
    loading: false,
    error: null,
    status: 'success'
  })),

  on(AuthActions.logoutFailure, (state, { error }): AuthState => ({
    ...state,
    loading: false,
    error,
    status: 'error'
    // Note: We don't clear authentication state on logout failure
  })),

  // Refresh Token
  on(AuthActions.refreshToken, (state): AuthState => ({
    ...state,
    loading: true,
    error: null,
    status: 'loading'
  })),

  on(AuthActions.refreshTokenSuccess, (state): AuthState => ({
    ...state,
    loading: false,
    error: null,
    status: 'success'
  })),

  on(AuthActions.refreshTokenFailure, (state, { error }): AuthState => ({
    ...state,
    isAuthenticated: false, // Clear authentication state on token refresh failure
    user: null,
    loading: false,
    error,
    status: 'error'
  })),

  // Registration
  on(AuthActions.register, (state): AuthState => ({
    ...state,
    loading: true,
    error: null,
    status: 'loading'
  })),

  on(AuthActions.registerSuccess, (state): AuthState => ({
    ...state,
    loading: false,
    error: null,
    status: 'success'
    // We don't authenticate the user after registration since they need to confirm email
  })),

  on(AuthActions.registerFailure, (state, { error }): AuthState => ({
    ...state,
    loading: false,
    error,
    status: 'error'
  }))
);