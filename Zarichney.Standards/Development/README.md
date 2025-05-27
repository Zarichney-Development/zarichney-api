# Module/Directory: Zarichney.Standards/Development

**Version:** 2.0
**Last Updated:** 2025-05-26

> **Parent:** [`Zarichney.Standards`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory houses guides and information for local development setup and common development tasks for the Zarichney API application.
* **Key Responsibilities:**
    * Providing comprehensive guides for local development environment setup.
    * Documenting logging configuration and best practices.
    * Outlining testing setup procedures and requirements.
* **Why it exists:** To centralize all development-related setup information and common development tasks, ensuring developers can quickly get started and understand the development environment requirements.
* **Core Documents within this Directory:**
    * [`LocalSetup.md`](./LocalSetup.md): Comprehensive guide for setting up the local development environment, including dependencies, configuration, and initial setup steps.
    * [`LoggingGuide.md`](./LoggingGuide.md): Comprehensive guide for the enhanced logging system, including configuration and best practices.
    * [`TestingSetup.md`](./TestingSetup.md): Guide for setting up and configuring the testing environment, including Docker requirements and test execution.
* **Related Standards:**
    * [`../Coding/CodingStandards.md`](../Coding/CodingStandards.md): Coding standards that apply to development work.
    * [`../Testing/TestingStandards.md`](../Testing/TestingStandards.md): Testing standards and best practices.
    * [`../Documentation/DocumentationStandards.md`](../Documentation/DocumentationStandards.md): Documentation standards for maintaining READMEs and documentation.

## 2. Architecture & Key Concepts

* **Development Environment Setup:** The guides in this directory assume a .NET 8 development environment with supporting tools and dependencies as outlined in each specific guide.
* **Local Configuration:** Development setup includes proper configuration of secrets, database connections, and external service integrations for local development and testing.
* **Testing Infrastructure:** Testing setup includes Docker for integration tests and proper configuration of test databases and external service mocks.

## 3. Interface Contract & Assumptions

* **Prerequisites:** Assumes .NET 8 SDK, Docker Desktop, and Git are installed on the development machine.
* **Configuration:** Assumes proper configuration of user secrets and environment variables as detailed in the setup guides.
* **External Dependencies:** Some setup procedures require access to external services and APIs as documented in each guide.

## 4. Local Conventions & Constraints

* **Guide Format:** All guides follow a step-by-step format with clear prerequisites and verification steps.
* **Environment Specificity:** Setup instructions are provided for multiple operating systems where applicable.
* **Version Dependencies:** Specific tool and framework versions are documented where critical for compatibility.

## 5. How to Work With This Documentation

* **Initial Setup:** Start with [`LocalSetup.md`](./LocalSetup.md) for comprehensive environment setup.
* **Logging Configuration:** Refer to [`LoggingGuide.md`](./LoggingGuide.md) for logging setup and best practices.
* **Testing Setup:** Use [`TestingSetup.md`](./TestingSetup.md) for test environment configuration.
* **Troubleshooting:** Each guide includes common issues and troubleshooting steps.

## 6. Dependencies

* **Parent:** [`Zarichney.Standards`](../README.md) - This directory is part of the overall documentation structure.
* **Related Standards:** The setup procedures align with standards defined in [`../Coding/`](../Coding/README.md), [`../Testing/`](../Testing/README.md), and [`../Documentation/`](../Documentation/README.md).

## 7. Rationale & Key Historical Context

* This directory consolidates development setup information that was previously scattered across different documentation files.
* The TestingSetup.md was moved here from Maintenance/ as it's more relevant to development setup than operational maintenance.

## 8. Known Issues & TODOs

* Setup guides require regular updates to reflect changes in tool versions and dependencies.
* Additional platform-specific setup instructions may be needed as the development team grows.

-----
