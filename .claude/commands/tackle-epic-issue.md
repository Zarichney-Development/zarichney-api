---
description: "Tackle Epic #291 issue with spec review, iterative breakdown, and epic branching"
argument-hint: "<issue-number>"
category: "epic"
requires-skills: ["working-directory-coordination", "documentation-grounding", "core-issue-focus"]
---

# Tackle Epic Issue - Epic #291 Specialized Command

Execute a GitHub issue within Epic #291 context with full specification awareness, proper epic branching strategy, and sectioned iteration approach.

## Purpose
This command orchestrates the execution of Epic #291 subtasks using:
- Epic specification review from `Docs/Specs/epic-291-skills-commands/`
- Iterative task breakdown following `implementation-iterations.md`
- Epic branch strategy with section subbranches
- Multi-agent coordination per CLAUDE.md protocols
- ComplianceOfficer validation after section completion (not per subtask)

## Usage
```bash
/tackle-epic-issue <issue-number>
```

## Arguments
- `<issue-number>` (required): GitHub issue number (e.g., 311, 310, 307, etc.)

## What this command does:

### 1. Context Loading & Epic Review
- Loads Epic #291 from GitHub issue
- Reads all specification files from `Docs/Specs/epic-291-skills-commands/`:
  - `README.md` - Master epic specification
  - `implementation-iterations.md` - Detailed iteration breakdown
  - `skills-catalog.md` - All 8 skills specifications
  - `commands-catalog.md` - All 4 commands specifications
  - `documentation-plan.md` - Documentation reorganization plan
- Reads assigned GitHub issue to understand specific task
- Identifies which iteration section the issue belongs to (1-5)

### 2. Epic Branch Preparation
- Verifies epic branch exists: `epic/skills-commands-291` (created from main)
- Determines section branch based on issue number:
  - Issues #311-308 → `section/iteration-1` (Foundation)
  - Issues #307-304 → `section/iteration-2` (Meta-Skills & Commands)
  - Issues #303-299 → `section/iteration-3` (Documentation Alignment)
  - Issues #298-295 → `section/iteration-4` (Agent Refactoring)
  - Issues #294-292 → `section/iteration-5` (Integration & Validation)
- Creates section branch from epic branch if not exists
- Ensures section branch is up-to-date with epic branch

### 3. Task Breakdown & Planning
- Analyzes issue dependencies (what blocks it, what it blocks)
- Breaks down into subtasks following Epic #291 patterns:
  - **Skills:** metadata.json creation, SKILL.md writing, resources/ organization
  - **Commands:** frontmatter definition, usage examples, argument handling
  - **Documentation:** comprehensive guides following templates
  - **Agent Refactoring:** skill extraction, context optimization, validation
  - **Validation:** testing, benchmarking, compliance checking
- Creates implementation plan using CLAUDE.md delegation protocols:
  - Core issue identification (specific technical problem)
  - Agent selection (specialists vs. primary agents)
  - Intent recognition (analysis vs. implementation)
  - Quality gates planning
- Outputs plan to working directory for transparency

### 4. Iterative Agent Execution
- Executes subtasks sequentially following the plan
- Engages appropriate agents per CLAUDE.md delegation protocols:
  - **PromptEngineer:** For skills/commands creation (owns .claude/ directory)
  - **DocumentationMaintainer:** For guide creation (owns docs)
  - **TestEngineer:** For validation testing
  - **WorkflowEngineer:** For CI/CD integration
  - **CodeChanger:** For file creation/modification
  - **Specialists:** As needed for domain expertise
- After each agent completion:
  - Reviews deliverables against acceptance criteria
  - Commits to section branch with conventional commit message
  - Updates working directory with progress
  - **Does NOT invoke ComplianceOfficer** (waits for section completion)

### 5. Section Completion & Validation
After all subtasks in the section complete:
- Runs comprehensive validation (build success, tests passing)
- **NOW invokes ComplianceOfficer** for section-level review
- Addresses any compliance issues found
- Pushes section branch to remote
- Creates Pull Request against epic branch:
  - **Title:** `epic: complete Iteration N - [section name] (#291)`
  - **Body:** Lists all completed subtasks with issue links
  - **Labels:** `type: epic-task`, `priority: high`, `status: review`
- Reports PR URL and completion status

### 6. Handoff Documentation
- Updates working directory with:
  - Section completion summary
  - Deliverables checklist
  - Integration notes for next section
  - Outstanding dependencies that must be resolved
- Provides clear link to created PR for review
- Documents any blockers or issues for next iteration

## Branch Strategy

### Branch Hierarchy
```
main
└── epic/skills-commands-291 (epic branch - created from main)
    ├── section/iteration-1 (Foundation)
    │   └── Multiple commits: #311, #310, #309, #308
    ├── section/iteration-2 (Meta-Skills & Commands)
    │   └── Multiple commits: #307, #306, #305, #304
    ├── section/iteration-3 (Documentation Alignment)
    │   └── Multiple commits: #303, #302, #301, #300, #299
    ├── section/iteration-4 (Agent Refactoring)
    │   └── Multiple commits: #298, #297, #296, #295
    └── section/iteration-5 (Integration & Validation)
        └── Multiple commits: #294, #293, #292
```

### Commit Strategy
- **One commit per subtask** within the issue
- **Conventional commit messages** referencing issue number
- **All commits on section branch** (not separate feature branches)
- **Section PR after iteration complete** for review

### Example Commits (Section iteration-1)
```bash
feat: create working-directory-coordination skill metadata (#311)
feat: implement working-directory-coordination SKILL.md (#311)
feat: add working-directory-coordination resources (#311)
test: validate working-directory-coordination skill loading (#311)
docs: document working-directory-coordination integration (#311)
```

### Epic Branch Pruning
Before final merge to main:
- Section commits will be squashed/reorganized into logical iteration commits
- Maintains clean, reviewable history
- Preserves work history in section branches for reference

## Quality Gates

### Per-Subtask Quality
- Deliverables match acceptance criteria from issue
- Files follow project standards (CodingStandards.md, DocumentationStandards.md)
- Conventional commit messages with issue references
- Build succeeds with zero warnings
- Tests pass (if applicable to subtask)

### Per-Section Quality (ComplianceOfficer Validation)
- All section subtasks complete
- Comprehensive build validation (zero warnings)
- All executable tests passing (100% pass rate)
- Documentation complete and cross-referenced
- Standards compliance verified
- Working directory artifacts properly managed
- AI Sentinel readiness confirmed

### Per-Iteration Quality (PR Review)
- Section PR approved before next iteration begins
- Integration with epic branch successful
- No breaking changes to existing functionality
- Acceptance criteria from all section issues met

## Integration Points

### Specification Files
- `Docs/Specs/epic-291-skills-commands/README.md` - Epic objectives and architecture
- `Docs/Specs/epic-291-skills-commands/implementation-iterations.md` - Detailed task breakdown
- `Docs/Specs/epic-291-skills-commands/skills-catalog.md` - Skills specifications
- `Docs/Specs/epic-291-skills-commands/commands-catalog.md` - Commands specifications
- `Docs/Specs/epic-291-skills-commands/documentation-plan.md` - Documentation requirements

### Orchestration Protocols
- `CLAUDE.md` - Core orchestration and delegation protocols
- `Docs/Standards/TaskManagementStandards.md` - Epic branching strategy (Section 7)
- `Docs/Standards/GitHubLabelStandards.md` - Proper issue/PR labeling
- `.claude/agents/` - Agent definitions and capabilities

### Quality Systems
- ComplianceOfficer validation (section-level)
- AI Sentinels review (PR-level, automatic)
- Working directory progress tracking
- Build and test validation gates

## Usage Examples

### Execute Foundation Task (Iteration 1)
```bash
/tackle-epic-issue 311
# Creates section/iteration-1 branch
# Implements working-directory-coordination skill
# Commits all subtasks to section branch
# After #311, #310, #309, #308 complete → creates section PR
```

### Execute Meta-Skills Task (Iteration 2)
```bash
/tackle-epic-issue 307
# Creates section/iteration-2 branch
# Implements agent-creation and skill-creation meta-skills
# Commits all subtasks to section branch
# After #307, #306, #305, #304 complete → creates section PR
```

### Execute Documentation Task (Iteration 3)
```bash
/tackle-epic-issue 303
# Creates section/iteration-3 branch
# Creates SkillsDevelopmentGuide and CommandsDevelopmentGuide
# Commits all subtasks to section branch
# After #303, #302, #301, #300, #299 complete → creates section PR
```

### Execute Agent Refactoring (Iteration 4)
```bash
/tackle-epic-issue 298
# Uses section/iteration-4 branch
# Refactors 5 high-impact agents with skill references
# Commits all refactoring work to section branch
# After #298, #297, #296, #295 complete → creates section PR
```

### Execute Integration Task (Iteration 5)
```bash
/tackle-epic-issue 294
# Uses section/iteration-5 branch
# Optimizes CLAUDE.md and runs comprehensive testing
# Commits all integration work to section branch
# After #294, #293, #292 complete → creates section PR
```

## Error Handling & Validation

### Issue Not Found
```
❌ Error: Issue #XXX not found in repository
→ Verify issue number and try again
→ Epic #291: https://github.com/Zarichney-Development/zarichney-api/issues/291
```

### Issue Not Part of Epic #291
```
❌ Error: Issue #XXX is not part of Epic #291
→ This command is specialized for Epic #291 subtasks only
→ Valid issue range: #292-311 (20 subtasks across 5 iterations)
→ Use standard workflow for non-epic issues
```

### Dependencies Not Met
```
⚠️ Warning: Issue #XXX has unmet dependencies
→ Blocking issues that must complete first:
  - #YYY: [Issue title]
  - #ZZZ: [Issue title]
→ Complete blocking issues before executing this task
```

### Branch Conflicts
```
❌ Error: Section branch has conflicts with epic branch
→ Resolution steps:
  1. Checkout section branch: git checkout section/iteration-N
  2. Merge epic branch: git merge epic/skills-commands-291
  3. Resolve conflicts manually
  4. Re-run command after resolution
```

### ComplianceOfficer Validation Failure
```
❌ Compliance Failure: Section validation did not pass
→ Issues identified:
  [List of compliance issues from ComplianceOfficer]
→ Action required:
  1. Address compliance issues
  2. Re-run validation: invoke ComplianceOfficer
  3. Fix and commit any required changes
  4. Command will resume section PR creation
```

### Build or Test Failures
```
❌ Quality Gate Failure: Build/Tests did not pass
→ Build errors: [error details]
→ Test failures: [failing tests]
→ Action required:
  1. Fix build/test issues
  2. Commit fixes to section branch
  3. Re-run validation
  4. Command will resume after all gates pass
```

## Output Format

### Execution Start
```
🎯 EPIC #291 ISSUE EXECUTION

Epic: Agent Skills & Slash Commands Integration
Issue: #311 - Working Directory Coordination Skill
Iteration: 1 (Foundation)
Section: Iteration 1 - Core Skills & Templates

📂 BRANCH STRATEGY:
Epic Branch: epic/skills-commands-291
Section Branch: section/iteration-1 (will be created)
PR Target: epic/skills-commands-291 ← section/iteration-1

📋 SPECIFICATION REVIEW:
✅ Epic specification loaded
✅ Implementation iterations loaded
✅ Skills catalog loaded
✅ Issue dependencies analyzed

🎯 TASK BREAKDOWN:
[Detailed list of subtasks with assigned agents]
```

### Execution Progress
```
🔨 EXECUTION PROGRESS:

✅ Subtask 1: Create skill metadata.json
   Agent: PromptEngineer
   Commit: feat: create working-directory-coordination metadata (#311)

✅ Subtask 2: Implement SKILL.md
   Agent: PromptEngineer
   Commit: feat: implement working-directory-coordination SKILL.md (#311)

🔄 Subtask 3: Organize resources/
   Agent: PromptEngineer
   Status: In progress...
```

### Section Completion
```
✅ SECTION COMPLETION

All Iteration 1 Tasks Complete:
- ✅ #311: Working Directory Coordination
- ✅ #310: Documentation Grounding & Core Issue Focus
- ✅ #309: GitHub Issue Creation Skill
- ✅ #308: Validation Framework & Templates

🔍 COMPLIANCE VALIDATION:
Agent: ComplianceOfficer
Status: ✅ PASSED
- Build: Success (zero warnings)
- Tests: All passing (100% pass rate)
- Documentation: Complete and cross-referenced
- Standards: Fully compliant
- Working Directory: Properly managed

📦 SECTION PR CREATED:
Title: epic: complete Iteration 1 - Foundation (#291)
URL: https://github.com/Zarichney-Development/zarichney-api/pull/XXX
Base: epic/skills-commands-291
Head: section/iteration-1
Status: Ready for review

📊 DELIVERABLES:
Created:
- .claude/skills/coordination/working-directory-coordination/ (complete)
- .claude/skills/documentation/documentation-grounding/ (complete)
- .claude/skills/coordination/core-issue-focus/ (complete)
- .claude/skills/github/github-issue-creation/ (complete)
- Scripts/validate-skills-metadata.sh
- Scripts/validate-commands-frontmatter.sh
- Docs/Templates/SkillTemplate.md
- Docs/Templates/CommandTemplate.md

Modified:
- .git/hooks/pre-commit (validation hooks)
- .github/workflows/validate-skills-commands.yml (CI validation)

🔗 NEXT ACTIONS:
1. Review and approve Section PR #XXX
2. Merge section/iteration-1 → epic/skills-commands-291
3. Ready to begin Iteration 2 (Issues #307-304)
4. Blocking dependencies: None (foundation complete)
```

## Perfect for:
- Systematic execution of Epic #291 subtasks
- Maintaining proper epic branch strategy with sections
- Ensuring specification compliance throughout implementation
- Coordinating multiple agents per CLAUDE.md protocols
- Section-level quality validation (not per-subtask overhead)
- Creating reviewable PRs after each iteration
- Maintaining clean commit history with logical grouping
- Preparing epic for final pruning before main merge
