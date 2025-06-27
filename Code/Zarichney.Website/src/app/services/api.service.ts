import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../startup/environments';
import { LoggingService } from '../services/log.service';

export interface ApiResponse {
  message: string;
}

export interface TranscriptionResponse {
  message: string;
  audioFile: string;
  transcriptFile: string;
  timestamp: string;
  transcript: string;
}

export interface CompletionResponse {
  response: string;
  sourceType: 'audio' | 'text';
  transcribedPrompt: string | null;
}

export interface PdfOptions {
  rebuild?: boolean;
  email?: boolean;
}

export interface RequestOptions {
  headers?: HttpHeaders | { [header: string]: string | string[] };
  params?: HttpParams | { [param: string]: string | number | boolean | ReadonlyArray<string | number | boolean> };
  responseType?: 'json' | 'text' | 'blob' | 'arraybuffer';
  withCredentials?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private apiUrl = environment.apiUrl;

  constructor(private log: LoggingService, private http: HttpClient) {
    this.log.verbose('ApiService.constructor()', http);
  }

  private handleError = (error: HttpErrorResponse) => {
    // Log all errors for debugging
    this.log.debug(`ApiService caught error (${error.status}):`, error);
    
    // Special case: Do not handle 401 errors here, let the auth interceptor handle them
    if (error.status === 401) {
      this.log.debug('🔑 ApiService: Detected 401 error, preserving for interceptor');
      // Re-throw the original error for the interceptor to catch
      return throwError(() => error);
    }
    
    // Handle other errors normally
    let errorMessage = '';
    if (typeof ErrorEvent !== 'undefined' && error.error instanceof ErrorEvent) {
      // A client-side or network error occurred
      errorMessage = `An error occurred: ${error.error.message}`;
    } else {
      // The backend returned an unsuccessful response code
      if (error.status === 400 && error.error && typeof error.error === 'object' && 'message' in error.error) {
        // Handle specific 400 errors with a message property (like the AuthResponse structure)
        errorMessage = error.error.message;
      } else if (error.error && typeof error.error === 'string') {
        // Handle error.error as a string message
        errorMessage = error.error;
      } else if (error.message) {
        // Use error message if available
        errorMessage = error.message;
      } else {
        // Default to status code message
        errorMessage = `${error.status} response: ${error.statusText || 'Unknown error'}`;
      }
    }
    
    // Log and return a user-friendly message for non-401 errors
    this.log.error(errorMessage, error);
    return throwError(() => new Error(errorMessage));
  }

  // Generic HTTP methods for JSON responses
  get<T>(endpoint: string, options?: Omit<RequestOptions, 'responseType'>): Observable<T> {
    const url = `${this.apiUrl}${endpoint}`;
    this.log.verbose(`GET request for <${url}>`);

    return this.http.get<T>(url, { ...options, responseType: 'json' })
      .pipe(catchError(this.handleError));
  }

  // Special method for blob responses
  getBlob(endpoint: string, options?: Omit<RequestOptions, 'responseType'>): Observable<Blob> {
    const url = `${this.apiUrl}${endpoint}`;
    this.log.verbose(`GET blob request for <${url}>`);

    return this.http.get(url, { ...options, responseType: 'blob' })
      .pipe(catchError(this.handleError));
  }

  // You can add similar methods for other response types if needed
  getText(endpoint: string, options?: Omit<RequestOptions, 'responseType'>): Observable<string> {
    const url = `${this.apiUrl}${endpoint}`;
    this.log.verbose(`GET text request for <${url}>`);

    return this.http.get(url, { ...options, responseType: 'text' })
      .pipe(catchError(this.handleError));
  }

  post<T>(endpoint: string, data: any, options?: Omit<RequestOptions, 'responseType'>): Observable<T> {
    const url = `${this.apiUrl}${endpoint}`;
    this.log.verbose(`POST request to <${url}>`);

    return this.http.post<T>(url, data, { ...options, responseType: 'json' })
      .pipe(catchError(this.handleError));
  }

  patch<T>(endpoint: string, data: any, options?: Omit<RequestOptions, 'responseType'>): Observable<T> {
    const url = `${this.apiUrl}${endpoint}`;
    this.log.verbose(`PATCH request to <${url}>`);

    return this.http.patch<T>(url, data, { ...options, responseType: 'json' })
      .pipe(catchError(this.handleError));
  }

  put<T>(endpoint: string, data: any, options?: Omit<RequestOptions, 'responseType'>): Observable<T> {
    const url = `${this.apiUrl}${endpoint}`;
    this.log.verbose(`PUT request to <${url}>`);

    return this.http.put<T>(url, data, { ...options, responseType: 'json' })
      .pipe(catchError(this.handleError));
  }

  delete<T>(endpoint: string, options?: Omit<RequestOptions, 'responseType'>): Observable<T> {
    const url = `${this.apiUrl}${endpoint}`;
    this.log.verbose(`DELETE request to <${url}>`);

    return this.http.delete<T>(url, { ...options, responseType: 'json' })
      .pipe(catchError(this.handleError));
  }

  // Legacy API methods
  $message(): Observable<ApiResponse> {
    return this.get<ApiResponse>('');
  }

  getTest(): Observable<ApiResponse> {
    return this.get<ApiResponse>('/test');
  }

  getApiUrl(): string {
    this.log.verbose(`ApiService.apiUrl - providing: `, this.apiUrl);
    return this.apiUrl;
  }

  validateKey(key: string): Observable<ApiResponse> {
    return this.post<ApiResponse>('/key/validate', { Key: key });
  }

  getHealth(): Observable<{ success: boolean; time: string }> {
    return this.get<{ success: boolean; time: string }>('/health');
  }
}