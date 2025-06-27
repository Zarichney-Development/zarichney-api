# Module/Directory: /src/app/styles

**Last Updated:** 2025-06-27

> **Parent:** [`/src/app`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Global SCSS stylesheets providing the foundation for the application's visual design system, responsive layout utilities, and cross-component styling patterns. This directory contains the core styling architecture that supports all UI components.
* **Key Responsibilities:**
    * CSS reset and normalization for cross-browser consistency
    * Responsive design system with breakpoint management
    * Global SCSS variables, mixins, and utility functions
    * Consistent color palette and typography definitions
    * Cross-component styling patterns and shared utilities
    * Mobile-first responsive design implementation
* **Why it exists:** To establish a consistent, maintainable design system that provides foundational styles, responsive utilities, and shared patterns across all application components while following modern CSS architecture principles.

## 2. Architecture & Key Concepts

* **High-Level Design:** SCSS-based design system following ITCSS (Inverted Triangle CSS) principles with clear separation between settings, tools, generic styles, and utilities. The architecture supports component-level style encapsulation while providing global foundations.
* **Core Logic Flow:**
    1. **Variable Definition:** Global variables define colors, typography, and spacing
    2. **Mixin Creation:** Reusable mixins provide responsive and styling utilities
    3. **Reset Application:** CSS reset ensures consistent baseline across browsers
    4. **Breakpoint System:** Responsive mixins handle mobile-first design patterns
    5. **Component Integration:** Components inherit global styles and use provided mixins
* **Key Data Structures:**
    * SCSS variables for design tokens (colors, spacing, typography)
    * Responsive breakpoint definitions for mobile-first design
    * Mixin libraries for common styling patterns
    * Utility classes for rapid UI development
* **State Management:**
    * **CSS Custom Properties:** For runtime style modifications
    * **SCSS Variables:** For compile-time style definitions
    * **Class-based State:** State classes for component styling
* **Diagram:**
    ```mermaid
    graph TD
        A[Styles Architecture] --> B[Variables]
        A --> C[Mixins]
        A --> D[Reset]
        A --> E[Utilities]
        
        B --> F[Colors]
        B --> G[Typography]
        B --> H[Spacing]
        B --> I[Breakpoints]
        
        C --> J[Responsive Mixins]
        C --> K[Layout Mixins]
        C --> L[Animation Mixins]
        
        D --> M[CSS Reset]
        D --> N[Normalize]
        
        E --> O[Screen Utilities]
        E --> P[Status Colors]
        
        Q[Components] --> B
        Q --> C
        R[Global Styles] --> A
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * SCSS Variables (`_variables.scss`):
        * **Purpose:** Provide consistent design tokens across the application
        * **Critical Preconditions:** SCSS compilation configured, variables imported before use
        * **Critical Postconditions:** Consistent colors, spacing, and typography throughout app
        * **Non-Obvious Error Handling:** Default values provided for missing variables
    * Responsive Mixins (`_mixins.scss`):
        * **Purpose:** Enable mobile-first responsive design patterns
        * **Critical Preconditions:** Breakpoint variables defined, SCSS mixin support
        * **Critical Postconditions:** Responsive styles applied at appropriate breakpoints
        * **Non-Obvious Error Handling:** Graceful degradation for unsupported breakpoints
    * Screen Utilities (`screens.scss`):
        * **Purpose:** Provide responsive layout utilities and screen-specific styles
        * **Critical Preconditions:** CSS custom properties supported, viewport meta tag configured
        * **Critical Postconditions:** Responsive layouts adapt to screen sizes
        * **Non-Obvious Error Handling:** Fallback styles for older browsers
* **Critical Assumptions:**
    * **SCSS Compilation:** Assumes SCSS preprocessing is configured in build process
    * **Modern Browser Support:** Targets modern browsers with CSS custom properties support
    * **Mobile-First Design:** Assumes mobile-first responsive design approach
    * **Component Encapsulation:** Works with Angular's ViewEncapsulation for component styles

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **SCSS Architecture:**
    * **File Organization:** Separate files for variables, mixins, reset, and utilities
    * **Import Order:** Variables → Mixins → Reset → Base → Utilities
    * **Naming Convention:** BEM methodology for CSS classes, kebab-case for variables
    * **Nesting Limit:** Maximum 3 levels of SCSS nesting for maintainability
* **Responsive Design:**
    * **Mobile-First:** All styles written mobile-first with progressive enhancement
    * **Breakpoints:** Consistent breakpoint system across desktop, tablet, mobile
    * **Flexible Units:** Use of rem, em, and viewport units for scalable design
    * **Touch-Friendly:** Interactive elements sized for touch interaction
* **Design Tokens:**
    * **Color System:** Semantic color names with consistent palette
    * **Typography Scale:** Modular typography scale with consistent line heights
    * **Spacing System:** 8px grid system for consistent spacing
    * **Z-Index Scale:** Managed z-index values to prevent stacking issues
* **Performance:**
    * **Critical CSS:** Important styles inlined for performance
    * **Unused CSS:** Build process removes unused styles
    * **Compression:** SCSS compiled and minified for production

## 5. How to Work With This Code

* **Setup:**
    * SCSS compilation configured in Angular build process
    * Global styles imported in main styles.scss file
    * Component styles can import variables and mixins as needed
* **Development Guidelines:**
    * **Style Creation:** Use existing variables and mixins before creating new ones
    * **Responsive Design:** Always start with mobile styles and enhance for larger screens
    * **Component Styles:** Keep component styles scoped, use global utilities sparingly
    * **Testing:** Test styles across target browsers and devices
* **Testing:**
    * **Visual Testing:** Manual testing across different screen sizes and browsers
    * **Responsive Testing:** Verify breakpoint behavior and layout adaptation
    * **Cross-Browser Testing:** Ensure consistent appearance across target browsers
* **Common Pitfalls / Gotchas:**
    * **Specificity Issues:** Avoid overly specific selectors that are hard to override
    * **Global Pollution:** Be careful with global styles affecting component encapsulation
    * **Breakpoint Overlaps:** Ensure responsive breakpoints don't create conflicts
    * **Performance Impact:** Large SCSS files can impact compilation time
    * **Browser Support:** Ensure new CSS features have appropriate fallbacks

## 6. Dependencies

* **Internal Code Dependencies:**
    * No direct internal dependencies - styles are foundational
* **External Library Dependencies:**
    * **SCSS:** Sass preprocessing for variables, mixins, and nesting
    * **Angular Build:** Angular CLI build process for SCSS compilation
    * **PostCSS:** Post-processing for vendor prefixes and optimization
* **Dependents (Impact of Changes):**
    * [`../components`](../components/README.md) - All components depend on global styles and variables
    * [`../routes`](../routes/README.md) - Route components use responsive utilities and design tokens
    * **Application-wide:** Any component using SCSS variables or mixins

## 7. Rationale & Key Historical Context

* **SCSS Architecture:** Chosen for its mature ecosystem, variable support, and mixin capabilities
* **Mobile-First Design:** Implemented to ensure good mobile experience as primary consideration
* **Design System Approach:** Structured to support consistent design language across the application
* **Modular Organization:** Files separated by concern for better maintainability and team collaboration
* **Performance Focus:** Architecture designed to minimize CSS bundle size and runtime performance impact

## 8. Known Issues & TODOs

* **Design Token Documentation:** Complete documentation of design token usage and guidelines
* **CSS Custom Properties:** Migration to CSS custom properties for runtime theme switching
* **Performance Optimization:** Further optimization of CSS bundle size and loading
* **Accessibility:** Enhanced high contrast and reduced motion support
* **Style Guide:** Creation of living style guide for design system documentation

---