# Module/Directory: /src

**Last Updated:** 2025-06-27

> **Parent:** [`Code/Zarichney.Website`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** The source code directory containing all application code, assets, and configuration files for the Angular frontend application. This directory represents the complete source structure for both client-side and server-side rendering implementations.
* **Key Responsibilities:**
    * Application source code organization and structure
    * Static asset management (images, documents, meta files)
    * Application startup and bootstrapping configuration
    * Development and production environment setup
    * Server-side rendering implementation
    * Build process entry points and configuration
* **Why it exists:** To provide a clear, organized structure for all application source code, assets, and configuration while supporting both development workflows and production deployment requirements.
* **Submodules:**
    * **App:** [`./app`](./app/README.md) - Core Angular application logic, components, and features
    * **Assets:** `./assets/` - Static files including images, documents, and meta files
    * **Startup:** `./startup/` - Application bootstrap configuration and environment setup

## 2. Architecture & Key Concepts

* **High-Level Design:** Standard Angular application structure with separation between application logic (`app/`), static assets (`assets/`), and startup configuration (`startup/`). The architecture supports both client-side and server-side rendering with appropriate entry points for each environment.
* **Core Logic Flow:**
    1. **Application Bootstrap:** Startup files initialize Angular application for client or server
    2. **Asset Loading:** Static assets loaded and served appropriately for each environment
    3. **Environment Configuration:** Environment-specific settings applied during bootstrap
    4. **Module Loading:** Angular modules loaded based on client or server requirements
* **Key Data Structures:**
    * Angular application modules for client and server environments
    * Static asset files and metadata for SEO and PWA features
    * Environment configuration objects for different deployment targets
    * HTML templates and server configuration for SSR
* **State Management:**
    * **Application State:** Managed within Angular application modules
    * **Asset State:** Static files served by build process and server
    * **Environment State:** Configuration loaded at startup based on target environment
* **Diagram:**
    ```mermaid
    graph TD
        A[src Directory] --> B[app/]
        A --> C[assets/]
        A --> D[startup/]
        
        B --> E[Angular Application]
        B --> F[Components & Services]
        B --> G[Feature Modules]
        
        C --> H[Static Images]
        C --> I[Documents]
        C --> J[SEO Files]
        
        D --> K[Client Bootstrap]
        D --> L[Server Bootstrap]
        D --> M[Environment Config]
        D --> N[HTML Template]
        
        O[Build Process] --> A
        P[Development Server] --> A
        Q[Production Server] --> A
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * Client Application Entry (`startup/app.client.ts`):
        * **Purpose:** Bootstrap Angular application in browser environment
        * **Critical Preconditions:** DOM available, browser environment, static assets loaded
        * **Critical Postconditions:** Angular application rendered and interactive
        * **Non-Obvious Error Handling:** Handles missing polyfills, browser compatibility issues
    * Server Application Entry (`startup/app.server.ts`):
        * **Purpose:** Bootstrap Angular application for server-side rendering
        * **Critical Preconditions:** Node.js environment, Express server context
        * **Critical Postconditions:** HTML rendered on server, hydration-ready markup
        * **Non-Obvious Error Handling:** Handles SSR-specific errors, missing browser APIs
    * Static Assets:
        * **Purpose:** Provide static files for application functionality and SEO
        * **Critical Preconditions:** Build process configured, asset optimization enabled
        * **Critical Postconditions:** Assets served with appropriate caching and optimization
        * **Non-Obvious Error Handling:** Fallback assets for missing files, compression handling
* **Critical Assumptions:**
    * **Build System:** Assumes Angular CLI or compatible build system for processing
    * **Environment Support:** Supports both browser and Node.js server environments
    * **Asset Pipeline:** Assumes proper asset optimization and serving infrastructure
    * **Modern Browser:** Targets modern browsers with ES6+ support

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Directory Structure:**
    * **Clear Separation:** Distinct separation between application logic, assets, and startup
    * **Build Targets:** Structure supports multiple build targets (development, production, SSR)
    * **Asset Organization:** Static assets organized by type and purpose
* **Environment Configuration:**
    * **Environment Files:** Separate configuration for development and production
    * **Build-Time Substitution:** Environment variables substituted during build process
    * **Runtime Configuration:** Some configuration loaded at runtime for flexibility
* **SSR Implementation:**
    * **Universal Compatibility:** Code written to work in both browser and server environments
    * **Hydration Support:** Server-rendered content designed for client hydration
    * **Performance Optimization:** SSR optimized for initial page load performance
* **Asset Management:**
    * **Optimization:** Images and assets optimized for web delivery
    * **Caching Strategy:** Assets configured for appropriate browser caching
    * **SEO Files:** Meta files (robots.txt, sitemap.xml) for search engine optimization

## 5. How to Work With This Code

* **Setup:**
    * Node.js and npm required for development and build processes
    * Angular CLI provides development server and build tools
    * Environment configuration may need customization for local development
* **Development Guidelines:**
    * **Source Organization:** Keep application logic in `app/`, assets in `assets/`, config in `startup/`
    * **Environment Management:** Use environment files for configuration management
    * **Asset Optimization:** Optimize images and static assets for web delivery
    * **SSR Compatibility:** Ensure code works in both browser and server environments
* **Testing:**
    * **Build Testing:** Test build process for both development and production
    * **SSR Testing:** Verify server-side rendering works correctly
    * **Asset Testing:** Ensure static assets load correctly in all environments
* **Common Pitfalls / Gotchas:**
    * **SSR Compatibility:** Avoid browser-specific APIs in code that runs on server
    * **Asset Paths:** Use relative paths for assets to ensure proper build output
    * **Environment Configuration:** Ensure environment-specific settings are properly applied
    * **Build Performance:** Large assets can impact build time and bundle size

## 6. Dependencies

* **Internal Code Dependencies:**
    * No external internal dependencies - this is the root source directory
* **External Library Dependencies:**
    * **Angular Framework:** Core framework for application functionality
    * **Angular CLI:** Build tools and development server
    * **Node.js:** Server environment for SSR and build processes
    * **TypeScript:** Language and compilation toolchain
* **Dependents (Impact of Changes):**
    * **Build Process:** Angular CLI build depends on this structure
    * **Development Tools:** Development server and tooling depend on organization
    * **Deployment:** Production deployment processes depend on build outputs

## 7. Rationale & Key Historical Context

* **Standard Angular Structure:** Follows Angular CLI conventions for familiarity and tooling support
* **SSR Implementation:** Added server-side rendering for SEO benefits and performance
* **Asset Organization:** Structured for optimal build output and serving performance
* **Environment Separation:** Clear environment configuration for different deployment scenarios
* **Modern Toolchain:** Leverages modern JavaScript/TypeScript tooling for development efficiency

## 8. Known Issues & TODOs

* **Bundle Optimization:** Further optimization of JavaScript and CSS bundles
* **Asset Performance:** Potential for additional image optimization and lazy loading
* **Build Configuration:** Potential customization of build process for specific requirements
* **Environment Management:** Enhanced environment configuration management
* **Development Experience:** Improvements to development server and hot reloading

---