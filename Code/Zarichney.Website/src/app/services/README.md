# Module/Directory: /src/app/services

**Last Updated:** 2025-06-27

> **Parent:** [`Code/Zarichney.Website`](../../../README.md)

## 1. Purpose & Responsibility

* **What it is:** Business logic and data access layer containing Angular services that handle API communication, authentication, payment processing, logging, and other core application functionality. These services provide a clean abstraction between components and external systems.
* **Key Responsibilities:**
    * Authentication and authorization management with JWT token handling
    * HTTP API communication with the Zarichney.Server backend
    * Payment processing integration via Stripe API
    * Application logging and error tracking
    * Responsive design utilities and breakpoint detection
    * SEO and meta tag management for SSR
    * AI service integration for voice processing and recipe generation
* **Why it exists:** To centralize business logic, provide consistent API interfaces, handle cross-cutting concerns like authentication and logging, and maintain separation of concerns between presentation and data layers.

## 2. Architecture & Key Concepts

* **High-Level Design:** Singleton services following Angular's dependency injection pattern, with each service focused on a specific domain responsibility. Services communicate with external APIs, manage application state, and provide reactive interfaces using RxJS observables.
* **Core Logic Flow:**
    1. **Service Injection:** Angular DI provides service instances to components and other services
    2. **API Communication:** Services make HTTP requests to backend APIs with authentication
    3. **Error Handling:** Centralized error handling with user-friendly error messages
    4. **State Management:** Services maintain reactive state using BehaviorSubjects and observables
    5. **Authentication Flow:** Token-based authentication with automatic refresh and logout
* **Key Data Structures:**
    * `AuthUser`: User authentication state and profile information
    * `ApiResponse`: Standardized API response format with error handling
    * `PaymentIntent`: Stripe payment processing data structures
    * `LogEntry`: Structured logging format for application events
    * `BreakpointState`: Responsive design breakpoint information
* **State Management:**
    * **Service State:** Each service maintains its own reactive state using RxJS
    * **Token Storage:** Secure token storage with automatic expiration handling
    * **Error State:** Global error state management with user notifications
* **Diagram:**
    ```mermaid
    graph TD
        A[Components] --> B[Services Layer]
        
        B --> C[Auth Service]
        B --> D[API Service]
        B --> E[Payment Service]
        B --> F[Log Service]
        B --> G[Responsive Service]
        B --> H[SEO Service]
        B --> I[AI Service]
        
        C --> J[Token Storage]
        D --> K[HTTP Client]
        E --> L[Stripe SDK]
        F --> M[Console/Remote]
        G --> N[Media Queries]
        H --> O[Meta Tags]
        I --> P[Backend AI API]
        
        K --> Q[Zarichney.Server]
        L --> R[Stripe API]
        P --> Q
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `AuthService.login(credentials)`:
        * **Purpose:** Authenticate user and establish secure session
        * **Critical Preconditions:** Valid email/password credentials, network connectivity
        * **Critical Postconditions:** User authenticated, tokens stored, auth state updated
        * **Non-Obvious Error Handling:** Handles account lockout, email verification requirements, invalid credentials
    * `ApiService.request<T>(endpoint, options)`:
        * **Purpose:** Generic HTTP request wrapper with authentication and error handling
        * **Critical Preconditions:** Valid endpoint URL, auth token for protected routes
        * **Critical Postconditions:** Typed response data returned, errors handled globally
        * **Non-Obvious Error Handling:** Automatic token refresh, network failure retry, global error interception
    * `PaymentService.createPaymentIntent(amount, metadata)`:
        * **Purpose:** Initialize Stripe payment process for order transactions
        * **Critical Preconditions:** Valid amount, Stripe configuration loaded, user authenticated
        * **Critical Postconditions:** Payment intent created, client secret returned
        * **Non-Obvious Error Handling:** Handles Stripe API errors, currency validation, payment method failures
    * `ResponsiveService.isBreakpoint(breakpoint)`:
        * **Purpose:** Reactive breakpoint detection for responsive design
        * **Critical Preconditions:** Browser environment with media query support
        * **Critical Postconditions:** Observable boolean stream for breakpoint state
        * **Non-Obvious Error Handling:** Graceful degradation in SSR environment
* **Critical Assumptions:**
    * **Backend API:** Assumes Zarichney.Server is accessible and follows expected API contracts
    * **Authentication:** JWT token-based authentication with refresh token support
    * **Payment Processing:** Stripe integration configured with valid publishable keys
    * **Browser Environment:** Modern browser with local storage, media queries, and Fetch API
    * **Network Connectivity:** Services assume network availability with graceful offline handling

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Service Patterns:**
    * **Singleton Pattern:** All services provided at root level for application-wide availability
    * **Observable Interfaces:** Services expose reactive interfaces using RxJS observables
    * **Error Handling:** Consistent error handling with user-friendly messages and logging
    * **Type Safety:** Full TypeScript typing for API requests and responses
* **Authentication Strategy:**
    * **Token Management:** Secure storage with automatic refresh and expiration handling
    * **Route Protection:** Integration with route guards for access control
    * **Session Persistence:** User session maintained across browser restarts
* **API Communication:**
    * **Base URL Configuration:** Environment-based API endpoint configuration
    * **Request Interceptors:** Automatic authentication header injection
    * **Response Transformation:** Consistent response format handling
    * **Error Interception:** Global error handling with user notifications
* **Performance Considerations:**
    * **Request Caching:** Strategic caching for frequently accessed data
    * **Debouncing:** Input debouncing for search and validation requests
    * **Lazy Initialization:** Services initialize resources only when needed

## 5. How to Work With This Code

* **Setup:**
    * Services are automatically provided through Angular DI at application root
    * Configure environment variables for API endpoints and external service keys
    * Ensure HTTP client and interceptors are properly configured
* **Development Guidelines:**
    * **Service Creation:** Use Angular CLI: `ng generate service services/service-name`
    * **Error Handling:** Implement comprehensive error handling with user-friendly messages
    * **Testing:** Create corresponding `.spec.ts` files with proper mocking of dependencies
    * **Documentation:** Document service APIs with TSDoc comments for better IDE support
* **Testing:**
    * **Location:** `.spec.ts` files co-located with service files
    * **Testing Strategy:** Unit tests with mocked HTTP requests, error scenarios, and state management
    * **Mock Services:** Create mock implementations for component testing
* **Common Pitfalls / Gotchas:**
    * **Memory Leaks:** Properly unsubscribe from observables to prevent memory leaks
    * **Error Propagation:** Handle errors appropriately without breaking user experience
    * **Token Expiration:** Handle token refresh scenarios gracefully
    * **SSR Compatibility:** Ensure services work in both browser and server environments
    * **Race Conditions:** Handle concurrent requests and state updates properly

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`../models`](../models/README.md) - TypeScript interfaces for data structures and API contracts
    * [`../../startup`](../../startup/) - Environment configuration and application settings
* **External Library Dependencies:**
    * **Angular HTTP Client:** HTTP request handling and interceptors
    * **RxJS:** Reactive programming utilities and observable patterns
    * **Stripe.js:** Payment processing integration
    * **Angular Router:** Navigation and route state integration
    * **Angular CDK:** Breakpoint observer and layout utilities
* **Dependents (Impact of Changes):**
    * [`../routes`](../routes/README.md) - Route components depend on services for data and authentication
    * [`../components`](../components/README.md) - UI components consume services for business logic
    * **Application Security:** Authentication service changes affect entire security model

## 7. Rationale & Key Historical Context

* **Service-Oriented Architecture:** Services encapsulate business logic to promote reusability and testability
* **Reactive Programming:** RxJS observables provide consistent async data handling patterns
* **Token-Based Authentication:** JWT tokens chosen for stateless authentication with refresh capabilities
* **Stripe Integration:** Selected for robust payment processing with strong security and compliance
* **Responsive Service:** Custom implementation provides more control than CSS-only responsive design
* **Centralized Logging:** Structured logging enables better debugging and application monitoring

## 8. Known Issues & TODOs

* **Error Handling Enhancement:** More sophisticated error recovery and retry mechanisms
* **Caching Strategy:** Implement more comprehensive caching for improved performance
* **Offline Support:** Add service worker integration for offline functionality
* **Testing Coverage:** Comprehensive unit tests for all service methods and error scenarios
* **Documentation:** API documentation generation for service interfaces
* **Performance Monitoring:** Integration with application performance monitoring tools

---