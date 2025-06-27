import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoggingService } from '../../../services/log.service';
import { ApiKeyResponse, CreateApiKeyCommand } from '../../../models/auth.models';
import { DOCUMENT } from '@angular/common';

@Component({
  selector: 'app-create-api-key',
  templateUrl: './create-api-key.component.html',
  styleUrls: ['./create-api-key.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule]
})
export class CreateApiKeyComponent implements OnInit {
  apiKeyForm!: FormGroup<{
    description: FormControl<string>;
    expiresInDays: FormControl<number | null>;
  }>;
  isLoading = false;
  error: string | null = null;
  newApiKey: ApiKeyResponse | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private log: LoggingService,
    private router: Router,
    @Inject(DOCUMENT) private document: Document
  ) { }

  ngOnInit(): void {
    this.apiKeyForm = this.fb.group({
      description: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
      expiresInDays: this.fb.control<number | null>(null)
    });
  }

  onSubmit(): void {
    if (this.apiKeyForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.error = null;
    this.newApiKey = null;

    const formValues = this.apiKeyForm.value;

    const command: CreateApiKeyCommand = {
      description: formValues.description ?? ''
    };

    // If expiresInDays is provided, calculate the expiration date
    if (formValues.expiresInDays) {
      const expirationDate = new Date();
      expirationDate.setDate(expirationDate.getDate() + formValues.expiresInDays);
      command.expiresAt = expirationDate.toISOString();
    }

    this.authService.createApiKey(command).subscribe({
      next: (response) => {
        this.newApiKey = response;
        this.isLoading = false;
        this.apiKeyForm.reset();
        this.log.info('API key created successfully', { id: response.id });
      },
      error: (err) => {
        this.isLoading = false;
        this.error = 'Failed to create API key. Please try again later.';
        this.log.error('Error creating API key', err);
      }
    });
  }

  navigateToList(): void {
    this.router.navigate(['/admin/api-keys']);
  }

  copyToClipboard(text: string): void {
    if (navigator.clipboard) {
      navigator.clipboard.writeText(text).then(() => {
        this.log.info('API key copied to clipboard');
      }).catch(err => {
        this.log.error('Could not copy text to clipboard', err);
      });
    } else {
      // Fallback for browsers that don't support clipboard API
      const textArea = this.document.createElement('textarea');
      textArea.value = text;
      this.document.body.appendChild(textArea);
      textArea.select();

      try {
        this.document.execCommand('copy');
        this.log.info('API key copied to clipboard (fallback)');
      } catch (err) {
        this.log.error('Could not copy text to clipboard (fallback)', err);
      }

      this.document.body.removeChild(textArea);
    }
  }
}
