# Module/Directory: Zarichney.Standards/Documentation

**Version:** 2.0
**Last Updated:** 2025-05-26

> **Parent:** [`Zarichney.Standards`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains the official standards for creating and maintaining all documentation within the Zarichney API project.
* **Key Responsibilities:**
    * Defining mandatory structure and content requirements for per-directory README.md files.
    * Establishing standards for creating and maintaining Mermaid diagrams.
    * Providing guidelines for documentation quality and consistency.
    * Ensuring documentation supports both human developers and AI coding assistants.
* **Why it exists:** To maintain high-quality, consistent documentation that provides essential context for understanding and maintaining the codebase, particularly for stateless AI coding assistants.
* **Core Documents within this Directory:**
    * [`DocumentationStandards.md`](./DocumentationStandards.md): The comprehensive documentation standards that define structure, content, and quality requirements for all per-directory README files.
    * [`DiagrammingStandards.md`](./DiagrammingStandards.md): Standards for creating, maintaining, and embedding Mermaid diagrams within documentation.

## 2. Architecture & Key Concepts

* **AI-First Documentation:** Standards are designed to maximize value for stateless AI coding assistants while remaining useful for human developers.
* **Structured Consistency:** All documentation follows standardized templates and structures for predictable navigation and content organization.
* **Visual Communication:** Emphasizes the use of diagrams to supplement textual descriptions for complex architectural concepts.
* **Living Documentation:** Documentation is treated as code - it must be maintained, reviewed, and kept current with the evolving codebase.

## 3. Interface Contract & Assumptions

* **Mandatory Compliance:** All directory-level README.md files **MUST** follow the standards defined in this directory.
* **Template Usage:** All new README files must use the template structure defined in [`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md).
* **Review Process:** Documentation changes should be reviewed for compliance with these standards.
* **Diagram Standards:** All embedded diagrams must follow the conventions in `DiagrammingStandards.md`.

## 4. Local Conventions & Constraints

* **Markdown Format:** All documentation uses GitHub-flavored Markdown.
* **Mermaid Diagrams:** Diagrams use Mermaid syntax for consistency and maintainability.
* **Relative Linking:** Internal links use relative paths to maintain portability.
* **Stateless Perspective:** Documentation assumes the reader has no prior context beyond the current directory and its README.

## 5. How to Work With This Documentation

* **Creating New READMEs:** Start with [`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md) and follow [`DocumentationStandards.md`](./DocumentationStandards.md).
* **Adding Diagrams:** Follow the guidelines in [`DiagrammingStandards.md`](./DiagrammingStandards.md) for creating and embedding diagrams.
* **Updating Documentation:** Regularly review and update documentation to reflect code changes, following the pruning guidelines for outdated information.
* **Quality Assurance:** Use the standards as a checklist during documentation reviews.

## 6. Dependencies

* **Parent:** [`Zarichney.Standards`](../README.md) - This directory is part of the overall documentation structure.
* **Related Components:**
    * [`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md): The mandatory template structure for all README files.
    * [`../Maintenance/DocAuditorAssistant.md`](../Maintenance/DocAuditorAssistant.md): Process for auditing and improving documentation quality.
* **Tool Dependencies:** Relies on Markdown renderers that support Mermaid diagrams (GitHub, VS Code, etc.).

## 7. Rationale & Key Historical Context

* Documentation standards were established to support AI-assisted development workflows where clear, structured context is critical.
* The focus on stateless AI perspective ensures documentation provides complete context without assuming prior knowledge.
* Diagram standards were added to support visual communication of complex architectural concepts.

## 8. Known Issues & TODOs

* Standards require periodic review to ensure they continue to meet the needs of both human and AI developers.
* Additional guidance may be needed for documenting specific types of components (e.g., API endpoints, database schemas).