import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { LoggingService } from '../../../services/log.service';
import { ToastrService } from 'ngx-toastr';
// Removed unused ForgotPasswordRequest import

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  isLoading = false;
  submitted = false;

  constructor(
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    private logger: LoggingService
  ) {
    this.forgotPasswordForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    this.submitted = true;
    
    if (this.forgotPasswordForm.invalid) {
      return;
    }

    this.isLoading = true;
    const email = this.forgotPasswordForm.get('email')?.value;
    
    this.authService.forgotPassword({ email }).subscribe({
      next: () => {
        this.isLoading = false;
        this.logger.info('Password reset request submitted successfully', { email });
        // Form is kept as is to show the success message
      },
      error: (error) => {
        this.isLoading = false;
        this.logger.error('Failed to submit password reset request', error);
        this.toastr.error('Failed to submit password reset request. Please try again later.', 'Error');
      }
    });
  }
}
