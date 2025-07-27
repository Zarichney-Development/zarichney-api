# OAuth Auto-refresh Test

This commit tests the automatic OAuth token refresh functionality implemented in the tech debt analysis workflow.

The workflow now uses grll/claude-code-action-with-oauth@v1 which automatically refreshes expired tokens using the provided refresh token and GitHub PAT.

Expected behavior:
- Tech debt analysis should complete successfully
- No 401 authentication errors
- Token should refresh automatically if needed
