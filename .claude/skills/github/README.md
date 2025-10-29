# GitHub Workflow Skills Category

**Purpose:** GitHub workflow automation and issue management
**Created:** 2025-10-25
**Parent:** [`.claude/skills/`](../README.md)

---

## 1. Purpose & Responsibility

**What it is**: Skills for automating GitHub workflows, issue creation, PR management, and repository operations

**Key objectives**:
- **Issue Creation Automation**: Reduce manual context gathering from 5 min to 1 min (80% time savings)
- **Workflow Consistency**: Template-driven issue creation ensuring completeness and label compliance
- **Context Preservation**: Systematic discovery of related code, issues, and documentation
- **Quality Assurance**: Validation and duplicate prevention before issue submission

**Why it exists**: GitHub issue creation previously required extensive manual context gathering ("hand bombing"). These skills automate discovery, template application, and validation, dramatically reducing time while improving issue quality and consistency.

---

## 2. Current Skills

### [github-issue-creation](./github-issue-creation/)

**Purpose**: Streamline GitHub issue creation with automated context collection, template application, and proper labeling

**Use when**: Creating feature requests, documenting bugs, proposing architectural improvements, tracking technical debt, or creating epic milestones

**Key features**:
- **4-Phase Workflow**: Context Collection → Template Selection → Issue Construction → Validation
- **5 Issue Templates**: Feature requests, bug reports, epics, technical debt, documentation
- **Automated Context Collection**: grep/glob/gh CLI for systematic discovery
- **Label Compliance**: GitHubLabelStandards.md enforcement with validation
- **Duplicate Prevention**: Systematic checking before creation
- **/create-issue Integration**: Prepared for slash command delegation (Iteration 2)

**Token budget**:
- Metadata: ~100 tokens (YAML frontmatter)
- Instructions: ~3,000 tokens (SKILL.md)
- Resources: 2,000-4,000 tokens on-demand (templates, examples, documentation)

**Time savings**: 5 minutes → 1 minute (80% reduction)

**Resources**:
- **Templates** (5): feature-request, bug-report, epic, technical-debt, documentation-request
- **Examples** (3): comprehensive-feature, bug-with-reproduction, epic-milestone
- **Documentation** (3): issue-creation-guide, label-application-guide, context-collection-patterns

---

## 3. When to Create GitHub Workflow Skills

**Create new skill in this category when**:
- Pattern used by 3+ agents for GitHub operations
- Workflow can be systematized and automated
- Significant time savings achievable (>50% reduction)
- Quality improvement through consistency and validation

**Examples of future skills**:
- `pull-request-creation` - Automated PR creation with comprehensive descriptions and review requests
- `issue-tracking` - Epic progress tracking, dependency resolution, milestone coordination
- `label-management` - Automated label application and validation across repositories
- `github-analytics` - Repository metrics, contribution analysis, workflow optimization

**DON'T create skill when**:
- Simple one-off gh CLI command (use direct command)
- Repository-specific workflow (document in repo README)
- Requires interactive decision-making (slash command better fit)

---

## 4. GitHub Skills Architecture Patterns

### Pattern 1: Automated Discovery

**Principle**: Eliminate manual context gathering through systematic tool usage

**Tools**:
- **Grep**: Search code for related functionality, integration points
- **Glob**: Find files by pattern for comprehensive discovery
- **gh CLI**: Query issues, PRs, labels, milestones for context
- **Read**: Load documentation, standards, module-specific context

**Example (github-issue-creation)**:
```bash
# Find related code
grep -r "filter" --include="*.cs" Code/Zarichney.Server/Services/

# Locate relevant files
glob "**/*Recipe*.cs"

# Check similar issues
gh issue list --search "recipe filter"

# Load standards
read Docs/Standards/GitHubLabelStandards.md
```

---

### Pattern 2: Template-Driven Consistency

**Principle**: Standardized templates ensure completeness and quality

**Structure**:
- Multiple templates for different issue types
- All template sections guide comprehensive information capture
- Placeholders replaced with discovered context
- Validation ensures no sections skipped

**Example (github-issue-creation templates)**:
- Feature request: User value proposition, acceptance criteria, technical considerations
- Bug report: Environment, reproduction steps, error messages, impact assessment
- Epic: Vision, component breakdown, dependency graph, risk assessment

---

### Pattern 3: Validation Before Submission

**Principle**: Catch issues early through systematic checks

**Validation types**:
- **Template completeness**: All sections filled, no placeholders remaining
- **Label compliance**: Mandatory labels present per GitHubLabelStandards.md
- **Duplicate prevention**: Search existing issues/PRs before creating
- **Acceptance criteria clarity**: SMART criteria (Specific, Measurable, Achievable, Relevant, Time-bound)

**Example (github-issue-creation validation)**:
```bash
# Label validation
validate-labels.sh "type: feature,priority: high,effort: medium,component: api"

# Duplicate check
gh issue list --search "recipe filter" --json number,title,state
```

---

### Pattern 4: Command-Skill Delegation

**Principle**: Slash commands handle user interaction, skills provide implementation

**Separation of concerns**:
- **Command** (CLI interface): Argument parsing, user prompts, error messages, output formatting
- **Skill** (implementation): Automated discovery, template population, validation, submission

**Example (/create-issue command → github-issue-creation skill)**:
```
User: /create-issue feature --title "Add dark mode"

Command responsibilities:
- Parse arguments
- Prompt for missing info
- Display result

Skill responsibilities:
- Execute 4-phase workflow
- Collect context automatically
- Validate and submit issue
- Return structured result
```

---

## 5. Integration with zarichney-api Workflows

### GitHubLabelStandards.md Compliance

**All GitHub skills MUST enforce label standards**:
- **Mandatory labels**: type, priority, effort, component (all 4 required)
- **Optional labels**: epic coordination, coverage phases, automation context
- **Validation**: Programmatic checks before issue creation
- **Consistency**: Automated application eliminating manual errors

### Epic Branch Strategy

**Skills support epic coordination per TaskManagementStandards.md**:
- Epic issues created with proper milestones and labeling
- Component issues linked to parent epic
- Epic branch creation guidance
- Dependency tracking and milestone coordination

### Multi-Agent Coordination

**Skills enable efficient agent collaboration**:
- **Claude**: Epic creation with comprehensive planning
- **BugInvestigator**: Bug reports with root cause analysis
- **ArchitecturalAnalyst**: Technical debt documentation
- **TestEngineer**: Test coverage issue tracking
- **All agents**: Consistent issue creation reducing coordination overhead

### AI Sentinel Integration

**Proper labeling enables AI-powered analysis**:
- Issue categorization for automated routing
- Priority-based workflow triggers
- Epic coordination for multi-PR analysis
- Quality gate integration

---

## 6. Maintenance Notes

### Adding New GitHub Workflow Skills

**Process**:
1. **Validate need**: 3+ agents benefit, >50% time savings achievable
2. **Design workflow**: Map manual process, identify automation opportunities
3. **Create structure**: SKILL.md with YAML frontmatter, resources/ directory
4. **Develop resources**: Templates, examples, documentation for progressive loading
5. **Validate**: Test with at least 2 agents, measure time savings
6. **Document**: Update this README with new skill entry

### Updating Existing Skills

**When to update**:
- GitHubLabelStandards.md changes requiring template updates
- New issue templates needed for emerging patterns
- User feedback identifying pain points or missing features
- Integration requirements for new slash commands

**Update process**:
1. Modify SKILL.md or resources as needed
2. Update examples demonstrating new capabilities
3. Test with agents to validate improvements
4. Document changes in this README
5. Communicate updates to team

### Deprecation Strategy

**When to deprecate**:
- GitHub workflow changes making skill obsolete
- Better skill supersedes functionality
- Usage analytics show skill not providing value

**Deprecation process**:
1. Mark skill as deprecated in this README
2. Provide migration path to replacement
3. Keep skill available for 1 quarter
4. Remove after validation no agents depend on it

---

## 7. Related Documentation

**Official Skills Structure**:
- [`Docs/Specs/epic-291-skills-commands/official-skills-structure.md`](../../Docs/Specs/epic-291-skills-commands/official-skills-structure.md)
- [`Docs/Specs/epic-291-skills-commands/skills-catalog.md`](../../Docs/Specs/epic-291-skills-commands/skills-catalog.md)

**Project Standards**:
- [`Docs/Standards/GitHubLabelStandards.md`](../../Docs/Standards/GitHubLabelStandards.md) - Label taxonomy and application rules
- [`Docs/Standards/TaskManagementStandards.md`](../../Docs/Standards/TaskManagementStandards.md) - Epic branch strategy, PR standards
- [`Docs/Standards/DocumentationStandards.md`](../../Docs/Standards/DocumentationStandards.md) - Template structure and consistency

**Parent Documentation**:
- [Skills Root README](../README.md) - Skills architecture and progressive loading
- [Claude Code Official Docs](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/)

---

## 8. Quick Reference

**Current Skill Count**: 1 skill (github-issue-creation)

**Token Budget**:
- Average skill metadata: ~100 tokens
- Average skill instructions: ~3,000 tokens
- Average resources: ~2,500 tokens on-demand
- **Total context efficiency**: >90% savings vs. embedded approach

**Time Savings**:
- github-issue-creation: 5 min → 1 min (80% reduction)
- **Projected category savings**: ~4 minutes per issue creation across all agents

**Future Skills** (Iteration 2-3):
- pull-request-creation
- issue-tracking
- label-management
- github-analytics

---

**Category Status**: ✅ Active (Epic #291 Iteration 1.3)

**Next Updates**:
- Iteration 2.4: /create-issue command integration
- Future: Additional GitHub workflow automation skills based on agent usage patterns
