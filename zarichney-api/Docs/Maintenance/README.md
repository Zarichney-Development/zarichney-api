# Module/Directory: Docs/Maintenance

**Last Updated:** 2025-04-13

**(Optional: Link to Parent Directory's README)**
> **Parent:** [`Docs`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains documentation focused on the operational maintenance, administration, and troubleshooting of the Zarichney API's infrastructure and database components.
* **Key Responsibilities:** Provides detailed guides and procedures for essential maintenance tasks.
* **Why it exists:** To centralize operational documentation, ensuring maintainers have access to standardized procedures for keeping the system running smoothly and resolving common issues. This separates operational concerns from development standards or architectural documentation.
* **Documents within this Directory:**
    * [`AmazonWebServices.md`](./AmazonWebServices.md): Covers maintenance tasks for the AWS infrastructure hosting the application (EC2, Security Groups, CloudFront, Secrets Manager, SSM Parameter Store), including deployment steps, service control, resource monitoring, and troubleshooting common AWS-related issues [cite: zarichney-api/Docs/AmazonWebServices.md].
    * [`PostgreSqlDatabase.md`](./PostgreSqlDatabase.md): Details the maintenance procedures for the PostgreSQL database (`zarichney_identity`), focusing on EF Core migrations, user/role management (via EF or direct SQL), refresh token management, backup/restore operations, and general PostgreSQL administration commands [cite: zarichney-api/Docs/PostgreSqlDatabase.md].
    * [`Authentication System Maintenance Guide`](./AuthenticationSystem.md): Provides guidelines for maintaining and troubleshooting the authentication system, including user management, token handling (JWT & Refresh Tokens), API Key management, email verification, configuration, and security best practices. This guide consolidates information previously found in `AuthSystemMaintenance.md` and `ApiKeyAuthentication.md`.
    * [`DocAuditorAssistant.md`](./DocAuditorAssistant.md): Defines the workflow for using an AI assistant to audit and improve documentation quality, ensuring READMEs remain accurate and valuable over time [cite: zarichney-api/Docs/Maintenance/DocAuditorAssistant.md].

## 2. Architecture & Key Concepts

* Not applicable for this documentation directory. Refer to specific service/module READMEs (e.g., `Server/Services/Auth/README.md`) for architectural details.

## 3. Interface Contract & Assumptions

* Not applicable for this documentation directory.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* Not applicable for this documentation directory. The conventions are defined *within* the maintenance guides themselves.

## 5. How to Work With This Documentation

* **Consult Before Maintenance:** Refer to the relevant guide (`AmazonWebServices.md`, `PostgreSqlDatabase.md`, `Authentication System Maintenance Guide.md`) before performing infrastructure updates, database changes, deployment troubleshooting, or authentication system maintenance.
* **Troubleshooting:** Use the troubleshooting sections within each guide to diagnose common operational problems related to AWS, PostgreSQL, or Authentication.
* **Standard Procedures:** Follow the outlined procedures for tasks like applying database migrations (`PostgreSqlDatabase.md`), managing AWS resources (`AmazonWebServices.md`), or handling authentication issues (`Authentication System Maintenance Guide.md`) to ensure consistency.

## 6. Dependencies

* **Parent:** [`Docs`](../README.md) - This directory is part of the overall documentation structure.
* **Related Module Docs:** The procedures outlined here relate to systems described in detail within specific module READMEs, such as:
    * [`Server/Services/Auth/README.md`](../../Server/Services/Auth/README.md) (For Auth System context)
    * [`Server/Services/Auth/Migrations/README.md`](../../Server/Services/Auth/Migrations/README.md) (For DB Migration context)
    * Deployment scripts or CI/CD configuration files may also be relevant.

## 7. Rationale & Key Historical Context

* This directory was created to separate operational maintenance documentation from developer-focused documentation (like coding standards or API usage guides), making it easier for maintainers to find relevant procedures.

## 8. Known Issues & TODOs

* **Regular Updates:** These maintenance guides require regular review and updates to reflect any changes in the AWS infrastructure, database schema/configuration, authentication system, or deployment processes. Outdated information can lead to errors during maintenance.
* **Tool Versions:** Specific commands (e.g., `aws`, `psql`, `dotnet ef`) may depend on particular tool versions. Ensure the environment where maintenance is performed has compatible versions installed.