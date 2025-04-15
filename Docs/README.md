# Module/Directory: Docs

**Last Updated:** 2025-04-13

## Purpose & Responsibility

* **What it is:** This directory is the central repository for all project documentation for the Zarichney API application. It contains standards, guidelines, maintenance procedures, and templates necessary for understanding, developing, and maintaining the codebase.
* **Rationale:** This documentation structure was organized into `Development` and `Maintenance` workflow categories to clearly separate guidelines for building the software from procedures for operating and maintaining it.
* **Key Responsibilities:**
    * Establishing development standards (coding, documentation).
    * Providing operational guides for system maintenance (AWS, Database, Authentication).
    * Supplying templates for consistent documentation across the project.
    * Acting as the primary entry point for project-related documentation.
* **Why it exists:** To serve as a single source of truth for project knowledge, facilitate developer onboarding, ensure consistency in development and maintenance practices, and support AI coding assistants by providing necessary context.
* **Workflow Subdirectories & Key Files:**
    * [`Development/`](./Development/README.md): Contains documentation related to the software development lifecycle, including coding standards and documentation guidelines.
    * [`Maintenance/`](./Maintenance/README.md): Contains operational guides for maintaining the application's infrastructure and database.

## How to Work With This Documentation

* **Starting Point:** Use this README as the entry point to navigate the project's documentation.
* **Development:** Refer to the `Development/` subdirectory for coding and documentation standards before contributing code or writing module READMEs.
* **Maintenance:** Consult the `Maintenance/` subdirectory for procedures related to AWS infrastructure, database administration, or authentication system upkeep.
* **New Module Documentation:** Use the `README_template.md` as the basis for creating README files for new code directories, following the guidelines in `Development/DocumentationStandards.md`.
