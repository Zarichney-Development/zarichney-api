### Overview

You are creating a frontend interface at `https://zarichney.com/transcribe` using Angular Universal (SSR) that allows the user (a single known individual) to record audio, stop the recording, and send it to a backend for transcription. The main objective is a simple UI to capture audio, authenticate once per device, show recording length, and handle success/error states.

### Key Points

- **Single User, Multiple Devices**: The app is used by one person, potentially on multiple devices.
- **Password Validation**: On first-time use per device, the frontend must prompt for a password and send it to `/api/key/validate`. On success, remember this state locally so subsequent visits start recording immediately.
- **Recording**: Uses `MediaRecorder` API to record audio as `mp3`. Recordings can be long (over an hour).
- **UI Flow**:
  1. **First-Time Access on a New Device**:
     - Prompt user for microphone permission.
     - Prompt user for a password, POST to `/api/key/validate`. If 200 OK, store validation state locally.
  2. **Subsequent Visits**:
     - Immediately request microphone access and start recording if authenticated.
     - Display a timer showing how long the recording has been ongoing.
     - Display a “Stop” button during recording.
  3. **Stopping the Recording**:
     - When “Stop” is pressed, finalize the audio file.
     - Show a spinner (replacing the stop button) while uploading the file to `/api/transcribe` with `x-api-key` set to the password.
     - On success (transcript successfully processed and committed by the backend), silently close the tab.
     - On error, display the error message on screen and do not close the tab.
- **Authentication Handling**:
  - If authentication fails at any point (e.g., invalid stored state), revert to the initial prompt-for-password state.
- **Error Display**:
  - Any client-side or server-side error messages are shown on screen for debugging.
  - The backend will also email errors to the admin, but that’s not your concern on the frontend side. Just display what you receive.
- **SSR Considerations**:
  - The recording logic should only run in the browser. Use Angular’s `isPlatformBrowser()` checks before calling `getUserMedia()` or starting the recorder.
  - On SSR, serve the basic HTML/initial state without engaging recording logic.
- **Screen Awake**:
  - Attempt to keep the screen awake while recording (e.g., use the Wake Lock API if available).
- **Additional Notes**:
  - Silent closure on success. No confirmation dialog.
  - Timer is displayed during recording to show elapsed time.
  - If large file tests fail in the future, chunking can be considered, but for now proceed with a direct file upload.
