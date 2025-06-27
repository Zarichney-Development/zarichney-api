# Module/Directory: Code/Zarichney.Website.Tests

**Last Updated:** 2025-06-27

> **Parent:** [`/`](../../README.md)

## 1. Purpose & Responsibility

* **What it is:** Comprehensive testing suite for the Angular frontend application (`Code/Zarichney.Website`), providing unit tests, integration tests, and end-to-end tests to ensure code quality, functionality, and user experience.
* **Key Responsibilities:**
    * Unit testing of Angular components, services, directives, and utilities
    * Integration testing of component interactions and NgRx store operations
    * End-to-end testing of complete user workflows and application behavior
    * Test coverage reporting and quality metrics
    * Testing infrastructure and configuration management
    * Mock data and test fixture management
* **Why it exists:** To establish a robust testing foundation that ensures the frontend application works correctly, maintains high code quality, and prevents regressions during development and deployment.

## 2. Architecture & Key Concepts

* **High-Level Design:** Multi-layered testing strategy using Jest for unit/integration tests and Playwright for end-to-end tests. Tests are organized by type and mirror the application structure for easy navigation and maintenance.
* **Core Testing Philosophy:**
    1. **Unit Tests:** Test components and services in isolation with mocked dependencies
    2. **Integration Tests:** Test component interactions, NgRx store operations, and service integrations
    3. **End-to-End Tests:** Test complete user workflows through the deployed application
* **Key Data Structures:**
    * Jest test configurations and setup files
    * Playwright browser automation scripts
    * Mock data factories and test fixtures
    * Test utilities and helper functions
* **Test Organization:**
    * **Unit:** Component and service isolation tests
    * **Integration:** Multi-component and store interaction tests
    * **E2E:** Full application workflow tests
    * **Support:** Shared utilities, fixtures, and helpers
* **Diagram:**
    ```mermaid
    graph TD
        A[Frontend Tests] --> B[Unit Tests]
        A --> C[Integration Tests]
        A --> D[E2E Tests]
        
        B --> E[Components]
        B --> F[Services]
        B --> G[Directives]
        B --> H[Utils]
        
        C --> I[Component + Template]
        C --> J[NgRx Store]
        C --> K[Service + HTTP]
        
        D --> L[User Workflows]
        D --> M[Navigation]
        D --> N[Authentication]
        
        O[Support] --> P[Fixtures]
        O --> Q[Helpers]
        O --> R[Config]
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * **npm test**: Run Jest unit and integration tests
        * **Purpose:** Execute all unit and integration tests for the frontend application
        * **Critical Preconditions:** Jest configuration setup, Angular testing utilities available
        * **Critical Postconditions:** Test results reported, coverage generated if requested
        * **Non-Obvious Error Handling:** Graceful failure reporting, detailed error messages for debugging
    * **npm run test:e2e**: Run Playwright end-to-end tests
        * **Purpose:** Execute full application workflow tests against running application
        * **Critical Preconditions:** Application running on localhost:4200, Playwright browsers installed
        * **Critical Postconditions:** User workflows validated, UI interactions tested
        * **Non-Obvious Error Handling:** Screenshots on failure, video recording for debugging
    * **Test Coverage Reporting**: Generate and view test coverage metrics
        * **Purpose:** Monitor test coverage and identify untested code areas
        * **Critical Preconditions:** Jest coverage configuration, source code mapping
        * **Critical Postconditions:** HTML and text coverage reports generated
        * **Non-Obvious Error Handling:** Coverage threshold enforcement, missing coverage highlighting
* **Critical Assumptions:**
    * **Angular Framework:** Tests assume Angular 19+ with standalone components and NgRx state management
    * **Browser Compatibility:** E2E tests target modern browsers (Chrome, Firefox, Safari/WebKit)
    * **Development Environment:** Assumes Node.js 18+ and npm for dependency management
    * **Application Architecture:** Tests are designed for the specific frontend application structure and routing

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Test Organization:**
    * **File Naming:** Component tests use `.spec.ts`, service tests use `.spec.ts`, E2E tests use `.spec.ts`
    * **Directory Structure:** Mirror application structure under respective test type directories
    * **Test Grouping:** Related tests grouped in `describe` blocks with clear hierarchical organization
* **Testing Standards:**
    * **Unit Tests:** Complete isolation with mocked dependencies using Jest mocks
    * **Integration Tests:** Real Angular testing utilities with minimal mocking
    * **E2E Tests:** Full browser automation targeting user workflows and critical paths
    * **Coverage Goals:** Aim for 80%+ line coverage, 70%+ branch coverage
* **Mock Strategy:**
    * **Services:** Mock external API calls and dependencies
    * **Components:** Mock child components that are not under test
    * **Store:** Use NgRx testing utilities for state management testing
    * **Browser APIs:** Mock browser-specific APIs for consistent test environment
* **Test Data:**
    * **Fixtures:** Reusable test data in support/fixtures directory
    * **Builders:** Data builder pattern for complex object creation
    * **Factories:** Factory functions for generating test instances

## 5. How to Work With This Code

* **Setup:**
    1. Ensure Node.js 18+ is installed
    2. Install dependencies: `npm install` in Code/Zarichney.Website
    3. Install Playwright browsers: `npx playwright install`
    4. For E2E tests, ensure frontend application is running: `npm start`
* **Running Tests:**
    * **Unit/Integration Tests:** `npm test` (watch mode: `npm run test:watch`)
    * **Coverage Report:** `npm run test:coverage`
    * **E2E Tests:** `npm run test:e2e`
    * **Specific Tests:** `npm test -- --testNamePattern="ComponentName"`
* **Writing Tests:**
    * **Unit Tests:** Create `.spec.ts` files alongside source code or in unit/ directory
    * **Integration Tests:** Create in integration/ directory for cross-component tests
    * **E2E Tests:** Create in e2e/ directory for user workflow tests
    * **Test Structure:** Follow AAA pattern (Arrange, Act, Assert)
* **Common Pitfalls / Gotchas:**
    * **Async Testing:** Always await async operations and use proper Jest async patterns
    * **Component Testing:** Ensure proper Angular testing module setup and component lifecycle handling
    * **Store Testing:** Use NgRx testing utilities for proper state management testing
    * **E2E Timing:** Use proper wait strategies in Playwright to handle dynamic content

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`../Zarichney.Website`](../Zarichney.Website/README.md) - Angular frontend application under test
* **External Library Dependencies:**
    * **Jest:** JavaScript testing framework for unit and integration tests
    * **Jest Preset Angular:** Angular-specific Jest configuration and utilities
    * **Playwright:** Browser automation framework for end-to-end testing
    * **Angular Testing Utilities:** Angular framework testing support (@angular/core/testing)
    * **NgRx Testing:** State management testing utilities (@ngrx/store/testing)
* **Dependents (Impact of Changes):**
    * **CI/CD Pipeline:** Test results affect build and deployment processes
    * **Development Workflow:** Tests provide quality gates for code changes
    * **Code Coverage:** Test coverage metrics influence code quality assessments

## 7. Rationale & Key Historical Context

* **Testing Strategy Choice:** Multi-layered approach chosen to provide comprehensive coverage from unit-level isolation to full user workflow validation
* **Jest Framework:** Selected for its excellent Angular integration, powerful mocking capabilities, and comprehensive testing features
* **Playwright Selection:** Chosen for E2E testing due to cross-browser support, modern browser automation features, and alignment with backend Playwright usage
* **Test Organization:** Structured to parallel application architecture for intuitive navigation and maintenance
* **Coverage Goals:** Set to balance thorough testing with practical development velocity

## 8. Known Issues & TODOs

* **NgRx Integration:** Complete NgRx store testing utilities integration for complex state scenarios
* **Visual Testing:** Consider adding visual regression testing for UI component stability
* **Performance Testing:** Add performance testing capabilities for critical user workflows
* **Accessibility Testing:** Integrate accessibility testing tools and automated a11y checks
* **Test Data Management:** Enhance test data builders and factories for complex scenarios
* **Parallel Testing:** Optimize test execution for faster feedback in CI/CD pipeline

---