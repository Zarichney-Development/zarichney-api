# Token Optimization Techniques for Skills

**Purpose:** Practical methods for measuring and reducing token consumption in Claude Code skills
**Target Audience:** PromptEngineer creating token-efficient skills
**Context:** Progressive loading architecture enables unbounded context through on-demand resource loading

---

## TOKEN ESTIMATION METHODS

### Line-to-Token Approximation

**Quick Estimation Rule:**
```
1 line ≈ 8 tokens (average for markdown with moderate density)
```

**Density Variations:**
- **Sparse content** (headers, lists): ~5-6 tokens/line
- **Normal prose**: ~8 tokens/line
- **Dense technical** (code, YAML): ~10-12 tokens/line

**Example Estimation:**
```markdown
YAML Frontmatter:
  5-10 lines → ~50-100 tokens

SKILL.md Sections:
  Purpose (10 lines) → ~80 tokens
  When to Use (30 lines) → ~240 tokens
  Workflow Steps (150 lines) → ~1,200 tokens
  Resources (40 lines) → ~320 tokens
  Total: ~230 lines → ~1,840 tokens

Resources:
  Template (30 lines) → ~240 tokens
  Example (100 lines) → ~800 tokens
  Documentation (250 lines) → ~2,000 tokens
```

### Validation Method

**Process:**
1. Draft content
2. Count total lines
3. Estimate tokens: `lines × 8`
4. Validate within skill category budget

**Token Budgets by Category:**
| Category | SKILL.md Target | Maximum |
|----------|----------------|---------|
| Coordination | 2,000-3,500 | 3,500 |
| Documentation | 2,500-4,000 | 4,000 |
| Technical | 3,000-5,000 | 5,000 |
| Meta | 3,500-5,000 | 5,000 |
| Workflow | 2,000-3,500 | 3,500 |

**If over budget:** Extract content to resources/

---

## PROGRESSIVE LOADING TOKEN EFFICIENCY

### Three-Tier Loading Architecture

**Tier 1: Metadata Discovery (~100 tokens)**
- **Loaded:** YAML frontmatter only from all skills
- **Context:** Agent browsing skills directory to find relevant capability
- **Efficiency:** Scan 10+ skills in ~1,000 tokens

**Tier 2: Instruction Loading (2,000-5,000 tokens)**
- **Loaded:** Complete SKILL.md when skill activated
- **Context:** Agent executing skill workflow
- **Efficiency:** Core guidance without resources

**Tier 3: Resource Access (Variable tokens)**
- **Loaded:** Specific template/example/doc as needed
- **Context:** Agent needs format/pattern/deep dive
- **Efficiency:** Targeted loading only when required

### Progressive Loading Flow Example

```
Phase 1 - Discovery (Minimal):
  Load: YAML frontmatter from 10 skills
  Total: ~1,000 tokens
  Decision: Identify relevant skill

Phase 2 - Invocation (Core):
  Load: SKILL.md instructions
  Total: ~2,500 tokens average
  Decision: Execute workflow, identify resource needs

Phase 3 - Resource (Targeted):
  Load: Specific template
  Total: ~3,000 tokens cumulative
  Decision: Apply format to task

Phase 4 - Deep Dive (Optional):
  Load: Documentation for edge case
  Total: ~5,000 tokens cumulative
  Decision: Resolve complex scenario
```

**Efficiency Comparison:**
- **Embedded Approach:** ~2,500 tokens always loaded in agent
- **Skill Reference:** ~20 tokens in agent + ~2,500 when invoked
- **Savings:** 2,480 tokens (99% reduction when not needed)

---

## CONTENT EXTRACTION STRATEGIES

### When to Extract to Resources

**Extract to `resources/templates/` when:**
- Content is copy-paste format with placeholders
- Agent needs exact structure specification
- Template rarely changes
- 30-60 lines (200-500 tokens)

**Extract to `resources/examples/` when:**
- Content demonstrates complete workflow execution
- Realistic scenario showing decision points
- Annotated implementation
- 100-200 lines (500-1,500 tokens)

**Extract to `resources/documentation/` when:**
- Content explains philosophy or design rationale
- Comprehensive troubleshooting guide
- Advanced optimization techniques
- 250-400 lines (1,000-3,000 tokens)

**Keep in SKILL.md when:**
- Essential to executing basic workflow
- Core decision criteria agents need immediately
- Brief format examples (<10 lines)
- Critical warnings or constraints

### Extraction Decision Matrix

| Content Type | Lines | Tokens | Location | Rationale |
|--------------|-------|--------|----------|-----------|
| Workflow overview | 20-30 | 160-240 | SKILL.md | Always needed |
| Detailed process | 80-120 | 640-960 | SKILL.md | Core execution |
| Template format | 30-60 | 240-480 | resources/templates/ | On-demand |
| Complete example | 100-200 | 800-1,600 | resources/examples/ | Pattern learning |
| Philosophy guide | 250-400 | 2,000-3,200 | resources/documentation/ | Deep understanding |

---

## REFERENCE OPTIMIZATION

### One-Level Deep Rule

**Official Best Practice:**
> "All reference files should link directly from SKILL.md to ensure Claude reads complete files when needed."

**Problem with Nested References:**
```
❌ BAD: SKILL.md → overview.md → details.md → examples.md
Result: Claude may partially load overview.md, miss details
```

**Solution - Flat Reference Structure:**
```
✅ GOOD:
SKILL.md → details.md
SKILL.md → examples.md
SKILL.md → reference.md

Result: Claude loads complete files when referenced
```

### Efficient Reference Patterns

**In SKILL.md:**
```markdown
**Resource:** See `resources/templates/artifact-reporting-template.md` for format
```

**In Resources Overview Section:**
```markdown
## RESOURCES

### Templates
- **artifact-reporting-template.md**: Format for working directory communication

**Location:** `resources/templates/`
**Usage:** Copy template, fill specifics, use in agent work
```

**Token Efficiency:**
- Reference in SKILL.md: ~15 tokens
- Resources overview: ~50 tokens for all templates
- Total overhead: ~65 tokens for unlimited template access

---

## AGENT INTEGRATION TOKEN SAVINGS

### Skill Reference Format (~20 tokens)

**Standard Pattern:**
```markdown
### skill-name
**Purpose:** [1-line capability]
**Key Workflow:** [3-5 words]
**Integration:** [When/how used - 1 sentence]
```

**Example:**
```markdown
### working-directory-coordination
**Purpose:** Team communication for artifact discovery and reporting
**Key Workflow:** Discovery → Reporting → Integration
**Integration:** Execute all 3 protocols for every working directory interaction
```

**Token Count:** ~22 tokens

### Savings Calculation

**Before Skill Extraction:**
```yaml
Agent Definition:
  Lines: 350
  Tokens: 350 × 8 = 2,800
  Embedded Patterns: working-directory (~150) + documentation-grounding (~150) + domain (~500) = 800 tokens embedded
  Total Always Loaded: 2,800 tokens
```

**After Skill Extraction:**
```yaml
Agent Definition:
  Lines: 180
  Tokens: 180 × 8 = 1,440
  Skill References: working-directory (~20) + documentation-grounding (~20) + domain (~60) = 100 tokens referenced
  Total Base Load: 1,440 tokens
  Skills Loaded: On-demand only when invoked

Savings Per Agent:
  Base Context: 1,360 tokens saved (49% reduction)
  On-Demand Loading: Skills loaded only when needed
  Effective Savings: 50-70% reduction in agent context load
```

### Multi-Agent Ecosystem Efficiency

**12-Agent Team Calculation:**
```yaml
Embedded Approach:
  12 agents × 2,800 tokens = 33,600 tokens total

Skill Reference Approach:
  12 agents × 1,440 tokens = 17,280 tokens (base)
  Skills: Loaded on-demand per agent need

Ecosystem Savings:
  Base reduction: 16,320 tokens (49%)
  Context window impact: Can load 11 agents vs. 5 simultaneously
  Orchestration capacity: 120% increase
```

---

## YAML FRONTMATTER OPTIMIZATION

### Description Efficiency

**Constraints:**
- Maximum: 1,024 characters
- Pre-loaded in system prompt for all skills
- Multiplied across skill ecosystem

**Target:** 300-600 characters for optimal balance

**Pattern:**
```yaml
description: [WHAT: 1-2 sentences]. Use when [WHEN: specific triggers]. [METRIC: optional efficiency gain].
```

**Example - Concise (346 characters):**
```yaml
description: Team communication protocols for artifact discovery, immediate reporting, and context integration across multi-agent workflows. Use when creating/discovering working directory files to maintain team awareness. Prevents communication gaps and enables seamless context flow between agent engagements.
```

**Token Estimate:** ~60-80 tokens

**Efficiency:**
- All skills loaded in discovery: 30 skills × 80 tokens = ~2,400 tokens baseline
- Optimization: Reduce each description by 20 tokens = 600 tokens saved ecosystem-wide

---

## DOMAIN-SPECIFIC FILE SPLITTING

### When to Split by Domain

**Pattern (Official Best Practices):**
```
skill-name/
├── SKILL.md           (Overview + links)
├── sales-process.md   (Sales domain only)
├── finance-rules.md   (Finance domain only)
└── product-specs.md   (Product domain only)
```

**Benefit:** Load only relevant domain context, not entire knowledge base

**Zarichney-API Example:**
```
api-design-patterns/
├── SKILL.md                    (Overview)
├── rest-patterns.md            (REST-specific)
├── graphql-patterns.md         (GraphQL-specific)
└── validation-patterns.md      (Cross-domain)
```

**Efficiency:**
- Without splitting: Load 3,000 tokens for any API question
- With splitting: Load 1,000 tokens (SKILL.md) + 800 tokens (relevant domain) = 1,800 tokens
- Savings: 40% when domain-specific question asked

### Mutual Exclusivity Benefits

**Create specialized skills rather than omnibus skills:**

❌ **BAD - Omnibus Skill:**
```
mega-document-processor/
└── SKILL.md (10,000 tokens covering PDFs, contracts, invoices, forms)
```

✅ **GOOD - Specialized Skills:**
```
pdf-form-processor/ (2,000 tokens, rarely needed)
contract-analyzer/ (2,000 tokens, rarely needed)
invoice-extractor/ (2,000 tokens, rarely needed)
```

**Efficiency:**
- Omnibus: Load 10,000 tokens for any document task
- Specialized: Load 2,000 tokens for specific task
- Savings: 80% through mutual exclusivity

---

## EXECUTABLE SCRIPTS FOR TOKEN EFFICIENCY

**Principle (Official Anthropic):**
> "Pre-written scripts consume zero context tokens until executed"

### When to Use Scripts vs. Instructions

**Use Scripts (Zero Context Until Execution):**
- Deterministic operations
- Data validation
- Format conversion
- API interactions
- File processing

**Use Instructions (Prose in SKILL.md):**
- Reasoning workflows
- Decision-making processes
- Creative tasks
- Contextual judgment

**Example Split:**
```markdown
# SKILL.md: Brief instruction (~50 tokens)
"Extract form fields using scripts/extract_fields.py, then validate results"

# scripts/extract_fields.py: Actual implementation (~0 tokens until execution)
[Full Python code - not loaded into context until execution]
```

**Token Efficiency:**
- Instructions describing extraction: ~300 tokens
- Script doing extraction: ~0 tokens (until run)
- Savings: 300 tokens per skill using scripts

---

## STRUCTURED LARGE FILES

**For files exceeding 100 lines (Best Practices):**
- Include table of contents at top
- Use clear section headers
- Enable Claude to navigate efficiently
- Support partial loading strategies

**Example:**
```markdown
# Comprehensive Guide to [Topic]

## Table of Contents
1. [Section 1]
2. [Section 2]
3. [Section 3]

---

## Section 1: [Topic]
[Content]

---

## Section 2: [Topic]
[Content]
```

**Benefit:** Claude can scan TOC (~50 tokens) and jump to relevant section rather than reading entire file

---

## TOKEN MEASUREMENT VALIDATION

### Pre-Deployment Checklist

**Metadata Efficiency:**
- [ ] YAML description under 600 characters (target)
- [ ] Description includes WHAT + WHEN (no wasted words)
- [ ] Name descriptive yet concise (<64 chars)

**SKILL.md Efficiency:**
- [ ] Line count within category budget (<500 lines max)
- [ ] Token estimate: lines × 8 within budget
- [ ] Critical content front-loaded (lines 1-80)
- [ ] No embedded templates/examples (extracted to resources/)

**Resource Efficiency:**
- [ ] Templates 30-60 lines each (200-500 tokens)
- [ ] Examples 100-200 lines each (500-1,500 tokens)
- [ ] Documentation 250-400 lines each (1,000-3,000 tokens)
- [ ] All references one level deep from SKILL.md

**Agent Integration Efficiency:**
- [ ] Skill reference ~20 tokens per agent
- [ ] No embedded skill content in agents
- [ ] Token savings calculated vs. embedded approach
- [ ] Multi-agent efficiency validated

### Success Metrics

**Per-Skill Efficiency:**
- YAML frontmatter: ~100 tokens or less
- SKILL.md body: Within category budget
- Agent reference: ~20 tokens
- Total base overhead: ~120 tokens per agent integration

**Ecosystem Efficiency:**
- 30 skills × 100 tokens (metadata) = 3,000 tokens baseline
- Agent definitions: 50% smaller with skill references
- Context window: 2x orchestration capacity increase

---

## OPTIMIZATION ITERATION PROCESS

### When SKILL.md Over Budget

**Step 1: Identify Heavy Sections**
```bash
# Count lines per section
wc -l SKILL.md
# Identify sections >100 lines
```

**Step 2: Classify Content**
- Essential workflow: Keep in SKILL.md
- Template format: Extract to resources/templates/
- Complete example: Extract to resources/examples/
- Philosophy/troubleshooting: Extract to resources/documentation/

**Step 3: Extract to Resources**
```markdown
# Before (in SKILL.md):
## Artifact Reporting Format

Use this exact template:
[30 lines of template content]

# After (in SKILL.md):
## Artifact Reporting Format

**Resource:** See `resources/templates/artifact-reporting-template.md` for complete format

# New file: resources/templates/artifact-reporting-template.md
[30 lines of template content - loaded on-demand]
```

**Step 4: Validate Savings**
- Measure new line count
- Estimate tokens: lines × 8
- Confirm within budget
- Test skill still functional

---

## REFERENCES

**Official Sources:**
- https://www.anthropic.com/engineering/equipping-agents-for-the-real-world-with-agent-skills

**Related Resources:**
- `resources/documentation/anti-patterns.md` - Token waste patterns to avoid
- `resources/documentation/progressive-loading-guide.md` - Architecture philosophy
- `/working-dir/claude-skills-best-practices-distilled.md` - Official Anthropic guidance
