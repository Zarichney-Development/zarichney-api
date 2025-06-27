import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { orderReducer, ORDER_FEATURE_KEY, initialState } from './order.reducer';
import { OrderEffects } from './order.effects';

@NgModule({
  imports: [
    CommonModule,
    StoreModule.forFeature(ORDER_FEATURE_KEY, orderReducer, { initialState }),
    EffectsModule.forFeature([OrderEffects])
  ],
  declarations: []
})
export class OrderStoreModule { }
