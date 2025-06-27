import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { EmailConfirmedComponent } from './email-confirmed/email-confirmed.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ResendConfirmationComponent } from './resend-confirmation/resend-confirmation.component';

export const AUTH_ROUTES: Routes = [
  { path: 'login', component: LoginComponent, title: 'Login' },
  { path: 'register', component: RegisterComponent, title: 'Register' },
  { path: 'email-confirmation', component: EmailConfirmedComponent, title: 'Email Confirmed' },
  { path: 'forgot-password', component: ForgotPasswordComponent, title: 'Forgot Password' },
  { path: 'reset-password', component: ResetPasswordComponent, title: 'Reset Password' },
  { path: 'resend-confirmation', component: ResendConfirmationComponent, title: 'Resend Confirmation' },
  { path: '', redirectTo: 'login', pathMatch: 'full' } // Default redirect within auth module
];
