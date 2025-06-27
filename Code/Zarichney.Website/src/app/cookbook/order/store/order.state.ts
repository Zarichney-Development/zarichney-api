import { Order } from '../order.model';

export interface OrderState {
    orders: Order[];
    loading: boolean;
    error: string | null;
}

export const initialOrderState: OrderState = {
    orders: [],
    loading: false,
    error: null
};
