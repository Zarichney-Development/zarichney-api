import { Routes } from '@angular/router';
import { OrderOverviewComponent } from './order-overview/order-overview.component';

// Constants for route paths to be used throughout the application
export const COOKBOOK_ROUTES = {
    BASE: 'cookbook',
    ORDER: 'order',
    ORDER_DETAIL: 'order/:orderId'
};

// Helper to generate full paths
export const COOKBOOK_PATHS = {
    ORDER_DETAIL_PATTERN: `${COOKBOOK_ROUTES.BASE}/${COOKBOOK_ROUTES.ORDER_DETAIL}`,
    ORDER_DETAIL: (orderId: string) => `/${COOKBOOK_ROUTES.BASE}/${COOKBOOK_ROUTES.ORDER}/${orderId}`
};

// Helper to test if a URL path matches a cookbook order detail route
export function isCookbookOrderDetailRoute(path: string): boolean {
    return !!path.match(new RegExp(`^\/${COOKBOOK_ROUTES.BASE}\/${COOKBOOK_ROUTES.ORDER}\/[^/]+$`));
}

export const orderRoutes: Routes = [
    {
        path: COOKBOOK_ROUTES.ORDER_DETAIL,
        component: OrderOverviewComponent
    }
];
