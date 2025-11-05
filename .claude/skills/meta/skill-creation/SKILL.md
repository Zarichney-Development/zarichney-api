---
name: skill-creation
description: Systematic framework for creating Claude Code skills with progressive loading design, following official Anthropic best practices. Use when creating new skills, refactoring embedded patterns, or establishing skill templates for cross-agent workflows. Includes evaluation-first methodology, anti-pattern prevention, and token optimization techniques. Enables 50-87% token reduction through skill extraction.
---

# Skill Creation Meta-Skill

**Version:** 2.0.0 (Optimized for Best Practices)
**Category:** Meta (Tools for PromptEngineer)
**Target User:** PromptEngineer exclusively

---

## PURPOSE

This meta-skill provides PromptEngineer with a systematic framework for creating Claude Code skills following official Anthropic best practices and zarichney-api's progressive loading architecture.

### Core Mission
Transform skill creation from ad-hoc documentation into a disciplined 5-phase design process that produces optimized, reusable skills achieving 50-87% token savings vs. embedded patterns in agent definitions.

### Why This Matters
Without systematic skill creation methodology:
- **Skill Bloat:** Unnecessary skills created for single-agent patterns
- **Inconsistent Structure:** Skills lack standard sections, inefficient discovery
- **Context Inefficiency:** Skills embed content better suited for progressive loading
- **Quality Variability:** No validation framework ensuring effectiveness
- **Best Practice Gaps:** Skills don't follow official Anthropic guidance

This meta-skill ensures every new skill integrates seamlessly into zarichney-api's progressive loading architecture while following proven best practices from official Anthropic engineering guidance.

### Mandatory Application
- **Required For:** All new skill creation or comprehensive skill refactoring by PromptEngineer
- **Authority Scope:** PromptEngineer has EXCLUSIVE authority over `.claude/skills/` directory
- **Quality Gate:** No skill deployment without completion of all 5 phases and best practices validation
- **Efficiency Target:** Skills enable 50-87% token reduction per integration vs. embedded patterns

---

## WHEN TO USE

### 1. Creating New Cross-Cutting Coordination Skill (PRIMARY USE CASE)
**Trigger:** Pattern used by 3+ agents (e.g., working directory communication, documentation grounding)
**Action:** Execute complete 5-phase workflow from scope definition through agent integration
**Rationale:** Coordination skills reduce redundancy across agent team through shared reusable patterns

### 2. Extracting Domain Technical Skill from Agent Definition (OPTIMIZATION)
**Trigger:** Agent definition exceeds 240 lines or contains deep technical content >500 lines applicable to multiple specialists
**Action:** Execute Phases 1-4 to extract pattern into domain technical skill
**Rationale:** Reduces agent token load while enabling cross-domain knowledge sharing

### 3. Creating Meta-Skill for AI System Component Creation (META-CAPABILITY)
**Trigger:** PromptEngineer needs systematic framework for creating agents/skills/commands
**Action:** Execute all 5 phases to establish reusable meta-capability with comprehensive resources
**Rationale:** Meta-skills accelerate AI system evolution through standardized creation methodologies

### 4. Designing Workflow Automation Skill (PRODUCTIVITY)
**Trigger:** Repeatable process with clear steps used by 2+ agents
**Action:** Execute Phases 1-4 focusing on actionable workflow steps and validation checklists
**Rationale:** Workflow skills automate repetitive tasks reducing agent cognitive load and errors

### 5. Preventing Skill Bloat - Validating Skill Need (ANTI-BLOAT)
**Trigger:** PromptEngineer considering new skill but uncertain if extraction justified
**Action:** Execute Phase 1 Skill Scope Definition to apply anti-bloat decision framework
**Rationale:** Ensures skills created only when reusability threshold met, preventing ecosystem clutter

---

## 5-PHASE WORKFLOW STEPS

### Phase 1: Skill Scope Definition

**Purpose:** Determine if skill creation appropriate and define clear boundaries, preventing skill bloat

**Process:**
1. **Cross-Cutting Concern Assessment:** Evaluate if pattern qualifies as coordination skill
   - Reusability: Used by 3+ agents currently or within 6 months?
   - Coordination Value: Extracting reduces agent token load by 100+ tokens per integration?
   - Standardization Need: Embedding causes inconsistencies across agents?
   - Team Awareness: Pattern enables multi-agent communication?

2. **Domain Specialization Evaluation:** Assess if pattern qualifies as technical skill
   - Content Depth: Deep technical content >500 lines for specific domain?
   - Specialist Relevance: Enhances multiple specialists' effectiveness?
   - Progressive Loading Value: Content loaded on-demand vs. always present?

3. **Anti-Bloat Decision Framework:** Apply reusability threshold
   - **CREATE SKILL:** 3+ agents (coordination), 2+ agents (workflow/technical), meta-capability
   - **DO NOT CREATE:** Single-agent pattern, <100 tokens embedded, rapidly changing content, agent-specific identity

4. **Skill Categorization:** Determine appropriate structure
   - **Coordination:** 3+ agents, team workflow patterns (2,000-3,500 tokens)
   - **Technical:** Domain expertise for specialists (3,000-5,000 tokens)
   - **Meta:** Agent/skill/command creation frameworks (3,500-5,000 tokens)
   - **Workflow:** Repeatable automation for 2+ agents (2,000-3,500 tokens)

**Checklist:**
- [ ] Reusability threshold validated
- [ ] Anti-bloat framework applied - skill creation justified
- [ ] Skill category identified
- [ ] Token savings calculated (target 100+ tokens saved per agent integration)
- [ ] Integration with existing skills assessed

**Resource:** See `resources/documentation/anti-patterns.md` for detailed anti-bloat framework

---

### Phase 2: Skill Structure Design

**Purpose:** Apply consistent SKILL.md structure with YAML frontmatter and required sections optimized for progressive loading

**Process:**
1. **YAML Frontmatter Design:**
   ```yaml
   ---
   name: skill-name-here  # Lowercase-hyphens only, <64 chars, not reserved word
   description: [WHAT skill does]. Use when [WHEN triggers]. [EFFICIENCY metric if applicable].  # Max 1024 chars
   ---
   ```

2. **Apply Required SKILL.md Sections (in optimal progressive loading order):**
   - **Lines 1-80 (Always Loaded):** YAML frontmatter, PURPOSE (Core Mission, Why This Matters, Mandatory Application), WHEN TO USE scenarios
   - **Lines 81-300 (Loaded When Invoked):** WORKFLOW STEPS with processes and checklists, TARGET AGENTS, RESOURCES overview
   - **Lines 301-500 (On-Demand):** INTEGRATION WITH TEAM WORKFLOWS, SUCCESS METRICS, TROUBLESHOOTING

3. **Token Budget Allocation:**
   | Category | SKILL.md Target | Maximum |
   |----------|----------------|---------|
   | Coordination | 2,000-3,500 | 3,500 |
   | Technical | 3,000-5,000 | 5,000 |
   | Meta | 3,500-5,000 | 5,000 |
   | Workflow | 2,000-3,500 | 3,500 |

**Checklist:**
- [ ] YAML frontmatter valid (name and description meet official specification)
- [ ] Description includes both "what" and "when" elements
- [ ] All required sections present in optimal progressive loading order
- [ ] Token budget appropriate for skill category (<500 lines SKILL.md)

**Resource:** See `resources/templates/skill-template.md` for complete section scaffolding

---

### Phase 3: Progressive Loading Design

**Purpose:** Optimize context efficiency through metadata discovery, instruction loading, and on-demand resource access

**Progressive Loading Flow:**
```
Phase 1 - Discovery (~100 tokens):
  Load: YAML frontmatter from all skills
  Decision: Identify relevant skill

Phase 2 - Invocation (2,000-5,000 tokens):
  Load: Complete SKILL.md instructions
  Decision: Execute workflow, identify resource needs

Phase 3 - Resource Access (Variable):
  Load: Specific template/example/doc as needed
  Decision: Apply resource to resolve specific need
```

**Process:**
1. **Metadata Discovery Optimization:** YAML description includes clear triggers ("Use when...")
2. **Instruction Loading Design:** Front-load critical content (purpose, when to use, basic workflow in first 80 lines)
3. **Resource Access Design:** Organize resources for on-demand loading (templates/examples/documentation)
4. **Total Context Efficiency Validation:** Measure progressive loading scenarios

**Token Estimation:**
- **Quick Rule:** 1 line ≈ 8 tokens (average markdown)
- **Validation:** Count lines, multiply by 8, confirm within budget
- **If Over Budget:** Extract content to resources/

**Checklist:**
- [ ] YAML frontmatter optimized for discovery (<150 tokens)
- [ ] Critical content front-loaded (purpose, when to use, basic workflow in first 80 lines)
- [ ] Resource references one level deep from SKILL.md (not nested)
- [ ] Total context efficiency validated (discovery → invocation → resources flow)

**Resource:** See `resources/documentation/token-optimization-techniques.md` for detailed measurement methods

---

### Phase 4: Resource Organization

**Purpose:** Structure skill resources for maximum reusability, clarity, and progressive loading efficiency

**Standard Directory Structure:**
```
.claude/skills/[category]/[skill-name]/
├── SKILL.md (YAML frontmatter + workflow steps)
└── resources/
    ├── templates/ (reusable formats, 200-500 tokens each)
    ├── examples/ (reference implementations, 500-1,500 tokens each)
    └── documentation/ (deep guides, 1,000-3,000 tokens each)
```

**Process:**
1. **Templates Directory:** Create ready-to-use formats with {{PLACEHOLDERS}}, 30-60 lines each
2. **Examples Directory:** Demonstrate complete workflows in realistic zarichney-api scenarios, 100-200 lines each
3. **Documentation Directory:** Provide deep dives for complex concepts with table of contents, 250-400 lines each
4. **Resource Reference Patterns:** Link resources from SKILL.md efficiently (one-level deep)

**Example Reference from SKILL.md:**
```markdown
**Resource:** See `resources/templates/artifact-reporting-template.md` for format
```

**Checklist:**
- [ ] Directory structure follows standard hierarchy
- [ ] Templates actionable and standalone (200-500 tokens each)
- [ ] Examples realistic and complete (500-1,500 tokens each)
- [ ] Documentation comprehensive with TOC for >100 lines (1,000-3,000 tokens)
- [ ] Resource references one level deep from SKILL.md

**Resource:** See `resources/templates/skill-template.md` for resource organization patterns

---

### Phase 5: Agent Integration Pattern

**Purpose:** Define how agents reference and use this skill effectively, achieving maximum token efficiency

**Standard Skill Reference Template (~20 tokens):**
```markdown
### skill-name
**Purpose:** [1-line capability description]
**Key Workflow:** [3-5 word workflow summary or primary steps]
**Integration:** [When/how agent uses this skill - 1 sentence]
```

**Process:**
1. **Skill Reference Format Design:** Standardize ~20 token references in agent definitions
2. **Integration Point Positioning:** Determine where in agent definition to reference skill (mandatory vs. domain vs. optional)
3. **Usage Trigger Definition:** Specify when agent should load complete skill instructions
4. **Token Efficiency Validation:** Measure and document savings vs. embedded approach

**Token Savings Calculation:**
```yaml
BEFORE: Agent with embedded pattern = 350 lines × 8 = 2,800 tokens
AFTER: Agent with skill reference = 180 lines × 8 = 1,440 tokens
SAVINGS: 1,360 tokens (49% reduction) + on-demand skill loading
```

**Checklist:**
- [ ] Skill reference format standardized (~20 tokens per reference)
- [ ] Integration point positioning appropriate (mandatory vs. domain vs. optional)
- [ ] Usage triggers clearly defined
- [ ] Token efficiency calculated and validated (target 50-87% reduction)
- [ ] Integration testing completed with 2+ target agents

**Resource:** See `resources/documentation/token-optimization-techniques.md` for efficiency calculations

---

## EVALUATION-FIRST METHODOLOGY (CRITICAL)

**Best Practice (Official Anthropic):**
> "Build evaluations first before extensive documentation."

**6-Step Process:**
1. **Identify Performance Gaps:** Where does Claude struggle without this skill?
2. **Create Test Scenarios:** Write 3+ realistic test scenarios (minimum requirement)
3. **Establish Baseline:** Measure Claude's performance WITHOUT skill
4. **Write Minimal Skill:** Start with 200-500 tokens addressing identified gaps
5. **Test Against Evaluations:** Compare behavior to baseline, validate improvements
6. **Iterate Based on Results:** Expand documentation only if tests prove value

**Cross-Model Testing (Required):**
- Test with Haiku, Sonnet, and Opus (effectiveness depends on model)
- Use two-Claude approach (separate creator/tester sessions)
- Validate no regressions introduced

**Resource:** See `resources/documentation/evaluation-first-workflow.md` for complete testing methodology

---

## TARGET AGENTS

### Primary User: PromptEngineer
**Authority:** EXCLUSIVE modification rights over `.claude/skills/` directory
**Use Cases:**
- Creating new cross-cutting coordination skills
- Extracting domain technical skills from bloated agent definitions
- Creating meta-skills for AI system component creation workflows
- Designing workflow automation skills for repeatable processes
- Validating skill need through anti-bloat decision framework

### Secondary User: Codebase Manager (Claude)
**Use Cases:**
- Understanding skill creation methodology for better orchestration
- Validating skills follow progressive loading architecture when delegating to PromptEngineer
- Preventing skill bloat through reusability threshold enforcement
- Optimizing context windows through skill-based agent definitions

---

## RESOURCES

### Templates (Ready-to-Use Formats)
- **skill-template.md**: Complete SKILL.md scaffolding with all required sections and best practices checklist embedded

**Location:** `resources/templates/`
**Usage:** Copy template, customize for specific skill, validate against embedded checklist

### Examples (Reference Implementations)
- **coordination-skill-example.md**: Complete 5-phase workflow for working-directory-coordination skill
- **technical-skill-example.md**: Complete 5-phase workflow for api-design-patterns skill
- **meta-skill-example.md**: Self-referential example of skill-creation meta-skill development

**Location:** `resources/examples/`
**Usage:** Review for realistic demonstrations of complete skill creation workflows

### Documentation (Deep Dives)
- **anti-patterns.md**: Common mistakes to avoid when creating Claude Code skills (Official Anthropic guidance)
- **evaluation-first-workflow.md**: Testing methodology ensuring skills improve Claude's actual performance
- **token-optimization-techniques.md**: Practical methods for measuring and reducing token consumption
- **progressive-loading-guide.md**: Comprehensive guide to context efficiency design architecture
- **integration-patterns.md**: Effective skill reference and usage patterns for agent integration

**Location:** `resources/documentation/`
**Usage:** Deep understanding of design principles, troubleshooting complex scenarios, optimization strategies

---

## INTEGRATION WITH TEAM WORKFLOWS

### Multi-Agent Coordination Enhancement
- **Reduced Redundancy:** Coordination patterns extracted to skills eliminate duplication across 12-agent team
- **Consistent Interfaces:** All skills follow same structure making discovery and usage efficient
- **Progressive Loading Efficiency:** Context window optimization enables more agents loaded simultaneously
- **Reusability Standards:** Anti-bloat framework ensures only valuable patterns extracted to skills

### Claude's Orchestration Enhancement
- Delegate skill creation systematically to PromptEngineer with clear quality gates
- Validate new skills integrate with progressive loading architecture before deployment
- Prevent skill bloat through reusability threshold enforcement
- Optimize context windows through skill-based agent definitions (50-70% token reduction)

### Quality Gate Integration
- **PromptEngineer Validation:** Self-validation following 5-phase methodology and best practices checklist
- **ComplianceOfficer:** Pre-deployment validation ensuring skill follows team standards
- **Real-World Testing:** New skills validated through actual agent task execution (evaluation-first)
- **Token Efficiency Audits:** Regular measurement of skill token savings vs. embedded approach

### CLAUDE.md Integration
Directly supports CLAUDE.md Section 2: Multi-Agent Development Team by:
- Establishing skill creation methodology ensuring consistency across skill ecosystem
- Defining progressive loading architecture enabling context efficiency
- Documenting anti-bloat framework preventing unnecessary skill proliferation
- Standardizing agent integration patterns for team-wide reusability

---

## SUCCESS METRICS

### Skill Creation Efficiency
- **Systematic Process:** 5-phase structured workflow vs. unguided ad-hoc documentation
- **Structural Consistency:** All skills follow same YAML frontmatter and SKILL.md structure
- **Best Practices Compliance:** Skills validated against official Anthropic guidance
- **First-Time Deployment Success:** Skills pass validation without rework

### Context Optimization Effectiveness
- **Token Budget Compliance:** SKILL.md within category budget (2,000-5,000 tokens)
- **Progressive Loading Validated:** Discovery → Invocation → Resources scenarios tested
- **87% Token Savings Per Integration:** Skill reference (~20 tokens) vs. embedded pattern (~150 tokens)
- **50-70% Agent Definition Reduction:** Agent context load reduced through skill extraction

### Ecosystem Health Metrics
- **Reusability Threshold Met:** Skills used by 3+ agents (coordination) or 2+ agents (workflow/technical)
- **Zero Skill Bloat:** No skills created for single-agent unique patterns
- **Evaluation-First Compliance:** All skills validated with 3+ test scenarios before extensive documentation
- **Cross-Model Testing:** Skills tested with Haiku, Sonnet, and Opus

### Multi-Agent Team Efficiency
- **16,320 Token Ecosystem Savings:** 49% reduction across 12-agent team vs. embedded approach
- **120% Orchestration Capacity Increase:** Can load 11 agents simultaneously vs. 5 with embedded patterns
- **Consistent Resource Patterns:** Templates/examples/documentation follow standard organization

---

## TROUBLESHOOTING

### Issue: Skill Created for Single-Agent Pattern (Bloat Violation)
**Symptoms:** Skill referenced by only 1 agent, no anticipated expansion
**Solution:** Re-evaluate scope using Phase 1 anti-bloat framework. If agent-specific, delete skill and move content to agent definition.
**Prevention:** Phase 1 checklist must validate 3+ agent consumers before proceeding

### Issue: SKILL.md Exceeds Token Budget
**Symptoms:** Line count >625 lines (meta-skills), estimated tokens >5,000
**Solution:** Extract detailed templates to resources/templates/, examples to resources/examples/, documentation to resources/documentation/
**Prevention:** Phase 3 token measurement must validate budget before resource creation

### Issue: YAML Description Missing "When to Use" Element
**Symptoms:** Description explains what but agents uncertain when to load
**Solution:** Update description to include: "[WHAT skill does]. Use when [trigger scenarios]."
**Prevention:** Phase 2 YAML frontmatter checklist validates both "what" and "when" present

### Issue: Agents Load Skill But Don't Use Resources Effectively
**Symptoms:** Agents execute workflow without loading templates/examples, inconsistent outputs
**Solution:** Review SKILL.md workflow steps, add explicit "Resource:" callouts after each step
**Prevention:** Phase 4 resource organization must include usage triggers in SKILL.md workflow steps

### Escalation Path
1. **PromptEngineer Review:** Re-execute problematic phase with comprehensive resource review
2. **Claude Orchestration:** Validate skill need through business requirement assessment
3. **Anti-Bloat Re-Evaluation:** Apply Phase 1 decision framework - is skill truly justified?
4. **User Escalation:** Fundamental design issues requiring clarification or architectural guidance

**Resource:** See `resources/documentation/anti-patterns.md` for comprehensive troubleshooting

---

**Skill Status:** ✅ **OPERATIONAL** (v2.0 - Optimized for Best Practices)
**Target User:** PromptEngineer exclusively
**Efficiency Gains:** 50-87% token reduction per skill integration vs. embedded patterns
**Best Practices:** Compliant with official Anthropic skills engineering guidance
**Progressive Loading:** YAML frontmatter (~100 tokens) → SKILL.md (~3,200 tokens) → resources (on-demand)
