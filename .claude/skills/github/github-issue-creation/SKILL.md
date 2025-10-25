---
name: github-issue-creation
description: Streamline GitHub issue creation with automated context collection, template application, and proper labeling. Use when creating feature requests, documenting bugs, proposing architectural improvements, tracking technical debt, or creating epic milestones.
---

# GitHub Issue Creation Skill

Eliminate manual "hand bombing" of context into GitHub issues. This skill automates the entire issue creation workflow, reducing creation time from 5 minutes to 1 minute (80% reduction) through systematic context collection, template-driven consistency, and automated label compliance.

## Purpose

This skill provides a comprehensive 4-phase workflow for creating high-quality GitHub issues with:

- **Automated Context Collection**: Systematic discovery of relevant code, issues, and documentation
- **Template-Driven Consistency**: Five specialized templates ensuring complete information capture
- **Label Compliance Automation**: Automatic application per GitHubLabelStandards.md requirements
- **Duplicate Prevention**: Systematic checking to avoid redundant issues
- **Integration Ready**: Seamless delegation from /create-issue slash command

**Time Savings**: Reduces manual context gathering and formatting from 5 minutes to 1 minute through automation.

## When to Use

Invoke this skill when you need to:

- **Create Feature Requests**: From user requirements to comprehensive specifications with acceptance criteria
- **Document Bugs**: With reproduction steps, error messages, and impact assessment
- **Propose Architectural Improvements**: With analysis, rationale, and migration paths
- **Track Technical Debt**: Systematically documenting current vs. ideal state with remediation plans
- **Create Epic Milestones**: Multi-issue initiatives with component breakdown and dependency tracking
- **Any GitHub Issue Creation**: When comprehensive context and proper labeling are required

## 4-Phase Workflow

### Phase 1: Context Collection (Automated)

**Objective**: Gather comprehensive context eliminating manual "hand bombing"

**Automated Discovery Steps**:

1. **User Requirements Analysis**
   - Extract key requirements from conversation history
   - Identify acceptance criteria and success metrics
   - Capture user value proposition and pain points

2. **Codebase Analysis** (using Grep/Glob tools)
   - Search for related functionality: `grep -r "similar_feature" --include="*.cs"`
   - Find relevant files: `glob "**/*Service.cs"` for service-related issues
   - Identify integration points and affected components
   - Extract code snippets demonstrating current behavior

3. **Similar Issues Analysis** (using gh CLI)
   - Search existing issues: `gh issue list --search "keyword1 keyword2" --json number,title,labels`
   - Review related PRs: `gh pr list --search "keyword" --state all`
   - Identify patterns, common solutions, and dependencies

4. **Documentation Review**
   - Check module READMEs for context
   - Review relevant standards from /Docs/Standards/
   - Identify documentation gaps

5. **Acceptance Criteria Identification**
   - Convert requirements into testable outcomes
   - Define success metrics (quantitative and qualitative)
   - Identify integration validation needs

**Context Package Output**:
- Relevant file paths and line numbers
- Related issue/PR numbers and summaries
- Code snippets demonstrating current behavior
- User requirements translated to acceptance criteria
- Component impact assessment

### Phase 2: Template Selection

**Decision Criteria**: Choose template based on issue purpose and scope

**Template 1: Feature Request** (resources/templates/feature-request-template.md)
- **Use When**: New functionality or capability needed
- **Focus Areas**: User value proposition, acceptance criteria, technical considerations
- **Labels**: `type: feature`, `priority: [based on business impact]`, `effort: [estimation]`

**Template 2: Bug Report** (resources/templates/bug-report-template.md)
- **Use When**: Something is broken or behaving unexpectedly
- **Focus Areas**: Reproduction steps, error messages, impact assessment
- **Labels**: `type: bug`, `priority: [based on severity]`, `effort: [estimation]`

**Template 3: Epic** (resources/templates/epic-template.md)
- **Use When**: Large initiative requiring multiple issues and coordination
- **Focus Areas**: Vision, component breakdown, dependencies, milestones
- **Labels**: `type: epic`, `effort: epic`, `status: epic-planning`

**Template 4: Technical Debt** (resources/templates/technical-debt-template.md)
- **Use When**: Code quality improvement, refactoring needed
- **Focus Areas**: Current vs. ideal state, impact of NOT fixing, migration path
- **Labels**: `type: debt`, `priority: [based on impact]`, `technical-debt`

**Template 5: Documentation Request** (resources/templates/documentation-request-template.md)
- **Use When**: Documentation is missing or unclear
- **Focus Areas**: Knowledge gap, user impact, proposed content outline
- **Labels**: `type: docs`, `component: docs`, `effort: [estimation]`

### Phase 3: Issue Construction

**Title Format**: `[Type]: [Clear, actionable description]`

**Examples**:
- `Feature: Add recipe filtering by dietary restrictions`
- `Bug: UserService.GetUserById returns 500 for valid IDs`
- `Epic: Recipe Management Feature Set`
- `Debt: Refactor ProcessExecutor to use dependency injection`
- `Docs: Add API authentication guide for external integrators`

**Description Construction**:
1. **Load Selected Template**: Fill all template sections with collected context
2. **Code Snippets**: Format with proper markdown syntax and file paths
3. **Cross-References**: Link to related files, issues, PRs with full context
4. **Acceptance Criteria**: Specific, testable outcomes with clear success metrics
5. **Technical Context**: Affected components, dependencies, integration points

**Label Application (MANDATORY per GitHubLabelStandards.md)**:

**Required Labels (All 4 Must Be Present)**:
1. **Type Label** (exactly one):
   - `type: feature` - New functionality
   - `type: bug` - Defect or broken behavior
   - `type: epic` - Multi-issue initiative
   - `type: debt` - Technical debt or refactoring
   - `type: docs` - Documentation improvement

2. **Priority Label** (exactly one):
   - `priority: critical` - Security vulnerabilities, production down, data loss
   - `priority: high` - Major functionality broken, required for next milestone
   - `priority: medium` - Important but workaround exists, planned improvements
   - `priority: low` - Nice-to-have, future consideration, cosmetic

3. **Effort Label** (exactly one):
   - `effort: tiny` - <1 hour
   - `effort: small` - 1-4 hours
   - `effort: medium` - 1-3 days
   - `effort: large` - 1-2 weeks
   - `effort: epic` - Multi-week initiative

4. **Component Label** (at least one):
   - `component: api` - Backend API and service layer
   - `component: website` - Angular frontend application
   - `component: testing` - Test framework and infrastructure
   - `component: ci-cd` - Build, deployment, automation
   - `component: docs` - Documentation and standards
   - `component: database` - Schema, migrations

**Optional Labels (Apply When Relevant)**:
- **Epic Coordination**: `epic: testing-excellence`, `epic: [initiative-name]`
- **Coverage Phases**: `coverage: phase-1` through `coverage: phase-5`
- **Automation Context**: `automation: ci-ready`, `automation: local-only`
- **Quality**: `technical-debt`, `architecture`, `breaking-change`

**Milestone Assignment**:
- Link to epic if this issue is part of larger initiative
- Use epic label for coordination
- Example: `epic/testing-excellence` milestone for coverage tasks

**Assignee Identification**:
- **BackendSpecialist**: .NET/C# API issues
- **FrontendSpecialist**: Angular/TypeScript frontend issues
- **TestEngineer**: Test coverage and quality improvements
- **SecurityAuditor**: Security vulnerabilities and hardening
- **WorkflowEngineer**: CI/CD, GitHub Actions automation
- **ArchitecturalAnalyst**: Architecture decisions and patterns
- **BugInvestigator**: Complex debugging and root cause analysis
- **DocumentationMaintainer**: Documentation updates and guides

**Related Issues Linking**:
- **Depends On**: Issues that must complete before this one
- **Blocks**: Issues that depend on this one
- **Related To**: Similar issues, alternative approaches
- **Duplicate Of**: If discovered during duplicate check

### Phase 4: Validation & Submission

**Pre-Submission Validation Checklist**:

**Template Completeness**:
- [ ] All template sections filled with meaningful content (not just placeholders)
- [ ] Code snippets properly formatted with file paths and line numbers
- [ ] Acceptance criteria specific and testable
- [ ] Related issues linked with clear dependency relationships

**Label Compliance Validation**:
- [ ] Exactly one `type:` label applied
- [ ] Exactly one `priority:` label applied
- [ ] Exactly one `effort:` label applied
- [ ] At least one `component:` label applied
- [ ] Epic labels present if part of larger initiative
- [ ] Coverage phase labels for testing-related issues
- [ ] Automation labels for CI-dependent work

**Duplicate Prevention**:
```bash
# Search for similar issues by title keywords
gh issue list --search "keyword1 keyword2" --json number,title,state,labels

# Check for related PRs that might have addressed this
gh pr list --search "keyword" --state all --json number,title,state
```

**Validation Rules**:
- If similar issue OPEN: Add comment instead of creating duplicate
- If similar issue CLOSED: Reference in new issue, explain why reopening or creating new
- If related PR exists: Link to PR, identify gaps not addressed

**Acceptance Criteria Clarity Check**:
- [ ] Each criterion is specific and testable
- [ ] Success metrics are quantifiable where possible
- [ ] Validation approach is clear
- [ ] Edge cases and error scenarios considered

**Submission Command**:
```bash
gh issue create \
  --title "type: Brief actionable description" \
  --body "$(cat /path/to/populated-template.md)" \
  --label "type: feature,priority: high,effort: medium,component: api" \
  --milestone "epic-name" \
  --assignee "@BackendSpecialist"
```

**Post-Submission Actions**:
- Capture issue URL for reference
- Update related issues with cross-links
- Add to epic tracking if applicable
- Communicate issue number to stakeholders

## Target Agent Patterns

### Claude (Codebase Manager)
**Use Case**: Epic creation with comprehensive planning

**Workflow**:
1. Analyze initiative scope and strategic objectives
2. Invoke github-issue-creation skill with Epic template
3. Break down into component issues
4. Create dependency graph with Mermaid diagrams
5. Define milestones and phasing
6. Create epic parent issue with all sub-issues linked

### BugInvestigator
**Use Case**: Bug reports with root cause analysis

**Workflow**:
1. Perform diagnostic investigation
2. Collect reproduction steps and error messages
3. Invoke github-issue-creation skill with Bug template
4. Include root cause analysis if identified
5. Assess impact and suggest fixes
6. Create bug issue with comprehensive context

### ArchitecturalAnalyst
**Use Case**: Technical debt and architecture improvements

**Workflow**:
1. Analyze current architecture patterns
2. Identify technical debt or improvement opportunities
3. Invoke github-issue-creation skill with Technical Debt template
4. Document current vs. ideal state
5. Propose migration path with risk assessment
6. Create technical debt issue with refactoring plan

### TestEngineer
**Use Case**: Test coverage gaps and quality improvements

**Workflow**:
1. Analyze test coverage reports
2. Identify coverage gaps or quality issues
3. Invoke github-issue-creation skill with Feature template (for test infrastructure)
4. Define coverage improvement scope and acceptance criteria
5. Apply coverage phase labels per testing initiative
6. Create test coverage issue linked to epic

## Label Compliance Automation

### Mandatory Label Logic

**Every GitHub Issue MUST Have Exactly 4 Mandatory Labels**:

```yaml
Type Label (REQUIRED - exactly one):
  - type: feature      # New capability or enhancement
  - type: bug          # Defect, broken functionality
  - type: epic         # Multi-issue initiative
  - type: debt         # Technical debt, refactoring
  - type: docs         # Documentation improvement

Priority Label (REQUIRED - exactly one):
  - priority: critical # System down, data loss, security vulnerability
  - priority: high     # Major functionality broken, required for milestone
  - priority: medium   # Important but workaround exists
  - priority: low      # Nice-to-have, future consideration

Effort Label (REQUIRED - exactly one):
  - effort: tiny       # <1 hour
  - effort: small      # 1-4 hours
  - effort: medium     # 1-3 days
  - effort: large      # 1-2 weeks
  - effort: epic       # Multi-week initiative

Component Label (REQUIRED - at least one):
  - component: api         # Backend API and service layer
  - component: website     # Angular frontend application
  - component: testing     # Test framework and infrastructure
  - component: ci-cd       # Build, deployment, automation
  - component: docs        # Documentation and standards
  - component: database    # Schema, migrations
  - component: scripts     # Shell scripts and tooling
```

### Validation Rules

**Pre-Submission Validation**:
```bash
# Validate all 4 mandatory labels present
if [[ ! "$LABELS" =~ "type:" ]]; then
  echo "ERROR: Missing type: label"
  exit 1
fi

if [[ ! "$LABELS" =~ "priority:" ]]; then
  echo "ERROR: Missing priority: label"
  exit 1
fi

if [[ ! "$LABELS" =~ "effort:" ]]; then
  echo "ERROR: Missing effort: label"
  exit 1
fi

if [[ ! "$LABELS" =~ "component:" ]]; then
  echo "ERROR: Missing component: label"
  exit 1
fi
```

**Label Selection Decision Trees**: See resources/documentation/label-application-guide.md

## Integration with /create-issue Command

**Command-Skill Delegation Pattern**:

**Command Responsibilities** (CLI Interface):
- Argument parsing: `/create-issue <type> [--title "..."] [--description "..."]`
- User interaction: Prompt for missing required information
- Error messaging: Clear validation feedback to user
- Output formatting: Display created issue URL and confirmation

**Skill Responsibilities** (Implementation Logic):
- Execute 4-phase workflow (Context → Template → Construction → Validation)
- Perform automated context collection via grep/glob
- Apply template and populate with collected context
- Validate label compliance per GitHubLabelStandards.md
- Submit issue via gh CLI
- Return structured result to command

**Example Integration**:
```bash
# User invokes command
/create-issue feature --title "Add dark mode toggle"

# Command delegates to github-issue-creation skill
# Skill executes Phase 1-4 workflow
# Skill returns issue URL: https://github.com/org/repo/issues/123

# Command displays to user
✅ Created feature issue #123: Add dark mode toggle
📋 https://github.com/org/repo/issues/123
```

## Resources

### Templates (resources/templates/)
Comprehensive issue type templates for immediate use:

- **feature-request-template.md**: User value proposition, acceptance criteria, technical considerations
- **bug-report-template.md**: Environment, reproduction steps, error messages, impact assessment
- **epic-template.md**: Vision, component breakdown, dependencies, milestones
- **technical-debt-template.md**: Current vs. ideal state, impact analysis, migration path
- **documentation-request-template.md**: Knowledge gap, user impact, content outline

### Examples (resources/examples/)
Realistic demonstrations of complete workflow:

- **comprehensive-feature-example.md**: Recipe filtering feature with full context collection
- **bug-with-reproduction.md**: UserService bug with detailed reproduction and error analysis
- **epic-milestone-example.md**: Recipe Management epic with component breakdown and dependencies

### Documentation (resources/documentation/)
Deep-dive guides for advanced usage:

- **issue-creation-guide.md**: Step-by-step walkthrough of 4-phase workflow with examples
- **label-application-guide.md**: Complete GitHubLabelStandards.md integration with decision trees
- **context-collection-patterns.md**: Automated context gathering strategies and best practices

## Integration & Quality Gates

**Standards Compliance**:
- All issues MUST meet GitHubLabelStandards.md mandatory label requirements
- TaskManagementStandards.md integration for epic branch coordination
- DocumentationStandards.md alignment for consistent formatting

**AI Sentinel Compatibility**:
- Proper labeling enables AI-powered analysis workflows
- Epic coordination supports multi-agent development patterns
- Coverage phase labels integrate with testing excellence initiatives

**No Bypassed Quality Gates**:
- Template completeness enforced before submission
- Label compliance validated programmatically
- Duplicate prevention through systematic searching
- Acceptance criteria clarity required for all issues

## Progressive Loading Integration

**Level 1: Metadata Discovery (~100 tokens)**
- YAML frontmatter loaded at startup for skill discovery
- Enables selection from 100+ potential skills

**Level 2: Instructions Loading (~3,000 tokens)**
- This SKILL.md body loaded when skill is invoked
- Complete 4-phase workflow and integration guidance

**Level 3: Resources Access (2,000-4,000 tokens on-demand)**
- Templates loaded when specific issue type needed
- Examples referenced for pattern demonstration
- Documentation accessed for advanced scenarios

**Total Context Efficiency**: >90% savings vs. embedding in agent definitions
