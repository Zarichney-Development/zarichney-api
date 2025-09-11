# ⚠️ DEPRECATED WORKFLOW

**Status:** This workflow has been superseded by the **12-agent orchestration model**.  
**Effective Date:** August 2025  
**Migration Path:** See [CONTRIBUTING.md](../../CONTRIBUTING.md) for current development methods.

---

## Why This Workflow Was Deprecated

This manual workflow has been consolidated into the comprehensive 12-agent orchestration model that provides:
- **Automated Standards Compliance:** Documentation grounding protocols ensure all standards are systematically loaded
- **Enhanced Quality Gates:** ComplianceOfficer partnership provides dual validation before PR creation  
- **Reduced Overhead:** Working directory communication eliminates telephone-game coordination
- **Context Preservation:** Rich artifact sharing enables better documentation and decision tracking

**For Current Development:** Activate the orchestration model via [CLAUDE.md](../../CLAUDE.md) and leverage specialized subagents instead of following these manual steps.

**Agent Mapping:** This coverage enhancement workflow is now handled by **TestEngineer** (systematic gap analysis and targeted test development) + **ComplianceOfficer** (validation and epic progression tracking) coordination, with integration to the [Automated Coverage Epic](AutomatedCoverageEpicWorkflow.md) for systematic improvement.

**Reference:** The consolidated [Coverage Enhancement Process](../../CONTRIBUTING.md#coverage-enhancement-process) in CONTRIBUTING.md preserves the analytics-driven approach and coverage measurement tools while integrating them into the modern AI orchestration framework with epic progression toward 90% backend coverage by January 2026.

---

## Historical Documentation

The content below is preserved for reference and understanding of workflow evolution:

# Test Coverage Enhancement Workflow

**Version:** 1.1 (DEPRECATED)  
**Last Updated:** 2025-09-01  
**Status:** DEPRECATED  
**Parent:** `../README.md`

## 1. Purpose and Goal

* **Purpose:** This document defines the systematic workflow for increasing and maintaining automated test coverage for the `Code/Zarichney.Server` project. It is particularly tailored for AI Coding Assistants assigned tasks related to test coverage enhancement but is applicable to all developers.
* **Project Goal:** To achieve and maintain:
    * **>=90% unit test coverage** for all non-trivial business logic, services, and utility classes.
    * **Comprehensive integration test coverage** for all public API endpoints, critical workflows, and component interactions.
* **Focus:** This workflow emphasizes not just adding tests, but adding *high-quality, maintainable tests* that adhere to all established project standards.

## 2. Prerequisites & Required Reading

Before initiating any test coverage enhancement task, the assigned developer (human or AI) **must** thoroughly review and understand the following documents:

* **Core Testing Standards & Guides:**
    * `../../Docs/Standards/TestingStandards.md`: The overarching testing philosophy, mandatory tooling, and general practices.
    * `../../Docs/Standards/UnitTestCaseDevelopment.md`: Detailed instructions for writing effective unit tests.
    * `../../Docs/Standards/IntegrationTestCaseDevelopment.md`: Detailed instructions for writing effective integration tests.
* **Code & Documentation Standards:**
    * `../../Docs/Standards/CodingStandards.md`: Essential for understanding how to write testable production code and when/how to refactor for testability.
    * `../../Docs/Standards/DocumentationStandards.md`: For updating any relevant README files if code is refactored or key test strategies are noted.
* **System Under Test (SUT) Context:**
    * The specific `README.md` file for the directory/module containing the SUT.
    * Any relevant architectural diagrams associated with the SUT (as per `../../Docs/Standards/DiagrammingStandards.md`).
* **Test Framework Blueprint:**
    * `../../Zarichney.Server.Tests/TechnicalDesignDocument.md`: For understanding the testing framework's components and capabilities.

## 3. Test Coverage Workflow Steps

This workflow is designed to be iterative and ensure quality.

**Step 1: Understand the Task & Identify Coverage Gaps**
* **Action:** Carefully review the assigned test coverage task (e.g., from a GitHub Issue based on `../../Docs/Templates/GHTestCoverageTask.md`).
* **Action:** Clearly identify the System Under Test (SUT) – the specific class(es), method(s), or module(s) requiring improved coverage.
* **Action:** Review any existing tests for the SUT to understand current coverage and testing patterns.
* **Action:** **Locally generate a code coverage report** for the SUT (see Section 4: "Tools for Measuring Coverage") to pinpoint specific lines, branches, or methods that are currently uncovered.

**Step 2: Analyze Code for Testability & Plan Refactoring (If Needed)**
* **Action:** Examine the SUT's source code. Assess its current testability based on principles in `../../Docs/Standards/CodingStandards.md` (e.g., adherence to DI, SOLID, Humble Object pattern, avoidance of statics).
* **Constraint:** Production code (`Zarichney.Server/`) **should not be changed** unless it is essential for enabling testability of critical, currently untestable logic.
* **Action (If Refactoring Needed):**
    * If the SUT is difficult to test due to its design:
        * Identify specific refactorings (e.g., extracting dependencies, applying the Humble Object pattern) that would improve testability.
        * **If changes are minor and clearly improve testability without altering external contracts or core business logic significantly, proceed with the refactoring.** Document these changes clearly in the commit and Pull Request.
        * **If changes are substantial or impact public contracts, discuss the proposed refactoring with the project maintainers/leads before proceeding.** This might involve creating a separate small task/PR for the refactoring itself.
    * Any refactoring **must** be covered by the new or existing tests to ensure no regressions.

**Step 3: Design Test Cases**
* **Action:** Based on the coverage gap analysis and understanding of the SUT, design specific test cases.
* **Focus:**
    * Prioritize uncovered logical paths, branches, and methods.
    * Design unit tests for isolated business logic within classes/methods.
    * Design integration tests if the uncovered areas involve interactions between multiple components, API endpoints, or database operations that are not adequately covered.
* **Considerations:** Test positive paths, negative paths (invalid inputs, error conditions), boundary conditions, and edge cases.
* **Guidance:** Refer extensively to `../../Docs/Standards/UnitTestCaseDevelopment.md` and `../../Docs/Standards/IntegrationTestCaseDevelopment.md` for patterns and best practices.

**Step 4: Write Unit Tests**
* **Action:** Implement the designed unit test cases.
* **Adherence:** Strictly follow all standards in `../../Docs/Standards/UnitTestCaseDevelopment.md`. This includes:
    * Complete isolation of the SUT by mocking all dependencies using Moq.
    * Clear Arrange-Act-Assert (AAA) structure.
    * Descriptive test naming.
    * Using AutoFixture (e.g., `[AutoData]`, `[Frozen]`) for test data.
    * Writing expressive assertions with FluentAssertions and including clear reasons using the assertion's optional message parameter.
    * Marking tests with `[Trait("Category", "Unit")]`.

**Step 5: Write Integration Tests (If Applicable to Scope)**
* **Action:** Implement the designed integration test cases if the coverage task involves API endpoints or component interactions.
* **Adherence:** Strictly follow all standards in `../../Docs/Standards/IntegrationTestCaseDevelopment.md`. This includes:
    * Using the established testing framework (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`).
    * Interacting with the API via the Refit `ApiClient`.
    * Managing database state correctly (e.g., `await DbFixture.ResetDatabaseAsync();`).
    * Simulating authentication using `AuthTestHelper`.
    * Setting up mocks for internal services (via `Factory.Services`) or stubs for external HTTP services (WireMock.Net, once implemented as per TDD FRMK-004).
    * Marking tests with `[Trait("Category", "Integration")]` and relevant dependency/mutability traits. Use `[DependencyFact]` where appropriate.

**Step 6: Run Tests Locally & Generate Coverage Report**
* **Action:** Execute the newly written tests and all other tests relevant to the SUT to ensure they pass.
* **Action:** Generate a local code coverage report using Coverlet (see Section 4).
* **Analysis:** Analyze the report to confirm that the new tests effectively cover the targeted uncovered areas and that the overall coverage for the SUT has improved as expected. Ensure new tests are actually hitting the intended code paths.

**Step 7: Refine Tests & Code (Iterate if Necessary)**
* **Action:** Based on test results and coverage analysis:
    * Refine test cases for clarity, robustness, or better coverage.
    * Add more test cases if gaps are still evident.
    * If SUT refactoring was done, ensure it's clean and doesn't introduce issues.
* **Constraint (Reiteration):** Avoid changing production code (`Zarichney.Server/`) unless it was a pre-approved or minor refactoring strictly for testability.

**Step 8: Ensure All Project Tests Pass**
* **Action:** Before finalizing, run all unit tests (`dotnet test --filter "Category=Unit"`) and all relevant integration tests (e.g., `dotnet test --filter "Category=Integration"`) for the entire project, or at least those potentially impacted by your changes, to catch any unintended regressions.

**Step 9: Update Documentation**
* **Action:** If any significant refactoring of the SUT was performed that alters its public contract, assumptions, or key behaviors, update the SUT's corresponding `README.md` file as per `../../Docs/Standards/DocumentationStandards.md`.
* **Action:** If the testing strategy for the specific module warrants a note in its README (e.g., particular complex scenarios now covered), add it.

**Step 10: Commit and Create Pull Request**
* **Action:** Commit all new/modified production code (if any), test code, and documentation updates.
* **Action:** Adhere to commit message conventions outlined in `../../Docs/Standards/TaskManagementStandards.md`.
* **Action:** Create a Pull Request, clearly describing the work done, the SUTs addressed, the improvement in coverage (qualitatively or quantitatively if possible), and any refactoring undertaken. Link to the original task/issue.

## 4. Tools for Measuring Coverage

* **Coverlet:** This is the primary tool used for generating code coverage information for .NET projects.
* **Local Report Generation:**
    * Execute tests with coverage collection enabled:
      ```bash
      dotnet test --collect:"XPlat Code Coverage"
      ```
    * This command will generate a `coverage.cobertura.xml` file (typically in a `TestResults` subdirectory).
* **Viewing Reports:**
    * To view coverage reports in a human-readable HTML format, you can use a tool like **ReportGenerator**:
      ```bash
      # Install if you haven't: dotnet tool install -g dotnet-reportgenerator-globaltool
      reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:coveragereport" "-reporttypes:Html"
      ```
    * Open the `index.html` in the `coveragereport` directory in a browser.
* **CI/CD:** The GitHub Actions workflow is configured to generate and publish coverage reports automatically (see `../TechnicalDesignDocument.md` Section 11).

## 5. Handling Low Coverage in Existing Code

* **Prioritization:** When tackling areas of existing low coverage, prioritize based on:
    * Criticality of the business logic.
    * Complexity and likelihood of bugs in the module.
    * Recently changed or historically bug-prone areas.
* **Task Assignment:** Focused test coverage tasks will be created using the `../../Docs/Templates/GHTestCoverageTask.md` template, clearly defining the scope for each task.

## 6. AI Coder Specific Instructions

* **Strict Adherence:** AI Coders **must** strictly follow all linked testing standards and development guides (`UnitTestCaseDevelopment.md`, `IntegrationTestCaseDevelopment.md`).
* **Contextual Understanding:** Before writing tests, ensure a thorough understanding of the SUT by reviewing its code, its README, and any associated diagrams. Do not write tests based on assumptions.
* **Coverage Focus:** The primary goal is to cover *uncovered executable lines and branches* within the SUT as identified by the coverage report.
* **Reporting:** In your task completion report or Pull Request description:
    * List the SUTs (classes/methods) for which tests were added/updated.
    * Briefly describe the types of scenarios covered by the new tests.
    * If possible, state the previous and new coverage percentage for the SUTs you worked on (based on local reports).
    * Detail any refactoring performed on the SUT for testability, explaining the rationale.
    * Confirm all new and existing relevant tests pass.

## 7. Definition of Done for a Test Coverage Task

A test coverage enhancement task is considered "Done" when:
* New unit and/or integration tests have been written for the specified SUT(s), covering previously uncovered code paths.
* All new and existing tests related to the SUT (and potentially the broader project) pass successfully.
* Code coverage for the targeted SUT(s) has demonstrably increased and is verified with a local coverage report.
* All new test code strictly adheres to the standards defined in `../../Docs/Standards/UnitTestCaseDevelopment.md` or `../../Docs/Standards/IntegrationTestCaseDevelopment.md`.
* Production code was refactored for testability (if applicable and approved) without introducing regressions, and such refactoring adheres to `../../Docs/Standards/CodingStandards.md`.
* Relevant documentation (e.g., SUT's README) has been updated if necessary.
* Code, tests, and documentation changes are committed and a Pull Request is created with a clear description.

---
