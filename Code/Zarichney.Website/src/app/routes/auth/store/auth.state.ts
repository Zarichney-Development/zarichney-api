export const AUTH_FEATURE_KEY = 'auth';

export interface AuthUser {
  userId: string;
  email: string;
  roles: string[];
}

export type AuthStatus = 'idle' | 'loading' | 'success' | 'error';

export interface AuthState {
  isAuthenticated: boolean;
  user: AuthUser | null;
  loading: boolean;
  error: string | null;
  status: AuthStatus;
}

export const initialAuthState: AuthState = {
  isAuthenticated: false,
  user: null,
  loading: false,
  error: null,
  status: 'idle'
};