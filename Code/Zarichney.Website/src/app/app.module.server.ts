import { NgModule } from '@angular/core';
import { ServerModule } from '@angular/platform-server';
import { RootComponent } from './components/root/root.component';
import { ClientAppModule } from './app.module.client';
import { provideRouter } from '@angular/router';
import { ssrRoutes } from './routes/ssr.routes';
import { provideHttpClient, withFetch } from '@angular/common/http';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';

@NgModule({
    imports: [
        ClientAppModule,
        ServerModule,
        RootComponent,
        StoreModule.forRoot({}),
        EffectsModule.forRoot([])
    ],
    providers: [
        provideHttpClient(withFetch()),
        provideRouter(ssrRoutes)
    ]
})
export class AppServerModule { }
