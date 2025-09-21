import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { LoggingService } from '../../services/log.service';
import { AuthService } from '../../services/auth.service';
import * as AuthActions from './store/auth.actions';

import { isCookbookOrderDetailRoute } from '../../cookbook/routes/cookbook.routes';

export const authGuard: CanActivateFn = (route, state) => {
  const store = inject(Store);
  const router = inject(Router);
  const log = inject(LoggingService);
  const authService = inject(AuthService);

  // Special case: For cookbook order routes, check without redirection
  if (isCookbookOrderDetailRoute(state.url)) {
    log.warn(`COOKBOOK ROUTE: ${state.url} - EXPLICITLY ALLOWING ACTIVATION`);
    // We allow the route to activate, but API calls will still be subject to 401 handling
    return true;
  }

  log.info(`Auth guard checking: ${state.url}`);

  // Add extensive debugging to see what's happening at every step
  log.debug(`Auth guard activated for URL: ${state.url}`, { routeConfig: route.routeConfig });

  // Directly check with the server first, bypass store check for immediate verification
  return authService.checkAuthentication().pipe(
    tap(response => {
      log.debug('🔐 AUTH CHECK RESPONSE:', response);

      // Always update the store with the latest auth state from server
      store.dispatch(AuthActions.checkAuthSuccess({ response }));

      // Log the full response for debugging
      log.debug('Auth check details:', {
        isAuthenticated: response.isAuthenticated,
        userId: response.userId,
        isAdmin: response.isAdmin,
        roles: response.roles,
        authenticationType: response.authenticationType
      });
    }),
    map(response => {
      // Decision based solely on server response
      if (response.isAuthenticated === true) {
        log.info(`✅ Auth guard ALLOWING access: Server confirmed user is authenticated for ${state.url}`);
        return true;
      } else {
        log.warn(`❌ Auth guard DENYING access: Server says user is NOT authenticated for ${state.url}`);
        return router.createUrlTree(['/auth/login'], {
          queryParams: { returnUrl: state.url }
        });
      }
    }),
    catchError(error => {
      // Log the specific error
      log.error(`Auth guard error checking authentication for ${state.url}:`, error);

      // Return a redirect to login
      return of(router.createUrlTree(['/auth/login'], {
        queryParams: { returnUrl: state.url }
      }));
    })
  );
};
