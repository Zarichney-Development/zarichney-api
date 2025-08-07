# Module/Directory: Services/ProcessExecution

**Last Updated:** 2025-08-06

**Parent:** [`Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Abstracted service for executing system processes and commands with proper error handling, timeout management, and cancellation token support.
* **Key Responsibilities:** 
  - Execute Docker, systemd, and other system commands safely
  - Provide configurable timeout handling for process operations
  - Support proper cancellation token propagation
  - Abstract process execution concerns from business logic services
* **Why it exists:** To separate process execution concerns from business logic, improve testability by allowing process mocking, and provide consistent error handling for system command operations.

## 2. Architecture & Key Concepts

* **High-Level Design:** 
  - **`IProcessExecutor`**: Interface defining process execution contract
  - **`ProcessExecutor`**: Implementation using `System.Diagnostics.Process` with timeout and cancellation support
  - Primary constructor pattern for dependency injection
  - Structured logging for process execution events

* **Key Features:**
  - Configurable timeout handling (default 5000ms)
  - Proper cancellation token propagation
  - Process output capture and return
  - Graceful error handling without throwing exceptions
  - Logging for debugging process execution issues

## 3. Interface Contract & Assumptions

* **Core Service Contract (`IProcessExecutor`):**
  ```csharp
  Task<(int exitCode, string output)> RunCommandAsync(
    string command, 
    string arguments, 
    int timeoutMs = 5000,
    CancellationToken cancellationToken = default);
  ```

* **Key Assumptions:**
  - System commands (docker, systemctl) are available in PATH
  - Process execution permissions are properly configured
  - Commands do not require interactive input
  - Output can be captured via StandardOutput

* **Error Handling:**
  - Process failures return exit code -1 with empty output
  - Timeout scenarios are handled gracefully without throwing exceptions
  - All exceptions are logged for debugging purposes
  - Cancellation token requests are properly propagated

## 4. How to Work With This Code

* **Usage Example:**
  ```csharp
  public class DockerService(IProcessExecutor processExecutor)
  {
    public async Task<bool> IsContainerRunningAsync(string containerName)
    {
      var result = await processExecutor.RunCommandAsync(
        "docker", 
        $"ps --filter name={containerName} --format {{{{.Names}}}}", 
        5000, 
        cancellationToken);
        
      return result.exitCode == 0 && !string.IsNullOrWhiteSpace(result.output);
    }
  }
  ```

* **Testing Strategy:**
  - Mock `IProcessExecutor` in unit tests to avoid actual process execution
  - Test timeout scenarios using cancellation tokens
  - Validate command argument construction and parsing

## 5. Dependencies

* **Internal Dependencies:** None - this is a foundational service

* **Internal Dependents:**
  - [`Services/Logging`](../Logging/README.md) - For Docker and systemd command execution

* **External Libraries:**
  - `System.Diagnostics` - Process execution and management
  - `Microsoft.Extensions.Logging` - Structured logging for debugging

## 6. Rationale & Key Historical Context

* **Extraction Rationale:** Originally, process execution was embedded directly within `LoggingService`, violating Single Responsibility Principle and making testing difficult.

* **Testability Improvement:** By abstracting process execution, dependent services can be unit tested without requiring actual system commands or Docker installation.

* **Consistency:** Provides consistent timeout handling, error management, and logging across all system command operations.