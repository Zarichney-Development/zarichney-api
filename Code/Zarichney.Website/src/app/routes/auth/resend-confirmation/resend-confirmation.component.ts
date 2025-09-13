import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { LoggingService } from '../../../services/log.service';
import { ToastrService } from 'ngx-toastr';
import { ResendConfirmationRequest } from '../../../models/auth.models';

@Component({
  selector: 'app-resend-confirmation',
  templateUrl: './resend-confirmation.component.html',
  styleUrls: ['./resend-confirmation.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class ResendConfirmationComponent {
  resendForm: FormGroup;
  isLoading = false;
  submitted = false;

  constructor(
    private authService: AuthService,
    private formBuilder: FormBuilder,
    private toastr: ToastrService,
    private logger: LoggingService
  ) {
    this.resendForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    this.submitted = true;
    
    if (this.resendForm.invalid) {
      return;
    }

    this.isLoading = true;
    const email = this.resendForm.get('email')?.value;
    
    this.authService.resendConfirmation({ email } as ResendConfirmationRequest).subscribe({
      next: () => {
        this.isLoading = false;
        this.logger.info('Confirmation email resend request submitted', { email });
        // Form is kept as is to show the success message
      },
      error: (error) => {
        this.isLoading = false;
        this.logger.error('Failed to resend confirmation email', error);
        this.toastr.error('Failed to submit request. Please try again later.', 'Error');
      }
    });
  }
}
