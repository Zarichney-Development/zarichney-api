import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoggingService } from '../../../services/log.service';
import { ToastrService } from 'ngx-toastr';
import { ResetPasswordRequest } from '../../../models/auth.models';
import { Store } from '@ngrx/store';
import * as AuthActions from '../store/auth.actions';

// Custom validator to check if password and confirmPassword match
export function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('newPassword');
  const confirmPassword = control.get('confirmPassword');

  if (password && confirmPassword && password.value !== confirmPassword.value) {
    // Set the error on the confirmPassword control
    confirmPassword.setErrors({ 'passwordMismatch': true });
    return { 'passwordMismatch': true };
  }

  // If confirmPassword has errors other than passwordMismatch, don't overwrite them
  if (confirmPassword?.errors && !confirmPassword.errors['passwordMismatch']) {
    return null;
  }

  // Clear the passwordMismatch error if passwords match
  if (confirmPassword) {
    confirmPassword.setErrors(null);
  }

  return null;
}

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule]
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  isLoading = false;
  error: string | null = null;
  success = false;
  email: string | null = null;
  token: string | null = null;

  constructor(
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private logger: LoggingService,
    private store: Store
  ) {
    this.resetPasswordForm = this.formBuilder.group({
      newPassword: ['', Validators.required],
      confirmPassword: ['', Validators.required]
    }, {
      validators: passwordMatchValidator
    });
  }

  ngOnInit(): void {
    // Replace subscription with snapshot query params for proper parsing
    const params = this.route.snapshot.queryParams;
    this.logger.info('Query Params', params);

    let email = params['email'];
    let token = params['token'];

    // Handle case where keys include "=" within key names
    if (!email && !token) {
      for (const key of Object.keys(params)) {
        if (key.startsWith('email=')) {
          email = key.substring(6);
        }
        if (key.startsWith('token=')) {
          token = key.substring(6);
        }
      }
    }

    if (!email) {
      this.error = 'Email parameter not received.';
      this.logger.error('Email parameter missing in reset password link, parsed query params:', params);
      return;
    }
    if (!token) {
      this.error = 'Token parameter not received.';
      this.logger.error('Token parameter missing in reset password link, parsed query params:', params);
      return;
    }

    this.email = email;
    this.token = token;
  }

  onSubmit(): void {
    if (this.resetPasswordForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.error = null;
    this.success = false;

    const { newPassword } = this.resetPasswordForm.value;

    var resetPasswordRequest: ResetPasswordRequest = {
      email: this.email!,
      token: this.token!,
      newPassword: newPassword
    };

    this.authService.resetPassword(resetPasswordRequest).subscribe({
      next: () => {
        this.isLoading = false;
        this.success = true;
        this.logger.info('Password reset successful', { email: this.email });
        this.toastr.success('Password reset successfully. You can now log in.', 'Success');

        // Use the consolidated action
        this.logger.debug('Dispatching uiFlowComplete action for password-reset');
        this.store.dispatch(AuthActions.uiFlowComplete({
          source: 'password-reset',
          returnUrl: null
        }));
      },
      error: (error) => {
        this.isLoading = false;
        let errorMessage = 'Failed to reset password. Please try again.';

        // Extract a more specific error message if available
        if (error.error && error.error.message) {
          errorMessage = error.error.message;
        } else if (error.status === 400) {
          errorMessage = 'Invalid or expired token. Please request a new password reset link.';
        }

        this.error = errorMessage;
        this.logger.error('Password reset failed', error);
        this.toastr.error(errorMessage, 'Error');
      }
    });
  }
}
