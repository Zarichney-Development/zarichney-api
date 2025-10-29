# Project Documentation Standards (Per-Directory README.md Files)

**Version:** 1.7
**Last Updated:** 2025-10-27

## 1. Purpose and Scope

This document defines the mandatory standards and practices for creating and maintaining `README.md` files within *each directory* of this codebase.

* **Primary Goal:** To provide essential context, design rationale, interface contracts, and operational guidance specifically tailored for **stateless AI coding assistants** who perform maintenance and development tasks. These READMEs, potentially including embedded architectural diagrams, are critical for enabling AI to understand and safely modify code within a specific directory context.
* **Scope:** This document governs *only* the per-directory `README.md` files. It dictates their structure, content focus, linking strategy, and maintenance requirements.
* **Relationship to Other Standards:**
    * This document complements **[`./CodingStandards.md`](./CodingStandards.md)**. While `CodingStandards.md` focuses on *how to write C# code*, this document focuses exclusively on *how to document code modules via README.md files*.
    * Information within these READMEs, particularly regarding interface contracts, dependencies, and module-specific operational details, directly supports the testing activities outlined in **`Docs/Standards/TestingStandards.md`** (the overarching testing guide), **`Docs/Standards/UnitTestCaseDevelopment.md`**, and **`Docs/Standards/IntegrationTestCaseDevelopment.md`**.
    * Embedded diagrams within READMEs **MUST** adhere to **[`./DiagrammingStandards.md`](./DiagrammingStandards.md)**.
    * The mandatory structure for all READMEs is defined in the **[`./README_template.md`](./README_template.md)** file. This document explains *how* to effectively utilize that template.

## 2. Core Philosophy & Principles

* **Target Audience: Stateless AI:** Write clearly, explicitly, and unambiguously. Assume the reader (a future AI instance) has **no prior context** about the specific module beyond the code in the directory, *this* README, and any embedded diagrams. Structure information logically for efficient parsing.
* **Context is King:** Focus documentation efforts on the *'why'* behind design decisions, implicit assumptions the code makes, non-obvious behaviors, and the module's specific role and boundaries within the larger system. Maximize contextual value for the AI.
* **Visual Clarity:** Supplement textual descriptions with embedded Mermaid diagrams where appropriate (following `DiagrammingStandards.md`) to visually communicate architecture, key workflows, and component relationships.
* **Value over Volume:** Prioritize information *not* immediately obvious from the code or diagrams. **Avoid** simple code-to-English translation or exhaustive lists of public members readily available via static code analysis. The README should supplement, not repeat, the code or diagrams.
* **Maintainability & Pruning:** Documentation (text and diagrams) **MUST** be kept current as the code evolves. Outdated information, especially historical rationale (Section 7) or resolved issues (Section 8) that no longer illuminate the *current* state, **MUST be pruned** during updates. Keep the README focused and relevant.
* **Consistency:** Adhere strictly to the structure defined in **[`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md)** for all per-directory README files.
* **Discoverability (Linking):** Create a navigable documentation network by consistently linking between parent, child, and related module READMEs using relative paths. This is crucial for AI navigation.

## 3. Utilizing the `README_template.md`

The **[`../Templates/ReadmeTemplate.md`](../Templates/ReadmeTemplate.md)** file provides the mandatory structure for all directory-specific READMEs. Ensure every section is thoughtfully completed or explicitly marked as not applicable. Key sections and their intent for the AI audience:

* **Header & Parent Link:** Essential for identification and navigation within the documentation network.
* **Section 1 (Purpose & Responsibility):** High-level overview. *What* does this module do functionally? *Why* does it exist as a separate unit? Crucially links to child READMEs if they exist.
* **Section 2 (Architecture & Key Concepts):** Internal design overview. *How* does it work internally? Key components, data structures, and interactions with immediate neighbors. Focus on the conceptual model, not exhaustive detail. This section is the primary location for embedding relevant Mermaid diagrams visualizing the module's **architecture or key workflows/sequences**. Diagrams illustrating data flow, state transitions, or complex decision logic can be particularly useful for designing comprehensive test cases.
* **Section 3 (Interface Contract & Assumptions):** **CRITICAL Section.** Defines the rules for *interacting* with this module from the outside. **MUST** focus intensely on *preconditions (required input states/values), postconditions (expected output states/values, side effects), non-obvious error handling (specific exceptions thrown under certain conditions), invariants,* and *critical assumptions* the code makes about its inputs, dependencies, or environment. This information is vital for designing both positive and negative test cases. This is **NOT** for simply listing public method signatures but for explaining the *behavioral contract*. Diagrams in Section 2 may help illustrate these contracts.
* **Section 4 (Local Conventions & Constraints):** Rules specific to *this* directory that augment or override global standards. **MUST** detail local deviations. This includes any specific configuration values or environmental conditions that are critical for the module's operation *and testing*. For example, if certain features of this module behave differently or require special setup for testing (e.g., specific test data profiles, mock configurations not handled globally), detail them here.
* **Section 5 (How to Work With This Code):** Practical guidance for developers/AI. **MUST** include:
    * Setup steps unique to this module.
    * **Module-Specific Testing Strategy:** Briefly outline the primary testing approach for this module (e.g., "Primarily unit tested due to complex internal logic; integration tests cover API contract via `PublicControllerTests`").
    * **Key Test Scenarios:** Highlight any particularly important, complex, or non-obvious scenarios that **must** be covered by tests for this module.
    * **Test Data Considerations:** Mention any specific types of test data or data generation strategies that are particularly relevant for testing this module's logic or edge cases.
    * Specific commands to run tests relevant to this module (if different from global commands).
    * Known pitfalls or non-obvious behaviors ('gotchas') that could affect development or testing.
* **Section 6 (Dependencies):** Maps the module's place in the system. **MUST** list:
    * Key internal modules directly consumed by this module (link to their READMEs). *Understanding these is crucial for mocking dependencies in unit tests.*
    * Key internal modules that directly consume this module (link to their READMEs). *Understanding these helps assess impact of changes.*
    * Key external libraries or services (e.g., specific NuGet packages with non-obvious configurations, external SaaS providers). *Highlight any that have specific implications for testing, such as requiring virtualization in integration tests.*
      This information is critical for understanding change impact and for designing appropriate test doubles or test environment configurations.
* **Section 7 (Rationale & Key Historical Context):** Explain *why* the current design exists, especially non-obvious choices. Include historical notes *only* if they illuminate the *present* state. **Prune aggressively** when context becomes obsolete.
* **Section 8 (Known Issues & TODOs):** Track current limitations or planned work *specific* to this module. Remove items when resolved.

## 4. Linking Strategy (Mandatory)

Create a navigable documentation network for the AI:

* **Parent Link:** Every README (except the root `README.md`) MUST link to its immediate parent directory's README using a relative path (e.g., `../README.md`).
* **Child Links:** A README MUST link to the README of each immediate subdirectory that contains significant code/config and has its own README (Section 1). Use relative paths (e.g., `./SubModule/README.md`).
* **Dependency/Dependent Links:** When mentioning other internal modules in Section 6 (Dependencies/Dependents), ALWAYS link to their respective README files using relative paths (e.g., `../../OtherModule/README.md`). Do not cite files, using the `[cite]` tag.
* **Diagram Links (Optional):** If complex diagrams are stored in separate `.mmd` files (per `DiagrammingStandards.md`), link to them from Section 2 using relative paths (e.g., `../../../docs/diagrams/MyModule/DetailedFlow.mmd`).

## 5. Maintenance and Updates (AI Coder Responsibility)

* **Trigger:** Any task (performed by human or AI) that modifies the code or associated tests within a directory in a way that impacts its documented purpose, architecture, interface contracts, assumptions, dependencies, **module-specific testing considerations (as outlined in Section 5 of the README template)**, known issues, or **visualized architecture/flows** **MUST** also update the corresponding `README.md` file (including any embedded or linked diagrams) within the same commit/change, following the standards herein and in **[`./DiagrammingStandards.md`](./DiagrammingStandards.md)**.
* **Pruning:** When updating, review Section 7 (Rationale) and Section 8 (Known Issues) and **actively remove** any information that is no longer relevant due to the code changes. Keep the README focused on the *current* state.
* **`Last Updated` Date:** Always update the `Last Updated: [YYYY-MM-DD]` field at the top of the README when making any changes to its text or embedded diagrams.
* **Annotation Instruction:** **Do NOT** include explanatory annotations like "<-- UPDATED -->" or "`// Updated this line`" within the documentation content itself.

## 6. Skills Documentation Requirements

### Metadata Standards
- All skills MUST have YAML frontmatter in SKILL.md with required fields (name, description)
- Name MUST follow constraints: max 64 chars, lowercase/numbers/hyphens only, no reserved words
- Description MUST be non-empty, max 1024 chars
- Token estimates SHOULD be tracked for metadata and instructions phases
- Category and tags MUST enable accurate discovery (coordination/technical/meta/workflow)
- Agent targeting MUST specify applicable agents

### Resource Organization
- Templates in resources/templates/ for reusable formats
- Examples in resources/examples/ for reference implementations
- Documentation in resources/documentation/ for deep dives
- File naming: kebab-case, descriptive, no version numbers
- Skill categories MUST have README.md for navigation

### Discovery Mechanism Documentation
- README.md in each skill category directory
- Skill catalog in .claude/skills/README.md
- Cross-references from agent definitions
- Integration examples in development documentation

### Progressive Loading Design
- Metadata optimized for discovery (<150 tokens target)
- Instructions focused and actionable (2,000-5,000 tokens)
- Resources loaded on-demand, not preloaded
- Clear separation: frontmatter → instructions → resources

**See Also:**
- [SkillsDevelopmentGuide.md](../Development/SkillsDevelopmentGuide.md) - Comprehensive skills creation guide
- [ContextManagementGuide.md](../Development/ContextManagementGuide.md) - Progressive loading strategies

## 7. Epic Archiving Standards

### Purpose and Scope
When epics are completed with all issues closed, PRs merged, and final validation complete, epic artifacts must be systematically archived to preserve historical context while maintaining clean active workspace directories.

### Archive Directory Structure
```
./Docs/Archive/<epic-number-name>/
├── README.md                      # Archive overview with epic summary
├── Specs/                         # Complete spec directory contents
│   ├── README.md
│   ├── [all spec files]
│   └── [subdirectories]
└── working-dir/                   # All working directory artifacts
    ├── [execution plans]
    ├── [completion reports]
    ├── [validation reports]
    └── [all coordination artifacts]
```

### Naming Conventions
- **Archive Directory:** `epic-{number}-{kebab-case-name}`
  - Examples: `epic-291-skills-commands`, `epic-246-language-model-service`
  - Must match original spec directory naming pattern
- **Subdirectories:** Exactly two - `Specs/` and `working-dir/`
- **README.md:** Required at archive root level

### Archive README Requirements
The archive README.md must include:

1. **Epic Header:**
   ```markdown
   # Epic #{number}: {Full Name}
   **Status:** ARCHIVED
   **Completion Date:** YYYY-MM-DD
   **Total Iterations:** X iterations
   **Total Issues:** Y issues
   ```

2. **Executive Summary:**
   - 2-3 sentence overview of epic purpose and outcomes
   - Key performance achievements with quantified metrics
   - Strategic impact statement

3. **Iterations Overview:**
   - Bulleted list of all iterations with issue ranges
   - One-line summary per iteration

4. **Key Deliverables:**
   - Major outcomes (skills created, commands implemented, modules refactored)
   - Performance results (context reduction, productivity gains, ROI)
   - Quality achievements (test coverage, standards compliance)

5. **Documentation Network:**
   - Links to committed documentation in `./Docs/Development/`
   - Links to related standards updates
   - Links to module READMEs affected by epic

6. **Archive Contents:**
   - Summary of Specs directory contents
   - Summary of working-dir artifacts (count, categories)
   - Navigation guidance for archive exploration

### Archiving Procedures

**Pre-Archive Validation:**
- All epic issues must be closed
- All section PRs must be merged
- Final section PR must be ready for review
- ComplianceOfficer validation must show GO decision
- Documentation index must be current

**Archive Operations Sequence:**
1. Create archive directory: `./Docs/Archive/epic-{number}-{name}/`
2. Create Specs subdirectory: `./Docs/Archive/epic-{number}-{name}/Specs/`
3. Move spec directory contents: `./Docs/Specs/epic-{number}-{name}/*` → archive Specs/
4. Create working-dir subdirectory: `./Docs/Archive/epic-{number}-{name}/working-dir/`
5. Move working directory artifacts: `./working-dir/*` → archive working-dir/
6. Restore working-dir README: Keep `./working-dir/README.md` in active workspace
7. Generate archive README.md at archive root with epic summary
8. Update `./Docs/DOCUMENTATION_INDEX.md` with archive reference

**Move Strategy:**
- **Specs directory:** Complete move (remove from original location after successful archive)
- **Working directory:** Complete move of all artifacts except README.md
- **Rationale:** Clean slate for next epic while preserving complete historical record

**Post-Archive Validation:**
- Archive directory structure matches standard
- All expected files present in archive
- Archive README comprehensive and accurate
- Original spec directory removed (or empty)
- Working directory cleaned (only README.md remains)
- DOCUMENTATION_INDEX.md updated with archive entry
- No broken links in documentation network

### DOCUMENTATION_INDEX.md Integration

Add or update "Completed Epics" section in DOCUMENTATION_INDEX.md:

```markdown
## Completed Epics

Archived epics with complete historical context including specs and working directory artifacts.

### Epic #291: Agent Skills & Slash Commands Integration
**Archive:** [./Archive/epic-291-skills-commands/](./Archive/epic-291-skills-commands/README.md)
**Completion:** 2025-10-27
**Summary:** Progressive loading architecture achieving 50-51% context reduction, 144-328% session token savings, 42-61 min/day productivity gains
**Key Deliverables:** 7 skills, 4 commands, 11 agents refactored, comprehensive performance documentation
**Performance Documentation:** [Epic291PerformanceAchievements.md](./Development/Epic291PerformanceAchievements.md)
```

### Maintenance and Updates

**Trigger:** Epic completion with all validation passed
**Responsibility:** Codebase Manager coordinates archiving (may delegate to `/epic-complete` command)
**Verification:** ComplianceOfficer validates archiving completeness
**Frequency:** Once per completed epic (non-recurring per epic)

**Archive Immutability:**
Once archived, epic directories should remain unchanged except for:
- Critical link corrections if committed documentation moves
- README clarifications if historical context misrepresented
- **Never:** Remove artifacts, restructure directories, or modify working-dir contents

**See Also:**
- [TaskManagementStandards.md Section 10](./TaskManagementStandards.md#10-epic-completion-workflow) - Epic completion workflow procedures
- [Epic Completion Skill](../../.claude/skills/coordination/epic-completion/) - Automated archiving skill (when created)
- [/epic-complete Command](../../.claude/commands/epic-complete.md) - Epic completion command (when created)

---
