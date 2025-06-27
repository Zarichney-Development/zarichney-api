import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { RootComponent } from '../app/components/root/root.component';
import { ApplicationConfig, mergeApplicationConfig } from '@angular/core';
import { provideServerRendering } from '@angular/platform-server';
import { clientAppConfig } from './app.client';
import { provideServerRouting, RenderMode, ServerRoute } from '@angular/ssr';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { authReducer } from '../app/routes/auth/store/auth.reducer';
import { AuthEffects } from '../app/routes/auth/store/auth.effects';
import { AUTH_FEATURE_KEY } from '../app/routes/auth/store/auth.state';
import { provideAnimations } from '@angular/platform-browser/animations';

export const ssrRoutes: ServerRoute[] = [
    { path: '', renderMode: RenderMode.Prerender },
    { path: 'test', renderMode: RenderMode.Prerender },
    { path: '**', renderMode: RenderMode.Server }
];

const serverOverrideConfig: ApplicationConfig = {
    providers: [
        provideServerRendering(),
        provideServerRouting(ssrRoutes),
        provideStore({
            [AUTH_FEATURE_KEY]: authReducer
        }),
        provideEffects(AuthEffects),
        provideAnimations()
    ]
};

export const serverConfig = mergeApplicationConfig(clientAppConfig, serverOverrideConfig);

const bootstrap = () => bootstrapApplication(RootComponent, serverConfig);

export default bootstrap;
