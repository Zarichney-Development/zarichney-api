import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, firstValueFrom, BehaviorSubject, interval } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AiService } from '../../services/ai.service';
import { LoggingService } from '../../services/log.service';

enum RecordingState {
    REQUESTING_PERMISSIONS,
    RECORDING,
    UPLOADING,
    ERROR,
    SUCCESS,
    COMPLETE
}

export enum RecordingMode {
    TRANSCRIBE = 'transcribe',
    PROMPT = 'prompt'
}

@Component({
    selector: 'recorder',
    templateUrl: './recorder.component.html',
    styleUrls: ['./recorder.component.scss'],
    standalone: true,
    imports: [CommonModule, FormsModule],
})
export class RecorderComponent implements OnInit {
    private mediaRecorder: MediaRecorder | null = null;
    private mediaStream: MediaStream | null = null;
    private recordedChunks: Blob[] = [];
    private startTime: number = 0;
    private wakeLock: any = null;
    private destroy$ = new Subject<void>();

    currentState = RecordingState.REQUESTING_PERMISSIONS;
    RecordingState = RecordingState; // For template access
    RecordingMode = RecordingMode; // For template access
    errorMessage: string = '';
    currentMode: RecordingMode = RecordingMode.PROMPT;
    responseText: string = '';

    // Track elapsed time in seconds
    private timeElapsed$ = new BehaviorSubject<number>(0);
    formattedTime$ = new BehaviorSubject<string>('00:00:00');

    isBrowser = false;
    closeMessage: string = "";

    private get AUTO_CLOSE(): boolean {
        return this.currentMode === RecordingMode.TRANSCRIBE;
    }

    constructor(
        @Inject(PLATFORM_ID) private platformId: Object,
        private aiService: AiService,
        private router: Router,
        private log: LoggingService
    ) {
        this.isBrowser = isPlatformBrowser(this.platformId);
        this.log.verbose('RecorderComponent.constructor()');

        // Determine mode based on route
        const currentPath = this.router.url;
        this.currentMode = currentPath.includes('transcribe')
            ? RecordingMode.TRANSCRIBE
            : RecordingMode.PROMPT;
    }

    async ngOnInit() {
        if (!this.isBrowser) {
            this.log.verbose('RecorderComponent: SSR environment detected, skipping browser-specific initialization');
            return;
        }

        await this.startRecordingProcess();
    }

    private async startRecordingProcess() {
        this.currentState = RecordingState.REQUESTING_PERMISSIONS;
        this.errorMessage = '';

        try {
            this.log.verbose('RecorderComponent: Requesting microphone permissions');
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            await this.initializeRecording(stream);
            await this.requestWakeLock();
        } catch (error) {
            this.log.error('Failed to start recording process', error);
            this.handleError('Failed to access microphone. Ensure permissions are granted.');
        }
    }

    private async initializeRecording(stream: MediaStream) {
        this.recordedChunks = [];
        this.mediaStream = stream;

        const mimeType = MediaRecorder.isTypeSupported('audio/webm') ? 'audio/webm' : 'audio/ogg';
        this.log.verbose(`RecorderComponent: Initializing recording with mime type: ${mimeType}`);

        this.mediaRecorder = new MediaRecorder(stream, { mimeType });

        this.mediaRecorder.ondataavailable = (event) => {
            if (event.data.size > 0) this.recordedChunks.push(event.data);
        };

        this.mediaRecorder.start();
        this.currentState = RecordingState.RECORDING;
        this.startTime = Date.now();
        this.startTimer();
    }

    private cleanupMediaStream() {
        if (this.mediaStream) {
            this.mediaStream.getTracks().forEach(track => {
                track.stop();
                this.log.verbose('Stopped media track:', track.kind);
            });
            this.mediaStream = null;
        }
    }

    private startTimer() {
        interval(1000)
            .pipe(takeUntil(this.destroy$))
            .subscribe(() => {
                const elapsed = Math.floor((Date.now() - this.startTime) / 1000);
                this.timeElapsed$.next(elapsed);

                const hours = Math.floor(elapsed / 3600);
                const minutes = Math.floor((elapsed % 3600) / 60);
                const seconds = elapsed % 60;

                this.formattedTime$.next(
                    `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds
                        .toString()
                        .padStart(2, '0')}`
                );
            });
    }

    private async requestWakeLock() {
        if ('wakeLock' in navigator) {
            try {
                this.wakeLock = await (navigator as any).wakeLock.request('screen');
                this.log.verbose('RecorderComponent: Wake lock acquired');
            } catch (err) {
                this.log.warn('Wake Lock not available or request failed:', err);
            }
        }
    }

    async stopRecording() {
        if (!this.mediaRecorder) return;

        this.log.verbose('RecorderComponent: Stopping recording');
        this.currentState = RecordingState.UPLOADING;
        this.mediaRecorder.stop();

        await new Promise<void>((resolve) => {
            this.mediaRecorder!.onstop = () => resolve();
        });

        const audioBlob = new Blob(this.recordedChunks, { type: this.mediaRecorder.mimeType });
        this.cleanupMediaStream();
        await this.uploadRecording(audioBlob);
    }

    private async uploadRecording(audioBlob: Blob) {

        const formData = new FormData();
        const fileExtension = this.mediaRecorder?.mimeType.includes('webm') ? 'webm' : 'ogg';
        const file = new File([audioBlob], `recording.${fileExtension}`, {
            type: audioBlob.type
        });
        // Use 'audioFile' for transcribe endpoint and 'audioPrompt' for completion endpoint
        formData.append('audioFile', file);

        try {
            this.log.verbose('RecorderComponent: Uploading recording');

            if (this.currentMode === RecordingMode.TRANSCRIBE) {
                const response = await firstValueFrom(
                    this.aiService.transcribeAudio(formData)
                );
                await this.copyToClipboard(response.transcript);
                this.responseText = response.transcript;
            } else {
                const response = await firstValueFrom(
                    this.aiService.getCompletion(formData)
                );
                this.responseText = response.response;
            }

            if (this.AUTO_CLOSE) {
                this.attemptToClose();
            }
            this.currentState = RecordingState.COMPLETE;

        } catch (error) {
            this.log.error('Upload failed', error);
            if (error instanceof Error) {
                this.handleError(error.message);
            } else {
                this.handleError('Failed to upload recording');
            }
        } finally {
            if (this.wakeLock) {
                await this.wakeLock.release();
                this.wakeLock = null;
            }
        }
    }


    attemptToClose() {
        if (!this.isBrowser) return;

        try {
            // Try multiple approaches
            const approaches = [
                () => window.close(),
                () => window.location.href = 'about:blank',
                () => window.history.go(-window.history.length),
                () => window.location.replace('about:blank')
            ];

            // Try each approach
            for (const approach of approaches) {
                try {
                    approach();
                    // If we get here, the approach didn't throw an error
                    break;
                } catch (e) {
                    this.log.warn('Close approach failed', e);
                    continue;
                }
            }

            // If we're still here after all attempts, show guidance
            setTimeout(() => {
                if (!document.hidden) {
                    this.closeMessage = 'Unable to close';
                }
            }, 100);
        } catch (error) {
            this.closeMessage = 'Unable to close';
        }
    }

    async copyToClipboard(text: string) {
        if (!this.isBrowser) return;
        try {
            await navigator.clipboard.writeText(text);
            this.log.verbose('Text copied to clipboard');
        } catch (error) {
            this.log.error('Failed to copy to clipboard', error);
        }
    }

    async copyResponse() {
        if (this.responseText) {
            await this.copyToClipboard(this.responseText);
        }
    }

    resetRecording() {
        this.currentState = RecordingState.REQUESTING_PERMISSIONS;
        this.errorMessage = '';
        this.recordedChunks = [];
        this.startRecordingProcess();
    }

    private handleError(message: string) {
        this.errorMessage = message;
        this.currentState = RecordingState.ERROR;
        this.log.error(`RecorderComponent Error:`, message);
    }

    ngOnDestroy() {
        this.log.verbose('RecorderComponent: Destroying component');
        this.destroy$.next();
        this.destroy$.complete();
        if (this.wakeLock && typeof this.wakeLock.release === 'function') {
            this.wakeLock.release().catch((err: any) => this.log.error('Failed to release wake lock', err));
        }
    }
}