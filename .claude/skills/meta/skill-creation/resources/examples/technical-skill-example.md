# Skill Creation Example: Security Threat Modeling

**Skill Category**: Technical/Domain-Specific Skill
**Created**: 2025-10-25
**Purpose**: Demonstrate complete 5-phase workflow for creating a deep technical skill used by specialist agents

---

## Example Overview

This example shows how a **security-threat-modeling** skill would be created to provide SecurityAuditor and BackendSpecialist with comprehensive threat analysis frameworks. This skill contains deep technical knowledge about OWASP methodologies, attack vectors, and mitigation patterns specific to .NET 8 / Angular 19 applications.

### The Problem This Skill Solves
- **Knowledge Depth**: SecurityAuditor needed comprehensive threat modeling frameworks embedded in definition
- **Token Inefficiency**: 3,000+ tokens of OWASP methodology duplicated in agent definition
- **Specialist Collaboration**: BackendSpecialist needed same threat knowledge for secure implementation
- **Maintenance Burden**: Security best practices evolve, requiring updates in multiple locations

### The Solution Approach
Create a technical skill that:
- Provides comprehensive OWASP threat modeling methodology
- Offers .NET 8 / Angular 19 specific attack vectors and mitigations
- Enables progressive loading (quick reference → deep analysis)
- Supports both SecurityAuditor analysis and BackendSpecialist implementation

---

## Phase 1: Scope Definition & Discovery

### 1.1 Initial Scope Assessment

**CORE ISSUE IDENTIFIED**:
```yaml
Problem: SecurityAuditor lacks comprehensive threat modeling framework without excessive token overhead
Impact: Incomplete security analysis, missed vulnerability classes, inconsistent threat prioritization
Affected_Agents: SecurityAuditor (primary), BackendSpecialist (secondary for secure implementation)
Current_State: 3,000+ tokens of OWASP methodology embedded in SecurityAuditor definition
Desired_State: Progressive loading of threat modeling knowledge with domain-specific examples
```

**DESIGN DECISION #1**: This is a **Technical/Domain-Specific Skill** because:
- ✅ Used by 2 specialist agents (not all agents)
- ✅ Contains deep technical domain knowledge (security threat analysis)
- ✅ Provides specialist expertise (OWASP methodologies, attack patterns)
- ❌ NOT coordination-focused (solves knowledge depth, not communication)
- ✅ Benefits from progressive loading (quick reference → comprehensive analysis)

### 1.2 Token Budget Analysis

**Current Embedded State** (before skill extraction):
```markdown
# Agent Definition: SecurityAuditor.md

## OWASP Threat Modeling Methodology
### STRIDE Framework
- Spoofing: Identity impersonation attacks...
- Tampering: Data integrity violations...
- Repudiation: Denial of action attacks...
- Information Disclosure: Data leakage vulnerabilities...
- Denial of Service: Availability attacks...
- Elevation of Privilege: Authorization bypasses...
[~1,500 tokens of STRIDE details]

### Attack Vector Analysis
#### Authentication Attacks
- Credential stuffing patterns
- Session hijacking techniques
- Token theft vulnerabilities
[~800 tokens of attack vectors]

### .NET 8 Specific Vulnerabilities
- Deserialization exploits
- SQL injection in EF Core
- XSS in Razor views
[~700 tokens of platform-specific threats]

TOTAL EMBEDDED: ~3,000 tokens in SecurityAuditor.md
```

**Proposed Skill Reference State**:
```markdown
# Agent Definition: SecurityAuditor.md

## Threat Modeling Expertise
Use `/security-threat-modeling` skill for comprehensive OWASP methodologies, attack vector analysis, and .NET 8/Angular 19 specific threat patterns.

**Quick Reference**: STRIDE framework, OWASP Top 10 mapping, platform-specific vulnerabilities
[~50 tokens reference]

# Agent Definition: BackendSpecialist.md

## Security Implementation Support
See `/security-threat-modeling` skill for threat-aware implementation patterns when implementing security-sensitive features.

**Focus**: Mitigation strategies, secure coding patterns, defense-in-depth approaches
[~40 tokens reference]
```

**Token Savings Analysis**:
- **Before**: 3,000 tokens in SecurityAuditor + 0 in BackendSpecialist = 3,000 tokens
- **After**: 50 tokens (SecurityAuditor) + 40 tokens (BackendSpecialist) + 6,000 tokens (skill) = 90 tokens repeated references
- **Net Efficiency**: 3,000 embedded → 90 references = **97% reduction in repeated content**
- **Trade-off**: Skill contains MORE comprehensive content (6,000 tokens vs. 3,000 embedded) for better analysis quality

**DESIGN DECISION #2**: Invest in comprehensive skill content
- Embedded version was incomplete due to token pressure
- Skill approach enables 2× knowledge depth without repeated cost
- Progressive loading means agents only load needed sections

### 1.3 Skill Category Selection

**Template Choice**: `technical-skill-template.md`

**Rationale**:
- Domain-specific expertise (security threat analysis)
- Deep technical content (OWASP methodologies, attack patterns)
- Specialist audience (SecurityAuditor, BackendSpecialist)
- Progressive disclosure needed (quick reference → comprehensive analysis)
- NOT coordination-focused (knowledge provision, not workflow)

### 1.4 Progressive Loading Strategy

**Metadata Layer** (~120 tokens):
```yaml
name: security-threat-modeling
category: technical
domain: security-analysis
audience:
  primary: [SecurityAuditor]
  secondary: [BackendSpecialist, FrontendSpecialist]
trigger_patterns:
  - "threat model"
  - "STRIDE"
  - "OWASP"
  - "attack vector"
  - "vulnerability assessment"
technical_focus:
  - OWASP threat methodologies
  - STRIDE framework application
  - Platform-specific vulnerabilities (.NET 8, Angular 19)
  - Attack pattern recognition
  - Mitigation strategy selection
token_budget:
  metadata: ~120
  full_skill: ~6,000
  resources: ~4,000
  total: ~10,120
```

**SKILL.md Layer** (~6,000 tokens):
- STRIDE Framework (comprehensive methodology)
- OWASP Top 10 mapping to .NET 8 / Angular 19
- Attack Vector Taxonomy (authentication, authorization, data, infrastructure)
- Platform-Specific Vulnerabilities
- Threat Prioritization Matrix
- Mitigation Pattern Library

**Resources Layer** (~4,000 tokens):
- STRIDE analysis templates for common components
- Attack scenario examples (.NET API, Angular SPA)
- Mitigation implementation guides
- Threat model examples (complete analyses)

**Total Skill Budget**: ~10,120 tokens
- **Higher than embedded** (10,120 vs. 3,000) BUT
- **2× knowledge depth** enables better security analysis
- **Shared across agents** (SecurityAuditor + BackendSpecialist + FrontendSpecialist)
- **Progressive loading** means most engagements only load metadata + relevant sections (~2,000 tokens)

**DESIGN DECISION #3**: Technical skills justify higher token investment
- Quality of security analysis improved significantly with comprehensive framework
- Progressive loading enables "load only what you need" efficiency
- Shared knowledge across multiple specialists increases ROI

---

## Phase 2: Structure Design

### 2.1 Skill Structure Blueprint

**Directory Structure**:
```
.claude/skills/security-threat-modeling/
├── metadata.yaml                           # ~120 tokens
├── SKILL.md                                # ~6,000 tokens
└── resources/
    ├── frameworks/
    │   ├── stride-methodology.md           # Comprehensive STRIDE guide
    │   ├── owasp-top10-mapping.md          # OWASP Top 10 → platform mapping
    │   └── attack-vector-taxonomy.md       # Attack pattern classification
    ├── vulnerabilities/
    │   ├── dotnet8-vulnerabilities.md      # .NET 8 specific threats
    │   ├── angular19-vulnerabilities.md    # Angular 19 specific threats
    │   ├── api-vulnerabilities.md          # REST API attack patterns
    │   └── database-vulnerabilities.md     # SQL injection, data leakage
    ├── mitigations/
    │   ├── authentication-patterns.md      # Secure auth implementation
    │   ├── authorization-patterns.md       # Role/policy-based access control
    │   ├── data-protection-patterns.md     # Encryption, hashing, sanitization
    │   └── infrastructure-patterns.md      # Network, deployment, monitoring
    ├── templates/
    │   ├── stride-analysis-template.md     # Component threat modeling template
    │   ├── attack-surface-analysis.md      # Surface area assessment template
    │   └── security-requirements.md        # Security requirement specification
    └── examples/
        ├── api-threat-model-example.md     # Complete REST API threat analysis
        ├── spa-threat-model-example.md     # Complete Angular SPA threat analysis
        └── auth-flow-threat-model.md       # OAuth2 authentication threat model
```

**DESIGN DECISION #4**: Resource organization by security concern type
- **Frameworks**: Methodologies and analysis approaches
- **Vulnerabilities**: Platform-specific threat catalogs
- **Mitigations**: Secure implementation patterns
- **Templates**: Reusable analysis structures
- **Examples**: Complete threat model demonstrations

This structure enables:
- SecurityAuditor to load frameworks + vulnerabilities for analysis
- BackendSpecialist to load mitigations for secure implementation
- Both to reference examples for quality benchmarks

### 2.2 SKILL.md Content Structure

**Section Planning**:
```markdown
# Security Threat Modeling Skill

## Purpose & Scope
[Problem solved, specialist coverage, use cases]

## STRIDE Framework (Core Methodology)
### Spoofing Threats
[Identity impersonation attack patterns, detection, mitigation]
### Tampering Threats
[Data integrity violation patterns, detection, mitigation]
### Repudiation Threats
[Action denial patterns, detection, mitigation]
### Information Disclosure Threats
[Data leakage patterns, detection, mitigation]
### Denial of Service Threats
[Availability attack patterns, detection, mitigation]
### Elevation of Privilege Threats
[Authorization bypass patterns, detection, mitigation]

## OWASP Top 10 Platform Mapping
[Map OWASP vulnerabilities to .NET 8 / Angular 19 specifics]

## Attack Vector Taxonomy
### Authentication Attacks
### Authorization Attacks
### Data Attacks
### Infrastructure Attacks

## Threat Prioritization Matrix
[Risk assessment: Likelihood × Impact, CVSS scoring, remediation urgency]

## Platform-Specific Vulnerability Patterns
### .NET 8 Backend Vulnerabilities
### Angular 19 Frontend Vulnerabilities
### API Security Vulnerabilities
### Database Security Vulnerabilities

## Mitigation Pattern Library
[Cross-reference to resources/mitigations/ for implementation]

## Threat Modeling Workflow
[Step-by-step process for component threat analysis]
```

**Token Distribution**:
- STRIDE Framework: ~2,000 tokens (comprehensive coverage of all 6 categories)
- OWASP Mapping: ~800 tokens (Top 10 → platform-specific examples)
- Attack Vectors: ~1,200 tokens (taxonomy with detection guidance)
- Threat Prioritization: ~600 tokens (risk matrix, CVSS integration)
- Platform Vulnerabilities: ~1,000 tokens (quick reference, detailed in resources)
- Mitigation Patterns: ~400 tokens (overview, detailed in resources)

**Total**: ~6,000 tokens

**DESIGN DECISION #5**: SKILL.md provides complete analytical framework
- SecurityAuditor can perform threat analysis using only SKILL.md
- Resources provide deeper dive when specific vulnerability class encountered
- Progressive loading: Metadata → SKILL.md → Specific resource file as needed

### 2.3 Resource Organization

**Framework Philosophy**:
```yaml
frameworks/stride-methodology.md:
  Purpose: Comprehensive STRIDE application guide
  Content: Step-by-step analysis for each STRIDE category
  Audience: SecurityAuditor performing detailed component analysis
  Token_Budget: ~1,500 tokens

frameworks/owasp-top10-mapping.md:
  Purpose: OWASP Top 10 vulnerabilities mapped to tech stack
  Content: Each OWASP category with .NET/Angular specific examples
  Audience: SecurityAuditor and specialists understanding vulnerability landscape
  Token_Budget: ~1,200 tokens

frameworks/attack-vector-taxonomy.md:
  Purpose: Comprehensive attack pattern classification
  Content: Attack trees, exploit chains, detection signatures
  Audience: SecurityAuditor performing deep threat analysis
  Token_Budget: ~1,300 tokens
```

**Vulnerability Catalog Philosophy**:
```yaml
vulnerabilities/dotnet8-vulnerabilities.md:
  Purpose: .NET 8 specific vulnerability patterns
  Content: Deserialization, dependency injection, EF Core, middleware threats
  Audience: BackendSpecialist implementing secure .NET code
  Token_Budget: ~1,000 tokens

vulnerabilities/angular19-vulnerabilities.md:
  Purpose: Angular 19 specific vulnerability patterns
  Content: XSS, CSRF, DOM manipulation, routing, state management threats
  Audience: FrontendSpecialist implementing secure Angular code
  Token_Budget: ~900 tokens

vulnerabilities/api-vulnerabilities.md:
  Purpose: REST API attack patterns
  Content: Injection, broken auth, excessive data exposure, SSRF, security misconfig
  Audience: BackendSpecialist and SecurityAuditor for API security
  Token_Budget: ~1,100 tokens
```

**Mitigation Pattern Philosophy**:
```yaml
mitigations/authentication-patterns.md:
  Purpose: Secure authentication implementation patterns
  Content: OAuth2, JWT, session management, MFA, password policies
  Audience: BackendSpecialist implementing auth features
  Token_Budget: ~800 tokens

mitigations/authorization-patterns.md:
  Purpose: Secure authorization implementation patterns
  Content: RBAC, ABAC, policy-based access, resource-level permissions
  Audience: BackendSpecialist implementing authorization logic
  Token_Budget: ~700 tokens

mitigations/data-protection-patterns.md:
  Purpose: Data security implementation patterns
  Content: Encryption (at-rest, in-transit), hashing, sanitization, masking
  Audience: BackendSpecialist and FrontendSpecialist for data handling
  Token_Budget: ~900 tokens
```

**DESIGN DECISION #6**: Separate vulnerability catalogs from mitigation patterns
- SecurityAuditor focuses on vulnerabilities/ during analysis
- BackendSpecialist focuses on mitigations/ during implementation
- Clear separation reduces cognitive load and token overhead

---

## Phase 3: Progressive Loading Implementation

### 3.1 Metadata Design (Tier 1)

**File**: `metadata.yaml`
```yaml
# Security Threat Modeling Skill Metadata
name: security-threat-modeling
display_name: "Security Threat Modeling"
version: 1.0.0
category: technical
domain: security-analysis
description: >
  Comprehensive OWASP threat modeling methodologies, STRIDE framework application,
  and platform-specific vulnerability analysis for .NET 8 / Angular 19 applications.
  Provides attack vector taxonomy, threat prioritization, and mitigation pattern library.

audience:
  primary:
    - SecurityAuditor
  secondary:
    - BackendSpecialist
    - FrontendSpecialist
  use_cases:
    SecurityAuditor: "Comprehensive threat analysis, vulnerability assessment, risk prioritization"
    BackendSpecialist: "Threat-aware implementation, secure coding patterns, mitigation strategies"
    FrontendSpecialist: "Client-side security, XSS prevention, secure state management"

trigger_patterns:
  methodologies:
    - "threat model"
    - "STRIDE"
    - "OWASP"
    - "threat analysis"
  vulnerabilities:
    - "vulnerability assessment"
    - "attack vector"
    - "security vulnerability"
    - "exploit"
  mitigations:
    - "secure implementation"
    - "security pattern"
    - "mitigation strategy"

technical_focus:
  frameworks:
    - STRIDE threat modeling methodology
    - OWASP Top 10 application
    - Attack vector taxonomy
    - Threat prioritization matrix (Likelihood × Impact)
  platforms:
    - .NET 8 backend vulnerabilities
    - Angular 19 frontend vulnerabilities
    - REST API security patterns
    - Database security patterns
  capabilities:
    - Component threat modeling
    - Attack surface analysis
    - Risk assessment and prioritization
    - Mitigation pattern selection
    - Security requirement specification

token_budget:
  metadata: ~120
  full_skill: ~6,000
  resources:
    frameworks: ~4,000
    vulnerabilities: ~4,000
    mitigations: ~2,400
    templates: ~800
    examples: ~3,000
  total: ~20,320

progressive_loading_strategy:
  tier1_metadata: "Quick trigger matching, audience identification, token budget awareness"
  tier2_skill: "Complete STRIDE framework, OWASP mapping, attack taxonomy, threat prioritization"
  tier3_resources: "Deep-dive vulnerability catalogs, mitigation implementations, complete examples"
  typical_usage: "Metadata + SKILL.md (~6,120 tokens) for most threat analyses"
  deep_analysis: "Metadata + SKILL.md + specific resource files (~8,000-10,000 tokens)"

learning_resources:
  - "resources/examples/api-threat-model-example.md"
  - "resources/examples/spa-threat-model-example.md"
  - "resources/frameworks/stride-methodology.md"

related_skills:
  - security-compliance-frameworks
  - penetration-testing-methodologies
  - incident-response-protocols

maintenance:
  update_triggers:
    - "New OWASP Top 10 release"
    - ".NET or Angular major version updates"
    - "Emerging vulnerability patterns"
    - "Updated CVSS scoring methodology"
  review_frequency: quarterly
  owner: PromptEngineer

last_updated: 2025-10-25
```

**Token Count**: 118 tokens

**DESIGN DECISION #7**: Rich technical metadata enables:
- Precise skill matching based on security terminology
- Multi-audience support (SecurityAuditor vs. BackendSpecialist use cases)
- Token budget transparency for progressive loading decisions
- Maintenance planning for evolving security landscape

### 3.2 SKILL.md Design (Tier 2)

**Opening Context** (~400 tokens):
```markdown
# Security Threat Modeling Skill

## Purpose & Scope

### Problem Solved
Security vulnerabilities arise from incomplete threat analysis during design and implementation.
This skill provides:
- **Comprehensive Methodology**: STRIDE framework for systematic threat identification
- **Platform Expertise**: .NET 8 / Angular 19 specific vulnerability patterns
- **Attack Intelligence**: Taxonomy of attack vectors, exploit chains, detection signatures
- **Mitigation Guidance**: Secure implementation patterns and defense-in-depth strategies
- **Risk Prioritization**: Likelihood × Impact matrix for remediation sequencing

### Audience & Use Cases

**SecurityAuditor** (Primary):
- Perform comprehensive threat modeling for components, APIs, features
- Assess attack surface and identify vulnerability classes
- Prioritize threats using CVSS and business impact
- Recommend specific mitigations with implementation guidance

**BackendSpecialist** (Secondary):
- Understand threat landscape when implementing security-sensitive features
- Apply secure coding patterns addressing identified threats
- Validate mitigation effectiveness through threat model lens
- Design defense-in-depth architectures

**FrontendSpecialist** (Secondary):
- Analyze client-side attack vectors (XSS, CSRF, DOM manipulation)
- Implement secure state management and data handling
- Apply Angular-specific security patterns
- Coordinate with backend on security contract validation

### Integration with Security Workflow
1. **Design Phase**: Threat modeling using STRIDE framework
2. **Implementation Phase**: Mitigation pattern application
3. **Review Phase**: Vulnerability assessment against threat model
4. **Deployment Phase**: Attack surface validation and monitoring
```

**STRIDE Framework Section** (~2,000 tokens):
```markdown
## STRIDE Framework

### Overview
STRIDE is a comprehensive threat modeling methodology identifying six threat categories:
- **S**poofing: Identity impersonation and authentication attacks
- **T**ampering: Data integrity violations and manipulation
- **R**epudiation: Denial of actions and non-repudiation failures
- **I**nformation Disclosure: Data leakage and confidentiality breaches
- **D**enial of Service: Availability attacks and resource exhaustion
- **E**levation of Privilege: Authorization bypasses and privilege escalation

### Spoofing Threats

**Definition**: Attacker impersonates another entity (user, service, system) to gain unauthorized access.

**Common Patterns**:
- **Credential Theft**: Password phishing, keylogging, credential stuffing
- **Session Hijacking**: Session token theft via XSS, network sniffing, CSRF
- **Identity Forgery**: JWT manipulation, SAML assertion injection, OAuth token theft
- **Service Impersonation**: DNS spoofing, SSL stripping, man-in-the-middle attacks

**.NET 8 Specific Vulnerabilities**:
```csharp
// VULNERABLE: Weak authentication without MFA
[HttpPost("login")]
public IActionResult Login(LoginRequest request)
{
    var user = _userService.Authenticate(request.Username, request.Password);
    if (user == null) return Unauthorized();

    var token = GenerateJWT(user); // No MFA, no rate limiting
    return Ok(new { token });
}

// SECURE: Multi-factor authentication with rate limiting
[HttpPost("login")]
[RateLimit(MaxAttempts = 5, WindowMinutes = 15)]
public async Task<IActionResult> Login(LoginRequest request)
{
    var user = await _userService.AuthenticateAsync(request.Username, request.Password);
    if (user == null) return Unauthorized();

    // Require MFA for privilege escalation
    if (user.Roles.Contains("Admin"))
    {
        await _mfaService.SendChallengeAsync(user);
        return Ok(new { requiresMFA = true, challengeId = Guid.NewGuid() });
    }

    var token = GenerateSecureJWT(user);
    return Ok(new { token });
}
```

**Angular 19 Specific Vulnerabilities**:
```typescript
// VULNERABLE: Client-side token storage in localStorage
login(credentials: LoginRequest): Observable<void> {
  return this.http.post<AuthResponse>('/api/auth/login', credentials)
    .pipe(
      tap(response => {
        localStorage.setItem('token', response.token); // XSS can steal token
        this.currentUser$.next(response.user);
      })
    );
}

// SECURE: HttpOnly cookie with CSRF protection
login(credentials: LoginRequest): Observable<void> {
  return this.http.post<AuthResponse>('/api/auth/login', credentials, {
    withCredentials: true // Enables HttpOnly cookie
  }).pipe(
    tap(response => {
      // Token stored in HttpOnly cookie by server, not accessible to JavaScript
      this.currentUser$.next(response.user);
    })
  );
}
```

**Detection Signatures**:
- Multiple failed login attempts from same IP
- Successful login from geographically unlikely location
- Session token usage from multiple IPs simultaneously
- Unusual authentication flow deviations

**Mitigation Strategies**:
- **Multi-Factor Authentication** (TOTP, SMS, biometric)
- **Rate Limiting** on authentication endpoints
- **Anomaly Detection** for login patterns
- **HttpOnly + Secure Cookies** for session tokens
- **Certificate Pinning** for service-to-service communication

**Risk Assessment**:
- **Likelihood**: HIGH (authentication is primary attack target)
- **Impact**: CRITICAL (full account compromise, lateral movement)
- **CVSS Base Score**: 9.1 (Network, Low Complexity, No Privileges, Confidentiality+Integrity Impact)
- **Priority**: P0 - Address immediately in all authentication flows

---

### Tampering Threats

**Definition**: Attacker modifies data (in transit, at rest, or in memory) without authorization.

**Common Patterns**:
- **Data Manipulation**: SQL injection, parameter tampering, serialized object modification
- **Code Injection**: XSS, command injection, template injection
- **Cryptographic Attacks**: Hash collision, encryption oracle, padding oracle
- **File/Configuration Tampering**: Malicious file upload, config override, race conditions

**.NET 8 Specific Vulnerabilities**:
```csharp
// VULNERABLE: SQL injection via string concatenation
public async Task<User> GetUserByUsername(string username)
{
    var sql = $"SELECT * FROM Users WHERE Username = '{username}'"; // INJECTION!
    return await _context.Users.FromSqlRaw(sql).FirstOrDefaultAsync();
}

// SECURE: Parameterized query with EF Core
public async Task<User> GetUserByUsername(string username)
{
    return await _context.Users
        .Where(u => u.Username == username) // EF Core parameterizes automatically
        .FirstOrDefaultAsync();
}

// VULNERABLE: Deserialization without type validation
public IActionResult ProcessCommand([FromBody] object command)
{
    var json = JsonSerializer.Serialize(command);
    var cmd = JsonSerializer.Deserialize<ICommand>(json); // Polymorphic deserialization attack
    cmd.Execute();
    return Ok();
}

// SECURE: Explicit type validation and allowlist
public IActionResult ProcessCommand([FromBody] CommandDto commandDto)
{
    if (!_allowedCommandTypes.Contains(commandDto.Type))
        return BadRequest("Invalid command type");

    var cmd = _commandFactory.Create(commandDto); // Type-safe factory
    cmd.Execute();
    return Ok();
}
```

**Angular 19 Specific Vulnerabilities**:
```typescript
// VULNERABLE: DOM XSS via innerHTML
displayUserContent(content: string): void {
  this.elementRef.nativeElement.innerHTML = content; // XSS!
}

// SECURE: Angular sanitization with DomSanitizer
constructor(private sanitizer: DomSanitizer) {}

displayUserContent(content: string): SafeHtml {
  return this.sanitizer.sanitize(SecurityContext.HTML, content) || '';
}
```

**Detection Signatures**:
- Unusual SQL query patterns or execution times
- File modifications outside expected change windows
- Checksum/hash validation failures
- Unexpected data type or format in API requests

**Mitigation Strategies**:
- **Parameterized Queries** (ORM, prepared statements)
- **Input Validation** (allowlist, type checking, length limits)
- **Output Encoding** (context-aware escaping)
- **Cryptographic Integrity** (HMAC, digital signatures)
- **File Upload Restrictions** (type validation, size limits, sandboxing)
- **Immutable Infrastructure** (configuration management, version control)

**Risk Assessment**:
- **Likelihood**: HIGH (injection attacks are prevalent)
- **Impact**: CRITICAL (data breach, code execution, privilege escalation)
- **CVSS Base Score**: 8.8 (Network, Low Complexity, Low Privileges, Confidentiality+Integrity+Availability Impact)
- **Priority**: P0 - Address in all data input/output boundaries

---

[Additional STRIDE categories: Repudiation, Information Disclosure, Denial of Service, Elevation of Privilege follow similar structure]

[Each category ~300-400 tokens with patterns, platform vulnerabilities, detection, mitigation, risk assessment]
```

**OWASP Top 10 Mapping Section** (~800 tokens):
```markdown
## OWASP Top 10 Platform Mapping

### A01:2021 – Broken Access Control

**.NET 8 Manifestation**:
- Missing `[Authorize]` attributes on controllers/actions
- Inconsistent policy enforcement across endpoints
- Direct object reference without ownership validation

**Example Vulnerability**:
```csharp
// VULNERABLE: No authorization check for resource ownership
[HttpGet("recipes/{id}")]
public async Task<IActionResult> GetRecipe(int id)
{
    var recipe = await _context.Recipes.FindAsync(id);
    return Ok(recipe); // Any authenticated user can access any recipe
}

// SECURE: Resource-level authorization
[HttpGet("recipes/{id}")]
[Authorize]
public async Task<IActionResult> GetRecipe(int id)
{
    var recipe = await _context.Recipes.FindAsync(id);
    if (recipe == null) return NotFound();

    // Verify user owns recipe or has admin role
    if (recipe.UserId != User.GetUserId() && !User.IsInRole("Admin"))
        return Forbid();

    return Ok(recipe);
}
```

**Angular 19 Manifestation**:
- Client-side route guards only (no backend validation)
- UI element hiding without enforcing access control
- Role information exposed in JWT accessible to client

**Mitigation**:
- Implement authorization policies at API layer (never trust client)
- Use resource-based authorization for ownership validation
- Apply defense-in-depth with route guards + API authorization

---

[Additional OWASP categories: A02 Cryptographic Failures, A03 Injection, etc. follow similar platform-specific mapping]

[Each category ~80-100 tokens with .NET + Angular specific examples and mitigations]
```

**Attack Vector Taxonomy Section** (~1,200 tokens):
```markdown
## Attack Vector Taxonomy

### Authentication Attacks

**Credential-Based Attacks**:
- **Brute Force**: Systematic password guessing (mitigate with rate limiting, account lockout)
- **Credential Stuffing**: Using leaked credentials from other breaches (mitigate with breach detection, MFA)
- **Password Spraying**: Trying common passwords across many accounts (mitigate with password complexity, anomaly detection)

**Session-Based Attacks**:
- **Session Hijacking**: Stealing session tokens via XSS, network sniffing (mitigate with HttpOnly cookies, HTTPS, short expiration)
- **Session Fixation**: Forcing victim to use attacker-controlled session (mitigate with session regeneration on login)
- **CSRF**: Tricking authenticated user into unwanted actions (mitigate with anti-CSRF tokens, SameSite cookies)

**Token-Based Attacks**:
- **JWT Tampering**: Modifying JWT claims or signature (mitigate with signature validation, short expiration)
- **Token Replay**: Reusing captured tokens (mitigate with short expiration, token rotation, one-time tokens)
- **None Algorithm Attack**: Exploiting JWT libraries accepting "none" algorithm (mitigate with algorithm allowlist)

---

### Authorization Attacks

**Privilege Escalation**:
- **Vertical Escalation**: User gaining admin privileges (mitigate with strict RBAC, principle of least privilege)
- **Horizontal Escalation**: Accessing other users' resources (mitigate with resource-level authorization)

**Insecure Direct Object Reference (IDOR)**:
- **Predictable IDs**: Sequential IDs enabling enumeration (mitigate with UUIDs, authorization checks)
- **Missing Ownership Validation**: No check if user owns requested resource (mitigate with ownership queries)

**Parameter Tampering**:
- **Role/Permission Manipulation**: Modifying role claims in requests (mitigate with server-side validation only)
- **Price/Quantity Manipulation**: Changing transaction amounts (mitigate with server-side calculation, audit logging)

---

[Additional attack vector categories: Data Attacks, Infrastructure Attacks follow similar taxonomy]

[Each category provides attack tree, exploit chain, detection signature, mitigation pattern]
```

**Threat Prioritization Matrix Section** (~600 tokens):
```markdown
## Threat Prioritization Matrix

### Risk Assessment Framework

**Likelihood Assessment**:
- **HIGH**: Easily exploitable, common attack pattern, publicly known vulnerabilities
- **MEDIUM**: Requires moderate skill/access, attack tools available
- **LOW**: Requires advanced skill, specific conditions, limited opportunity

**Impact Assessment**:
- **CRITICAL**: Complete system compromise, data breach, financial loss
- **HIGH**: Significant data exposure, service disruption, user impact
- **MEDIUM**: Limited data exposure, degraded functionality
- **LOW**: Minimal impact, edge case scenarios

**Risk Matrix**:
```
              Impact →
Likelihood ↓  | LOW    | MEDIUM | HIGH   | CRITICAL
-------------------------------------------------
HIGH          | P2     | P1     | P0     | P0
MEDIUM        | P3     | P2     | P1     | P0
LOW           | P4     | P3     | P2     | P1
```

**Priority Definitions**:
- **P0 (Critical)**: Immediate remediation required, block deployment
- **P1 (High)**: Address within current sprint, high priority
- **P2 (Medium)**: Address within 1-2 sprints, normal priority
- **P3 (Low)**: Address in backlog, opportunistic fixes
- **P4 (Informational)**: Document for future consideration

**CVSS Integration**:
```yaml
CVSS_Base_Score: (Attack Vector × Attack Complexity × Privileges Required × User Interaction)
                 × (Confidentiality Impact + Integrity Impact + Availability Impact)

Priority_Mapping:
  CVSS_9.0-10.0: P0 (Critical)
  CVSS_7.0-8.9:  P1 (High)
  CVSS_4.0-6.9:  P2 (Medium)
  CVSS_0.1-3.9:  P3 (Low)
```

**Example Threat Prioritization**:
```
Threat: SQL Injection in recipe search endpoint
- Likelihood: HIGH (common attack, easy to exploit)
- Impact: CRITICAL (database access, data breach)
- CVSS: 9.8 (Network, Low Complexity, No Privileges, All Impacts)
- Priority: P0 - Immediate remediation required

Threat: Username enumeration via timing attack
- Likelihood: MEDIUM (requires analysis, moderate skill)
- Impact: LOW (only reveals valid usernames)
- CVSS: 3.7 (Network, High Complexity, No Privileges, Confidentiality Impact only)
- Priority: P3 - Address in backlog
```
```

**Platform Vulnerabilities Section** (~1,000 tokens):
```markdown
## Platform-Specific Vulnerability Patterns

### .NET 8 Backend Vulnerabilities

**Deserialization Exploits**:
- **Risk**: Remote code execution via malicious serialized objects
- **Vulnerable Code**: `BinaryFormatter`, `NetDataContractSerializer` with untrusted input
- **Mitigation**: Use `System.Text.Json` with type allowlists, avoid polymorphic deserialization

**EF Core Injection**:
- **Risk**: SQL injection via `FromSqlRaw` with unsanitized input
- **Vulnerable Code**: String concatenation in raw SQL queries
- **Mitigation**: Use parameterized LINQ queries, `FromSqlInterpolated` for necessary raw SQL

**Middleware Misconfiguration**:
- **Risk**: Authentication/authorization bypasses via middleware ordering
- **Vulnerable Code**: Authorization middleware before authentication middleware
- **Mitigation**: Enforce middleware order: Authentication → Authorization → Endpoint routing

**Dependency Injection Exploits**:
- **Risk**: Service substitution attacks in multi-tenant scenarios
- **Vulnerable Code**: Singleton services with tenant-specific data
- **Mitigation**: Use scoped services for tenant data, validate service resolution

---

### Angular 19 Frontend Vulnerabilities

**Cross-Site Scripting (XSS)**:
- **Risk**: Code execution in user's browser via DOM manipulation
- **Vulnerable Code**: `innerHTML`, `bypassSecurityTrust*` with unsanitized user input
- **Mitigation**: Use Angular templates, `DomSanitizer` for necessary HTML, Content Security Policy

**Cross-Site Request Forgery (CSRF)**:
- **Risk**: Unauthorized actions via authenticated user's session
- **Vulnerable Code**: State-changing requests without anti-CSRF tokens
- **Mitigation**: Angular `HttpClient` XSRF protection, `SameSite` cookies, verify origin headers

**Client-Side Data Exposure**:
- **Risk**: Sensitive data accessible in browser storage, source code, network traffic
- **Vulnerable Code**: Storing tokens in `localStorage`, hardcoded API keys, unencrypted sensitive data
- **Mitigation**: Use HttpOnly cookies for tokens, environment variables for keys, encrypt sensitive client data

**Routing Vulnerabilities**:
- **Risk**: Unauthorized route access via client-side guard bypasses
- **Vulnerable Code**: Route guards only (no backend authorization validation)
- **Mitigation**: Defense-in-depth with guards + API authorization, validate permissions server-side

---

[Additional platform sections: API Vulnerabilities, Database Vulnerabilities follow similar structure]

[Each provides vulnerability classes, code examples, detection, mitigation]
```

**Mitigation Pattern Library Section** (~400 tokens):
```markdown
## Mitigation Pattern Library

### Authentication Patterns
See `resources/mitigations/authentication-patterns.md` for comprehensive implementation:
- OAuth2 / OpenID Connect integration
- JWT generation, validation, refresh strategies
- Multi-factor authentication (TOTP, SMS, biometric)
- Session management (creation, rotation, termination)
- Password policies (complexity, history, breach detection)

### Authorization Patterns
See `resources/mitigations/authorization-patterns.md` for comprehensive implementation:
- Role-Based Access Control (RBAC)
- Attribute-Based Access Control (ABAC)
- Policy-based authorization (.NET Authorization Policies)
- Resource-level ownership validation
- Least privilege principle application

### Data Protection Patterns
See `resources/mitigations/data-protection-patterns.md` for comprehensive implementation:
- Encryption at rest (database encryption, file encryption)
- Encryption in transit (TLS 1.3, certificate management)
- Hashing (password hashing with bcrypt/Argon2, HMAC for integrity)
- Sanitization (input validation, output encoding, SQL parameterization)
- Masking (PII redaction, sensitive data logging prevention)

### Infrastructure Patterns
See `resources/mitigations/infrastructure-patterns.md` for comprehensive implementation:
- Network security (firewalls, segmentation, DMZ architecture)
- Deployment security (secrets management, environment isolation)
- Monitoring and logging (SIEM integration, anomaly detection, audit trails)
- Dependency management (vulnerability scanning, SCA tools, update policies)

**Pattern Selection Guidance**:
1. **Identify Threat Category** (STRIDE)
2. **Map to Mitigation Pattern** (authentication, authorization, data protection, infrastructure)
3. **Select Platform-Specific Implementation** (.NET 8, Angular 19, infrastructure)
4. **Apply Defense-in-Depth** (multiple layers, fail-secure design)
5. **Validate Effectiveness** (penetration testing, threat model review)
```

**Total SKILL.md**: ~6,000 tokens

### 3.3 Resources Design (Tier 3)

**Vulnerability Catalog Example**: `resources/vulnerabilities/dotnet8-vulnerabilities.md`
```markdown
# .NET 8 Specific Vulnerabilities

## Deserialization Exploits

### Overview
.NET deserialization vulnerabilities enable remote code execution by exploiting polymorphic type handling during object reconstruction from serialized formats (JSON, XML, binary).

### Vulnerable Patterns

**BinaryFormatter (DEPRECATED - DO NOT USE)**:
```csharp
// CRITICAL VULNERABILITY: Remote code execution
public object DeserializeData(byte[] data)
{
    var formatter = new BinaryFormatter(); // NEVER USE
    using var stream = new MemoryStream(data);
    return formatter.Deserialize(stream); // RCE via malicious payload
}
```

**Exploit Example**:
Attacker crafts serialized payload containing malicious `ObjectDataProvider` or `TypeConfuseDelegate` enabling arbitrary code execution:
```csharp
// Malicious payload (simplified)
ObjectDataProvider payload = new()
{
    MethodName = "Start",
    ObjectInstance = new Process(),
    MethodParameters = { "calc.exe" } // Executes calculator (proof of concept)
};
```

**System.Text.Json Polymorphic Deserialization**:
```csharp
// VULNERABLE: Type confusion attack via polymorphic deserialization
[JsonPolymorphic]
[JsonDerivedType(typeof(AdminCommand), "admin")]
[JsonDerivedType(typeof(UserCommand), "user")]
public interface ICommand { void Execute(); }

public IActionResult ProcessCommand([FromBody] ICommand command)
{
    command.Execute(); // Attacker can send "admin" type even without privileges
    return Ok();
}
```

### Secure Alternatives

**System.Text.Json with Type Allowlist**:
```csharp
// SECURE: Explicit type validation with allowlist
public class CommandDto
{
    public string Type { get; set; }
    public JsonElement Data { get; set; }
}

private static readonly HashSet<string> AllowedCommandTypes = new()
{
    "CreateRecipe",
    "UpdateRecipe",
    "DeleteRecipe"
};

public IActionResult ProcessCommand([FromBody] CommandDto commandDto)
{
    if (!AllowedCommandTypes.Contains(commandDto.Type))
        return BadRequest("Invalid command type");

    // Type-safe deserialization based on validated type
    var command = commandDto.Type switch
    {
        "CreateRecipe" => JsonSerializer.Deserialize<CreateRecipeCommand>(commandDto.Data),
        "UpdateRecipe" => JsonSerializer.Deserialize<UpdateRecipeCommand>(commandDto.Data),
        "DeleteRecipe" => JsonSerializer.Deserialize<DeleteRecipeCommand>(commandDto.Data),
        _ => throw new InvalidOperationException()
    };

    command.Execute();
    return Ok();
}
```

**Mitigation Checklist**:
- [ ] Remove all `BinaryFormatter`, `NetDataContractSerializer`, `SoapFormatter` usage
- [ ] Use `System.Text.Json` with concrete types (avoid polymorphic deserialization)
- [ ] Implement type allowlists for dynamic type handling
- [ ] Validate input types before deserialization
- [ ] Apply least privilege to deserialization processes
- [ ] Monitor for unusual deserialization patterns

---

[Additional .NET 8 vulnerability sections: EF Core Injection, Middleware Misconfiguration, DI Exploits, etc.]

[Each ~200-300 tokens with vulnerability explanation, exploit example, secure alternative, mitigation checklist]
```

**Mitigation Pattern Example**: `resources/mitigations/authentication-patterns.md`
```markdown
# Authentication Security Patterns

## OAuth2 / OpenID Connect Implementation

### Architecture Overview
```
┌─────────────────┐         ┌──────────────────┐         ┌────────────────┐
│  Angular SPA    │         │  .NET 8 API      │         │  Identity      │
│  (Public        │◄────────┤  (Resource       │◄────────┤  Provider      │
│   Client)       │  Access │   Server)        │  Token  │  (Auth Server) │
└─────────────────┘  Token  └──────────────────┘  Valid. └────────────────┘
```

### Authorization Code Flow with PKCE (Recommended)

**Why PKCE**: Prevents authorization code interception attacks on public clients (SPAs, mobile apps)

**Angular 19 Implementation**:
```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { OAuthService, AuthConfig } from 'angular-oauth2-oidc';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private authConfig: AuthConfig = {
    issuer: 'https://identity.example.com',
    redirectUri: window.location.origin + '/callback',
    clientId: 'zarichney-spa',
    responseType: 'code', // Authorization Code Flow
    scope: 'openid profile email api.read api.write',
    showDebugInformation: false,
    useSilentRefresh: true,
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html'
  };

  constructor(private oauthService: OAuthService) {
    this.oauthService.configure(this.authConfig);
    this.oauthService.setupAutomaticSilentRefresh(); // Token refresh
    this.oauthService.loadDiscoveryDocumentAndLogin(); // PKCE enabled by default
  }

  login(): void {
    this.oauthService.initLoginFlow(); // Generates PKCE code_challenge
  }

  logout(): void {
    this.oauthService.revokeTokenAndLogout(); // Revoke refresh token
  }

  get accessToken(): string {
    return this.oauthService.getAccessToken(); // Managed by library, not in localStorage
  }
}
```

**.NET 8 API Token Validation**:
```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://identity.example.com";
        options.Audience = "zarichney-api";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://identity.example.com",
            ValidateAudience = true,
            ValidAudience = "zarichney-api",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2), // Tolerance for clock drift
            ValidateIssuerSigningKey = true
        };

        // Additional security: verify token isn't revoked
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var tokenService = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenRevocationService>();

                var jti = context.Principal.FindFirst("jti")?.Value;
                if (await tokenService.IsTokenRevokedAsync(jti))
                {
                    context.Fail("Token has been revoked");
                }
            }
        };
    });
```

**Security Checklist**:
- [ ] Use Authorization Code Flow with PKCE (never Implicit Flow)
- [ ] Store tokens securely (OAuth library management, never localStorage)
- [ ] Implement token refresh (silent refresh for seamless UX)
- [ ] Validate `iss`, `aud`, `exp`, `nbf` claims server-side
- [ ] Verify token signature using OIDC discovery document
- [ ] Implement token revocation for logout
- [ ] Use short access token expiration (15 minutes) with refresh tokens
- [ ] Apply rate limiting to token endpoints
- [ ] Monitor for unusual token usage patterns

---

[Additional authentication pattern sections: JWT Custom Implementation, Multi-Factor Authentication, Session Management, Password Policies, etc.]

[Each ~300-400 tokens with architecture, code examples, security checklist]
```

**Complete Example**: `resources/examples/api-threat-model-example.md`
```markdown
# Complete Threat Model Example: Recipe API Endpoints

## Component Overview

**Scope**: Recipe CRUD API endpoints in Zarichney.Server
**Technology**: .NET 8 ASP.NET Core Web API
**Authentication**: JWT Bearer tokens (OAuth2)
**Authorization**: Resource-level ownership validation

**Endpoints**:
- `POST /api/recipes` - Create new recipe
- `GET /api/recipes/{id}` - Retrieve recipe by ID
- `PUT /api/recipes/{id}` - Update existing recipe
- `DELETE /api/recipes/{id}` - Delete recipe

## STRIDE Analysis

### Spoofing Threats

**Threat S1**: Attacker forges JWT to impersonate legitimate user
- **Attack Vector**: JWT signing key compromise or algorithm confusion attack
- **Likelihood**: LOW (requires key compromise or implementation flaw)
- **Impact**: CRITICAL (full account impersonation)
- **CVSS**: 9.1
- **Mitigation**:
  - Use strong signing algorithm (RS256, ES256)
  - Rotate signing keys quarterly
  - Validate `iss`, `aud`, `exp` claims
  - Implement token revocation for compromised tokens
- **Status**: MITIGATED (JWT validation in place)

**Threat S2**: Attacker hijacks user session via XSS token theft
- **Attack Vector**: XSS vulnerability in frontend enabling `localStorage` token access
- **Likelihood**: MEDIUM (XSS vulnerabilities common in SPAs)
- **Impact**: HIGH (session hijacking, unauthorized recipe access)
- **CVSS**: 7.4
- **Mitigation**:
  - Store tokens in memory only (not `localStorage`)
  - Implement Content Security Policy (CSP)
  - Apply Angular sanitization for user-generated content
  - Use HttpOnly cookies for refresh tokens
- **Status**: MITIGATED (token storage in OAuth service, CSP configured)

---

### Tampering Threats

**Threat T1**: SQL injection in recipe search/filter functionality
- **Attack Vector**: Unsanitized search parameters in raw SQL queries
- **Likelihood**: MEDIUM (if `FromSqlRaw` used with string concatenation)
- **Impact**: CRITICAL (database compromise, data exfiltration)
- **CVSS**: 8.8
- **Mitigation**:
  - Use parameterized LINQ queries (EF Core automatic parameterization)
  - Avoid `FromSqlRaw` with user input
  - If raw SQL necessary, use `FromSqlInterpolated`
  - Apply input validation (length limits, character allowlist)
- **Status**: MITIGATED (LINQ queries only, no raw SQL)

**Threat T2**: Recipe data manipulation via parameter tampering
- **Attack Vector**: Modifying `userId` field in update request to steal recipe ownership
- **Likelihood**: HIGH (common attack if no ownership validation)
- **Impact**: HIGH (unauthorized recipe modification/deletion)
- **CVSS**: 7.1
- **Mitigation**:
  - Server-side ownership validation (never trust client-provided userId)
  - Verify `recipe.UserId == User.GetUserId()` before updates
  - Use `[Authorize]` attribute on all endpoints
  - Implement resource-based authorization policies
- **Status**: MITIGATED (ownership validation implemented)

---

[Additional STRIDE categories: Repudiation, Information Disclosure, Denial of Service, Elevation of Privilege]

[Each threat includes: ID, attack vector, likelihood, impact, CVSS, mitigation, status]

## Threat Summary

| ID | Category | Threat | Likelihood | Impact | CVSS | Priority | Status |
|----|----------|--------|------------|--------|------|----------|--------|
| S1 | Spoofing | JWT forgery | LOW | CRITICAL | 9.1 | P0 | MITIGATED |
| S2 | Spoofing | Session hijacking | MEDIUM | HIGH | 7.4 | P1 | MITIGATED |
| T1 | Tampering | SQL injection | MEDIUM | CRITICAL | 8.8 | P0 | MITIGATED |
| T2 | Tampering | Ownership tampering | HIGH | HIGH | 7.1 | P1 | MITIGATED |
| R1 | Repudiation | Audit log bypass | LOW | MEDIUM | 4.2 | P3 | OPEN |
| I1 | Info Disclosure | Recipe enumeration | HIGH | LOW | 3.7 | P3 | ACCEPTED RISK |
| I2 | Info Disclosure | PII in logs | MEDIUM | MEDIUM | 5.4 | P2 | MITIGATED |
| D1 | DoS | Recipe upload flood | MEDIUM | MEDIUM | 5.9 | P2 | MITIGATED |
| E1 | Elevation | Admin role escalation | LOW | CRITICAL | 8.2 | P0 | MITIGATED |

**Summary**:
- **Total Threats**: 9 identified
- **P0 (Critical)**: 3 threats, all MITIGATED
- **P1 (High)**: 2 threats, all MITIGATED
- **P2 (Medium)**: 2 threats, all MITIGATED
- **P3 (Low)**: 2 threats, 1 OPEN (planned for next sprint), 1 ACCEPTED RISK

**Overall Risk Assessment**: LOW - All critical and high-priority threats mitigated

## Implementation Validation

**Secure Code Example** (demonstrating mitigations):
```csharp
[ApiController]
[Route("api/recipes")]
[Authorize] // Spoofing mitigation: Require authentication
public class RecipesController : ControllerBase
{
    private readonly RecipeDbContext _context;
    private readonly ILogger<RecipesController> _logger;

    [HttpPost]
    [RateLimit(MaxRequests = 10, WindowMinutes = 1)] // DoS mitigation: Rate limiting
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeDto dto)
    {
        // Tampering mitigation: Input validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Spoofing mitigation: Use authenticated user ID (never trust client)
        var userId = User.GetUserId();

        var recipe = new Recipe
        {
            Title = dto.Title,
            Ingredients = dto.Ingredients,
            Instructions = dto.Instructions,
            UserId = userId // Server-side ownership assignment
        };

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // Repudiation mitigation: Audit logging
        _logger.LogInformation(
            "User {UserId} created recipe {RecipeId}",
            userId, recipe.Id);

        return CreatedAtAction(nameof(GetRecipe), new { id = recipe.Id }, recipe);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRecipe(int id, [FromBody] UpdateRecipeDto dto)
    {
        // Tampering mitigation: SQL injection prevention (parameterized LINQ)
        var recipe = await _context.Recipes.FindAsync(id);
        if (recipe == null) return NotFound();

        // Elevation mitigation: Resource-level ownership validation
        var userId = User.GetUserId();
        if (recipe.UserId != userId && !User.IsInRole("Admin"))
        {
            _logger.LogWarning(
                "User {UserId} attempted unauthorized access to recipe {RecipeId}",
                userId, recipe.Id);
            return Forbid();
        }

        // Tampering mitigation: Input validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        recipe.Title = dto.Title;
        recipe.Ingredients = dto.Ingredients;
        recipe.Instructions = dto.Instructions;

        await _context.SaveChangesAsync();

        // Repudiation mitigation: Audit logging
        _logger.LogInformation(
            "User {UserId} updated recipe {RecipeId}",
            userId, recipe.Id);

        return Ok(recipe);
    }
}
```

## Ongoing Security Validation

**Penetration Testing Scenarios**:
- [ ] Attempt JWT forgery and algorithm confusion attacks
- [ ] Test SQL injection vectors in search/filter parameters
- [ ] Verify ownership validation prevents horizontal privilege escalation
- [ ] Confirm rate limiting prevents recipe upload flooding
- [ ] Validate audit logging captures all state-changing operations

**Continuous Monitoring**:
- Monitor for unusual recipe access patterns (enumeration attempts)
- Alert on authentication failures exceeding threshold (brute force detection)
- Track recipe creation rates for DoS early warning
- Review audit logs for privilege escalation attempts

**Next Threat Model Review**: After recipe sharing feature implementation (introduces new attack surface)
```

---

## Phase 4: Resource Organization

### 4.1 Resource Inventory

**Frameworks** (Methodologies):
- ✅ `stride-methodology.md` - Step-by-step STRIDE application
- ✅ `owasp-top10-mapping.md` - OWASP → platform mapping
- ✅ `attack-vector-taxonomy.md` - Attack classification

**Vulnerabilities** (Platform-specific catalogs):
- ✅ `dotnet8-vulnerabilities.md` - .NET 8 threats
- ✅ `angular19-vulnerabilities.md` - Angular 19 threats
- ✅ `api-vulnerabilities.md` - REST API threats
- ✅ `database-vulnerabilities.md` - SQL injection, data leakage

**Mitigations** (Secure patterns):
- ✅ `authentication-patterns.md` - OAuth2, JWT, MFA, sessions
- ✅ `authorization-patterns.md` - RBAC, ABAC, policies
- ✅ `data-protection-patterns.md` - Encryption, hashing, sanitization
- ✅ `infrastructure-patterns.md` - Network, deployment, monitoring

**Templates** (Analysis structures):
- ✅ `stride-analysis-template.md` - Component threat modeling
- ✅ `attack-surface-analysis.md` - Surface area assessment
- ✅ `security-requirements.md` - Security spec template

**Examples** (Complete analyses):
- ✅ `api-threat-model-example.md` - Recipe API STRIDE analysis
- ✅ `spa-threat-model-example.md` - Angular SPA threat model
- ✅ `auth-flow-threat-model.md` - OAuth2 flow analysis

### 4.2 Usage Patterns by Agent

**SecurityAuditor Analysis Workflow**:
1. Load `SKILL.md` for STRIDE framework (~6,000 tokens)
2. Load `resources/frameworks/stride-methodology.md` for detailed analysis steps (~1,500 tokens)
3. Load `resources/vulnerabilities/dotnet8-vulnerabilities.md` or `api-vulnerabilities.md` for specific threat catalog (~1,000 tokens)
4. Reference `resources/examples/api-threat-model-example.md` for quality benchmark (~2,000 tokens)
5. **Total typical usage**: ~10,500 tokens for comprehensive threat analysis

**BackendSpecialist Secure Implementation Workflow**:
1. Load `SKILL.md` for threat awareness (~6,000 tokens)
2. Load `resources/mitigations/authentication-patterns.md` or `authorization-patterns.md` for implementation (~800 tokens)
3. Reference `resources/vulnerabilities/dotnet8-vulnerabilities.md` for pitfalls to avoid (~1,000 tokens)
4. **Total typical usage**: ~7,800 tokens for threat-aware implementation

**FrontendSpecialist Secure Implementation Workflow**:
1. Load `SKILL.md` for client-side threat awareness (~6,000 tokens)
2. Load `resources/mitigations/data-protection-patterns.md` for sanitization, CSP (~900 tokens)
3. Load `resources/vulnerabilities/angular19-vulnerabilities.md` for XSS/CSRF patterns (~900 tokens)
4. **Total typical usage**: ~7,800 tokens for secure Angular development

### 4.3 Progressive Loading Efficiency

**Scenario 1: Quick threat category identification**
- Load: Metadata (~120 tokens)
- Action: Identify skill relevance, determine if deeper analysis needed
- Efficiency: 99% token savings vs. loading full skill

**Scenario 2: Comprehensive STRIDE analysis**
- Load: Metadata + SKILL.md (~6,120 tokens)
- Action: Perform complete STRIDE threat modeling
- Efficiency: 40% token savings vs. loading all resources

**Scenario 3: Deep vulnerability analysis (specific platform)**
- Load: Metadata + SKILL.md + specific vulnerability catalog (~7,120 tokens)
- Action: Detailed .NET 8 vulnerability assessment
- Efficiency: 30% token savings vs. loading all vulnerability catalogs

**Scenario 4: Complete threat model with examples**
- Load: All resources (~10,120 tokens)
- Action: Comprehensive analysis with quality benchmarking
- Efficiency: Maximum knowledge depth, still 50% more efficient than embedding equivalent knowledge across multiple agents

---

## Phase 5: Agent Integration

### 5.1 Agent Definition Updates

**Before Skill Extraction** (SecurityAuditor.md):
```markdown
# Agent: SecurityAuditor

## OWASP Threat Modeling Methodology
### STRIDE Framework
- Spoofing: Identity impersonation attacks, credential theft, session hijacking...
- Tampering: Data integrity violations, SQL injection, parameter tampering...
[~3,000 tokens of embedded security knowledge]

## .NET 8 Specific Vulnerabilities
### Deserialization Exploits
- BinaryFormatter remote code execution risks...
[~800 tokens of platform-specific threats]

TOTAL EMBEDDED: ~3,800 tokens
```

**After Skill Extraction** (SecurityAuditor.md):
```markdown
# Agent: SecurityAuditor

## Threat Modeling Expertise
Use `/security-threat-modeling` skill for comprehensive security analysis:
- **STRIDE Framework**: Systematic threat identification across 6 categories
- **OWASP Top 10**: Platform-specific vulnerability mapping
- **Attack Vectors**: Authentication, authorization, data, infrastructure threats
- **Threat Prioritization**: Likelihood × Impact matrix, CVSS scoring
- **Platform Expertise**: .NET 8, Angular 19, API, database vulnerabilities

**Progressive Loading**:
- Load SKILL.md for complete STRIDE methodology
- Load resources/vulnerabilities/ for platform-specific deep dives
- Load resources/examples/ for quality benchmarking

REFERENCE: ~50 tokens (99% reduction from embedded)
```

**Before Skill Extraction** (BackendSpecialist.md):
```markdown
# Agent: BackendSpecialist

## Security Considerations
When implementing security-sensitive features, consider:
- Input validation and sanitization
- Parameterized queries for SQL injection prevention
- Authentication and authorization patterns
[~400 tokens of generic security reminders]
```

**After Skill Extraction** (BackendSpecialist.md):
```markdown
# Agent: BackendSpecialist

## Secure Implementation Support
Use `/security-threat-modeling` skill for threat-aware development:
- **Threat Awareness**: Understand attack vectors during implementation
- **Mitigation Patterns**: OAuth2, RBAC, encryption, sanitization implementations
- **Platform Vulnerabilities**: .NET 8 pitfalls to avoid (deserialization, EF Core injection)
- **Defense-in-Depth**: Multiple security layers for robust protection

**Focus Areas**:
- Load resources/mitigations/ for secure coding patterns
- Load resources/vulnerabilities/dotnet8-vulnerabilities.md to avoid common pitfalls

REFERENCE: ~40 tokens (90% reduction from generic reminders)
```

### 5.2 Token Efficiency Analysis

**Before Skill Extraction**:
- SecurityAuditor: 3,800 tokens embedded
- BackendSpecialist: 400 tokens generic security reminders
- FrontendSpecialist: 0 tokens (no security expertise)
- **Total Embedded**: 4,200 tokens

**After Skill Extraction**:
- SecurityAuditor: 50 tokens reference
- BackendSpecialist: 40 tokens reference
- FrontendSpecialist: 35 tokens reference (new capability enabled)
- Skill content: 10,120 tokens (one-time load)
- **Total References**: 125 tokens

**Token Savings**: 4,200 → 125 tokens = **97% reduction in repeated references**

**Knowledge Depth Improvement**:
- Before: 4,200 tokens of incomplete, fragmented security knowledge
- After: 10,120 tokens of comprehensive, structured threat analysis framework
- **140% knowledge increase** while reducing repeated overhead by 97%

### 5.3 Capability Enhancements

**SecurityAuditor Enhancements**:
- ✅ Comprehensive STRIDE methodology (was incomplete due to token pressure)
- ✅ Complete OWASP Top 10 → platform mapping (was missing)
- ✅ Attack vector taxonomy with exploit chains (was generic)
- ✅ Threat prioritization matrix with CVSS integration (was subjective)
- ✅ Complete threat model examples for quality benchmarking (was missing)

**BackendSpecialist NEW Capabilities**:
- ✅ Threat awareness during implementation (previously no security expertise)
- ✅ Platform-specific vulnerability knowledge (avoid .NET 8 pitfalls)
- ✅ Mitigation pattern library (OAuth2, RBAC, encryption implementations)
- ✅ Defense-in-depth architecture guidance (previously generic)

**FrontendSpecialist NEW Capabilities**:
- ✅ Client-side threat analysis (XSS, CSRF, DOM manipulation)
- ✅ Angular-specific vulnerability patterns (was missing)
- ✅ Secure state management and data handling (was generic)
- ✅ CSP and sanitization implementation patterns (was missing)

### 5.4 Maintenance Advantages

**Before Skill Extraction**:
- Update 2 locations (SecurityAuditor + BackendSpecialist) for security pattern changes
- Risk of inconsistency between agent definitions
- No centralized security knowledge for new agents

**After Skill Extraction**:
- Update 1 location (SKILL.md) for security pattern changes
- Automatic propagation to all agents using skill
- New agents gain comprehensive security expertise instantly

**Example Evolution Scenario**: OWASP Top 10:2024 release
- **Before**: Update SecurityAuditor definition, risk BackendSpecialist becoming outdated
- **After**: Update `resources/frameworks/owasp-top10-mapping.md`, all agents immediately benefit

---

## Outcomes & Lessons Learned

### Measurable Outcomes

#### Token Efficiency
- **Embedded cost**: 4,200 tokens (fragmented across agents)
- **Skill reference cost**: 125 tokens (3 agents)
- **Skill content**: 10,120 tokens (comprehensive framework)
- **Net efficiency**: 97% reduction in repeated references

#### Knowledge Depth
- **Before**: Incomplete STRIDE coverage, missing OWASP mapping, generic mitigations
- **After**: Comprehensive methodology, platform-specific catalogs, implementation patterns
- **Improvement**: 140% knowledge increase while reducing repeated overhead

#### Capability Expansion
- **SecurityAuditor**: Enhanced from incomplete to comprehensive threat analysis
- **BackendSpecialist**: Gained threat-aware implementation capabilities (previously generic)
- **FrontendSpecialist**: Gained client-side security expertise (previously none)

### Lessons Learned

#### What Worked Well
1. **Progressive loading strategy** enabled appropriate knowledge depth per scenario
2. **Separate vulnerability/mitigation resources** optimized for analysis vs. implementation
3. **Complete examples** provided quality benchmarks and learning resources
4. **Platform-specific catalogs** (.NET 8, Angular 19) gave specialists actionable expertise

#### What Could Be Improved
1. **Resource file size** could be split further for finer-grained loading
2. **Cross-referencing between resources** could be more explicit
3. **Threat model templates** could include more diverse component types
4. **Metadata trigger patterns** needed refinement for better skill matching

#### Unexpected Benefits
1. **Multi-specialist collaboration** improved from shared security vocabulary
2. **Security analysis quality** increased from structured framework
3. **Implementation security** improved from specialist access to threat knowledge
4. **New agent onboarding** accelerated from centralized expertise

### Skill Evolution Recommendations

#### Version 1.1 Improvements
- **Split large resource files** for granular loading (e.g., separate STRIDE categories)
- **Add API reference** for quick vulnerability/mitigation lookup
- **Enhanced examples** covering more component types (background jobs, message queues)
- **Threat library** with reusable threat patterns for common components

#### Long-term Evolution
- **Integration with security tools** (SAST, DAST, SCA output interpretation)
- **Automated threat detection** from code analysis
- **Threat model versioning** tracking threat landscape evolution
- **Industry-specific threat patterns** (healthcare, finance, etc.)

---

## Skill Creation Process Reflection

### Phase Effectiveness Analysis

**Phase 1: Scope Definition & Discovery** (⭐⭐⭐⭐⭐)
- Token budget analysis revealed 97% efficiency opportunity
- Technical skill categorization enabled appropriate structure
- Progressive loading strategy essential for managing 10K+ token skill

**Phase 2: Structure Design** (⭐⭐⭐⭐⭐)
- Resource organization by concern type (frameworks, vulnerabilities, mitigations) optimized usage
- Separate templates/examples enabled learning and application
- Three-tier loading (metadata → SKILL → resources) balanced depth and efficiency

**Phase 3: Progressive Loading Implementation** (⭐⭐⭐⭐⭐)
- Metadata enabled quick skill matching without loading full content
- SKILL.md provided complete methodology for most analyses
- Resources enabled deep dives only when needed
- **Key Success**: 6,000-token SKILL.md handles 80% of use cases

**Phase 4: Resource Organization** (⭐⭐⭐⭐)
- Usage patterns by agent clarified which resources each specialist needs
- Progressive loading efficiency analysis validated multi-tier approach
- **Challenge**: Resource files could be split further for finer control

**Phase 5: Agent Integration** (⭐⭐⭐⭐⭐)
- 97% token reduction exceeded expectations
- 140% knowledge depth increase transformed analysis quality
- Enabled new capabilities (BackendSpecialist, FrontendSpecialist security expertise)
- **Challenge**: Metadata trigger patterns required refinement

### Applicability to Other Skills

**This pattern works best for**:
- ✅ Deep technical domain knowledge (security, performance, architecture)
- ✅ Specialist-focused expertise (2-4 agents, not all 11)
- ✅ Comprehensive frameworks requiring progressive disclosure
- ✅ Knowledge benefiting multiple specialists (shared vocabulary, cross-domain collaboration)

**This pattern is suboptimal for**:
- ❌ Simple, frequently changing technical patterns
- ❌ Single-agent exclusive knowledge (better embedded)
- ❌ Shallow technical content not requiring progressive loading
- ❌ Highly volatile knowledge requiring frequent updates

### Key Success Factors

1. **Substantial token budget** justifies comprehensive skill content
2. **Multi-agent benefit** increases ROI beyond single specialist
3. **Progressive loading optimization** enables appropriate depth per scenario
4. **Resource organization by usage pattern** (analysis vs. implementation)
5. **Complete examples** demonstrate quality expectations and learning
6. **Platform-specific catalogs** provide actionable specialist knowledge

---

**End of Technical Skill Creation Example**

**Key Takeaways for PromptEngineer**:
1. **Technical skills** provide deep domain expertise for specialist agents
2. **Progressive loading** balances comprehensive knowledge with token efficiency
3. **Multi-agent benefit** increases ROI (SecurityAuditor + BackendSpecialist + FrontendSpecialist)
4. **Resource organization** optimizes for different usage patterns (analysis vs. implementation)
5. **Knowledge depth investment** (10K tokens) justified by 140% capability improvement
6. **Platform-specific catalogs** enable actionable specialist expertise
