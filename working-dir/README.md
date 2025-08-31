# Working Directory - Inter-Agent Communication Hub

**Purpose**: Shared artifact space for rich communication between the 11-agent development team without overloading the Codebase Manager's context window.

**Last Updated**: 2025-08-31  
**Session Management**: All files except this README.md are session-scoped and gitignored

## Overview

The `/working-dir/` serves as a temporary communication hub where agents create, update, and consume artifacts for effective handoffs. This reduces the "telephone game" effect and preserves rich context between specialized agents.

## Session Lifecycle

### Session Start
- **Cleanup**: Remove all files except README.md from previous sessions
- **Setup**: Create `session-state.md` to track current mission progress
- **Initialize**: Document GitHub issue context and initial task decomposition

### Session Progress
- **Artifacts**: Agents create analysis reports, design decisions, implementation notes
- **Handoffs**: Rich context transfer between agents via shared documents
- **Tracking**: Session state updated with each major milestone and artifact creation
- **Coordination**: Codebase Manager monitors for critical updates and plan adjustments

### Session End
- **Validation**: ComplianceOfficer uses artifacts for comprehensive pre-PR validation
- **Archive**: Critical insights captured in permanent documentation
- **Cleanup**: All session files removed (handled by next session start)

## Artifact Types and Naming Conventions

### Core Session Files
- `session-state.md` - Current progress tracking and agent coordination
- `validation-checklist-{issue}.md` - Compliance Officer tracking for specific GitHub issue

### Investigation and Analysis
- `bug-analysis-{timestamp}.md` - BugInvestigator detailed reports
- `design-decisions-{timestamp}.md` - ArchitecturalAnalyst rationales
- `security-assessment-{timestamp}.md` - SecurityAuditor findings

### Development Artifacts
- `implementation-notes-{issue}.md` - CodeChanger detailed development context
- `test-strategy-{issue}.md` - TestEngineer coverage plans and rationales
- `frontend-decisions-{timestamp}.md` - FrontendSpecialist component designs
- `backend-decisions-{timestamp}.md` - BackendSpecialist API and service patterns

### Handoff Communications
- `{source-agent}-to-{target-agent}-handoff-{timestamp}.md` - Direct agent communication
- `multi-agent-coordination-{timestamp}.md` - Cross-team coordination notes

### Compliance and Quality
- `compliance-validation-{timestamp}.md` - ComplianceOfficer pre-PR reports
- `standards-review-{timestamp}.md` - Standards compliance analysis
- `quality-checklist-{issue}.md` - Quality gate tracking

## Agent Responsibilities

### All Agents Must
- **Report Artifacts**: Inform Codebase Manager when creating/updating working directory files
- **Review Context**: Check relevant artifacts before starting work
- **Preserve Context**: Document decisions, rationales, and important discoveries
- **Follow Naming**: Use standard naming conventions for artifact discoverability

### Codebase Manager Must
- **Monitor Directory**: Watch for critical updates that require plan adjustments
- **Maintain State**: Keep `session-state.md` current with all agent activities
- **Coordinate Handoffs**: Ensure agents consume relevant artifacts from other team members
- **Session Cleanup**: Initialize clean working directory for new missions

### ComplianceOfficer Must
- **Consume All Artifacts**: Review all agent outputs during pre-PR validation
- **Create Validation Reports**: Document comprehensive compliance assessment
- **Track Quality Gates**: Maintain validation checklists for issue completion

## Communication Protocol

### Creating Artifacts
```markdown
**Agent**: [Agent Name]
**Timestamp**: [ISO format]
**Mission Context**: [Related GitHub issue and subtask]
**Artifact Type**: [Analysis/Decision/Handoff/etc.]

## Summary
[Brief overview of artifact contents]

## Context for Other Agents
[What other agents need to know]

## Recommendations
[Any suggested actions or plan adjustments]

## Detailed Content
[Full analysis, decisions, findings, etc.]
```

### Updating Artifacts
- Always preserve original content
- Add timestamps for updates
- Clearly mark new sections
- Maintain continuity for consuming agents

## Integration with Standards

This working directory system integrates with:
- **DocumentationStandards.md**: Artifacts follow documentation standards for clarity
- **TaskManagementStandards.md**: Session tracking aligns with GitHub issue workflow
- **TestingStandards.md**: Test artifacts support comprehensive coverage planning
- **CodingStandards.md**: Implementation artifacts preserve architectural decisions

## Privacy and Security

- **No Secrets**: Never store API keys, passwords, or sensitive data in working directory
- **Session Scoped**: All files are temporary and session-specific
- **Gitignored**: Only this README.md is version controlled
- **Local Only**: All artifacts are local to the development session

## Examples

### Session State Template
```markdown
# Session State - Issue #123

**Started**: 2025-08-31T14:30:00Z
**GitHub Issue**: #123 - Implement user authentication
**Status**: In Progress

## Task Decomposition
- [ ] SecurityAuditor: Review auth patterns (ASSIGNED)
- [ ] BackendSpecialist: Implement JWT service (PENDING)
- [ ] TestEngineer: Create auth tests (PENDING)
- [ ] DocumentationMaintainer: Update auth docs (PENDING)

## Artifacts Created
- security-assessment-20250831-1430.md (SecurityAuditor)
- implementation-notes-123.md (BackendSpecialist)

## Plan Adjustments
- Added two-factor authentication requirement based on SecurityAuditor findings
```

### Handoff Example
```markdown
# SecurityAuditor to BackendSpecialist Handoff

**From**: SecurityAuditor  
**To**: BackendSpecialist  
**Issue**: #123 Authentication Implementation  
**Timestamp**: 2025-08-31T15:00:00Z

## Security Requirements Identified
1. JWT tokens must expire within 15 minutes
2. Refresh tokens required for persistent sessions
3. Rate limiting needed on auth endpoints
4. Password hashing with bcrypt minimum 12 rounds

## Implementation Recommendations
- Use ASP.NET Core Identity for user management
- Implement custom JWT service with refresh token rotation
- Add authentication middleware with proper error handling

## Critical Security Patterns
[Detailed security patterns and code examples]

## Next Agent Actions
BackendSpecialist should implement the JWT service following these security requirements and create implementation-notes-123.md with design decisions.
```

---

**Remember**: This working directory enables the 11-agent team to work cohesively while maintaining the self-contained knowledge philosophy of the zarichney-api project. Use it effectively to preserve context and reduce coordination overhead.