# Task Management & Git Standards


**Version:** 1.2
**Last Updated:** 2025-09-07

## 1. Purpose and Scope

This document defines the mandatory standards for associating development tasks with GitHub Issues and managing Git workflow (branching, committing, pull requests) within the AI Coder workflow. Adherence ensures traceability, consistency, and facilitates automated processes.

These standards **MUST** be consulted and followed by AI Coders during task execution, as instructed by the AI Coder Prompt Template.

* **Relationship to Other Standards:**
    * This document integrates with **[`./GitHubLabelStandards.md`](./GitHubLabelStandards.md)** to provide comprehensive GitHub Issue labeling requirements for task categorization and project management.
    * Label application requirements defined in this document ensure alignment with epic coordination, progressive testing phases, and automation workflows.

## 2. GitHub Issue Usage for AI Tasks (Mandatory)

* **Purpose:** GitHub Issues serve as the **canonical source of truth** for defining *what* needs to be done, *why* it's needed, and the **acceptance criteria** for verifying completion. They are the primary tool for task tracking and project management.
* **Task Definition:** All tasks intended for delegation to an AI Coder **MUST** have a corresponding GitHub Issue created.
* **Templates:** Use the specific GitHub Issue templates located in `/Docs/Templates/` to ensure necessary information is captured:
    * [`/Docs/Templates/GHCoderTaskTemplate.md`](../Templates/GHCoderTaskTemplate.md): For general coding tasks (features, fixes, refactors).
    * [`/Docs/Templates/GHTestCoverageTaskTemplate.md`](../Templates/GHTestCoverageTaskTemplate.md): For tasks focused solely on increasing test coverage.
* **Label Requirements:** All GitHub Issues **MUST** be labeled according to **[`./GitHubLabelStandards.md`](./GitHubLabelStandards.md)** with mandatory label combinations:
    * **Required:** Exactly one `type:`, `priority:`, `effort:`, and at least one `component:` label
    * **Epic Tasks:** Must include relevant `epic:` label for coordination
    * **Coverage Work:** Must include appropriate `coverage: phase-X` label
    * **CI Work:** Must include relevant `automation:` label for environment context
* **Information Division:**
    * **GitHub Issue:** Contains the **Goal/Why**, **Requirements**, **Specific Objective**, and **Acceptance Criteria**. May contain high-level implementation notes.
    * **AI Coder Prompt:** Contains the **Execution Details (How/Where)**, links to **Technical Context** (READMEs, Standards, Workflow Steps), and a **link back to this GitHub Issue**. The prompt summarizes the objective but does *not* duplicate detailed background or acceptance criteria from the Issue.
* **AI Coder Interaction:**
    * The AI Coder **MUST** reference the linked GitHub Issue (provided in the prompt) to fully understand the task goal and acceptance criteria (Step 1 of all workflows).
    * The AI Coder **MUST** reference the Issue ID in commit messages and Pull Requests (see sections below).
    * The AI Coder **DOES NOT** update the Issue status directly (this is currently handled manually by the orchestrator).

## 3. Task Association (via Prompt)

* **Requirement:** Every AI Coder Prompt **MUST** include a direct link to the associated GitHub Issue ID in the "Associated Task" section. This links the execution context (prompt) to the task definition (issue).

## 4. Branching Strategy (Mandatory)

* **Requirement:** AI Coders **MUST** create a feature or test branch for each assigned task, unless explicitly instructed otherwise.
* **Base Branch:** Branches **MUST** be created from the base branch specified in the AI Coder prompt (typically `develop` or `main`).
* **Naming Convention:** Branch names **MUST** follow the format based on task type:
    * Coding Tasks: `feature/issue-{ISSUE_ID}-{brief-description}`
    * Testing Tasks: `test/issue-{ISSUE_ID}-{brief-description}`
    * `{ISSUE_ID}`: The numeric ID of the associated GitHub Issue (e.g., `123`).
    * `{brief-description}`: A short (2-5 words), lowercase, hyphen-separated description (e.g., `add-recipe-service`, `recipeservice-coverage`).
    * **Example (Coding):** `feature/issue-123-add-recipe-service`
    * **Example (Testing):** `test/issue-789-recipeservice-coverage`
* **Implementation:** Use standard Git commands within the relevant AI Coder Workflow steps file (e.g., `StandardWorkflow.md`):
    ```bash
    git checkout [BASE_BRANCH_FROM_PROMPT]
    git pull origin [BASE_BRANCH_FROM_PROMPT] # Ensure base is up-to-date
    git checkout -b [branch-name] # e.g., feature/issue-123-add-recipe-service
    ```

## 5. Commit Message Standard (Mandatory)

* **Requirement:** Commit messages **MUST** follow the Conventional Commits specification (v1.0.0).
* **Format:**
    ```
    <type>[optional scope]: <description>

    [optional body]

    [optional footer(s)]
    ```
    * **Type:** Must be one appropriate for the change (`feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`). Use `test:` for commits related to test coverage tasks. Types should align with the primary `type:` label applied to the associated GitHub Issue.
    * **Description:** Concise summary of the change in present tense.
    * **Body (Optional):** Provides additional context or reasoning.
    * **Footer (Mandatory Issue Link):** **MUST** include a reference to the associated GitHub Issue ID. Use keywords like `Closes #XXX`, `Fixes #XXX`, or `Refs #XXX`.
        * **Example Footer:** `Refs #123`
* **Implementation:** Use standard Git commands within the relevant AI Coder Workflow steps file:
    ```bash
    git add .
    git commit -m "feat: Implement recipe search functionality (#123)" # Example
    git commit -m "test: Increase unit test coverage for RecipeService (#789)" # Example
    ```
* **Granularity:** Commit related changes (code, tests, docs) together logically. Avoid overly large commits.

## 6. Pull Request Standards (Mandatory)

### 6.1 Standard Pull Requests
* **Requirement:** Upon completing all steps for the task, the AI Coder **MUST** create a Pull Request (PR) using the GitHub CLI (`gh`).
* **Target Branch:** The PR **MUST** target the branch specified in the AI Coder prompt (typically `develop` or `main`).
* **Title:** PR titles **MUST** follow the Conventional Commit format, including the Issue ID. The type should align with the primary `type:` label from the associated GitHub Issue.
    * **Format:** `<type>: <Brief description> (#ISSUE_ID)`
    * **Example (Coding):** `feat: Implement recipe search (#123)`
    * **Example (Testing):** `test: Increase RecipeService coverage (#789)`
    * **Example (Security):** `fix: Resolve command injection vulnerability (#456)`
* **Body:** PR bodies **MUST** include:
    * A link that closes/references the associated GitHub Issue (e.g., `Closes #123.`).
    * A brief summary of the changes made.
    * **Label Validation:** Confirm that PR changes align with GitHub Issue labels, particularly `component:`, `epic:`, and `automation:` labels.
* **Implementation:** Use the GitHub CLI (`gh`) within the relevant AI Coder Workflow steps file. Ensure `gh` is authenticated (environment prerequisite).
    ```bash
    # Push the feature branch first
    git push origin [branch-name] # e.g., feature/issue-123-add-recipe-service

    # Create the Pull Request
    # Example: gh pr create --base develop --title "feat: Implement recipe search (#123)" --body "Closes #123. Implements backend search functionality."
    gh pr create --base [TARGET_BRANCH_FROM_PROMPT] --title "<type>: <Brief description> (#ISSUE_ID)" --body "Closes #{ISSUE_ID}. [Summary of changes]"
    ```
* **Output:** The URL of the created Pull Request is required as part of the task output specified in the AI Coder Prompt.

### 6.2 Orchestrator Pull Request Standards
#### **Orchestrator Consolidation PRs**
* **Title Format:** `epic: consolidate coverage improvements batch-[N] (#EPIC_ID)`
* **Labels:** `coverage`, `epic-subtask`, `automation: orchestrator`, `priority: medium`
* **Body Requirements:**
  - Individual PRs consolidated (numbers, titles, authors)
  - Conflict resolution summary and AI-powered decisions
  - Coverage impact assessment and framework enhancements
  - Quality validation results (build status, test results, coverage metrics)
  - Recovery branches created (if conflicts required manual intervention)

#### **Orchestrator Commit Conventions**
* `epic: consolidate coverage improvements batch-[N] (#EPIC_ID)` - Consolidation commits
* `test: resolve framework conflicts in [area] (#EPIC_ID)` - AI conflict resolution
* `refactor: consolidate test builders from multiple PRs (#EPIC_ID)` - Framework consolidation

## 7. Epic Branch Strategy (For Long-Term Initiatives)

### 7.1 Epic Branch Purpose
For long-term initiatives spanning multiple tasks and extending over months, an **epic branch strategy** provides coordination and isolation:

* **Epic Branches:** Long-running branches for major initiatives (e.g., `epic/testing-coverage-to-90`)
* **Base Branch:** Epic branches are created from and periodically updated with `develop`
* **Task Branches:** Individual tasks create branches off the epic branch, not `develop`
* **Integration:** Epic branches are merged to `develop` in chunks by the product owner

### 7.2 Epic Branch Naming Convention
* **Format:** `epic/[initiative-name]`
* **Examples:**
  * `epic/testing-coverage-to-90` - Backend coverage improvement initiative
  * `epic/api-v2-migration` - Major API version migration
  * `epic/performance-optimization` - System-wide performance improvements

### 7.3 Epic Branch Workflow

#### **Epic Branch Creation (Product Owner)**
```bash
# Create epic branch from develop
git checkout develop
git pull origin develop
git checkout -b epic/[initiative-name]
git push origin epic/[initiative-name]
```

#### **Epic Branch Maintenance (AI Agents)**
Before creating task branches, ensure epic branch is current:

```bash
# Update epic branch with latest develop changes
git checkout epic/[initiative-name]
git fetch origin
git merge origin/develop --no-edit
git push origin epic/[initiative-name]
```

#### **Task Branch Creation (AI Agents)**
Task branches are created **off the epic branch**, not develop:

```bash
# Create task branch from epic branch
git checkout epic/[initiative-name]
git pull origin epic/[initiative-name]
git checkout -b tests/issue-[ISSUE_ID]-[description]-[timestamp]
```

### 7.4 Epic Branch Naming Conventions
For tasks within an epic, use modified naming conventions:

#### **Individual Task Branches**
* **Testing Tasks:** `tests/issue-[EPIC_ISSUE_ID]-[description]-[timestamp]`
* **Feature Tasks:** `feature/issue-[EPIC_ISSUE_ID]-[description]-[timestamp]`
* **Examples:**
  * `tests/issue-94-cookbook-service-1628712345`
  * `feature/issue-95-api-versioning-1628712400`

#### **Orchestrator Staging Branches**
* **Coverage Epic Staging:** `epic/merge-staging-[timestamp]`
* **Examples:**
  * `epic/merge-staging-1757256239` (Coverage Epic Merge Orchestrator staging)
  * `epic/merge-staging-manual-20250907` (Manual staging with date identifier)

#### **Branch Lifecycle Management**
* **Task Branches:** Created by individual agents, target epic branch
* **Staging Branches:** Temporary branches created by orchestrator for safe consolidation
* **Epic Branches:** Long-lived branches accumulating consolidated work
* **Cleanup:** Staging branches automatically removed after successful consolidation

### 7.5 Epic Pull Request Management

#### **Task Pull Requests**
Task PRs target the **epic branch**, not develop:

```bash
# Create PR against epic branch
gh pr create --base epic/[initiative-name] \
  --title "[type]: [Description] (#EPIC_ISSUE_ID)" \
  --body "Epic contribution: [Description]. Refs #[EPIC_ISSUE_ID]."
```

#### **Orchestrator Consolidation Pull Requests**
Orchestrator consolidates multiple task PRs into single PRs targeting epic branch:

```bash
# Orchestrator creates consolidation PR automatically
# Title pattern: "epic: consolidate coverage improvements [batch-N] (#EPIC_ID)"
# Labels: coverage, epic-subtask, automation: orchestrator, priority: medium
# Base: epic/testing-coverage-to-90
# Head: epic/merge-staging-[timestamp]
```

**Consolidation PR Structure:**
- **Title:** `epic: consolidate coverage improvements batch-[N] (#94)`
- **Body:** Comprehensive summary including:
  - Individual PRs consolidated (numbers, titles, authors)
  - Conflict resolution summary and AI decisions
  - Coverage impact and framework enhancements
  - Quality validation results (build, tests, coverage)
  - Recovery branches created (if any conflicts occurred)
- **Labels:** `coverage`, `epic-subtask`, `automation: orchestrator`, `priority: medium`

#### **Epic Integration Pull Requests (Product Owner)**
Periodically, the product owner creates integration PRs from epic branch to develop:

```bash
# Create epic integration PR
gh pr create --base develop \
  --title "epic: [Initiative name] - Integration batch [N]" \
  --body "Integrates completed work from epic/[initiative-name]. Includes: [summary of completed tasks]"
```

### 7.6 Coverage Epic Merge Orchestrator Standards

#### **Orchestrator Execution Standards**
* **Trigger Conditions:** 3+ open PRs targeting epic branch with coverage labels
* **Batch Size:** Default 8 PRs, maximum 50 per execution
* **Dry Run Requirement:** Always execute dry run before production consolidation
* **Frequency:** Weekly consolidation recommended, daily for high-velocity periods

#### **Staging Branch Management**
* **Creation:** `epic/merge-staging-[timestamp]` from current epic branch head
* **Processing:** Sequential PR merging with conflict detection and AI resolution
* **Validation:** Build success and test pass rate validation before consolidation
* **Cleanup:** Automatic removal after successful consolidation or failure recovery

#### **Conflict Resolution Standards**
* **AI Resolution:** Powered by specialized conflict resolution prompt
* **Recovery Branches:** `epic/recovery-conflicts-[timestamp]` for unresolvable conflicts
* **Safety Constraints:** Production changes limited to testability improvements only
* **Escalation:** Complex conflicts escalated to manual review

#### **Quality Gates**
* **Pre-Consolidation:** All individual PRs must pass build and basic validation
* **Post-Consolidation:** Comprehensive test suite execution with coverage validation
* **AI Sentinel Integration:** Full AI analysis pipeline applies to consolidation PRs
* **Standards Compliance:** All existing standards apply to consolidated changes

### 7.7 Automated Epic Execution (CI Environment)

#### **Multi-Agent Coordination**
For automated epic execution (e.g., 4 agents per day):

* **Epic Branch Updates:** Each agent must update epic branch from develop before starting
* **Conflict Prevention:** Agents select different modules/files to minimize conflicts
* **Timestamp Naming:** Include timestamp in task branch names for uniqueness
* **Parallel Execution:** Multiple agents can work simultaneously on different areas

#### **CI Environment Epic Workflow**
```bash
# Step 1: Prepare epic branch
git checkout develop && git pull origin develop
git checkout epic/[initiative-name] || git checkout -b epic/[initiative-name]
git merge develop --no-edit
git push origin epic/[initiative-name]

# Step 2: Create unique task branch
TIMESTAMP=$(date +%s)
TASK_BRANCH="tests/issue-[EPIC_ID]-[area]-${TIMESTAMP}"
git checkout -b $TASK_BRANCH

# Step 3: Implement and create PR against epic branch
# [Implementation work]
gh pr create --base epic/[initiative-name] --title "[type]: [Description] (#EPIC_ID)"
```

### 7.8 Epic Label Integration

Epic branches require coordinated labeling per **[`./GitHubLabelStandards.md`](./GitHubLabelStandards.md)**:

#### **Epic Parent Issues**
Epic parent issues (e.g., Backend Coverage Epic #94) **MUST** include:
- `effort: epic` - Indicates multi-month initiative
- `status: epic-planning` or `status: epic-active` - Epic lifecycle status
- Relevant `epic:` category label (e.g., `epic: coverage-90`)
- `priority: high` or `priority: medium` based on business impact

#### **Epic Sub-Task Issues**  
All tasks within an epic **MUST** include:
- `type: epic-task` - Identifies as epic sub-component
- Parent epic's `epic:` label for coordination
- Appropriate `coverage: phase-X` for coverage-related work
- `automation:` labels for CI environment context
- Standard `component:` and `effort:` labels

#### **Example Epic Labeling**
```
Epic Parent (#94): Backend Test Coverage to 90%
Labels: effort: epic, status: epic-active, epic: coverage-90, priority: high

Epic Task (#123): Increase CookbookService unit test coverage  
Labels: type: epic-task, epic: coverage-90, coverage: phase-2, 
        component: testing, component: api, automation: ci-ready,
        effort: medium, priority: medium
```

### 7.9 Epic Branch Benefits

* **Coordination:** Multiple developers/agents can work on related tasks without conflicts
* **Integration Control:** Product owner controls when epic work integrates to develop
* **Progress Tracking:** Epic branch shows cumulative progress toward initiative goals
* **Risk Management:** Epic work is isolated from develop until ready for integration
* **Batch Processing:** Product owner can review and merge multiple related PRs efficiently
* **Label Alignment:** Systematic labeling enables automated epic progress tracking and AI agent coordination
* **Orchestrator Integration:** Automated consolidation reduces manual PR management overhead while maintaining quality

## 8. Quality Standards & Orchestrator Integration

### 8.1 Standard Quality Gates
All tasks and pull requests **MUST** meet these quality requirements:

* **Build Success:** All changes must compile without errors
* **Test Pass Rate:** 100% of executable tests must pass
* **Standards Compliance:** Code must adhere to all project coding standards
* **Documentation Updates:** Any interface or contract changes require documentation updates
* **Label Validation:** PR changes must align with GitHub Issue labels

### 8.2 Orchestrator Quality Standards
#### **Consolidation Integrity Requirements**
* **Value Preservation:** All individual PR contributions must be preserved in consolidated result
* **Behavior Preservation:** Consolidated changes must maintain identical functional behavior
* **Test Coverage Progression:** Consolidated PR must advance coverage toward 90% epic target
* **Framework Coherence:** Test framework improvements must be logically cohesive and maintainable
* **Documentation Consistency:** All consolidated changes must maintain project documentation standards

#### **AI-Powered Conflict Resolution Standards**
* **Conflict Detection:** Automated detection of merge conflicts and framework overlaps
* **Resolution Quality:** AI resolutions must preserve intent from all contributing PRs
* **Safety Constraints:** Production code changes limited to testability improvements only
* **Recovery Protocols:** Complex conflicts create recovery branches for manual review
* **Validation Requirements:** All AI resolutions subject to comprehensive build and test validation

#### **Batch Processing Quality Gates**
* **Pre-Consolidation Validation:** Individual PRs must pass build and basic quality checks
* **Sequential Integration:** PRs merged in dependency-aware order to prevent cascading failures
* **Intermediate Checkpoints:** Build and test validation after each PR integration
* **Rollback Capability:** Failed consolidations create recovery branches preserving all work
* **Final Validation:** Complete test suite execution with coverage impact analysis

### 8.3 Epic Integration Quality Standards
#### **Coverage Epic Specific Requirements**
* **Coverage Metrics:** Each consolidation must demonstrate measurable coverage improvement
* **Framework Enhancements:** Test infrastructure improvements must follow established patterns
* **Service Coverage Balance:** Coverage improvements distributed across multiple service areas
* **Integration Testing:** Consolidated changes must not break existing integration test suites
* **Epic Progression:** Each consolidation advances overall 90% coverage goal with quantifiable progress

#### **Multi-Agent Coordination Quality**
* **Context Preservation:** Individual agent contributions clearly identifiable in consolidated result
* **Attribution Maintenance:** Original authors properly credited in consolidation PR descriptions
* **Decision Transparency:** AI conflict resolution decisions documented for future reference
* **Team Communication:** Consolidation process communicates effectively with all team members
* **Knowledge Transfer:** Consolidated changes preserve learning and insights from individual contributions

---