// src/app/cookbook/order/store/order.effects.ts
import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { of } from 'rxjs';
import { map, mergeMap, catchError } from 'rxjs/operators';
import { OrderService } from '../order.service';
import * as OrderActions from './order.actions';

@Injectable()
export class OrderEffects {
    loadOrder$ = createEffect(() =>
        this.actions$.pipe(
            ofType(OrderActions.loadOrder),
            mergeMap(action =>
                this.orderService.fetchOrder(action.orderId).pipe(
                    map(order => OrderActions.loadOrderSuccess({ order })),
                    catchError(error => of(OrderActions.loadOrderFailure({ error: error.message })))
                )
            )
        )
    );

    constructor(
        private actions$: Actions,
        private orderService: OrderService
    ) { }
}