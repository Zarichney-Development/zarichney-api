import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { catchError, exhaustMap, map, switchMap, tap, concatMap, take, withLatestFrom } from 'rxjs/operators';
import * as AuthActions from './auth.actions';
import { AuthService } from '../../../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { LoggingService } from '../../../services/log.service';
import { Store } from '@ngrx/store';
import { selectIsAdmin } from '../../../routes/auth/store/auth.selectors';

@Injectable()
export class AuthEffects {
  constructor(
    private actions$: Actions,
    private authService: AuthService,
    private toastr: ToastrService,
    private log: LoggingService,
    private router: Router,
    private store: Store
  ) { }

  // Check Authentication
  checkAuth$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.checkAuth),
    exhaustMap(() => this.authService.checkAuthentication().pipe(
      map(response => AuthActions.checkAuthSuccess({ response })),
      catchError(error => {
        const specificErrorMessage = error instanceof Error ? error.message : 'An unknown error occurred.';
        this.log.error('Check authentication failed', specificErrorMessage);
        this.toastr.error(specificErrorMessage, 'Authentication Error');
        return of(AuthActions.checkAuthFailure({ error: specificErrorMessage }));
      })
    ))
  ));

  // Login
  login$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.login),
    exhaustMap(action => this.authService.login(action.credentials).pipe(
      concatMap(response => {
        if (response.success && response.email) {
          // Success: Dispatch success action THEN checkAuth action
          return [
            AuthActions.loginSuccess({
              email: response.email,
              returnUrl: action.returnUrl
            }),
            AuthActions.checkAuth()
          ];
        } else {
          // Logical failure from backend response
          const errorMsg = response.message || 'Login failed';
          this.toastr.error(errorMsg, 'Login Failed');
          this.log.error('Login failed', errorMsg);
          return of(AuthActions.loginFailure({ error: errorMsg }));
        }
      }),
      catchError(error => {
        // Catches HTTP errors or errors thrown above
        const specificErrorMessage = error instanceof Error ? error.message : 'An unknown error occurred during login.';
        this.log.error('Login failed', specificErrorMessage);
        this.toastr.error(specificErrorMessage, 'Login Error');
        return of(AuthActions.loginFailure({ error: specificErrorMessage }));
      })
    ))
  ));

  // Logout
  logout$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.logout),
    exhaustMap(() => this.authService.logout().pipe(
      map(() => AuthActions.logoutSuccess()),
      catchError(error => {
        const specificErrorMessage = error instanceof Error ? error.message : 'An unknown error occurred during logout.';
        this.log.error('Logout failed', specificErrorMessage);
        this.toastr.error(specificErrorMessage, 'Logout Error');
        return of(AuthActions.logoutFailure({ error: specificErrorMessage }));
      })
    ))
  ));

  // Refresh Token
  refreshToken$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.refreshToken),
    exhaustMap(() => this.authService.refreshToken().pipe(
      map(() => AuthActions.refreshTokenSuccess()),
      catchError(error => {
        const specificErrorMessage = error instanceof Error ? error.message : 'An unknown error occurred during token refresh.';
        this.log.error('Token refresh failed', specificErrorMessage);
        this.toastr.error(specificErrorMessage, 'Authentication Error');
        return of(AuthActions.refreshTokenFailure({ error: specificErrorMessage }));
      })
    ))
  ));

  // Register
  register$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.register),
    exhaustMap(action => this.authService.register(action.details).pipe(
      map(response => {
        if (response.success) {
          this.toastr.success('Registration successful! Please check your email for confirmation.', 'Success');
          return AuthActions.registerSuccess({ response });
        } else {
          throw new Error(response.message || 'Registration failed');
        }
      }),
      catchError(error => {
        const specificErrorMessage = error instanceof Error ? error.message : 'An unknown error occurred during registration.';
        this.log.error('Registration failed', specificErrorMessage);
        this.toastr.error(specificErrorMessage, 'Registration Error');
        return of(AuthActions.registerFailure({ error: specificErrorMessage }));
      })
    ))
  ));

  // Consolidated navigation effect for all UI flows
  uiFlowCompleteNavigation$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.uiFlowComplete),
    withLatestFrom(this.store.select(selectIsAdmin)),
    tap(([action, isAdmin]) => {
      this.log.debug(`UI Flow Complete: source=${action.source}, isAdmin=${isAdmin}, returnUrl=${action.returnUrl}`);

      let targetRoute: string;

      // Determine target route based on source and other conditions
      if (action.returnUrl) {
        targetRoute = action.returnUrl;
        this.log.debug(`Using provided returnUrl: ${targetRoute}`);
      } else if (action.source === 'password-reset') {
        targetRoute = '/auth/login';
        this.log.debug(`Password reset complete, redirecting to login`);
      } else if (isAdmin) {
        targetRoute = '/admin';
        this.log.debug(`User is admin, redirecting to admin dashboard`);
      } else {
        targetRoute = '/';
        this.log.debug(`Default redirection to home page`);
      }

      this.log.info(`Navigating to ${targetRoute} after ${action.source}`);

      // Fix navigation handling for root and other paths
      if (targetRoute === '/') {
        this.router.navigate(['/'], { replaceUrl: true });
      } else if (targetRoute.startsWith('/')) {
        this.router.navigate([targetRoute.substring(1)], { replaceUrl: true });
      } else {
        this.router.navigateByUrl(targetRoute, {
          replaceUrl: true,
          onSameUrlNavigation: 'reload'
        });
      }
    })
  ), { dispatch: false });

  // Update existing effects to use the consolidated navigation approach
  // For backward compatibility, map old actions to the new one

  emailConfirmationBackwardCompat$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.emailConfirmationUiComplete),
    map(() => AuthActions.uiFlowComplete({
      source: 'email-confirmation',
      returnUrl: null
    }))
  ));

  passwordResetBackwardCompat$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.passwordResetUiComplete),
    map(() => AuthActions.uiFlowComplete({
      source: 'password-reset',
      returnUrl: null
    }))
  ));

  // Replace the old navigation effects with compatibility code that works with the new pattern
  loginSuccessNavigation$ = createEffect(() => this.actions$.pipe(
    ofType(AuthActions.loginSuccess),
    tap(action => {
      this.log.debug('LoginSuccess action triggered with returnUrl:', action.returnUrl);
    }),
    concatMap(action => {
      // Store the returnUrl for later use
      const savedReturnUrl = action.returnUrl;

      // Always wait for checkAuthSuccess to ensure we have full auth state
      return this.actions$.pipe(
        ofType(AuthActions.checkAuthSuccess),
        take(1),
        tap(checkAuthAction => {
          this.log.debug('CheckAuthSuccess received, user info:', checkAuthAction.response);
          this.toastr.success('Login successful!', 'Success');
        }),
        map(() => AuthActions.uiFlowComplete({
          source: 'login',
          returnUrl: savedReturnUrl
        }))
      );
    })
  ));
}