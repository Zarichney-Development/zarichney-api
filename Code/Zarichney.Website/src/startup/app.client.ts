import 'zone.js';
import { bootstrapApplication, provideClientHydration, withHttpTransferCacheOptions } from '@angular/platform-browser';
import { enableProdMode } from '@angular/core';
import { RootComponent } from '../app/components/root/root.component';
import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi, withFetch, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideRouter, withViewTransitions } from '@angular/router';
import { spaRoutes, routerLoggingProvider } from '../app/routes/app.routes';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { ToastrModule } from 'ngx-toastr';
import { authReducer } from '../app/routes/auth/store/auth.reducer';
import { AuthEffects } from '../app/routes/auth/store/auth.effects';
import { AUTH_FEATURE_KEY } from '../app/routes/auth/store/auth.state';
import { AuthInterceptor } from '../app/routes/auth/auth.interceptor';
import { hmrBootstrap } from '../app/utils/hmr';
import { environment } from './environments';
import { provideAnimations } from '@angular/platform-browser/animations';

export const clientAppConfig: ApplicationConfig = {
  providers: [
    provideRouter(
      spaRoutes,
      withViewTransitions()
    ),
    provideClientHydration(
      withHttpTransferCacheOptions({
        includePostRequests: true,
        filter: (_req) => false // enable this to turn off client side api calls
      })
    ),
    { provide: 'APP_BASE_HREF', useValue: '/' },
    // Explicitly register HTTP interceptor
    provideHttpClient(
      withInterceptorsFromDi(),
      withFetch()
    ),
    // Explicitly provide the AuthInterceptor
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    provideStore({
      [AUTH_FEATURE_KEY]: authReducer
    }),
    provideEffects(AuthEffects),
    importProvidersFrom(ToastrModule.forRoot()),
    provideAnimations(),
    routerLoggingProvider
  ]
};

const bootstrap = () => bootstrapApplication(RootComponent, clientAppConfig);

const initializeApp = async () => {
  try {
    if (typeof window !== 'undefined') { // Ensure we're in browser context
      console.log('Initializing browser application');

      if (environment.development && environment.hmr && (window as unknown as { module?: { hot?: unknown } })['module']?.hot) {
        hmrBootstrap((window as unknown as { module: unknown })['module'], bootstrap);
        return;
      }

      if (environment.production) {
        enableProdMode();
      }
    }

    await bootstrap();
  } catch (err) {
    console.error('Error initializing application:', err);
  }
};

// Only execute if we're in the browser
if (typeof window !== 'undefined') {
  initializeApp();
}

export default bootstrap;
