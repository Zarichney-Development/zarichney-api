---
name: AI Test Coverage Task
about: Define a task focused on increasing automated test coverage for specific modules/classes.
title: 'test: Increase Coverage for [Module/Class Name]'
labels: 'ai-task, type:testing' # Add relevant module:X labels
assignees: '' # Assign to the human orchestrator

---

## 1. Overall Goal

Improve the robustness and maintainability of the codebase by increasing automated test coverage for existing production code, ensuring adherence to testing standards.

## 2. Specific Task Objective

*(Clearly state the specific goal. E.g., "Increase unit test coverage for the `RecipeService.cs` class, focusing on methods X, Y, and Z.", "Add integration tests for the AuthController login and refresh token flows.", "Achieve >80% branch coverage for the `PaymentService`.")*

## 3. Target Area(s)

*(List the specific production code files/classes/modules that are the primary focus for coverage improvement.)*

- `/Zarichney.Server/[module]/[ClassName.cs]`
- `/Zarichney.Server/[module]/...`

## 4. Acceptance Criteria

*(Provide a checklist of verifiable conditions. How will we know this specific task is done correctly?)*

- [ ] New unit and/or integration tests are added to `/Zarichney.Server.Tests/` covering previously untested logic/branches in the target area(s).
- [ ] All new tests pass consistently (`dotnet test`).
- [ ] All existing tests continue to pass (`dotnet test`).
- [ ] New tests adhere strictly to `/Docs/Standards/TestingStandards.md`.
- [ ] *(Optional)* Code coverage metrics (if measured) show an increase for the target area(s).
- [ ] Production code (`/Zarichney.Server/`) remains logically unchanged (unless essential refactoring for testability was explicitly approved).
- [ ] A Pull Request containing only test code additions/updates (and approved testability refactors, if any) is created, following `/Docs/Standards/