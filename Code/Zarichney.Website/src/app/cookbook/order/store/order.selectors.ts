import { createFeatureSelector, createSelector } from '@ngrx/store';
import { ORDER_FEATURE_KEY } from './order.reducer';
import { OrderState } from './order.reducer';

// Use the same feature key as in AppModule.
export const selectOrderState = createFeatureSelector<OrderState>(ORDER_FEATURE_KEY);

export const selectOrders = createSelector(
    selectOrderState,
    state => state ? state.orders : []
);

export const selectOrderById = (orderId: string) =>
    createSelector(
        selectOrderState,
        state => state ? state.orders.find(order => order.orderId === orderId) : undefined
    );
