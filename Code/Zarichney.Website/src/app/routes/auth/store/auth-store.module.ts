import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { authReducer } from './auth.reducer';
import { AuthEffects } from './auth.effects';
import { AUTH_FEATURE_KEY } from './auth.state';
import { importProvidersFrom } from '@angular/core';

// For NgModule-based approach
@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    StoreModule.forFeature(AUTH_FEATURE_KEY, authReducer),
    EffectsModule.forFeature([AuthEffects])
  ]
})
export class AuthStoreModule { }

// For standalone components and providedIn approach
export function provideAuthFeature() {
  return {
    ngModule: AuthStoreModule
  };
}