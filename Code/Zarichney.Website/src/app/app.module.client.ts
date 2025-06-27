import { NgModule, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { spaRoutes } from './routes/app.routes';
import { StoreModule, Store } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { OrderStoreModule } from './cookbook/order/store/order-store.module';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, provideHttpClient, withFetch } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';
import { AuthStoreModule } from './routes/auth/store/auth-store.module';
import { AuthInterceptor } from './routes/auth/auth.interceptor';
import { filter, first } from 'rxjs/operators';
import * as AuthActions from './routes/auth/store/auth.actions';
import { selectAuthStatus } from './routes/auth/store/auth.selectors';
import { AuthState } from './routes/auth/store/auth.state';
import { provideAppInitializer, provideEnvironmentInitializer } from '@angular/core';

export function initAuthFactory(store: Store<AuthState>) {
    return () => {
        console.log('Initializing auth state at app startup');
        // Dispatch checkAuth action to verify authentication state at startup
        store.dispatch(AuthActions.checkAuth());

        return store.select(selectAuthStatus).pipe(
            // Wait for auth check to complete (success or error)
            filter(status => status === 'success' || status === 'error'),
            first(), // Complete after the first non-pending status
            // Add timeout handling if needed
            // timeout(5000), // Optional: Add timeout if the auth check takes too long
        );
    };
}

@NgModule({
    declarations: [],
    imports: [
        BrowserModule,
        CommonModule,
        RouterModule.forRoot(spaRoutes),
        StoreModule.forRoot({}),
        EffectsModule.forRoot([]),
        OrderStoreModule,
        AuthStoreModule
    ],
    providers: [
        provideHttpClient(withFetch()),
        provideToastr(),
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
        provideAppInitializer(() => {
            const store = inject(Store<AuthState>);
            return initAuthFactory(store)();
        }),
        provideEnvironmentInitializer(() => {
            console.log('Environment initialized with auth check');
        })
    ]
})
export class ClientAppModule { }
