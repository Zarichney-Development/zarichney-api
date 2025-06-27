# Module/Directory: Code/Zarichney.Website

**Last Updated:** 2025-06-27

> **Parent:** [`/`](../../README.md)

## 1. Purpose & Responsibility

* **What it is:** The Angular-based frontend application for the Zarichney platform, providing a modern, responsive web interface for recipe management, user authentication, payment processing, and administrative functionality.
* **Key Responsibilities:**
    * User interface for recipe browsing, ordering, and cookbook management
    * Authentication workflows (login, registration, password management)
    * Payment integration via Stripe for order processing
    * Administrative interface for API key management and user roles
    * Voice recording capabilities for recipe generation
    * Real-time order status updates via NgRx state management
    * Server-side rendering (SSR) for improved SEO and performance
* **Why it exists:** To provide a seamless, modern web experience that interfaces with the Zarichney.Server backend API, enabling users to interact with the recipe and ordering system through an intuitive Angular application.
* **Submodules:**
    * **Components:** [`./src/app/components`](./src/app/components/README.md) - Core UI components (header, footer, menu, helper components)
    * **Routes:** [`./src/app/routes`](./src/app/routes/README.md) - Feature-based routing modules (auth, admin, home, recorder)
    * **Services:** [`./src/app/services`](./src/app/services/README.md) - Business logic and API communication services
    * **Models:** [`./src/app/models`](./src/app/models/README.md) - TypeScript interfaces and data models

## 2. Architecture & Key Concepts

* **High-Level Design:** Modern Angular 19 application with SSR capabilities, utilizing NgRx for state management, Angular Material for UI components, and a feature-based modular architecture. The application communicates with the Zarichney.Server backend via HTTP APIs and implements role-based authorization.
* **Core Logic Flow:**
    1. **Authentication Flow:** User accesses protected routes → Auth Guard checks token → Auth Service validates with backend → User granted/denied access
    2. **Recipe Ordering:** User browses recipes → Adds to cart → Payment Service processes via Stripe → Order submitted to backend
    3. **Admin Operations:** Admin users access admin routes → Role guards verify permissions → Admin services interact with backend APIs
    4. **Voice Recording:** User activates recorder → Audio captured and processed → Sent to AI service for transcription and recipe generation
* **Key Data Structures:**
    * `AuthUser`: User authentication and profile information
    * `OrderState`: NgRx state for order management and tracking
    * `Recipe`: Recipe data structure for cookbook functionality
    * `ApiKeyResponse`: API key management for administrative features
* **State Management:** 
    * **NgRx Store:** Centralized state management for authentication and order processing
    * **Services:** Singleton services for API communication and business logic
    * **Local Storage:** Token persistence and user session management
* **Diagram:**
    ```mermaid
    graph TD
        A[Angular App] --> B[Auth Guard]
        A --> C[Route Components]
        C --> D[Services Layer]
        D --> E[NgRx Store]
        D --> F[Backend API]
        
        B --> G[Auth Service]
        G --> F
        
        C --> H[Components]
        H --> I[Angular Material UI]
        
        E --> J[Auth State]
        E --> K[Order State]
        
        F --> L[Zarichney.Server]
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `AuthService.login(credentials)`:
        * **Purpose:** Authenticate user and establish session
        * **Critical Preconditions:** Valid email/password credentials provided
        * **Critical Postconditions:** User token stored, auth state updated, navigation to protected area
        * **Non-Obvious Error Handling:** Handles 401 unauthorized, email confirmation requirements, account lockout scenarios
    * `PaymentService.processPayment(orderData)`:
        * **Purpose:** Handle Stripe payment processing for orders
        * **Critical Preconditions:** Valid order data, Stripe integration configured, user authenticated
        * **Critical Postconditions:** Payment processed, order submitted to backend, user redirected to confirmation
        * **Non-Obvious Error Handling:** Handles Stripe errors, payment failures, network timeouts
    * `ApiService.makeRequest(endpoint, data)`:
        * **Purpose:** Centralized HTTP communication with backend
        * **Critical Preconditions:** Valid endpoint, authentication token (for protected routes)
        * **Critical Postconditions:** Response data returned, errors handled globally
        * **Non-Obvious Error Handling:** Automatic token refresh, error interceptor handling, network failure recovery
* **Critical Assumptions:**
    * **Backend API:** Assumes Zarichney.Server is running and accessible at configured endpoint
    * **Authentication:** Relies on JWT token-based authentication with automatic refresh
    * **External Services:** Assumes Stripe payment integration is properly configured with valid keys
    * **Browser Compatibility:** Designed for modern browsers with ES6+ support and local storage
    * **SSR Environment:** Assumes Node.js server environment for server-side rendering capabilities

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:**
    * **Environment Files:** `src/startup/environments.ts` (production), `src/startup/environments.dev.ts` (development)
    * **Critical Keys:** API base URL, Stripe publishable key, application name and version
    * **Build Configuration:** `angular.json` defines build targets, SSR setup, and asset management
* **Directory Structure:**
    * **Feature-Based:** Routes organized by feature (auth, admin, home, etc.)
    * **Shared Components:** Common UI components in `/components` directory
    * **Services:** Business logic services in `/services` directory with singleton pattern
    * **Assets:** Static files (images, documents) in `/src/assets`
    * **Styles:** SCSS files organized by responsive breakpoints and mixins
* **Technology Choices:**
    * **Angular 19:** Latest Angular version with SSR capabilities
    * **NgRx:** State management for complex application state
    * **Angular Material:** UI component library for consistent design
    * **Stripe.js:** Payment processing integration
    * **Axios:** HTTP client for API communication (alongside Angular HTTP)
* **Performance/Resource Notes:**
    * **SSR:** Server-side rendering for improved initial load and SEO
    * **Lazy Loading:** Feature modules loaded on-demand via routing
    * **Tree Shaking:** Build process eliminates unused code
    * **Hot Module Replacement:** Development mode supports HMR for faster development

## 5. How to Work With This Code

* **Setup:**
    1. Install Node.js (v18+ recommended)
    2. Install dependencies: `npm install`
    3. Configure environment variables in `src/startup/environments.dev.ts`
    4. Ensure backend API is running for full functionality
* **Development Commands:**
    * **Development Server:** `npm start` (with HMR enabled)
    * **Production Build:** `npm run build-prod`
    * **Development Build:** `npm run build-dev`
    * **Serve SSR:** `npm run serve:ssr` (requires build first)
    * **Clean Build:** `npm run clean-dist`
* **Testing:**
    * **Test Locations:** 
        * Unit/Integration tests: `src/**/*.spec.ts` (co-located with source files)
        * Additional test suite: [`../Zarichney.Website.Tests/`](../Zarichney.Website.Tests/README.md)
        * E2E tests: `e2e/**/*.spec.ts`
    * **How to Run:** 
        * Unit/Integration tests: `npm test` (watch mode: `npm run test:watch`)
        * Test coverage: `npm run test:coverage`
        * E2E tests: `npm run test:e2e` (requires application running)
    * **Testing Strategy:**
        * **Unit Tests:** Components and services tested in isolation with mocked dependencies using Jest
        * **Integration Tests:** Component interactions and NgRx store operations
        * **E2E Tests:** Full user workflows tested with Playwright across multiple browsers
    * **Testing Framework:** Jest with jest-preset-angular for unit/integration, Playwright for E2E testing
    * **Coverage Goals:** Aiming for 80%+ line coverage, 70%+ branch coverage
* **Common Pitfalls / Gotchas:**
    * **SSR Compatibility:** Ensure code works in both browser and Node.js environments
    * **State Management:** Be cautious with NgRx store mutations and side effects
    * **Authentication:** Handle token expiration and refresh scenarios properly
    * **Payment Processing:** Test Stripe integration thoroughly in sandbox mode
    * **Build Process:** Clean dist directory when switching between development and production builds

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`../Zarichney.Server`](../Zarichney.Server/README.md) - Backend API for all data operations and business logic
* **External Library Dependencies:**
    * **Angular 19:** Core framework and platform
    * **NgRx:** State management (@ngrx/store, @ngrx/effects)
    * **Angular Material:** UI component library
    * **Stripe.js:** Payment processing integration
    * **Axios:** HTTP client for API communication
    * **Express:** Server framework for SSR
    * **RxJS:** Reactive programming utilities
* **Dependents (Impact of Changes):**
    * **End Users:** Direct impact on user experience and functionality
    * **SEO/Marketing:** SSR changes affect search engine optimization
    * **Backend Integration:** API contract changes require coordination with server team

## 7. Rationale & Key Historical Context

* **Angular Framework Choice:** Selected for its enterprise-grade features, TypeScript support, and comprehensive ecosystem
* **SSR Implementation:** Added to improve SEO performance and initial page load times for better user experience
* **NgRx State Management:** Implemented for complex state scenarios, particularly authentication and order management
* **Feature-Based Architecture:** Organized by business features rather than technical layers for better maintainability
* **Material Design:** Chosen for consistent, accessible UI components and design system

## 8. Known Issues & TODOs

* **Testing Framework:** Jest and Playwright testing infrastructure is now in place; additional test coverage needed for existing components and services
* **Performance Optimization:** Bundle size optimization and lazy loading improvements
* **Accessibility:** Full WCAG compliance audit and implementation
* **Error Handling:** Enhanced error boundary implementation for better user experience
* **Documentation:** API integration documentation and component usage guides

---