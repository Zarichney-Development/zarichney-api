import { createAction, props } from '@ngrx/store';
import { Order } from '../order.model';

export const loadOrder = createAction(
    '[Order] Load Order',
    props<{ orderId: string }>()
);

export const loadOrderSuccess = createAction(
    '[Order] Load Order Success',
    props<{ order: Order }>()
);

export const loadOrderFailure = createAction(
    '[Order] Load Order Failure',
    props<{ error: string }>()
);
