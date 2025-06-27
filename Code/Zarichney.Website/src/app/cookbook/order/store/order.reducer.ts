import { createReducer, on } from '@ngrx/store';
import * as OrderActions from './order.actions';
import { Order } from '../order.model';

export interface OrderState {
    orders: Order[];
    loading: boolean;
    error: string | null;
}

export const initialState: OrderState = {
    orders: [],
    loading: false,
    error: null
};

export const ORDER_FEATURE_KEY = 'order';

export const orderReducer = createReducer(
    initialState,
    on(OrderActions.loadOrder, (state) => ({
        ...state,
        loading: true
    })),
    on(OrderActions.loadOrderSuccess, (state, { order }) => ({
        ...state,
        orders: [...state.orders, order],
        loading: false
    })),
    on(OrderActions.loadOrderFailure, (state, { error }) => ({
        ...state,
        error,
        loading: false
    }))
);

