import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { AUTH_ROUTES } from './auth.routes';
// Removed unused store-related imports

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild(AUTH_ROUTES)
  ],
  // Components will be standalone, no need to declare them here
  declarations: []
})
export class AuthModule { }
