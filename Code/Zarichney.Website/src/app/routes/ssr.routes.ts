import { Routes } from '@angular/router';
import { HomeScreenComponent } from './home/home.component';
import { TestScreenComponent } from './test/test.component';

export interface StaticRoute {
    path: string;
    component: any;
    title?: string;
}

export const staticRoutes: StaticRoute[] = [
    { path: '', component: HomeScreenComponent, title: 'Home' },
    { path: 'test', component: TestScreenComponent, title: 'Test' }
];

export const ssrRoutes: Routes = staticRoutes.map(route => ({
    ...route,
    data: { prerendered: true }
}));

// Helper function to get paths for prerender config
export const getStaticPaths = () =>
    staticRoutes.map(route => route.path === '' ? '/' : `/${route.path}`);
