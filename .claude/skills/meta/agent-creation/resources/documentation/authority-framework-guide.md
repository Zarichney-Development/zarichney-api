# Authority Framework Guide

**Purpose:** Deep dive into authority boundary design patterns with decision frameworks
**Target Audience:** PromptEngineer designing agent file edit rights and coordination
**Version:** 1.0.0

---

## INTRODUCTION - AUTHORITY ARCHITECTURE

### Why Authority Clarity Prevents Coordination Conflicts

**Core Problem:** In a multi-agent development team, multiple AI agents potentially modifying the same files creates:
- **Merge conflicts** when agents simultaneously edit files
- **Coordination overhead** determining which agent should handle which modifications
- **Authority hesitation** where agents unsure if they have permission to proceed
- **Orchestration failures** when Claude cannot determine clear delegation targets

**Solution:** Unambiguous authority boundaries using glob pattern file specifications and intent-based activation frameworks create zero-overlap zones where each agent knows exactly what they can/cannot modify.

### Three Authority Patterns

The zarichney-api multi-agent team uses three distinct authority patterns optimized for different agent roles:

| Pattern | File Modification Authority | Use Case | Example Agents |
|---------|----------------------------|----------|----------------|
| **Exclusive** | Owns specific file types exclusively | Primary file-editing agents | TestEngineer (`*Tests.cs`), DocumentationMaintainer (`*.md`) |
| **Flexible** | Domain authority activated by request intent | Specialist domain experts | BackendSpecialist (`.cs` via command intent), FrontendSpecialist (`.ts/.html` via command intent) |
| **Advisory-Only** | ZERO file modifications | Analysis and validation agents | ComplianceOfficer, ArchitecturalAnalyst, BugInvestigator |

**Design Principle:** Choose authority pattern based on agent role, not implementation complexity. Pattern determines coordination protocols.

### Zarichney-API Multi-Agent Coordination Context

**Current Team Authority Distribution:**

**Primary Agents (Exclusive File Authority):**
- **CodeChanger:** `Code/**/*.cs` (excluding tests), `Code/**/*.json` (excluding test configs)
- **TestEngineer:** `**/*Tests.cs`, `**/*.spec.ts`, test configurations
- **DocumentationMaintainer:** `**/*.md` (all Markdown documentation)
- **PromptEngineer:** `.claude/agents/*.md`, `.github/prompts/*.md`, application prompts

**Specialist Agents (Flexible Authority - Intent-Based):**
- **BackendSpecialist:** `Code/**/*.cs` (command intent), backend configs (command intent)
- **FrontendSpecialist:** `Code/**/*.ts`, `**/*.html`, `**/*.css` (command intent)
- **SecurityAuditor:** Security configs, vulnerability fixes (command intent)
- **WorkflowEngineer:** `.github/workflows/*.yml`, `Scripts/*`, build configs (command intent)

**Advisory Agents (Zero File Modifications):**
- **ComplianceOfficer:** Working directory validation reports only
- **ArchitecturalAnalyst:** Working directory architectural analysis only
- **BugInvestigator:** Working directory diagnostic reports only

**Key Observation:** 4 primary agents with exclusive authority, 5 specialists with flexible intent-based authority, 3 advisory agents with zero modifications. No authority overlap conflicts.

---

## PATTERN 1: EXCLUSIVE FILE AUTHORITY (Primary Agents)

### Concept: One Agent, One File Type, Zero Ambiguity

**Core Design Philosophy:**
- Primary agents have **exclusive ownership** of specific file types
- File patterns use glob syntax for unambiguous matching
- NO other agent can modify these files regardless of scenario
- Sequential workflow positioning ensures clear handoff chains

### File Pattern Specification Technique

**Glob Pattern Syntax:**

```yaml
**/ = Match any directory depth
* = Match any characters within filename
[!pattern] = Exclude specific patterns
{pattern1,pattern2} = Match multiple alternatives
```

**TestEngineer Exclusive Authority (Example):**

```yaml
EXCLUSIVE_AUTHORITY_PATTERNS:
  C#_Test_Files:
    Pattern: "**/*Tests.cs"
    Scope: All C# test files across entire zarichney-api project
    Locations:
      - "Code/Zarichney.Server.Tests/**/*Tests.cs" (backend tests)
      - "Code/Zarichney.Server.Tests/**/*IntegrationTests.cs" (integration suites)
      - Any future test projects following *Tests.cs naming
    Matches_Example:
      - "Code/Zarichney.Server.Tests/Services/RecipeServiceTests.cs" ✅
      - "Code/Zarichney.Server.Tests/Controllers/RecipeControllerTests.cs" ✅
    Does_NOT_Match:
      - "Code/Zarichney.Server/Services/RecipeService.cs" ❌ (not *Tests.cs)
      - "Code/Zarichney.Server/README.md" ❌ (not .cs)

  TypeScript_Test_Files:
    Pattern: "**/*.spec.ts"
    Scope: Angular/TypeScript test specifications
    Locations:
      - "Code/Zarichney.Website/**/*.spec.ts" (frontend tests)
    Matches_Example:
      - "Code/Zarichney.Website/src/app/recipe/recipe.component.spec.ts" ✅
    Does_NOT_Match:
      - "Code/Zarichney.Website/src/app/recipe/recipe.component.ts" ❌ (not .spec.ts)

  Test_Configuration:
    Patterns:
      - "xunit.runner.json" (xUnit execution settings)
      - "test-appsettings*.json" (test environment configs)
      - "karma.conf.js" (Angular test runner - if applicable)
    Scope: Test framework configurations only
    Excludes: "appsettings.json" (CodeChanger production config territory)
```

**Authority Validation Protocol:**

Before modifying file, TestEngineer validates:
```
1. Does file path match **/*Tests.cs OR **/*.spec.ts OR test config patterns?
   - YES → Proceed with exclusive authority
   - NO → Check if testability improvement needed
     - If YES: Document needed changes, escalate to CodeChanger
     - If NO: Reject modification (outside authority)
```

### Exclusion Patterns (Forbidden Territory)

**Critical Design Decision:** Explicitly document what primary agent CANNOT modify to prevent authority overreach.

**TestEngineer Forbidden Modifications:**

```yaml
FORBIDDEN_MODIFICATIONS:
  Production_Code:
    Patterns: "Code/**/*.cs" (excluding *Tests.cs)
    Reason: CodeChanger exclusive territory
    Scenario: "RecipeService needs testability improvements"
    Correct_Action: Create working directory analysis documenting needed changes → CodeChanger implements
    Incorrect_Action: Modify RecipeService.cs directly (authority violation)

  Documentation:
    Patterns: "**/*.md"
    Reason: DocumentationMaintainer exclusive territory
    Scenario: "New testing pattern established, README needs update"
    Correct_Action: Report to DocumentationMaintainer for documentation
    Incorrect_Action: Modify README.md directly (authority violation)

  Workflows:
    Patterns: ".github/workflows/*.yml"
    Reason: WorkflowEngineer exclusive territory
    Scenario: "Test execution failing in CI/CD pipeline"
    Correct_Action: Escalate to WorkflowEngineer with diagnostic information
    Incorrect_Action: Modify workflow YAML (authority violation)

  Agent_Definitions:
    Patterns: ".claude/agents/*.md"
    Reason: PromptEngineer exclusive territory
    Scenario: "TestEngineer behavior needs refinement"
    Correct_Action: Escalate to PromptEngineer or user for agent modification
    Incorrect_Action: Self-modify agent definition (authority violation)
```

**Validation Question:** "For any given file path in the project, can I unambiguously determine if TestEngineer has authority?"

**Test Cases:**

| File Path | TestEngineer Authority? | Rationale |
|-----------|------------------------|-----------|
| `Code/Zarichney.Server.Tests/Services/RecipeServiceTests.cs` | ✅ YES | Matches `**/*Tests.cs` |
| `Code/Zarichney.Server/Services/RecipeService.cs` | ❌ NO | Not `*Tests.cs` - CodeChanger/BackendSpecialist |
| `Code/Zarichney.Server.Tests/xunit.runner.json` | ✅ YES | Test configuration pattern |
| `Code/Zarichney.Server/appsettings.json` | ❌ NO | Production config - CodeChanger |
| `Code/Zarichney.Website/src/app/recipe/recipe.component.spec.ts` | ✅ YES | Matches `**/*.spec.ts` |
| `README.md` | ❌ NO | Markdown - DocumentationMaintainer |

**100% Unambiguous:** Every file path has clear YES/NO authority determination.

### Ownership Philosophy: Preventing Authority Conflicts

**Exclusive Authority Benefits:**

1. **Zero Coordination Overhead:** No negotiation needed - file type = agent authority
2. **Sequential Workflow Clarity:** CodeChanger implements → TestEngineer tests → ComplianceOfficer validates
3. **Simplified Orchestration:** Claude knows exactly which agent to engage for test file work
4. **No Merge Conflicts:** Only one agent ever modifies test files

**Common Authority Conflict Scenario (AVOIDED):**

**Bad Design (Ambiguous Shared Authority):**
```
CodeChanger: Modifies "all .cs files"
TestEngineer: Modifies "test-related .cs files"

File: RecipeServiceTests.cs
- Is this "all .cs files"? (CodeChanger claim)
- Is this "test-related .cs files"? (TestEngineer claim)
- AMBIGUOUS! Both agents could modify simultaneously!
```

**Good Design (Exclusive Glob Patterns):**
```
CodeChanger: "Code/**/*.cs" (EXCLUDING *Tests.cs) ← Explicit exclusion
TestEngineer: "**/*Tests.cs" ← Explicit test pattern

File: RecipeServiceTests.cs
- Matches "**/*Tests.cs" → TestEngineer ✅
- Does NOT match "Code/**/*.cs (EXCLUDING *Tests.cs)" → CodeChanger ❌
- UNAMBIGUOUS! Only TestEngineer has authority!
```

### Coordination Protocol Specification

**Autonomous Action Scenarios (No Coordination):**

```yaml
TestEngineer_Proceeds_Independently_When:
  - Creating new test files matching *Tests.cs or *.spec.ts patterns
  - Modifying existing test implementations for coverage improvements
  - Adding test utilities, builders, fixtures within test projects
  - Updating test configurations (xunit.runner.json, test-appsettings.json)
  - Creating working directory artifacts for test progress reporting

Rationale: Authority clear, no cross-agent dependencies, within exclusive file patterns
```

**Coordination Required Scenarios:**

```yaml
Testability_Issues:
  Trigger: Application code structure prevents comprehensive testing
  Example: "RecipeService tightly coupled to database, cannot mock for unit tests"
  Coordinate_With: CodeChanger or BackendSpecialist
  Protocol:
    1. TestEngineer creates working directory analysis: "testability-improvements-needed.md"
    2. Document specific code changes needed (dependency injection, interface extraction)
    3. CodeChanger or BackendSpecialist implements testability improvements
    4. TestEngineer creates comprehensive tests after refactoring
  Authority_Boundary: TestEngineer CANNOT modify RecipeService.cs directly

Test_Architecture_Changes:
  Trigger: Need new testing infrastructure or framework patterns
  Example: "Integration tests require test database setup infrastructure"
  Coordinate_With: ArchitecturalAnalyst
  Protocol:
    1. TestEngineer requests architectural guidance for testing infrastructure
    2. ArchitecturalAnalyst provides design recommendations
    3. TestEngineer implements within test project scope
  Authority_Boundary: Architectural decisions beyond test implementation scope

Documentation_Updates:
  Trigger: New testing patterns established requiring project documentation
  Example: "Established test data builder pattern for Recipe domain"
  Coordinate_With: DocumentationMaintainer
  Protocol:
    1. TestEngineer reports new testing pattern to DocumentationMaintainer
    2. DocumentationMaintainer updates module README.md with pattern documentation
    3. Future developers reference documentation for pattern
  Authority_Boundary: TestEngineer CANNOT modify README.md

CI_CD_Test_Execution:
  Trigger: Test execution failures in GitHub Actions pipeline
  Example: "Tests pass locally but fail in CI/CD with timeout errors"
  Coordinate_With: WorkflowEngineer
  Protocol:
    1. TestEngineer provides diagnostic information (test output, timing, dependencies)
    2. WorkflowEngineer investigates CI/CD environment issues
    3. WorkflowEngineer modifies workflow YAML for test execution improvements
  Authority_Boundary: TestEngineer CANNOT modify .github/workflows/*.yml
```

**Key Design Decision:** Coordination protocols specify WHICH agent to engage and WHAT information to provide, maintaining authority boundaries throughout.

### Exclusive Authority Decision Framework

**Use Exclusive File Authority Pattern When:**

✅ **File Type Has Single Clear Owner**
- Example: Test files naturally owned by testing specialist
- Counter-Example: .cs files shared between CodeChanger and BackendSpecialist (use flexible authority)

✅ **No Shared Modification Scenarios**
- Example: Only TestEngineer creates/modifies tests, never CodeChanger or specialists
- Counter-Example: Configuration files sometimes modified by CodeChanger, sometimes BackendSpecialist (use flexible authority or split patterns)

✅ **Sequential Workflow Positioning Clear**
- Example: CodeChanger implements → TestEngineer tests → ComplianceOfficer validates (sequential chain)
- Counter-Example: BackendSpecialist and FrontendSpecialist coordinate simultaneously on API contracts (parallel coordination)

**Examples of Good Exclusive Authority Assignments:**

| Agent | Exclusive Pattern | Rationale |
|-------|------------------|-----------|
| **TestEngineer** | `**/*Tests.cs`, `**/*.spec.ts` | Test files never modified by implementers |
| **DocumentationMaintainer** | `**/*.md` | Documentation has single owner for consistency |
| **PromptEngineer** | `.claude/agents/*.md`, `.github/prompts/*.md` | Agent definitions require specialized prompt engineering |
| **CodeChanger** | `Code/**/*.cs` (excluding tests/configs) | General production code primary owner |

**Examples Requiring Different Authority Pattern:**

| Scenario | Why NOT Exclusive | Better Pattern |
|----------|------------------|----------------|
| Backend .cs files shared by CodeChanger and BackendSpecialist | Need both general implementation AND domain expertise | **Flexible Authority** - CodeChanger for general, BackendSpecialist for complex via command intent |
| Security configuration files | SecurityAuditor analyzes but CodeChanger may also modify | **Flexible Authority** - SecurityAuditor command intent for security fixes, CodeChanger for general |
| Architectural analysis | ArchitecturalAnalyst provides guidance but never implements | **Advisory-Only** - Zero file modifications, working directory only |

---

## PATTERN 2: FLEXIBLE AUTHORITY (Specialist Agents)

### Concept: Dual-Mode Operation Based on Request Intent

**Core Design Philosophy:**
- Specialists have **domain expertise** requiring both advisory AND implementation capabilities
- **Intent recognition** determines authority activation (query = advisory, command = implementation)
- Authority **shared with primary agents** but differentiated through depth and intent
- Enables efficiency by **reducing coordination handoff overhead**

### Dual-Mode Operation Framework

**Query Intent (Analysis Mode):**

```yaml
AUTHORITY: ZERO file modifications, working directory artifacts only
DELIVERABLE: Architectural analysis, recommendations, design guidance
CONSUMER: Other agents implement recommendations (CodeChanger, primary agents)
MODE_TRIGGER: Request verbs indicating analysis need

Examples:
  - "Analyze the repository pattern implementation" → QUERY
  - "Review API architecture for performance issues" → QUERY
  - "What optimizations would improve database queries?" → QUERY
  - "Should we use CQRS or repository pattern?" → QUERY
```

**Command Intent (Implementation Mode):**

```yaml
AUTHORITY: Direct file modifications within domain expertise boundaries
DELIVERABLE: Code changes, architectural implementations, configuration updates
CONSUMER: TestEngineer validates implementations, ComplianceOfficer pre-PR validation
MODE_TRIGGER: Request verbs indicating implementation need

Examples:
  - "Implement CQRS pattern for RecipeService" → COMMAND
  - "Fix N+1 query issue in OrderRepository" → COMMAND
  - "Optimize database access for UserService" → COMMAND
  - "Create repository pattern for InventoryService" → COMMAND
```

**Why Dual-Mode Matters:**

**Before Flexible Authority (Coordination Overhead):**
```
User: "The RecipeService has N+1 query issues, fix them"

Workflow WITHOUT Flexible Authority:
1. Engage ArchitecturalAnalyst → Analyze N+1 query pattern
2. ArchitecturalAnalyst creates working directory analysis
3. Engage CodeChanger → Implement recommended eager loading
4. CodeChanger implements without deep EF Core expertise
5. Testing reveals optimization incomplete
6. Re-engage ArchitecturalAnalyst → Further analysis
7. Cycle continues with coordination overhead

Total: 3+ agent engagements, multiple coordination rounds
```

**With Flexible Authority (Efficiency Gain):**
```
User: "The RecipeService has N+1 query issues, fix them"

Workflow WITH Flexible Authority:
1. Engage BackendSpecialist (command intent recognized)
2. BackendSpecialist analyzes AND implements eager loading with EF Core expertise
3. Engage TestEngineer → Validate performance improvements
4. Done

Total: 2 agent engagements, direct implementation by domain expert
```

**Efficiency Gain:** 40-60% reduction in coordination handoffs for complex domain-specific implementations.

### Intent Recognition Design Patterns

**Verb Pattern Matching (Primary Recognition Method):**

**Query Intent Verb Patterns:**
```yaml
ANALYSIS_VERBS:
  - Analyze, Review, Assess, Evaluate, Examine
  - Investigate, Study, Inspect, Audit
  - Identify, Find, Detect, Discover, Locate

QUESTION_PATTERNS:
  - What [question about existing code]
  - How [question about implementation details]
  - Why [question about architectural decisions]
  - Should we [decision request]
  - Can we [feasibility question]

RECOMMENDATION_REQUESTS:
  - Recommend, Suggest, Propose, Advise
  - Compare [pattern A] vs [pattern B]
  - Evaluate options for [scenario]

Action: Working directory analysis artifacts
Authority: NO file modifications
```

**Command Intent Verb Patterns:**
```yaml
IMPLEMENTATION_VERBS:
  - Implement, Create, Build, Develop, Add
  - Fix, Resolve, Correct, Repair, Debug
  - Optimize, Enhance, Improve, Upgrade, Refactor
  - Update, Modify, Change, Adjust, Revise
  - Apply, Execute, Establish, Standardize

DIRECTIVE_PATTERNS:
  - [Verb] the [specific code element]
  - [Verb] [technology pattern] for [service/component]
  - [Verb] [architectural improvement]

Action: Direct file modifications within domain
Authority: Full implementation rights in expertise area
```

**From BackendSpecialist Example:**

**Query Intent Request Examples:**

| User Request | Intent Classification | BackendSpecialist Response |
|--------------|---------------------|---------------------------|
| "Analyze the current repository pattern implementation" | QUERY (verb "Analyze") | Working directory: `backend-repository-analysis-2025-10-25.md` with findings and recommendations |
| "How does our authentication flow work?" | QUERY (question "How") | Working directory: `backend-auth-flow-analysis.md` documenting architecture |
| "What database performance issues exist?" | QUERY (question "What") | Working directory: `database-performance-analysis.md` identifying bottlenecks |
| "Should we use CQRS or repository pattern for OrderService?" | QUERY (question "Should") | Working directory: `orderservice-pattern-comparison.md` evaluating options |

**Command Intent Request Examples:**

| User Request | Intent Classification | BackendSpecialist Response |
|--------------|---------------------|---------------------------|
| "Implement CQRS pattern for RecipeService" | COMMAND (verb "Implement") | Direct .cs file modifications: RecipeService.cs, RecipeQueryService.cs, RecipeController.cs |
| "Fix the N+1 query issue in OrderRepository" | COMMAND (verb "Fix") | Direct .cs file modification: OrderRepository.cs with eager loading |
| "Optimize database access for UserService" | COMMAND (verb "Optimize") | Direct .cs file modifications: UserService.cs, UserRepository.cs with query optimizations |
| "Create repository pattern for new InventoryService" | COMMAND (verb "Create") | Direct .cs files created: InventoryRepository.cs, IInventoryRepository.cs, DI configuration |

### Intent Recognition Edge Cases and Clarification Protocol

**Hybrid Request (Analyze + Implement):**

**User Request:** "Analyze the repository pattern issues and implement recommended fixes"

**BackendSpecialist Response (Sequential Query → Command):**
```
Intent Recognition: HYBRID (query + command sequential workflow)

Workflow:
1. QUERY Phase: Analyze repository pattern
   - Create working directory analysis: backend-repository-issues-2025-10-25.md
   - Document findings: Missing unit of work, inconsistent error handling
   - Formulate recommendations with specific implementation steps

2. Report Query Phase Completion:
   "Analysis complete. Findings documented in working directory.
    Recommendations: 1) Implement unit of work pattern, 2) Standardize error handling.
    Proceeding to COMMAND phase implementation."

3. COMMAND Phase: Implement recommended fixes
   - Modify RecipeRepository.cs, OrderRepository.cs with unit of work
   - Standardize error handling across repositories
   - Update dependency injection configuration

4. Report Command Phase Completion:
   "Implementation complete. Files modified: [list].
    TestEngineer coordination required for comprehensive testing."

Result: Single BackendSpecialist engagement handles both query and command phases sequentially
```

**Ambiguous Request (Unclear Intent):**

**User Request:** "Look at RecipeService performance"

**BackendSpecialist Response (Request Clarification):**
```
Intent Recognition: AMBIGUOUS

"I detected potential intent ambiguity in your request. Do you want me to:

A) QUERY INTENT: Analyze RecipeService performance and provide optimization recommendations
   - Deliverable: Working directory analysis with findings and suggested improvements
   - No code modifications

B) COMMAND INTENT: Investigate RecipeService performance and implement optimizations
   - Deliverable: Direct code changes optimizing performance
   - File modifications: RecipeService.cs, RecipeRepository.cs

Please clarify your preferred mode so I can proceed appropriately."
```

**Design Rationale:** "Look at" ambiguous between analysis and implementation. Specialist requests clarification rather than guessing intent and potentially violating authority (if user wanted query but specialist implements) or under-delivering (if user wanted command but specialist only analyzes).

**Ambiguous Trigger Words (Always Clarify):**

| Ambiguous Verb | Query Interpretation | Command Interpretation | Clarification Action |
|----------------|---------------------|----------------------|---------------------|
| "Look at" | Examine and analyze | Investigate and fix | REQUEST CLARIFICATION |
| "Check" | Review for issues | Verify and correct | REQUEST CLARIFICATION |
| "Handle" | Assess handling approach | Implement handling | REQUEST CLARIFICATION |
| "Address" | Identify issues | Fix issues | REQUEST CLARIFICATION |
| "Deal with" | Analyze problem | Solve problem | REQUEST CLARIFICATION |

### Domain Boundary Definition (Implementation Authority)

**BackendSpecialist Domain Authority (Command Intent):**

```yaml
BACKEND_PRODUCTION_CODE:
  Pattern: "Code/**/*.cs"
  Exclusions: "**/*Tests.cs" (TestEngineer exclusive)
  Scope: Controllers, Services, Models, Repositories, DTOs, Middleware
  Intent_Required: COMMAND
  Examples:
    - "Code/Zarichney.Server/Controllers/RecipeController.cs" ✅
    - "Code/Zarichney.Server/Services/RecipeService.cs" ✅
    - "Code/Zarichney.Server/Models/Recipe.cs" ✅
    - "Code/Zarichney.Server/Repositories/RecipeRepository.cs" ✅
  Shared_With: CodeChanger (primary implementer)
  Differentiation: BackendSpecialist handles complex architectural patterns, CodeChanger handles general features

BACKEND_CONFIGURATION:
  Patterns:
    - "config/**/*.json" (backend configs only)
    - "Code/Zarichney.Server/appsettings*.json"
  Exclusions: "Code/Zarichney.Website/config/**" (FrontendSpecialist)
  Intent_Required: COMMAND
  Scope: Database connection strings, service configurations, logging settings
  Examples:
    - "Code/Zarichney.Server/appsettings.json" ✅ (backend config)
    - "Code/Zarichney.Server/appsettings.Development.json" ✅
    - "Code/Zarichney.Website/config/app.config.json" ❌ (frontend - FrontendSpecialist)

DATABASE_MIGRATIONS:
  Pattern: "Code/Zarichney.Server/Migrations/**/*.cs"
  Intent_Required: COMMAND
  Scope: EF Core migration files
  Caution: Production database migrations require careful review
  Coordination: Major schema changes may require ArchitecturalAnalyst review before implementation
  Examples:
    - "Code/Zarichney.Server/Migrations/20251025_AddRecipeCategory.cs" ✅
```

**Cross-Domain Boundary Detection (Coordination Required):**

```yaml
WITHIN_BACKEND_AUTHORITY:
  - .cs files (excluding tests) ✅ Autonomous implementation
  - Backend configurations (appsettings.json) ✅ Autonomous implementation
  - Database migrations ✅ Autonomous implementation (with architectural awareness)
  Result: Proceed with command intent implementation

CROSS_DOMAIN_COORDINATION_REQUIRED:
  - Backend + Frontend contract changes:
      Example: "Implementing new RecipeController endpoints affecting frontend API client"
      Coordinate_With: FrontendSpecialist
      Protocol: Backend implements endpoints → FrontendSpecialist updates API client

  - Backend + Security patterns:
      Example: "Implementing JWT authentication in API"
      Coordinate_With: SecurityAuditor
      Protocol: SecurityAuditor reviews approach → BackendSpecialist implements with security validation

  - Backend + Infrastructure:
      Example: "Database migration requires infrastructure changes"
      Coordinate_With: WorkflowEngineer or ArchitecturalAnalyst
      Protocol: Architectural review → Infrastructure coordination → Implementation

OUTSIDE_BACKEND_AUTHORITY:
  - Frontend files (.ts, .html, .css) ❌ → FrontendSpecialist exclusive
  - Test files (*Tests.cs) ❌ → TestEngineer exclusive
  - Documentation (*.md) ❌ → DocumentationMaintainer exclusive
  - Workflows (.github/workflows/*.yml) ❌ → WorkflowEngineer exclusive
  Result: Escalate to appropriate specialist, DO NOT implement
```

**Authority Validation Protocol (Specialist Self-Check):**

```
Before modifying file in COMMAND intent mode:

1. Does file match backend domain patterns (.cs, config/*.json, migrations)?
   - NO → STOP, escalate to appropriate domain specialist
   - YES → Proceed to step 2

2. Is file excluded from my authority (*Tests.cs, *.md, *.yml)?
   - YES → STOP, escalate to exclusive owner
   - NO → Proceed to step 3

3. Does change span multiple specialist domains (backend + frontend + security)?
   - YES → Create coordination plan with affected specialists
   - NO → Proceed to step 4

4. Is this complex architectural pattern requiring deep .NET expertise?
   - YES → Implement with command intent authority
   - NO → Could CodeChanger handle? If yes, consider if specialist depth truly needed

5. Proceed with implementation, report to TestEngineer for coverage
```

### Coordination Trigger Identification

**Query Intent Coordination (Advisory Deliverables):**

```yaml
ANALYSIS_COORDINATION:
  Working_Directory_Artifact_Creation:
    - Create analysis document with findings and recommendations
    - Identify which agent should implement recommendations
    - Provide specific implementation guidance for consuming agents

  FrontendSpecialist_Coordination:
    Trigger: API contract analysis recommendations affect frontend
    Protocol: BackendSpecialist analysis → FrontendSpecialist reviews API impact
    Example: "Recommend migrating to CQRS - frontend may need separate read/write API clients"

  SecurityAuditor_Consultation:
    Trigger: Security-sensitive pattern analysis
    Protocol: BackendSpecialist architectural analysis → SecurityAuditor security review
    Example: "Analyzing authentication flow - request SecurityAuditor threat assessment"

  ArchitecturalAnalyst_Escalation:
    Trigger: System-wide architectural decisions beyond backend domain
    Protocol: BackendSpecialist identifies architectural complexity → ArchitecturalAnalyst provides system-level guidance
    Example: "Repository pattern vs CQRS decision affects entire application architecture"
```

**Command Intent Coordination (Implementation Deliverables):**

```yaml
IMPLEMENTATION_COORDINATION:
  FrontendSpecialist_API_Contracts:
    Trigger: Backend endpoint implementations affecting frontend integration
    Protocol: Coordinate DTOs, endpoint contracts, WebSocket patterns BEFORE implementation
    Example: "Implementing OrderController endpoints → Notify FrontendSpecialist of API contract changes"
    Workflow:
      1. BackendSpecialist designs API contract
      2. FrontendSpecialist reviews contract for frontend compatibility
      3. Both specialists agree on final contract
      4. BackendSpecialist implements backend, FrontendSpecialist updates API client

  TestEngineer_Testability:
    Trigger: ALL command intent implementations
    Protocol: Ensure testable architecture, coordinate test requirements
    Example: "BackendSpecialist implements RepositoryPattern → TestEngineer creates comprehensive repository tests"
    Workflow:
      1. BackendSpecialist implements with testability considerations (DI, interfaces)
      2. BackendSpecialist reports implementation complete
      3. TestEngineer creates comprehensive test coverage

  SecurityAuditor_Security_Patterns:
    Trigger: Authentication, authorization, security-critical implementations
    Protocol: SecurityAuditor reviews security approach BEFORE implementation
    Example: "Implementing JWT authentication → SecurityAuditor validates security pattern"
    Workflow:
      1. BackendSpecialist proposes JWT implementation approach
      2. SecurityAuditor reviews for vulnerabilities and OWASP compliance
      3. BackendSpecialist implements with security validation

  ArchitecturalAnalyst_Major_Changes:
    Trigger: Significant architectural pattern changes affecting system design
    Protocol: Request architectural analysis before large-scale implementation
    Example: "Migrating entire backend to CQRS pattern → ArchitecturalAnalyst validates approach"
    Workflow:
      1. BackendSpecialist identifies need for architectural change
      2. ArchitecturalAnalyst provides system-wide impact analysis
      3. BackendSpecialist implements with architectural guidance
```

### Flexible Authority Decision Framework

**Use Flexible Authority Pattern When:**

✅ **Deep Domain Expertise Required**
- Example: .NET 8/C# backend architecture needs principal-level expertise
- Pattern: BackendSpecialist provides depth CodeChanger may lack for complex patterns

✅ **Both Analysis AND Implementation Valuable**
- Example: Sometimes need backend architectural review, sometimes need direct implementation
- Pattern: Query intent provides analysis, command intent implements recommendations

✅ **Request Intent Determines Appropriate Response Mode**
- Example: "Analyze API" triggers query mode, "Implement API" triggers command mode
- Pattern: Intent recognition framework enables mode switching

✅ **Shared File Domain with Primary Agent**
- Example: BackendSpecialist and CodeChanger both modify .cs files
- Pattern: Intent and complexity differentiate activation (CodeChanger general, BackendSpecialist complex)

**Examples of Good Flexible Authority Assignments:**

| Specialist | Flexible Domain | Query Intent Use | Command Intent Use |
|------------|----------------|-----------------|-------------------|
| **BackendSpecialist** | `.cs`, backend configs | API architecture analysis | Complex pattern implementation |
| **FrontendSpecialist** | `.ts`, `.html`, `.css` | Component design review | Angular component implementation |
| **SecurityAuditor** | Security configs | Vulnerability analysis | Security fix implementation |
| **WorkflowEngineer** | `.github/workflows/*.yml` | CI/CD optimization analysis | Workflow automation implementation |

**Flexible Authority Advantages:**

1. **Reduced Coordination Overhead:** Expert can analyze AND implement without handoff
2. **Domain Expertise Preserved:** Specialist maintains deep knowledge through both advisory and implementation
3. **Efficient User Experience:** Single specialist engagement for analysis-to-implementation workflows
4. **Quality Assurance:** Deep expertise ensures implementations follow best practices

**Flexible Authority Challenges:**

1. **Intent Recognition Complexity:** Must accurately detect query vs. command intent
2. **Authority Ambiguity Potential:** Shared domains with primary agents require clear differentiation
3. **Mode Switching Clarity:** Specialist must communicate which mode activated for team awareness

---

## PATTERN 3: ADVISORY-ONLY AUTHORITY (Zero File Modifications)

### Concept: Pure Analysis with Zero Implementation Capability

**Core Design Philosophy:**
- Advisory agents provide **strategic guidance** through working directory artifacts
- **ABSOLUTELY ZERO** direct file modifications regardless of scenario
- Recommendations **consumed by other agents** for implementation
- **Simplest authority pattern** - no file modification complexity

### Zero File Modification Authority Specification

**ComplianceOfficer Advisory Authority (Example):**

```yaml
FILE_MODIFICATIONS: NONE (absolutely zero direct file changes under any circumstances)

READ_ACCESS_SCOPE:
  - ALL project files for comprehensive validation analysis
  - All standards documents (Docs/Standards/*.md)
  - All module README.md files for architectural context
  - Test execution results (observe via dotnet test commands, never modify tests)
  - GitHub issue content and requirements
  - Git status and branch state for validation purposes
  Purpose: Comprehensive information access enables thorough analysis

WRITE_ACCESS_SCOPE:
  - /working-dir/ ONLY for compliance validation reports
  Artifact_Patterns:
    - "compliance-validation-report-YYYY-MM-DD.md"
    - "pre-pr-validation-checklist.md"
    - "remediation-recommendations.md"
    - "quality-gate-status.md"
  Purpose: All advisory deliverables via working directory artifacts

FORBIDDEN_WRITE_ACCESS:
  - Code files (*.cs, *.ts, *.js) - ANY code modification forbidden
  - Configuration files (*.json, *.yml, *.yaml) - ANY config modification forbidden
  - Documentation files (*.md outside /working-dir/) - DocumentationMaintainer exclusive
  - Test files (*Tests.cs, *.spec.ts) - TestEngineer exclusive
  - Workflow files (.github/workflows/*) - WorkflowEngineer exclusive
  - Agent definitions (.claude/agents/*.md) - PromptEngineer exclusive
  - ANY file system write outside /working-dir/ - Absolute boundary
```

**Authority Validation Protocol (Advisory Self-Check):**

```
Before ANY write operation:

1. Is file path within /working-dir/ directory?
   - NO → FORBIDDEN, escalate if file modification needed
   - YES → Proceed to step 2

2. Is artifact format appropriate for advisory deliverable?
   - Analysis report, validation checklist, recommendations? YES → Proceed
   - Code, config, documentation modification? NO → FORBIDDEN

3. Does artifact clearly identify consuming agents for implementation?
   - YES → Create artifact with implementation guidance
   - NO → Enhance artifact with agent assignments
```

**Forbidden Action Examples:**

| Scenario | ComplianceOfficer Temptation | Correct Advisory Behavior |
|----------|----------------------------|--------------------------|
| "Compliance validation finds magic number in RecipeController.cs" | Modify RecipeController.cs directly to replace with constant | Create compliance report documenting violation → CodeChanger implements fix |
| "README.md missing API documentation for new endpoints" | Modify README.md with missing documentation | Create compliance report documenting gap → DocumentationMaintainer updates README |
| "Test coverage below target, missing RecipeServiceTests" | Create RecipeServiceTests.cs to meet coverage | Create compliance report documenting gap → TestEngineer creates tests |
| "GitHub workflow test execution timeout needs increase" | Modify .github/workflows/test.yml timeout value | Create compliance report documenting issue → WorkflowEngineer adjusts workflow |

**Key Design Decision:** Advisory agents IDENTIFY issues through analysis, NEVER implement fixes. Maintaining this boundary simplifies authority architecture and prevents advisory agents from conflicting with implementers.

### Analysis-Only Deliverable Patterns

**Working Directory Artifact Types:**

**1. Validation Reports:**
```markdown
# compliance-validation-report-2025-10-25.md

## Pre-PR Compliance Validation

**Status:** FAIL - Remediation Required ⚠️

### Standards Compliance Assessment

**CodingStandards.md:** VIOLATIONS (3 issues)
1. Magic numbers in RecipeController.cs (lines 45, 67)
   - Violation: Hard-coded values without named constants
   - Remediation: CodeChanger replaces with named constants
   - Priority: MINOR (non-blocking)

2. Missing XML documentation for public API method
   - Location: RecipeController.cs GetRecipes method (line 23)
   - Violation: Public API methods require XML documentation
   - Remediation: CodeChanger adds XML documentation comments
   - Priority: MINOR (non-blocking)

**TestingStandards.md:** COMPLIANT ✅
- All tests passing (100% pass rate: 73/73)
- AAA pattern followed consistently
- Coverage goals met for current epic phase

**DocumentationStandards.md:** GAPS (1 issue)
1. README.md not updated for API contract changes
   - Violation: CQRS endpoints added but not documented
   - Remediation: DocumentationMaintainer updates API section
   - Priority: MINOR (non-blocking, but recommended before PR)

**TaskManagementStandards.md:** COMPLIANT ✅
- Conventional commit messages used
- GitHub issue requirements met
- Epic progression tracking current

### Remediation Recommendations

**💻 CodeChanger:**
- Replace magic numbers in RecipeController.cs (lines 45, 67)
- Add XML documentation to GetRecipes method

**📖 DocumentationMaintainer:**
- Update README.md API section for CQRS endpoints

### PR Decision Recommendation

**REMEDIATION_RECOMMENDED** (minor issues, can proceed with follow-up issue if time-critical)

**Rationale:** No critical (PR-blocking) violations. All test-related compliance met. Minor standards issues can be addressed via follow-up issue if immediate PR creation prioritized.
```

**2. Architectural Analysis Reports:**
```markdown
# backend-repository-analysis-2025-10-25.md

## Repository Pattern Implementation Analysis

**Analysis Scope:** Current repository pattern implementation across backend services
**Requested By:** BackendSpecialist (query intent)
**Consuming Agents:** CodeChanger OR BackendSpecialist (command intent for complex patterns)

### Findings

**Strengths:**
- Repository abstraction properly implemented with IRepository interfaces
- Dependency injection correctly configured
- Basic CRUD operations follow consistent patterns

**Issues Identified:**

1. **Missing Unit of Work Pattern** (MAJOR)
   - Problem: Repositories create individual DbContext instances
   - Impact: Transaction consistency issues, cannot commit multiple repository changes atomically
   - Location: RecipeRepository.cs, OrderRepository.cs, UserRepository.cs
   - Risk: Data inconsistency during multi-entity operations

2. **N+1 Query Problems** (MAJOR)
   - Problem: Related entities loaded lazily causing multiple database round-trips
   - Impact: Performance degradation on list operations
   - Location: RecipeRepository.GetAllRecipes() method (line 45)
   - Example: 100 recipes = 1 query for recipes + 100 queries for categories = 101 database calls

3. **Inconsistent Error Handling** (MINOR)
   - Problem: Some repositories throw exceptions, others return null
   - Impact: Unpredictable error behavior for consumers
   - Locations: RecipeRepository (throws), OrderRepository (returns null)

### Recommendations

**Priority 1: Implement Unit of Work Pattern**
- Create UnitOfWork class managing DbContext lifecycle
- Repositories use injected DbContext from UnitOfWork
- Consumer code controls transaction boundaries
- Consuming Agent: BackendSpecialist (requires complex pattern knowledge) OR CodeChanger if guided

**Priority 2: Add Eager Loading for Related Entities**
- Include related entities in initial query using .Include()
- Implement specific query methods for different loading strategies
- Example: GetAllRecipesWithCategories() using eager loading
- Consuming Agent: BackendSpecialist (EF Core optimization expertise)

**Priority 3: Standardize Error Handling**
- Establish consistent pattern (recommend null object pattern or Result<T>)
- Apply across all repositories for consistency
- Document pattern in CodingStandards.md
- Consuming Agent: CodeChanger (straightforward refactoring)

### Next Actions

1. Determine if CodeChanger can implement Priority 1 (Unit of Work) with guidance OR
2. Re-engage BackendSpecialist with command intent for complex implementation
3. TestEngineer creates comprehensive repository tests after implementation
```

**3. Remediation Recommendation Documents:**
```markdown
# remediation-recommendations-security-2025-10-25.md

## Security Vulnerability Remediation Guidance

**Analysis Source:** SecurityAuditor vulnerability assessment
**Priority:** CRITICAL (PR-blocking)
**Consuming Agents:** BackendSpecialist (implementation), SecurityAuditor (validation)

### Critical Vulnerability: SQL Injection Risk

**Location:** RecipeController.cs SearchRecipes endpoint (line 67)

**Vulnerability:**
```csharp
// VULNERABLE CODE (line 67)
var query = $"SELECT * FROM Recipes WHERE Name LIKE '%{searchTerm}%'";
var results = _context.Recipes.FromSqlRaw(query).ToList();
```

**Issue:** User-provided searchTerm concatenated directly into SQL query enabling SQL injection attack

**Attack Example:**
```
searchTerm = "'; DROP TABLE Recipes; --"
Executed Query: SELECT * FROM Recipes WHERE Name LIKE '%'; DROP TABLE Recipes; --%'
Result: Recipes table deleted
```

**Remediation Steps (BackendSpecialist Implementation):**

1. Replace raw SQL with parameterized query:
```csharp
// SECURE CODE (recommended replacement)
var query = "SELECT * FROM Recipes WHERE Name LIKE {0}";
var searchPattern = $"%{searchTerm}%";
var results = _context.Recipes.FromSqlInterpolated($"SELECT * FROM Recipes WHERE Name LIKE {searchPattern}").ToList();
```

2. Or preferably, use LINQ for type safety:
```csharp
// BEST PRACTICE (LINQ - recommended)
var results = _context.Recipes
    .Where(r => r.Name.Contains(searchTerm))
    .ToList();
```

3. Add input validation for searchTerm (max length, allowed characters)

**Validation Protocol:**
- BackendSpecialist implements fix
- SecurityAuditor re-validates security
- TestEngineer creates security tests preventing regression

**Timeline:** IMMEDIATE (critical security vulnerability)
```

### Recommendation Consumption Workflow

**Advisory → Implementation → Validation Cycle:**

```yaml
STEP_1_ADVISORY_ANALYSIS:
  Agent: ComplianceOfficer (or ArchitecturalAnalyst, BugInvestigator)
  Deliverable: Working directory analysis report
  Content: Findings, recommendations, implementation guidance
  Consumer_Identification: Which agents should implement recommendations

STEP_2_IMPLEMENTATION_ASSIGNMENT:
  Coordinator: Claude (Codebase Manager)
  Decision: Which implementation agent based on complexity
  Options:
    - CodeChanger: Straightforward implementations
    - BackendSpecialist: Complex .NET patterns (command intent)
    - FrontendSpecialist: Angular implementations (command intent)
    - TestEngineer: Test coverage creation
    - DocumentationMaintainer: Documentation updates

STEP_3_IMPLEMENTATION_EXECUTION:
  Agent: [Selected implementation agent]
  Input: Advisory report from working directory
  Output: Direct file modifications OR further analysis if complexity exceeds scope
  Reporting: Implementation complete with files modified

STEP_4_VALIDATION:
  Agent: ComplianceOfficer (if validation requested)
  Action: Re-validate compliance after implementations
  Deliverable: Updated compliance report (PASS or remaining issues)
  Cycle: Repeat until all recommendations implemented and validated
```

**From ComplianceOfficer Example:**

**Cycle 1 - Initial Validation:**
```
ComplianceOfficer → Working directory compliance report → FAIL (3 violations)
↓
Recommendations: CodeChanger fix 2 issues, DocumentationMaintainer fix 1 issue
↓
Claude coordinates implementation agents
```

**Cycle 2 - Implementation:**
```
CodeChanger → Implements standards fixes (magic numbers, XML docs)
DocumentationMaintainer → Updates README.md
↓
Both agents report completion
```

**Cycle 3 - Re-Validation:**
```
ComplianceOfficer → Re-validates all deliverables → PASS ✅
↓
Working directory updated compliance report: All issues resolved
↓
Claude creates PR
```

### Advisory Authority Decision Framework

**Use Advisory-Only Authority Pattern When:**

✅ **Pure Analysis/Validation Role**
- Example: ComplianceOfficer validates compliance, never implements fixes
- Pattern: Working directory deliverables only, recommendations consumed by implementers

✅ **Recommendations Consumed by Other Agents**
- Example: ArchitecturalAnalyst provides design guidance, CodeChanger/specialists implement
- Pattern: Advisory output triggers implementation workflows

✅ **Quality Gate or Checkpoint Function**
- Example: ComplianceOfficer serves as pre-PR quality gate
- Pattern: Validation blocks workflow until compliance achieved

✅ **Independence from Implementation Required**
- Example: ComplianceOfficer validation independence from implementers
- Pattern: Advisory-only maintains unbiased validation

**Examples of Good Advisory-Only Assignments:**

| Agent | Analysis Focus | Why Advisory-Only |
|-------|---------------|------------------|
| **ComplianceOfficer** | Pre-PR compliance validation | Independent validation from implementers |
| **ArchitecturalAnalyst** | System design decisions | Strategic guidance, implementation by specialists |
| **BugInvestigator** | Root cause analysis | Diagnostic reporting, fixes by CodeChanger/specialists |

**Advisory Authority Advantages:**

1. **Simplest Authority Pattern:** No file modification complexity, no authority conflicts
2. **Fastest Agent Activation:** Smallest definitions (130-170 lines) due to no implementation workflows
3. **Independence Maintained:** Validation/analysis unbiased by implementation responsibilities
4. **Clear Coordination:** Advisory creates recommendations, implementers execute, advisory validates

**Advisory Authority Challenges:**

1. **Coordination Dependency:** Requires implementation agents to consume recommendations
2. **No Direct Impact:** Advisory cannot fix issues directly, relies on implementers
3. **Communication Critical:** Working directory artifacts must clearly specify implementation guidance

---

## AUTHORITY CONFLICT PREVENTION

### Pre-Deployment Authority Validation Checklist

**Before deploying new agent, validate against existing team:**

**Step 1: Load All Existing Agent Authorities**
```bash
# Review authority patterns from all 12 agents
cat .claude/agents/code-changer.md | grep -A 10 "EXCLUSIVE_AUTHORITY\|FLEXIBLE_AUTHORITY"
cat .claude/agents/test-engineer.md | grep -A 10 "EXCLUSIVE_AUTHORITY"
cat .claude/agents/backend-specialist.md | grep -A 10 "FLEXIBLE_AUTHORITY"
# ... repeat for all agents
```

**Step 2: Map Authority Boundaries**
```
File Pattern Ownership Map:
- *.cs (excluding *Tests.cs): CodeChanger (primary), BackendSpecialist (command intent complex)
- *Tests.cs: TestEngineer (exclusive)
- *.md: DocumentationMaintainer (exclusive)
- .claude/agents/*.md: PromptEngineer (exclusive)
- .github/workflows/*.yml: WorkflowEngineer (flexible command intent)
- *.ts, *.html, *.css: FrontendSpecialist (flexible command intent)
- Security configs: SecurityAuditor (flexible command intent)
```

**Step 3: Identify Potential Conflicts**
```yaml
NEW_AGENT: DatabaseSpecialist
PROPOSED_AUTHORITY: "Database migration files, database configurations"

CONFLICT_CHECK:
  - "migrations/**/*.cs" - Overlaps with BackendSpecialist authority? YES
  - Resolution:
      Option 1: BackendSpecialist retains migrations, DatabaseSpecialist advisory only
      Option 2: DatabaseSpecialist gets exclusive migrations, BackendSpecialist exclusion added
      Decision: Depends on organizational priorities (maintain backend cohesion vs. database specialization)
```

**Step 4: Coordination Protocol Design**
```yaml
IF authority overlap necessary:
  - Define clear differentiation (intent-based, complexity-based, or domain-based)
  - Document coordination triggers
  - Specify which agent for which scenarios
  - Update all affected agents' forbidden/coordination sections
```

### Authority Evolution and Refactoring

**When to Expand Agent Authority:**
- New file types emerge naturally fitting agent's domain expertise
- Coordination patterns reveal agent frequently requests same modifications
- Team consensus validates expanded scope maintains single responsibility

**When to Split Agent Responsibilities:**
- Single agent accumulates 2+ distinct responsibilities
- Authority boundaries become ambiguous due to scope growth
- Coordination overhead exceeds benefit of unified agent

**Migration Protocol Example:**

**Scenario:** BackendSpecialist accumulates both .NET implementation AND database administration responsibilities

**Before Split:**
```
BackendSpecialist:
  - .NET 8/C# architecture
  - Database schema design
  - EF Core migrations
  - API patterns
  - Service layer architecture
[Scope too broad, database vs. application backend conflation]
```

**After Split:**
```
BackendSpecialist (Retained):
  - .NET 8/C# architecture
  - API patterns
  - Service layer architecture
  Authority: Code/**/*.cs (excluding tests), config/backend/**

DatabaseSpecialist (New):
  - Database schema design
  - EF Core migrations and optimization
  - Query performance tuning
  Authority: migrations/**/*.cs, config/database/**

Coordination: BackendSpecialist designs services → DatabaseSpecialist optimizes data access
```

---

## CONCLUSION

### Authority Framework Mastery Achieved

You now understand three authority patterns for zarichney-api multi-agent team:

1. **Exclusive File Authority (Primary Agents):**
   - Glob patterns for unambiguous file ownership
   - Sequential workflow positioning
   - Zero coordination overhead for file type domain

2. **Flexible Authority (Specialist Agents):**
   - Intent recognition (query vs. command)
   - Dual-mode operation (advisory + implementation)
   - Domain expertise with efficiency gains

3. **Advisory-Only Authority (Analysis Agents):**
   - Zero file modifications
   - Working directory deliverables
   - Simplest coordination patterns

### Design Decision Framework Summary

| Scenario | Authority Pattern | Rationale |
|----------|------------------|-----------|
| Single clear file type owner | **Exclusive** | TestEngineer owns all test files |
| Domain expertise + shared files | **Flexible** | BackendSpecialist .cs files via command intent |
| Pure validation/analysis role | **Advisory-Only** | ComplianceOfficer pre-PR validation |

**Your next step:** Apply these authority patterns when creating new agents using agent-creation meta-skill, ensuring zarichney-api maintains zero-conflict multi-agent coordination.

---

**Document Status:** ✅ **COMPLETE**
**Version:** 1.0.0
**For:** PromptEngineer designing agent file edit rights and coordination protocols
**Next Steps:** Review skill-integration-patterns.md for progressive loading optimization strategies
