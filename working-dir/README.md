# Working Directory - Inter-Agent Communication Hub

**Purpose**: Shared artifact space for rich communication between the 12-agent development team without overloading the Codebase Manager's context window.

**Last Updated**: 2025-09-01  
**Session Management**: All files except this README.md are session-scoped and gitignored  
**Communication Standards**: Mandatory protocols ensure consistent artifact reporting across all 12 agents

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

## Mandatory Communication Standards

**CRITICAL FOR ALL AGENTS**: These protocols are MANDATORY, not optional. Every agent must follow these exact standards to ensure effective team coordination and prevent communication gaps.

### Universal Agent Requirements

All agents (analysis, file-editing, and specialized) MUST follow these three mandatory protocols:

#### 1. Pre-Work Artifact Discovery (MANDATORY)
**BEFORE STARTING ANY TASK**:
```
🔍 WORKING DIRECTORY DISCOVERY:
- Current artifacts reviewed: [list existing files checked]
- Relevant context found: [artifacts that inform current work]
- Integration opportunities: [how existing work will be built upon]
- Potential conflicts: [any overlapping concerns identified]
```

#### 2. Immediate Artifact Reporting (MANDATORY)
**WHEN CREATING/UPDATING ANY WORKING DIRECTORY FILE**:
```
🗂️ WORKING DIRECTORY ARTIFACT CREATED:
- Filename: [exact-filename-with-extension]
- Purpose: [brief description of content and intended consumers]
- Context for Team: [what other agents need to know about this artifact]
- Dependencies: [what other artifacts this builds upon or relates to]
- Next Actions: [any follow-up coordination needed]
```

#### 3. Context Integration Reporting (REQUIRED)
**WHEN BUILDING UPON OTHER AGENTS' WORK**:
```
🔗 ARTIFACT INTEGRATION:
- Source artifacts used: [specific files that informed this work]
- Integration approach: [how existing context was incorporated]
- Value addition: [what new insights or progress this provides]
- Handoff preparation: [context prepared for future agents]
```

### Communication Enforcement Standards

- **No Delayed Reporting**: Agents must report artifacts IMMEDIATELY upon creation, not in batch or summary format
- **Mandatory Discovery**: Every agent MUST check existing artifacts before starting work - no exceptions
- **Standard Format Compliance**: All reporting must use exact formats above for consistency
- **Team Awareness**: Every artifact must include context for other agents to understand relevance
- **Integration Continuity**: Agents must explicitly report how they build upon existing team work

## Agent Responsibilities

### All Agents Must
- **Execute Pre-Work Discovery**: Check existing artifacts using mandatory discovery format before starting ANY task
- **Report Artifacts Immediately**: Use exact artifact reporting format when creating/updating working directory files
- **Integrate Team Context**: Build upon existing artifacts and report integration using standard format
- **Follow Naming**: Use standard naming conventions for artifact discoverability
- **Preserve Context**: Document decisions, rationales, and important discoveries
- **Enable Team Coordination**: Provide context that enables other agents to understand and build upon work

### Codebase Manager Must
- **Monitor Directory**: Watch for critical updates that require plan adjustments
- **Maintain State**: Keep `session-state.md` current with all agent activities
- **Coordinate Handoffs**: Ensure agents consume relevant artifacts from other team members
- **Session Cleanup**: Initialize clean working directory for new missions
- **Verify Communication Compliance**: Confirm each agent reports artifacts using standardized formats
- **Enforce Discovery**: Ensure agents check existing artifacts before starting work
- **Track Continuity**: Monitor how artifacts build upon each other across agent engagements
- **Prevent Communication Gaps**: Intervene when agents miss mandatory reporting requirements

### ComplianceOfficer Must
- **Consume All Artifacts**: Review all agent outputs during pre-PR validation
- **Create Validation Reports**: Document comprehensive compliance assessment
- **Track Quality Gates**: Maintain validation checklists for issue completion

## Communication Protocol

### Mandatory Communication Sequence

**EVERY AGENT MUST FOLLOW THIS EXACT SEQUENCE**:

1. **Pre-Work Discovery** (MANDATORY): Check existing artifacts using discovery format before starting
2. **Work Execution**: Complete assigned task with full context awareness
3. **Immediate Artifact Reporting** (MANDATORY): Report any files created using artifact reporting format
4. **Context Integration Reporting** (REQUIRED): Report how existing work was incorporated

### Standard Artifact Content Structure
```markdown
**Agent**: [Agent Name]
**Timestamp**: [ISO format]
**Mission Context**: [Related GitHub issue and subtask]
**Artifact Type**: [Analysis/Decision/Handoff/etc.]
**Team Integration**: [Artifacts reviewed and how they informed this work]

## Summary
[Brief overview of artifact contents]

## Context for Other Agents
[What other agents need to know - MANDATORY SECTION]

## Dependencies and Integration
[What other artifacts this builds upon or relates to]

## Recommendations
[Any suggested actions or plan adjustments]

## Next Agent Actions
[Specific guidance for agents who will consume this artifact]

## Detailed Content
[Full analysis, decisions, findings, etc.]
```

### Communication Verification Checklist

Before completing any task, agents must verify:
- [ ] Pre-work discovery completed and reported
- [ ] All artifacts created are immediately reported using standard format
- [ ] Context integration documented when building upon existing work
- [ ] Team awareness information provided for future agents
- [ ] Next actions clearly identified for coordination

### Updating Artifacts
- Always preserve original content
- Add timestamps for updates
- Clearly mark new sections
- Maintain continuity for consuming agents
- **Report Updates**: Use artifact reporting format when making updates
- **Document Integration**: Explain how updates build upon or relate to other artifacts
- **Preserve Team Context**: Ensure updates maintain visibility for other agents

## Integration with Standards

This working directory system integrates with:
- **CLAUDE.md Version 1.5**: Communication protocols align with multi-agent orchestration standards
- **DocumentationStandards.md**: Artifacts follow documentation standards for clarity
- **TaskManagementStandards.md**: Session tracking aligns with GitHub issue workflow
- **TestingStandards.md**: Test artifacts support comprehensive coverage planning
- **CodingStandards.md**: Implementation artifacts preserve architectural decisions

### Multi-Agent Team Coordination Framework

These communication standards enable:
- **Context Preservation**: Rich context transfer between specialized agents
- **Team Awareness**: Every agent understands what others have accomplished
- **Integration Continuity**: Work builds systematically upon previous agent deliverables
- **Communication Gaps Prevention**: Mandatory reporting ensures no work happens in isolation
- **Coordination Efficiency**: Codebase Manager can effectively orchestrate with full visibility

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

## Critical Success Factors

### For Individual Agents
- **Mandatory Compliance**: Follow all three communication protocols without exception
- **Team Awareness**: Always consider how your work enables other agents
- **Context Building**: Build systematically upon existing team artifacts
- **Clear Communication**: Use exact standard formats for consistent team understanding

### For Team Coordination
- **No Communication Gaps**: Every artifact creation is immediately reported
- **Context Continuity**: Rich context flows seamlessly between agent engagements
- **Integration Awareness**: All agents understand interdependencies and build accordingly
- **Efficient Orchestration**: Codebase Manager maintains full visibility for effective coordination

---

**Remember**: This working directory enables the 12-agent team to work cohesively while maintaining the self-contained knowledge philosophy of the zarichney-api project. The mandatory communication protocols ensure no agent works in isolation, enabling effective context preservation and reducing coordination overhead through systematic artifact reporting and discovery.