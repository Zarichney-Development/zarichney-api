import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { BehaviorSubject, Observable, throwError, of } from 'rxjs';
import { catchError, filter, switchMap, take, finalize, tap } from 'rxjs/operators';
import { LoggingService } from '../../services/log.service';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../services/auth.service';
import { Store } from '@ngrx/store';
import { Router, NavigationExtras } from '@angular/router';
import * as AuthActions from './store/auth.actions';
import { getStaticPaths } from '../../routes/ssr.routes';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

  // Configuration for handling different endpoints
  private readonly endpointConfig = {
    // Public routes from SSR config - dynamically loaded from ssr.routes.ts
    // Will automatically update when SSR routes change
    get publicRoutes() {
      return getStaticPaths(); // Uses the exported helper function from ssr.routes.ts
    },

    // Auth endpoints that should not require authentication
    publicAuthEndpoints: [
      '/auth/login',
      '/auth/register',
      '/auth/logout',
      '/auth/resend-confirmation'
    ],

    // Auth endpoints that should trigger immediate logout on 401 (not token refresh)
    // This is for security when auth-specific endpoints fail
    authEndpoints: [
      '/auth/'          // Any other auth endpoint not in publicAuthEndpoints
    ],

    // Static file extensions to skip (no auth handling needed)
    staticFileExtensions: [
      '.jpg', '.jpeg', '.png', '.gif', '.svg', '.webp',  // Images
      '.css', '.scss', '.less',                          // Styles
      '.js', '.jsx', '.ts', '.tsx',                      // Scripts
      '.pdf', '.doc', '.docx', '.xls', '.xlsx',          // Documents
      '.ttf', '.woff', '.woff2', '.eot',                 // Fonts
      '.ico', '.xml', '.json', '.map',                   // Other common static files
      '.html', '.htm'                                    // Static HTML
    ]
  };

  constructor(
    private log: LoggingService,
    private toastr: ToastrService,
    private authService: AuthService,
    private store: Store,
    private router: Router
  ) {
    // Log the public routes loaded from SSR for debugging
    this.log.debug('🌐 AuthInterceptor: Public routes loaded from SSR:', this.endpointConfig.publicRoutes);
  }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // IMPORTANT DEBUG for understanding exactly which URLs are causing issues
    this.log.debug(`⚡ Intercepting HTTP request: ${request.method} ${request.url}`, {
      withCredentials: request.withCredentials,
      headers: request.headers.keys(),
      urlWithParams: request.urlWithParams
    });

    // Create a path from the URL for route matching (e.g., convert http://localhost:5000/test to /test)
    const urlPath = new URL(request.url).pathname;

    // Check if the request is for a static file by extension
    const isStaticFile = this.endpointConfig.staticFileExtensions.some(ext =>
      request.url.toLowerCase().endsWith(ext)
    );

    if (isStaticFile) {
      this.log.debug('⏩ Skipping auth interceptor for static file:', request.url);
      return next.handle(request);
    }

    // Check if the path matches a public route from SSR config
    const isPublicRoute = this.endpointConfig.publicRoutes.includes(urlPath);
    if (isPublicRoute) {
      this.log.debug('⏩ Skipping auth interceptor for public SSR route:', urlPath);
      return next.handle(request);
    }

    // Check if the endpoint is a public auth endpoint that doesn't need authentication
    const isPublicAuthEndpoint = this.urlMatchesAny(request.url, this.endpointConfig.publicAuthEndpoints);
    if (isPublicAuthEndpoint) {
      this.log.debug('⏩ Skipping auth interceptor for public auth endpoint:', request.url);
      return next.handle(request);
    }

    // At this point, all remaining requests require authentication
    this.log.debug('🔒 Intercepting authenticated request:', request.url);

    // Clone the request and add withCredentials: true
    const clonedRequest = request.clone({
      withCredentials: true
    });

    this.log.verbose('AuthInterceptor: Processing request with credentials', { url: clonedRequest.url });

    // Pass the modified request to the next handler, but immediately catch 401 errors
    return next.handle(clonedRequest).pipe(
      catchError((error: any) => {
        this.log.debug('🔴 Interceptor caught error:', error);

        // Check if it's a 401 error first - check both instanceof and direct status property
        const is401Error =
          (error instanceof HttpErrorResponse && error.status === 401) ||
          (error && error.status === 401);

        if (is401Error) {
          this.log.debug('🔄 Confirmed 401 Unauthorized error - attempting to handle with token refresh');
          return this.handle401Error(clonedRequest, next, error instanceof HttpErrorResponse ? error : new HttpErrorResponse({
            error: error.error || 'Unauthorized',
            status: 401,
            statusText: error.statusText || 'Unauthorized',
            url: request.url
          }));
        }

        // For all other errors, re-throw them to be handled elsewhere
        this.log.debug('⚠️ Not a 401 error - re-throwing for other handlers');
        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<unknown>, next: HttpHandler, originalError: HttpErrorResponse): Observable<HttpEvent<unknown>> {
    this.log.warn('⛔ Authentication error (401) - beginning token refresh process...', originalError);

    // Get the current URL for the returnUrl parameter (before any navigation happens)
    const currentUrl = window.location.pathname + window.location.search;
    this.log.debug('📍 Current URL for returnUrl:', currentUrl);

    // Log detailed request information for debugging
    this.log.debug('🔍 401 Error Request Details:', {
      url: request.url,
      method: request.method,
      urlWithParams: request.urlWithParams,
      headers: request.headers.keys(),
      responseStatus: originalError.status,
      responseStatusText: originalError.statusText
    });

    // Check if it's a protected auth endpoint (Auth endpoints not in the public list)
    const isProtectedAuthEndpoint = this.urlMatchesAny(request.url, this.endpointConfig.authEndpoints) &&
      !this.urlMatchesAny(request.url, this.endpointConfig.publicAuthEndpoints);

    // If it's a protected auth endpoint, log out immediately without token refresh
    if (isProtectedAuthEndpoint) {
      this.log.warn('⚠️ 401 on protected auth endpoint - logging out immediately without refresh attempt');
      this.store.dispatch(AuthActions.logout());
      this.toastr.warning('Your session has expired. Please log in again.', 'Session Expired');
      this.redirectToLogin(currentUrl);
      return throwError(() => originalError);
    }

    // For all other 401 errors, proceed with token refresh
    // If we're not already refreshing
    if (!this.isRefreshing) {
      // Set the refreshing flag to true
      this.isRefreshing = true;
      // Clear the token subject
      this.refreshTokenSubject.next(null);

      this.log.debug('Attempting token refresh...');
      // Attempt to refresh the token
      return this.authService.refreshToken().pipe(
        tap(response => {
          this.log.debug('Token refresh successful, response:', response);
        }),
        switchMap(response => {
          // Mark the refresh as complete
          this.isRefreshing = false;

          // Notify waiting requests that they can proceed
          this.refreshTokenSubject.next('new_token_placeholder');

          // Retry the original request with the new token
          return next.handle(request);
        }),
        catchError(error => {
          this.log.error('Token refresh failed, initiating logout sequence', error);

          // Reset the refreshing flag
          this.isRefreshing = false;

          // Dispatch logout action to clean up auth state
          this.store.dispatch(AuthActions.logout());

          // Show a message to the user
          this.toastr.error('Your session has expired. Please log in again.', 'Session Expired');

          // Redirect to login page with the return URL
          this.redirectToLogin(currentUrl);

          // Notify waiting requests that refresh failed
          this.refreshTokenSubject.error(error);

          // Return the original error
          return throwError(() => originalError);
        }),
        finalize(() => {
          // Ensure the refreshing flag is reset in case of any unexpected issues
          this.isRefreshing = false;
        })
      );
    } else {
      this.log.debug('Token refresh already in progress, waiting...');
      // If refresh is already in progress, wait for it to complete
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(() => {
          this.log.debug('Token refresh completed by another request, retrying original request');
          return next.handle(request);
        }),
        catchError(error => {
          this.log.error('Error during request retry after token refresh', error);

          // If the refresh process errors out, redirect and propagate the error
          this.redirectToLogin(currentUrl);

          return throwError(() => originalError);
        })
      );
    }
  }

  /**
   * Helper method to check if a URL contains any of the specified endpoints
   */
  private urlMatchesAny(url: string, endpoints: string[]): boolean {
    return endpoints.some(endpoint => url.includes(endpoint));
  }

  /**
   * Helper method to handle redirection to login page
   * This method uses multiple approaches to ensure redirection works reliably
   */
  private redirectToLogin(returnUrl: string): void {
    this.log.debug('🚨 REDIRECTING TO LOGIN FROM:', returnUrl);

    // Force immediate navigation with window.location.replace
    // window.location.replace is more aggressive than href as it doesn't add to history
    try {
      const loginUrl = `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;
      this.log.debug('🔀 FORCING NAVIGATION TO:', loginUrl);

      // Show toast notification first (this will be seen before redirect)
      this.toastr.warning('You need to log in to access this resource', 'Authentication Required');

      // Use a tiny delay to ensure toast is shown
      setTimeout(() => {
        // Force immediate redirect using window.location.replace
        this.log.debug('↪️ EXECUTING REDIRECT NOW');
        window.location.replace(loginUrl);

        // Backup in case replace doesn't trigger immediately
        setTimeout(() => {
          this.log.debug('↪️ BACKUP REDIRECT with location.href');
          window.location.href = loginUrl;
        }, 100);
      }, 10);
    } catch (e) {
      this.log.error('⛔ Exception during direct navigation, trying alternatives:', e);

      // Try using window.location.href as fallback
      try {
        window.location.href = `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`;
      } catch (hrefError) {
        this.log.error('⛔ window.location.href failed:', hrefError);

        // Last resort: Angular Router
        try {
          const navigationExtras: NavigationExtras = {
            queryParams: { returnUrl: encodeURIComponent(returnUrl) }
          };

          this.router.navigate(['/auth/login'], navigationExtras)
            .then(success => {
              if (success) {
                this.log.debug('✅ Router navigation completed');
              } else {
                this.log.warn('⚠️ Router navigation returned false');

                // Try one more approach - assign to location
                window.location.assign(`/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`);
              }
            })
            .catch(error => this.log.error('⛔ Router navigation failed:', error));
        } catch (routerError) {
          this.log.error('⛔ ALL NAVIGATION METHODS FAILED:', routerError);
        }
      }
    }
  }
}