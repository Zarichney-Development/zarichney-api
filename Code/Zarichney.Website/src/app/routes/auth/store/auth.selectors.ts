import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AuthState, AUTH_FEATURE_KEY } from './auth.state';

// Feature selector
export const selectAuthState = createFeatureSelector<AuthState>(AUTH_FEATURE_KEY);

// Derived selectors
export const selectIsAuthenticated = createSelector(
  selectAuthState,
  (state: AuthState) => state.isAuthenticated
);

export const selectUser = createSelector(
  selectAuthState,
  (state: AuthState) => state.user
);

export const selectUserRoles = createSelector(
  selectAuthState,
  (state: AuthState) => state.user?.roles || []
);

export const selectIsAdmin = createSelector(
  selectUserRoles,
  (roles: string[]) => roles.includes('admin')
);

export const selectAuthLoading = createSelector(
  selectAuthState,
  (state: AuthState) => state?.loading || false
);

export const selectAuthError = createSelector(
  selectAuthState,
  (state: AuthState) => state?.error || null
);

export const selectAuthStatus = createSelector(
  selectAuthState,
  (state: AuthState) => state.status
);