# Module/Directory: /src/app/routes

**Last Updated:** 2025-06-27

> **Parent:** [`Code/Zarichney.Website`](../../../README.md)

## 1. Purpose & Responsibility

* **What it is:** Feature-based routing modules that organize the application into distinct functional areas, each with their own components, guards, and route configurations. This directory contains all route-level components and routing logic for the Angular application.
* **Key Responsibilities:**
    * Application routing configuration and navigation structure
    * Feature-based route organization (authentication, administration, core features)
    * Route guards for authentication and authorization protection
    * Page-level components for different application features
    * Lazy loading configuration for performance optimization
    * Server-side rendering (SSR) route management
* **Why it exists:** To organize the application into logical feature areas, enable code splitting through lazy loading, provide secure route protection, and maintain a clear separation between different application concerns.
* **Submodules:**
    * **Authentication Routes:** User login, registration, password management, and email confirmation
    * **Admin Routes:** Administrative interface for user management, API keys, and system administration
    * **Core Application Routes:** Home page, recorder functionality, and dynamic content
    * **SSR Routes:** Server-side rendering route configuration

## 2. Architecture & Key Concepts

* **High-Level Design:** Feature-based routing architecture using Angular Router with lazy loading, route guards for security, and NgRx integration for state management. Each feature module is self-contained with its own routing configuration and components.
* **Core Logic Flow:**
    1. **Route Resolution:** Angular router matches URL to route configuration
    2. **Guard Execution:** Authentication and authorization guards verify access permissions
    3. **Module Loading:** Lazy loading loads feature modules on-demand
    4. **Component Activation:** Route component is instantiated and rendered
    5. **State Management:** NgRx effects and reducers manage feature-specific state
    6. **SSR Handling:** Server-side rendering processes routes for initial page load
* **Key Data Structures:**
    * `RouteConfig`: Angular route definitions with guards and lazy loading
    * `AuthState`: NgRx state for authentication and user session
    * `OrderState`: NgRx state for order management and tracking
    * `GuardResult`: Route guard return values for access control
* **State Management:**
    * **NgRx Store:** Feature-specific stores for auth and order management
    * **Route State:** Router state integration with application state
    * **Guard State:** Authentication and authorization state management
* **Diagram:**
    ```mermaid
    graph TD
        A[App Routes] --> B[Auth Module]
        A --> C[Admin Module]
        A --> D[Core Routes]
        A --> E[SSR Routes]
        
        B --> F[Auth Guard]
        C --> G[Admin Guard]
        
        F --> H[Auth Service]
        G --> I[Auth Service]
        
        B --> J[Login Component]
        B --> K[Register Component]
        B --> L[Password Reset]
        
        C --> M[Dashboard]
        C --> N[User Management]
        C --> O[API Key Management]
        
        D --> P[Home Component]
        D --> Q[Recorder Component]
        D --> R[Dynamic Component]
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `AuthGuard.canActivate()`:
        * **Purpose:** Protect routes requiring authentication
        * **Critical Preconditions:** Auth service initialized, token validation available
        * **Critical Postconditions:** Route access granted/denied, redirection to login if needed
        * **Non-Obvious Error Handling:** Handles token expiration, network failures, invalid tokens
    * `AdminGuard.canActivate()`:
        * **Purpose:** Protect administrative routes with role-based access
        * **Critical Preconditions:** User authenticated, role information available
        * **Critical Postconditions:** Admin access granted/denied based on user roles
        * **Non-Obvious Error Handling:** Handles role changes, permission updates, unauthorized access
    * `AuthInterceptor.intercept()`:
        * **Purpose:** Automatically attach authentication tokens to HTTP requests
        * **Critical Preconditions:** HTTP request initiated, auth token available
        * **Critical Postconditions:** Request includes authentication headers, token refresh handled
        * **Non-Obvious Error Handling:** Automatic token refresh, logout on persistent failures
* **Critical Assumptions:**
    * **Authentication Backend:** Assumes JWT-based authentication with refresh token support
    * **Role-Based Access:** Assumes backend provides user roles and permissions
    * **Network Connectivity:** Guards and interceptors assume network availability for token validation
    * **State Persistence:** Assumes NgRx store persists authentication state across navigation

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Route Organization:**
    * **Feature Modules:** Each major feature area has its own module and routing configuration
    * **Lazy Loading:** All feature modules use lazy loading for performance optimization
    * **Route Naming:** Routes follow kebab-case naming convention for URLs
    * **Route Parameters:** Consistent parameter naming across related routes
* **Guard Implementation:**
    * **Guard Chaining:** Multiple guards can protect a single route
    * **Return Types:** Guards return boolean, UrlTree, or Observable/Promise of these types
    * **Error Handling:** Guards handle errors gracefully with user-friendly redirects
* **State Integration:**
    * **NgRx Effects:** Route changes trigger appropriate effects for state updates
    * **Route Data:** Static route data provides context for components and guards
    * **Query Parameters:** Consistent handling of query parameters across features
* **SSR Configuration:**
    * **Pre-rendering:** Critical routes are pre-rendered for better performance
    * **Meta Tags:** Each route configures appropriate meta tags for SEO

## 5. How to Work With This Code

* **Setup:**
    * Routes are automatically configured through Angular router module
    * Ensure NgRx store modules are properly imported for each feature
    * Verify authentication service is configured before route guards
* **Development Guidelines:**
    * **New Routes:** Add routes to appropriate feature module or create new feature module
    * **Route Guards:** Implement guards following the existing pattern with proper error handling
    * **Lazy Loading:** Ensure new feature modules are properly configured for lazy loading
    * **State Management:** Connect route components to appropriate NgRx stores
* **Testing:**
    * **Location:** Route component tests in corresponding `.spec.ts` files
    * **Testing Strategy:** Test route resolution, guard behavior, component initialization, and state integration
    * **Mock Dependencies:** Mock route parameters, guards, and services in tests
* **Common Pitfalls / Gotchas:**
    * **Guard Execution Order:** Route guards execute in specific order; ensure dependencies are met
    * **Memory Leaks:** Properly unsubscribe from observables in route components
    * **State Synchronization:** Ensure NgRx state is properly updated on route changes
    * **SSR Compatibility:** Route components must work in both browser and server environments

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`../services`](../services/README.md) - Authentication, API, and business logic services
    * [`../models`](../models/README.md) - TypeScript interfaces for route data and state
    * [`../components`](../components/README.md) - Shared UI components used in route templates
* **External Library Dependencies:**
    * **Angular Router:** Route configuration and navigation
    * **NgRx Store:** State management for route-specific data
    * **Angular Material:** UI components for route templates
    * **RxJS:** Observable patterns for reactive route handling
* **Dependents (Impact of Changes):**
    * **Application Navigation:** Changes affect entire application navigation structure
    * **Security Model:** Guard changes impact application security posture
    * **User Experience:** Route changes affect user workflows and navigation patterns

## 7. Rationale & Key Historical Context

* **Feature-Based Organization:** Routes organized by business features rather than technical layers for better maintainability and team ownership
* **Lazy Loading Strategy:** Implemented to reduce initial bundle size and improve application startup performance
* **Guard Pattern:** Authentication and authorization guards provide centralized security enforcement
* **NgRx Integration:** State management integrated at route level to provide consistent data flow patterns
* **SSR Implementation:** Server-side rendering added to improve SEO and initial page load performance

## 8. Known Issues & TODOs

* **Route Testing:** Comprehensive integration tests needed for route guards and navigation flows
* **Error Handling:** Enhanced error boundary implementation for route-level error handling
* **Performance Optimization:** Further lazy loading opportunities and route preloading strategies
* **Accessibility:** Route focus management and announcement for screen readers
* **Analytics Integration:** Route tracking and user journey analytics implementation

---