import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, TranscriptionResponse, CompletionResponse, ApiResponse } from './api.service';

@Injectable({
  providedIn: 'root',
})
export class AiService {
  constructor(private apiService: ApiService) {}
  
  /**
   * Validates an API key with the backend
   * @param apiKey The API key to validate
   * @returns Observable with validation response
   */
  validateKey(apiKey: string): Observable<ApiResponse> {
    return this.apiService.post<ApiResponse>('/key/validate', { Key: apiKey });
  }

  /**
   * Transcribes audio data to text using the AI service
   * @param audioData FormData containing the audio file
   * @returns Observable with the transcription response
   */
  transcribeAudio(audioData: FormData): Observable<TranscriptionResponse> {
    // Explicitly set withCredentials: true to ensure cookies are sent
    return this.apiService.post<TranscriptionResponse>('/transcribe', audioData, { withCredentials: true });
  }

  /**
   * Gets AI completion for text or audio prompt
   * @param data FormData with audio file or string with text prompt
   * @returns Observable with the completion response
   */
  getCompletion(data: FormData | string): Observable<CompletionResponse> {
    const formData = new FormData();

    if (typeof data === 'string') {
      // Handle text prompt
      formData.append('textPrompt', data);
    } else {
      // Handle audio prompt
      const existingFile = data.get('audioFile') as File;
      if (existingFile) {
        formData.append('audioPrompt', existingFile);
      }
    }

    // Explicitly set withCredentials: true to ensure cookies are sent
    return this.apiService.post<CompletionResponse>('/completion', formData, { withCredentials: true });
  }
}