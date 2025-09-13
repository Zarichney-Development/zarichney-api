import { Routes, Router, NavigationEnd } from '@angular/router';
import { DynamicScreenComponent } from './dynamic/dynamic.component';
import { RecorderComponent } from './recorder/recorder.component';
import { OrderOverviewComponent } from '../cookbook/routes/order-overview/order-overview.component';
import { COOKBOOK_PATHS, isCookbookOrderDetailRoute } from '../cookbook/routes/cookbook.routes';
import { staticRoutes, getStaticPaths } from './ssr.routes';
import { inject, NgZone, PLATFORM_ID, APP_INITIALIZER, Provider } from '@angular/core';
import { LoggingService } from '../services/log.service';
import { isPlatformBrowser } from '@angular/common';
import { REQUEST } from '../utils/request.token';
import { authGuard } from './auth/auth.guard';
import { adminGuard } from './auth/admin.guard';
import { filter } from 'rxjs/operators';

// Router debugging provider
export const routerLoggingProvider: Provider = {
    provide: APP_INITIALIZER,
    multi: true,
    deps: [Router, LoggingService],
    useFactory: (router: Router, log: LoggingService) => {
        return () => {
            router.events.pipe(
                filter(event => event instanceof NavigationEnd)
            ).subscribe((event: any) => {
                log.routeNav(event.urlAfterRedirects || event.url, {
                    id: event.id,
                    navigationTrigger: event.navigationTrigger,
                    routerState: router.routerState.snapshot.url
                });
            });
        };
    }
};

const staticPaths = new Set(getStaticPaths());

function isStaticRoute(path: string): boolean {
    // Normalize the path similar to the server-side normalization
    const normalized = path === '/' ? '' : path.replace(/^\/+|\/+$/g, '');

    // Extract the base path without parameters for comparison
    // This handles routes with parameters like /cookbook/order/:orderId
    let routePath = normalized === '' ? '/' : `/${normalized}`;

    // Remove any query parameters
    if (routePath.includes('?')) {
        routePath = routePath.split('?')[0];
    }

    // For paths with dynamic segments (e.g., /cookbook/order/abc123),
    // check if there's a corresponding static route pattern
    if (routePath.includes('/cookbook/order/')) {
        console.log(`[Route Check] Cookbook order path detected: ${routePath}`);
        return false; // This is a dynamic route, not static
    }

    const isStatic = staticPaths.has(routePath);
    console.log(`[Route Check] Path: ${path}, Normalized: ${routePath}, IsStatic: ${isStatic}`);
    return isStatic;
}

function dynamicRouteGuard() {
    const log = inject(LoggingService);
    const platformId = inject(PLATFORM_ID);
    const ngZone = inject(NgZone);
    const request = inject(REQUEST, { optional: true });
    // Removed unused router injection

    return ngZone.runOutsideAngular(() => {
        let path = '/';
        if (isPlatformBrowser(platformId)) {
            path = window.location.pathname;
        } else if (request) {
            path = request.url;
        }

        // Special case handling for cookbook order routes using the imported function
        if (isCookbookOrderDetailRoute(path)) {
            log.info(`COOKBOOK ROUTE: ${path} - EXPLICITLY ALLOWING ACTIVATION`);
            return true;
        }

        log.verbose(`Guard checking path: ${path}`);
        const shouldAllow = !isStaticRoute(path);
        log.info(`${shouldAllow ? 'Dynamic' : 'Static'} route ${path} - ${shouldAllow ? 'handled by SPA' : 'allowing server-side render'}`);
        return shouldAllow;
    });
}

// Update preserveContentGuard similarly
const preserveContentGuard = () => {
    const platformId = inject(PLATFORM_ID);
    const request = inject(REQUEST, { optional: true });
    let path = '/';

    if (isPlatformBrowser(platformId)) {
        path = window.location.pathname;
    } else if (request) {
        path = request.url;
    }

    return isStaticRoute(path);
};

export const spaRoutes: Routes = [
    // Static/prerendered routes
    ...staticRoutes.map(route => ({
        ...route,
        data: { prerendered: true },
        canActivate: [preserveContentGuard],
        canDeactivate: [preserveContentGuard]
    })),

    // Dynamic routes
    { path: 'home', redirectTo: '/', pathMatch: 'full' },
    {
        path: 'transcribe',
        component: RecorderComponent,
        canActivate: [dynamicRouteGuard]
    },
    {
        path: 'prompt',
        component: RecorderComponent,
        canActivate: [dynamicRouteGuard]
    },
    // Import cookbook routes using constants
    {
        path: COOKBOOK_PATHS.ORDER_DETAIL_PATTERN,
        component: OrderOverviewComponent,
        // Only use dynamicRouteGuard here, authGuard now bypasses for this specific route
        canActivate: [dynamicRouteGuard],
        data: {
            renderMode: 'dynamic',
            routeType: 'dynamic-param', // Mark as dynamic route with parameters
            noAuth: true // Flag to indicate this route should bypass auth checks
        }
    },
    // You could also lazy load the entire cookbook module for better scalability
    {
        path: 'auth',
        loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule),
        canActivate: [dynamicRouteGuard]
    },
    // Simple direct route for debugging - no lazy loading
    {
        path: 'admin-simple',
        component: DynamicScreenComponent,
        canActivate: [dynamicRouteGuard, authGuard, adminGuard],
        data: { debugRoute: true }
    },
    {
        path: 'admin',
        loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule),
        canActivate: [dynamicRouteGuard, authGuard, adminGuard]
    },
    {
        path: '**',
        component: DynamicScreenComponent,
        canActivate: [dynamicRouteGuard]
    }
];
