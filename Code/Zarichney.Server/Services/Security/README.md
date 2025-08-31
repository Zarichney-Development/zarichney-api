# Module/Directory: Services/Security

**Last Updated:** 2025-08-31

> **Parent:** [`Code/Zarichney.Server/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Security services module providing URL validation and SSRF (Server-Side Request Forgery) prevention for outbound network requests within the application.
* **Key Responsibilities:**
    * Validating outbound URLs against configurable allowlists to prevent SSRF attacks
    * Blocking requests to private IP address ranges and internal network resources
    * Enforcing URL scheme restrictions (HTTP/HTTPS only)
    * Detecting and rejecting URLs containing embedded credentials
    * Providing structured security logging for monitoring and audit purposes
* **Why it exists:** To prevent Server-Side Request Forgery vulnerabilities in the `/api/logging/test-seq` endpoint and provide a reusable security validation service for other outbound URL functionality.

## 2. Architecture & Key Concepts

* **Primary Service Interface:** `IOutboundUrlSecurity` provides URL validation for SSRF prevention
* **Configuration-Driven Security:** `NetworkSecurityOptions` controls allowlist behavior and validation rules
* **Validation Pipeline:** Multi-step validation including scheme check, credential detection, allowlist verification, and DNS resolution validation
* **Structured Logging:** Security events logged with standardized Event IDs for monitoring and alerting

* **Diagram:**
    ```mermaid
    graph TD
        A[Controller] --> B[IOutboundUrlSecurity]
        B --> C[URL Parse & Validate]
        C --> D[Scheme Check]
        D --> E[Credential Check]
        E --> F[Allowlist Check]
        F --> G[DNS Resolution]
        G --> H[Private IP Check]
        H --> I[Validation Result]
        
        J[NetworkSecurityOptions] --> B
        K[Structured Logger] --> B
        
        B --> L[Event ID 5700: Success]
        B --> M[Event ID 5701: Blocked]
        B --> N[Event ID 5702: Redirects]
        B --> O[Event ID 5703: DNS Error]
    ```

## 3. Interface Contract & Assumptions

* **Service Interface:** `IOutboundUrlSecurity`
  - `ValidateSeqUrlAsync(string? url, CancellationToken cancellationToken)` - Validates URL for Seq connectivity
  - `GetEffectiveAllowedHosts()` - Returns current allowlist including defaults

* **Security Validation Checks:**
  - **Scheme Validation:** Only HTTP and HTTPS schemes permitted
  - **Credential Detection:** URLs with user:password@ format rejected
  - **Allowlist Verification:** Host must be in configured allowlist or default localhost entries
  - **DNS Resolution:** External hosts validated to not resolve to private IP ranges
  - **Private IP Blocking:** Blocks 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16, 169.254.0.0/16, ::1, fc00::/7

* **Configuration Requirements:**
  - NetworkSecurity section in appsettings.json with sensible defaults
  - AllowedSeqHosts can be empty (uses only localhost when EnableDefaultLocalhost=true)
  - DNS validation can be disabled for environments without external network access

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Security-First Design:**
    * **Fail-Safe Defaults:** Security validation failures result in request rejection, not warnings
    * **Explicit Allowlisting:** No implicit host approvals - all non-localhost hosts must be explicitly configured
    * **Minimal Information Disclosure:** Error messages provide minimal details to prevent information leakage
    * **Structured Logging:** All security events include sanitized information and standardized reason codes

* **Configuration Security:**
    * **Suffix Matching:** Allowlist entries starting with '.' match subdomains (e.g., '.example.com' matches 'api.example.com')
    * **Case Insensitive:** Host matching is case-insensitive for broader compatibility
    * **IPv6 Support:** Handles IPv6 addresses with bracket notation correctly

* **Performance Considerations:**
    * **DNS Timeout:** Configurable DNS resolution timeout (default 2 seconds)
    * **Localhost Optimization:** Localhost addresses skip DNS resolution for performance
    * **Caching Ready:** Service design supports future caching implementation for DNS results

## 5. How to Work With This Code

* **Setup:**
    * Service automatically registered in DI container via ServiceStartup
    * Configuration bound from NetworkSecurity section in appsettings.json
    * HttpClient injected for potential future redirect validation

* **Development Guidelines:**
    * **Adding New Validation:** Extend ValidateSeqUrlAsync method with new validation steps
    * **Configuration Changes:** Update NetworkSecurityOptions and corresponding appsettings.json defaults
    * **Logging Standards:** Use SecurityEventIds constants for consistent event logging
    * **Testing:** Create unit tests for all validation scenarios including edge cases

* **Testing:**
    * **Location:** Unit tests in `Code/Zarichney.Server.Tests/Unit/Services/Security/`
    * **Integration Tests:** Controller security tests in `Code/Zarichney.Server.Tests/Integration/Controllers/PublicController/`
    * **Mock Strategy:** Mock DNS resolution for deterministic unit tests, real DNS for integration tests

* **Common Pitfalls / Gotchas:**
    * **IPv6 Addresses:** URI.Host returns IPv6 addresses without brackets - handle both formats
    * **DNS Resolution:** Can be slow - use appropriate timeouts and consider async patterns
    * **Allowlist Order:** More specific entries should come before broader suffix matches

## 6. Dependencies

* **Internal Dependencies:**
  - [`Config`](../../Config/README.md) - For NetworkSecurityOptions configuration binding
  - [`Startup/ServiceStartup`](../../Startup/README.md) - For DI registration

* **Internal Dependents:**
  - [`Services/Logging`](../Logging/README.md) - Uses IOutboundUrlSecurity for Seq URL validation
  - [`Controllers/PublicController`](../../Controllers/README.md) - Security validation integrated into test-seq endpoint

* **External Libraries:**
  - `Microsoft.Extensions.Options` - Configuration options pattern
  - `Microsoft.Extensions.Logging` - Structured logging with Event IDs
  - `System.Net` - DNS resolution and IP address validation
  - `System.Net.Http` - HttpClient injection for future redirect validation

## 7. Rationale & Key Historical Context

* **SSRF Prevention (Issue #100):** Implemented in response to automated security findings in PR #97 identifying SSRF vulnerability in `/api/logging/test-seq` endpoint
* **Defense-in-Depth Strategy:** Complements other security measures by providing centralized outbound URL validation
* **Reusable Design:** Service interface designed for extension to other endpoints requiring outbound URL validation
* **Configuration-Driven:** Security policies configurable per environment without code changes

## 8. Known Issues & TODOs

* **IPv6 Edge Cases:** Consider additional IPv6 validation scenarios and link-local address handling
* **Redirect Validation:** Future enhancement to validate redirect chains and prevent protocol downgrades
* **Caching Strategy:** Implement DNS result caching to improve performance for repeated validations
* **Metrics Integration:** Add performance metrics for validation timing and rejection rates
* **Admin Override:** Consider administrative bypass mechanism for debugging purposes with appropriate audit logging

---