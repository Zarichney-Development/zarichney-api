import { CanActivateFn, UrlTree } from '@angular/router';
import { inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { LoggingService } from '../../services/log.service';
import { AuthService } from '../../services/auth.service';
import * as AuthActions from './store/auth.actions';

export const adminGuard: CanActivateFn = (route, state) => {
  const store = inject(Store);
  const router = inject(Router);
  const log = inject(LoggingService);
  const authService = inject(AuthService);

  log.info('🔑 Admin guard activated for route:', state.url);

  // Directly check with the server for authentication status
  return authService.checkAuthentication().pipe(
    tap(response => {
      log.debug('🔐 ADMIN CHECK RESPONSE:', response);

      // Always update the store with the latest auth state from server
      store.dispatch(AuthActions.checkAuthSuccess({ response }));

      // Log the full response for debugging
      log.debug('Admin check details:', {
        isAuthenticated: response.isAuthenticated,
        userId: response.userId,
        isAdmin: response.isAdmin,
        roles: response.roles,
        authenticationType: response.authenticationType
      });
    }),
    map(response => {
      // Check if user is authenticated first
      if (response.isAuthenticated !== true) {
        log.warn(`❌ Admin guard: User is NOT authenticated for ${state.url}`);
        return router.createUrlTree(['/auth/login'], {
          queryParams: { returnUrl: state.url }
        });
      }

      // Check if user is an admin
      const isAdmin = response.isAdmin === true || (response.roles && response.roles.includes('admin'));

      if (isAdmin) {
        log.info(`✅ Admin guard: User IS admin, allowing access to ${state.url}`);
        return true;
      } else {
        log.warn(`❌ Admin guard: User is authenticated but NOT admin, redirecting to home`);
        return router.parseUrl('/');
      }
    }),
    catchError(error => {
      log.error(`Admin guard error checking authentication for ${state.url}:`, error);
      return of(router.createUrlTree(['/auth/login'], {
        queryParams: { returnUrl: state.url }
      }));
    })
  );
};