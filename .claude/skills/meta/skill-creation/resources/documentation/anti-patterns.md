# Skill Creation Anti-Patterns

**Purpose:** Common mistakes to avoid when creating Claude Code skills
**Source:** Official Anthropic best practices and zarichney-api experience
**Target Audience:** PromptEngineer creating new skills

---

## CRITICAL ANTI-PATTERNS

### 1. Path Handling Errors

**CRITICAL (Official Docs):**
> "Use forward slashes (Unix-style) in all paths; Windows-style paths won't function correctly."

❌ **WRONG:** `\path\to\file.md`
✅ **CORRECT:** `/path/to/file.md`

**Why This Matters:** Skills must work across platforms. Windows-style backslashes break on Unix systems.

---

### 2. Deeply Nested References

**Problem (Best Practices):**
> "Deeply nested references [cause] partial file reads"

❌ **BAD PATTERN:**
```
SKILL.md → overview.md → details.md → examples.md
```

✅ **GOOD PATTERN:**
```
SKILL.md → details.md
SKILL.md → examples.md
SKILL.md → reference.md
```

**Rule:** All reference files must link directly from SKILL.md (one level deep maximum)

---

### 3. Skill Bloat - Single-Agent Patterns

**Symptoms:** Skill referenced by only 1 agent, no anticipated expansion to other agents

**Root Cause:** Phase 1 anti-bloat decision framework insufficiently applied

**Solution:**
1. Re-evaluate skill scope: Is this truly cross-cutting or agent-specific identity?
2. If agent-specific: Delete skill, move content back to agent definition
3. If potentially reusable: Document which other agents could use this within 6 months
4. If unclear: Escalate to Claude for business requirement clarification

**Prevention:** Phase 1 checklist must validate 3+ agent consumers before proceeding

**Example:**
- ❌ **bug-investigator-diagnostic-workflow:** Used only by BugInvestigator → Keep in agent
- ✅ **working-directory-coordination:** Used by all 12 agents → Valid skill

---

### 4. SKILL.md Exceeds Token Budget

**Symptoms:** Line count >625 lines for meta-skills, estimated tokens >5,000, skill feels bloated

**Root Cause:** Content not extracted to resources/, progressive loading design incomplete

**Solution:**
1. Move detailed templates to `resources/templates/` (reference from SKILL.md)
2. Move realistic examples to `resources/examples/` (reference from SKILL.md)
3. Move deep conceptual content to `resources/documentation/` (reference from SKILL.md)
4. Keep only workflow guidance in SKILL.md

**Prevention:** Phase 3 token measurement must validate budget before resource creation begins

**Token Budgets by Category:**
- Coordination: 2,000-3,500 tokens
- Documentation: 2,500-4,000 tokens
- Technical: 3,000-5,000 tokens
- Meta: 3,500-5,000 tokens
- Workflow: 2,000-3,500 tokens

---

### 5. Vague Description Without Activation Triggers

**Problem:** Description explains what skill does but agents uncertain when to load it

**Symptoms:** Agents browse skills but don't activate when appropriate

❌ **BAD:**
```yaml
description: Helps with documents
```

✅ **GOOD:**
```yaml
description: Processes PDF forms by extracting field values, validating data types, and generating completion reports. Use when analyzing form submissions, compliance documents, or structured PDFs requiring data extraction.
```

**Pattern:** "[WHAT skill does]. Use when [TRIGGER scenarios]. [EFFICIENCY metric if applicable]."

**Why This Matters:** Claude uses description to choose from 100+ skills - specificity is critical

---

### 6. Inconsistent Terminology

**Problem:** Claude struggles with multiple terms for same concept

❌ **BAD:** Mixing "user" and "customer" throughout skill
✅ **GOOD:** Pick "user" OR "customer", use everywhere

**Solution:**
- Choose one term per concept
- Use consistently across SKILL.md and all resources
- Define abbreviations once, use same abbreviation throughout

---

### 7. Time-Sensitive Information

**Problem:** Skills persist across months/years, dated content becomes misleading

❌ **BAD:**
```markdown
## Current Process
Use API v2 with tokens (2024 approach)
```

✅ **GOOD:**
```markdown
## Current Approach (2025+)
Use API v3 with authentication tokens

## Legacy Approach (deprecated 2024)
API v2 with API keys - only for old integrations
```

**Pattern:** Mark current vs. legacy approaches explicitly

---

### 8. Magic Numbers Without Justification

❌ **BAD:**
```python
threshold = 0.85  # Why 0.85?
max_items = 42    # Why 42?
```

✅ **GOOD:**
```python
# 0.85 threshold balances precision (0.92) and recall (0.78)
# validated against 10K production samples
threshold = 0.85

# 42 items per page optimizes load time (<200ms) while
# maintaining user engagement (avg session: 3.2 pages)
max_items = 42
```

**Rule:** All parameter values must include rationale

---

### 9. Excessive Option Offering

**Problem (Best Practices):**
> "Avoid excessive options: Present defaults with escape hatches rather than listing multiple approaches."

❌ **BAD:**
```markdown
You can process this data by:
1. Method A (fast but less accurate)
2. Method B (balanced)
3. Method C (slow but precise)
4. Method D (experimental)
Choose based on your needs.
```

✅ **GOOD:**
```markdown
Process data using balanced validation (recommended).

For time-critical workflows, use scripts/quick_validate.py instead.
For maximum precision, add --strict flag.
```

**Pattern:** Default recommendation + escape hatches for edge cases

---

### 10. Embedding Full Resources in SKILL.md

**Problem:** Templates, examples, documentation embedded in SKILL.md instead of resources/

**Symptoms:** SKILL.md exceeds 500 lines, token budget blown, context inefficiency

**Solution:**
1. Extract templates to `resources/templates/[name].md`
2. Extract examples to `resources/examples/[name].md`
3. Extract documentation to `resources/documentation/[name].md`
4. Reference from SKILL.md: "See `resources/templates/[name].md` for format"

**Why Progressive Loading Matters:**
- SKILL.md always loaded when skill invoked
- Resources loaded only when agent explicitly needs them
- Token efficiency: 2,500 tokens (SKILL.md) vs. 5,000+ tokens (SKILL.md + embedded resources)

---

### 11. Inconsistent Agent Integration Patterns

**Symptoms:** Some agents embed partial skill content, others reference correctly

**Root Cause:** Phase 5 agent integration pattern not standardized

**Solution:**
1. Audit all agent definitions referencing this skill
2. Standardize to ~20 token reference format: Purpose | Key Workflow | Integration
3. Remove any embedded skill content from agent definitions
4. Agents reference skill SKILL.md for full instructions

**Standard Reference Format:**
```markdown
### skill-name
**Purpose:** [1-line capability description]
**Key Workflow:** [3-5 word workflow summary]
**Integration:** [When/how agent uses this skill - 1 sentence]
```

**Token Efficiency:** ~20 tokens (reference) vs. ~150+ tokens (embedded) = 87% reduction

---

### 12. YAML Frontmatter Name Violations

❌ **INVALID NAMES:**
- `Skill-Name` (uppercase)
- `skill_name` (underscores)
- `skill name` (spaces)
- `help` (reserved word)
- `a-really-long-skill-name-exceeding-sixty-four-characters-total` (>64 chars)

✅ **VALID NAMES:**
- `working-directory-coordination`
- `api-design-patterns`
- `skill-creation`

**Official Constraints:**
- Lowercase letters, numbers, hyphens only
- Maximum 64 characters
- No reserved words (help, list, all, none)
- Globally unique across all active skills

---

### 13. Missing Cross-Model Testing

**Problem (Best Practices):**
> "Test Skills across Claude Haiku, Sonnet, and Opus since effectiveness depends on the underlying model."

**Why This Matters:** Skills behave differently across models
- Haiku: Speed-optimized, may miss nuanced instructions
- Sonnet: Balanced, production workload baseline
- Opus: Complex reasoning, catches edge cases

**Solution:** Test skill with all three models before deployment

---

### 14. No Evaluation-First Development

**Problem:** Creating extensive skill documentation before validating it actually improves Claude's performance

**Anti-Pattern:** Write 1,000+ lines of skill content, deploy, discover Claude doesn't use it

**Best Practice (Official Anthropic):**
> "Build evaluations first before extensive documentation."

**Correct Process:**
1. Identify actual gaps in Claude's performance
2. Create 3+ test scenarios representing real usage
3. Establish baseline behavior without skill
4. Write minimal skill instructions
5. Test against evaluations
6. Iterate based on results
7. Expand documentation only if tests prove value

---

## ZARICHNEY-API SPECIFIC ANTI-PATTERNS

### 15. Skill Created for Rapidly Changing Content

**Problem:** Content changes frequently, skill becomes stale, maintenance burden high

**Solution:** Use `/Docs/Standards/` instead of skills for frequently updated content

**Examples:**
- ❌ **Skill:** coding-conventions (changes with each standard update)
- ✅ **Documentation:** CodingStandards.md (easier to maintain)

**When to Use Skills:** Stable patterns, coordination protocols, meta-frameworks
**When to Use Documentation:** Frequently changing standards, project-specific details

---

### 16. Agent-Specific Identity Content Extracted

**Problem:** Core agent identity content extracted to skill, agent loses distinctive character

**Example:**
- ❌ **Extracting:** BugInvestigator's root cause analysis methodology to skill
- ✅ **Preserving:** BugInvestigator's unique diagnostic approach as agent identity

**Rule:** Agent-specific workflows that define agent's role should remain in agent definition

**When to Extract:** When 3+ agents share the same pattern (coordination)
**When to Preserve:** When pattern is unique to agent's identity (specialization)

---

## VALIDATION CHECKLIST

Before deploying any skill, validate against anti-patterns:

- [ ] All paths use forward slashes (Unix-style)
- [ ] All references one level deep from SKILL.md (no nesting)
- [ ] Skill used by 3+ agents (coordination) or 2+ agents (workflow/technical)
- [ ] SKILL.md within token budget for category
- [ ] Description includes both WHAT and WHEN elements
- [ ] Consistent terminology throughout skill and resources
- [ ] No time-sensitive content (or marked as legacy)
- [ ] All parameter values justified with rationale
- [ ] Default approach with escape hatches, not multiple options
- [ ] Resources in separate files, not embedded in SKILL.md
- [ ] Agent integrations standardized (~20 token references)
- [ ] YAML name valid (lowercase, hyphens, <64 chars, not reserved)
- [ ] Tested across Haiku, Sonnet, and Opus
- [ ] Evaluation scenarios validated skill effectiveness
- [ ] Content stable (not rapidly changing)
- [ ] Not core agent identity content

---

## REFERENCES

**Official Sources:**
- https://docs.claude.com/en/docs/agents-and-tools/agent-skills/best-practices
- https://www.anthropic.com/engineering/equipping-agents-for-the-real-world-with-agent-skills

**Related Resources:**
- `resources/documentation/progressive-loading-guide.md` - Context efficiency patterns
- `resources/documentation/evaluation-first-workflow.md` - Testing methodology
- `resources/documentation/token-optimization-techniques.md` - Budget management
