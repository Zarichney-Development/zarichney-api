# Module/Directory: /scripts

**Last Updated:** 2025-06-27

> **Parent:** [`Code/Zarichney.Website`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Build automation scripts and utilities that support the development and deployment workflow for the Angular frontend application. These scripts handle build optimization, asset cleanup, and deployment preparation tasks.
* **Key Responsibilities:**
    * Build artifact cleanup and management
    * HTML optimization and minification
    * Development and production build automation
    * Asset processing and optimization
    * Deployment preparation and validation
    * Build process enhancement and customization
* **Why it exists:** To provide automated build utilities that enhance the Angular CLI build process, optimize output for production deployment, and streamline development workflows with custom tooling.

## 2. Architecture & Key Concepts

* **High-Level Design:** Collection of Node.js scripts that integrate with the Angular build process to provide additional optimization and automation capabilities. Scripts are designed to be run as npm scripts or directly via Node.js.
* **Core Logic Flow:**
    1. **Build Trigger:** Scripts executed as part of npm scripts or build process
    2. **File Processing:** Scripts read, process, and modify build artifacts
    3. **Optimization:** Content optimization and minification applied
    4. **Output Generation:** Optimized files written to distribution directory
    5. **Validation:** Build output validated for deployment readiness
* **Key Data Structures:**
    * File system operations for reading and writing build artifacts
    * Configuration objects for optimization settings
    * Build manifest data for tracking processed files
    * Error handling and logging for build process feedback
* **State Management:**
    * **File System State:** Scripts operate on file system artifacts
    * **Build State:** Track build process progress and results
    * **Configuration State:** Static configuration for script behavior
* **Diagram:**
    ```mermaid
    graph TD
        A[Scripts Directory] --> B[clean-dist.mjs]
        A --> C[clean-html.mjs]
        
        B --> D[Distribution Cleanup]
        B --> E[Build Artifact Removal]
        
        C --> F[HTML Processing]
        C --> G[Content Optimization]
        
        H[NPM Scripts] --> A
        I[Build Process] --> A
        J[Angular CLI] --> K[Build Output]
        K --> A
        
        A --> L[Optimized Output]
        L --> M[Production Deployment]
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `clean-dist.mjs`:
        * **Purpose:** Clean and prepare distribution directory for fresh builds
        * **Critical Preconditions:** Node.js environment, file system access, dist directory exists
        * **Critical Postconditions:** Distribution directory cleaned, ready for new build artifacts
        * **Non-Obvious Error Handling:** Handles locked files, permission issues, missing directories
    * `clean-html.mjs`:
        * **Purpose:** Optimize and clean HTML output for production deployment
        * **Critical Preconditions:** Built HTML files available, Node.js HTML processing capabilities
        * **Critical Postconditions:** HTML optimized for production, unnecessary content removed
        * **Non-Obvious Error Handling:** Handles malformed HTML, encoding issues, large files
    * NPM Script Integration:
        * **Purpose:** Scripts integrated into package.json for easy execution
        * **Critical Preconditions:** NPM environment, script dependencies available
        * **Critical Postconditions:** Scripts execute as part of build workflow
        * **Non-Obvious Error Handling:** Script failures properly reported to build process
* **Critical Assumptions:**
    * **Node.js Environment:** Scripts assume Node.js runtime with ES modules support
    * **File System Access:** Assumes read/write access to build directories
    * **Build Process Integration:** Designed to work with Angular CLI build output
    * **NPM Workflow:** Assumes integration with npm scripts for execution

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Script Design:**
    * **ES Modules:** Scripts use ES module syntax (.mjs extension)
    * **Async Operations:** File operations use async/await patterns
    * **Error Handling:** Comprehensive error handling with meaningful messages
    * **Logging:** Informative logging for build process transparency
* **File Processing:**
    * **Safe Operations:** Scripts check file existence before processing
    * **Backup Strategy:** Critical operations create backups where appropriate
    * **Performance:** Optimized for processing large numbers of files
    * **Idempotent:** Scripts can be run multiple times safely
* **Integration:**
    * **NPM Scripts:** Integrated into package.json for standard workflow
    * **Build Process:** Designed to integrate with Angular CLI build pipeline
    * **Cross-Platform:** Scripts work on Windows, macOS, and Linux
* **Configuration:**
    * **Environment Aware:** Scripts adapt behavior based on environment
    * **Configurable:** Key parameters configurable through environment variables
    * **Default Values:** Sensible defaults for common use cases

## 5. How to Work With This Code

* **Setup:**
    * Node.js v16+ required for ES module support
    * Scripts automatically available through npm package.json integration
    * No additional dependencies required beyond Node.js built-ins
* **Development Guidelines:**
    * **Script Creation:** Use ES module syntax and async/await patterns
    * **Error Handling:** Implement comprehensive error handling and logging
    * **Testing:** Test scripts with various file scenarios and edge cases
    * **Documentation:** Document script purpose, parameters, and usage
* **Usage:**
    * **Via NPM:** `npm run clean-dist`, `npm run clean-html`
    * **Direct Execution:** `node scripts/clean-dist.mjs`
    * **Build Integration:** Scripts run automatically during build process
* **Common Pitfalls / Gotchas:**
    * **File Locking:** Handle cases where files may be locked by other processes
    * **Permission Issues:** Ensure scripts handle file permission problems gracefully
    * **Path Resolution:** Use absolute paths or proper relative path resolution
    * **Cross-Platform:** Test scripts on different operating systems
    * **Error Recovery:** Ensure scripts can recover from partial failures

## 6. Dependencies

* **Internal Code Dependencies:**
    * No internal dependencies - scripts operate on build output independently
* **External Library Dependencies:**
    * **Node.js Built-ins:** File system, path, and utility modules
    * **ES Modules:** Native ES module support in Node.js
* **Dependents (Impact of Changes):**
    * **Build Process:** Angular CLI build process may depend on script output
    * **NPM Scripts:** Package.json scripts reference these utilities
    * **Deployment:** Production deployment may depend on script optimizations

## 7. Rationale & Key Historical Context

* **Build Optimization:** Added to provide additional optimization beyond Angular CLI defaults
* **Custom Tooling:** Created to address specific build requirements not covered by standard tools
* **Performance Focus:** Designed to optimize build output for production performance
* **Automation:** Reduces manual steps in build and deployment process
* **ES Modules:** Uses modern JavaScript module system for better maintainability

## 8. Known Issues & TODOs

* **Script Testing:** Automated testing framework for script validation
* **Performance Monitoring:** Metrics collection for script execution time and effectiveness
* **Additional Optimizations:** Potential for more build optimization scripts
* **Error Reporting:** Enhanced error reporting and build failure diagnostics
* **Configuration Management:** Centralized configuration for script parameters

---