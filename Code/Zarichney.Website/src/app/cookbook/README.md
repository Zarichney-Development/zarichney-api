# Module/Directory: /src/app/cookbook

**Last Updated:** 2025-06-27

> **Parent:** [`/src/app`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Feature module dedicated to recipe management, cookbook functionality, and order processing within the Zarichney platform. This module handles the core business logic for recipe discovery, ordering workflows, and payment integration.
* **Key Responsibilities:**
    * Recipe browsing and search functionality with carousel display
    * Order creation, management, and status tracking
    * Payment processing integration with Stripe
    * Recipe image handling and optimization
    * Order state management using NgRx patterns
    * Recipe data modeling and service integration
* **Why it exists:** To encapsulate all cookbook-related functionality into a cohesive feature module, providing a complete recipe-to-order workflow while maintaining separation from authentication and administrative concerns.

## 2. Architecture & Key Concepts

* **High-Level Design:** Feature module with NgRx state management for orders, specialized components for recipe display and payment processing, and integration with backend recipe services. The module follows a container-presenter pattern with smart components managing state and dumb components handling presentation.
* **Core Logic Flow:**
    1. **Recipe Discovery:** User browses recipes via carousel and search interfaces
    2. **Order Creation:** Selected recipes are added to order with customization options
    3. **Payment Processing:** Stripe integration handles secure payment transactions
    4. **Order Tracking:** NgRx store manages order state and status updates
    5. **Backend Integration:** Services communicate with Zarichney.Server for recipe data and order processing
* **Key Data Structures:**
    * `Order`: Order model with items, pricing, and status information
    * `OrderState`: NgRx state for order management and tracking
    * `Recipe`: Recipe data structure with images and metadata
    * `PaymentIntent`: Stripe payment processing data
* **State Management:**
    * **NgRx Store:** Order state managed through actions, reducers, and effects
    * **Component State:** Local state for UI interactions and form data
    * **Service Cache:** Recipe data cached at service level for performance
* **Diagram:**
    ```mermaid
    graph TD
        A[Cookbook Module] --> B[Components]
        A --> C[Order Module]
        A --> D[Routes]
        
        B --> E[Recipe Carousel]
        B --> F[Payment Button]
        B --> G[Recipe Image]
        
        C --> H[Order Service]
        C --> I[Order Store]
        C --> J[Order Actions]
        
        I --> K[Order State]
        I --> L[Order Effects]
        
        D --> M[Order Overview]
        D --> N[Cookbook Routes]
        
        H --> O[Backend API]
        F --> P[Stripe Service]
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `OrderService.createOrder(recipes, options)`:
        * **Purpose:** Create new order from selected recipes
        * **Critical Preconditions:** Valid recipe data, user authentication, pricing configuration
        * **Critical Postconditions:** Order created in backend, local state updated, payment intent generated
        * **Non-Obvious Error Handling:** Handles recipe availability, pricing changes, authentication expiration
    * `RecipeCarouselComponent`:
        * **Purpose:** Display recipes in interactive carousel format
        * **Critical Preconditions:** Recipe data available, image assets loaded
        * **Critical Postconditions:** Recipes displayed with navigation, selection events emitted
        * **Non-Obvious Error Handling:** Graceful handling of missing images, loading states
    * `PaymentButtonComponent`:
        * **Purpose:** Handle Stripe payment processing for orders
        * **Critical Preconditions:** Valid order data, Stripe configuration, payment methods available
        * **Critical Postconditions:** Payment processed, order confirmed, user redirected
        * **Non-Obvious Error Handling:** Payment failures, card errors, network timeouts
* **Critical Assumptions:**
    * **Backend Integration:** Assumes Zarichney.Server provides recipe and order APIs
    * **Payment Processing:** Relies on Stripe integration for secure payment handling
    * **Image Assets:** Assumes recipe images are properly sized and optimized
    * **User Authentication:** Requires authenticated user context for order operations

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **State Management:**
    * **NgRx Pattern:** Order module follows strict NgRx patterns with actions, reducers, effects
    * **State Persistence:** Order state persisted across browser sessions
    * **Optimistic Updates:** UI updates optimistically with rollback on failures
* **Component Design:**
    * **Container Pattern:** Smart components handle state, dumb components handle presentation
    * **Event Emitters:** Components communicate via Angular event emitters
    * **Input Validation:** Form inputs validated both client-side and server-side
* **Styling:**
    * **Status Colors:** Order status uses consistent color scheme defined in SCSS variables
    * **Responsive Design:** Components adapt to mobile, tablet, and desktop breakpoints
    * **Material Design:** Integration with Angular Material for consistent UI patterns
* **Performance:**
    * **Image Optimization:** Recipe images lazy loaded and appropriately sized
    * **State Efficiency:** NgRx selectors memoized for performance
    * **Component OnPush:** Change detection optimized where appropriate

## 5. How to Work With This Code

* **Setup:**
    * Ensure NgRx store is configured at application level
    * Configure Stripe integration with appropriate environment keys
    * Verify backend API endpoints are accessible for recipe and order operations
* **Development Guidelines:**
    * **State Changes:** All order modifications go through NgRx actions
    * **Component Creation:** Follow container-presenter pattern for new components
    * **Testing:** Mock NgRx store and external services in component tests
    * **Payment Testing:** Use Stripe test mode for payment integration testing
* **Testing:**
    * **Location:** Component and service tests co-located with source files
    * **Testing Strategy:** Unit tests for components and services, integration tests for order workflows
    * **Mock Strategy:** Mock backend services, Stripe integration, and NgRx store
* **Common Pitfalls / Gotchas:**
    * **State Synchronization:** Ensure backend and frontend order state remain synchronized
    * **Payment Security:** Never expose sensitive payment data in client-side code
    * **Image Loading:** Handle slow or failed image loading gracefully
    * **Memory Management:** Properly dispose of subscriptions and event listeners

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`../services`](../services/README.md) - Payment service, API service for backend communication
    * [`../models`](../models/README.md) - Order and recipe data models
    * [`../components`](../components/README.md) - Shared UI components for layout and navigation
* **External Library Dependencies:**
    * **NgRx:** State management (@ngrx/store, @ngrx/effects)
    * **Stripe.js:** Payment processing integration
    * **Angular Material:** UI components for forms and buttons
    * **RxJS:** Reactive programming for data streams
* **Dependents (Impact of Changes):**
    * [`../routes`](../routes/README.md) - Main application routes depend on cookbook routing
    * **User Experience:** Changes directly impact core user workflows and business functionality

## 7. Rationale & Key Historical Context

* **NgRx Implementation:** Chosen for complex order state management requiring predictable state updates
* **Stripe Integration:** Selected for robust payment processing with strong security compliance
* **Component Architecture:** Container-presenter pattern adopted for better testability and reusability
* **Feature Module Design:** Self-contained module allows for independent development and testing
* **Responsive Design:** Mobile-first approach ensures good experience across all device types

## 8. Known Issues & TODOs

* **Testing Coverage:** Comprehensive test suite needed for order workflows and payment integration
* **Performance Optimization:** Image loading and carousel performance improvements
* **Error Handling:** Enhanced error recovery for payment failures and network issues
* **Accessibility:** Complete keyboard navigation and screen reader support
* **Offline Support:** Consider offline capabilities for recipe browsing

---