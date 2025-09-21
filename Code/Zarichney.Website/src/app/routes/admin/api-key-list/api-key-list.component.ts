import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable, catchError, of, tap, finalize, BehaviorSubject } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { LoggingService } from '../../../services/log.service';
import { ApiKeyResponse } from '../../../models/auth.models';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-api-key-list',
  templateUrl: './api-key-list.component.html',
  styleUrls: ['./api-key-list.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class ApiKeyListComponent implements OnInit {
  private apiKeysSubject = new BehaviorSubject<ApiKeyResponse[]>([]);
  apiKeys$: Observable<ApiKeyResponse[]> = this.apiKeysSubject.asObservable();
  isLoading = true;
  error: string | null = null;
  revokingKeyId: number | null = null;

  constructor(
    private authService: AuthService,
    private log: LoggingService,
    private toastr: ToastrService,
    private changeDetectorRef: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadKeys();
  }

  loadKeys(): void {
    this.isLoading = true;
    this.error = null;
    this.log.debug('Fetching API keys from AuthService...');
    
    this.authService.getApiKeys().pipe(
      tap(keys => {
        this.log.debug('Successfully retrieved API keys', { count: keys.length });
      }),
      catchError(err => {
        this.error = 'Failed to load API keys. Please try again later.';
        this.log.error('Error loading API keys', err);
        return of([]);
      }),
      finalize(() => {
        this.isLoading = false;
        this.log.debug('API key loading finished, setting isLoading to false');
        this.changeDetectorRef.detectChanges();
      })
    ).subscribe({
      next: (keys) => {
        this.log.debug('Updating apiKeysSubject with fetched keys', { count: keys.length });
        this.apiKeysSubject.next(keys);
      },
      error: (err) => {
        this.error = 'Failed to load API keys. Please try again later.';
        this.log.error('Error in loadKeys subscribe handler', err);
        this.apiKeysSubject.next([]);
      }
    });
  }

  revokeKey(id: number): void {
    if (!confirm('Are you sure you want to revoke this API key? This action cannot be undone.')) {
      return;
    }

    this.revokingKeyId = id;
    this.authService.revokeApiKey(id).subscribe({
      next: () => {
        this.toastr.success('API key revoked successfully.');
        this.loadKeys();
        this.revokingKeyId = null;
        this.changeDetectorRef.detectChanges();
      },
      error: (err) => {
        this.toastr.error('Failed to revoke API key.', 'Error');
        this.log.error('Error revoking API key', err);
        this.revokingKeyId = null;
        this.changeDetectorRef.detectChanges();
      }
    });
  }
}
