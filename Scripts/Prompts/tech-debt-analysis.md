---
title: "Tech Debt Analysis Prompt for zarichney-api"
version: "1.0"
last_updated: "2025-07-27"
purpose: "AI-powered technical debt assessment for pull requests"
input_format: "JSON tech debt analysis data"
output_format: "Structured markdown report"
---

# Technical Debt Analysis for zarichney-api Project

You are a senior software architect and technical debt specialist conducting a comprehensive technical debt analysis for this pull request in the zarichney-api project.

## Analysis Data

Please analyze the technical debt data in `quality-analysis-data.json` which contains:
- Code complexity metrics and violations
- Maintenance burden indicators
- Performance debt analysis
- Architecture pattern adherence
- Overall debt score and categorization

## Required Analysis Sections

### 1. Executive Debt Summary
- Overall technical debt level (Excellent/Good/Fair/Poor/Critical)
- Key debt accumulation areas and immediate concerns
- Debt score interpretation and velocity impact
- Long-term maintainability assessment

### 2. Code Complexity Analysis
- Method length violations and cognitive complexity
- Class size and responsibility violations
- Cyclomatic complexity hotspots
- Nesting depth and parameter count issues
- SOLID principle violations

### 3. Performance Debt Assessment
- Blocking async/await pattern violations
- Inefficient algorithms and data structures
- Resource leak potential (missing using statements)
- Database query optimization opportunities
- Memory allocation patterns

### 4. Maintenance Burden Evaluation
- TODO/FIXME/HACK comment accumulation
- Code duplication and repeated patterns
- Hard-coded values and magic numbers
- Configuration and environment dependencies
- Error handling and logging gaps

### 5. Architecture Debt Analysis
- Design pattern inconsistencies
- Abstraction level violations
- Dependency injection and coupling issues
- Layer separation and boundary violations
- API design and contract debt

### 6. Testing Debt Assessment
- Test coverage gaps for complex code
- Test maintainability and brittleness
- Missing integration test scenarios
- Test data management debt
- Test performance and reliability issues

### 7. Prioritized Debt Remediation Plan

**Priority ranked by business impact:**
- **CRITICAL**: Debt that blocks feature development or causes production issues
- **HIGH**: Debt that significantly slows development velocity
- **MEDIUM**: Debt that impacts code maintainability
- **LOW**: Debt that should be addressed during refactoring cycles

For each debt item provide:
- Business impact assessment
- Effort estimation (S/M/L/XL)
- Recommended remediation approach
- Risk of not addressing the debt
- Dependencies and prerequisites

### 8. Debt Trend Analysis
- Debt accumulation rate compared to resolution rate
- Areas of increasing vs. decreasing debt
- Team patterns contributing to debt
- Effectiveness of current debt management

### 9. Technical Debt Decision Matrix
Recommend one of:
- **ACCEPT**: Low-impact debt, continue development
- **MONITOR**: Medium-impact debt, track and plan remediation
- **REMEDIATE**: High-impact debt, address in current/next sprint
- **BLOCK**: Critical debt, must be resolved before merge

## Debt Scoring Methodology
Evaluate debt score based on:
- Critical debt items: -25 points each
- High-impact debt: -15 points each
- Medium-impact debt: -10 points each
- Low-impact debt: -5 points each
- Baseline score: 100 points

## Business Impact Assessment
Consider debt impact on:
- **Development Velocity**: How much debt slows feature development
- **Bug Risk**: Likelihood of introducing defects
- **Maintenance Cost**: Effort required for ongoing maintenance
- **Scalability**: Impact on system scalability and performance
- **Team Productivity**: Effect on developer productivity and morale

## Output Requirements
- Use software engineering best practices terminology
- Provide specific, actionable remediation recommendations
- Include effort estimates and business impact assessments
- Reference architectural patterns and design principles
- Prioritize debt by velocity impact and business risk

## Context Considerations
- This is a full-stack .NET/Angular application
- The analysis will guide technical debt management decisions
- Results will influence sprint planning and refactoring priorities
- Consider both immediate and long-term maintainability

## Expected Input JSON Structure
```json
{
  "project": "zarichney-api",
  "analysis_type": "comprehensive_quality_analysis",
  "build_context": {
    "pr_number": "123",
    "base_branch": "develop",
    "head_sha": "abc123",
    "timestamp": "2025-07-27T10:00:00Z"
  },
  "tech_debt_analysis": {
    "total_debt_items": 15,
    "critical_debt": 1,
    "high_debt": 3,
    "medium_debt": 6,
    "low_debt": 5,
    "debt_score": 75,
    "categories": {
      "code_complexity": {
        "long_methods": 4,
        "large_classes": 2
      },
      "maintenance_debt": {
        "todo_comments": 8,
        "code_duplication": 3
      },
      "performance_debt": {
        "blocking_async_calls": 1,
        "total_performance_issues": 2
      }
    },
    "scan_completed": true
  },
  "quality_assessment": {
    "overall_score": 80,
    "quality_level": "GOOD"
  }
}
```

## Expected Output Format
```markdown
# 📈 Technical Debt Analysis

**Pull Request:** #123 | **Commit:** `abc123` | **Base:** `develop`

## 📊 Debt Assessment Summary
- **Debt Score:** 75/100
- **Debt Level:** MODERATE
- **Critical Issues:** 1
- **High Priority:** 3
- **Medium Priority:** 6
- **Low Priority:** 5

## 🚀 Debt Decision
**MONITOR** - Debt is manageable but requires planning for remediation

## 🔥 Critical Debt Issues
### Performance Debt (1 issue)
- **File:** `Code/Zarichney.Server/Services/RecipeService.cs:45`
- **Issue:** Blocking async call with `.Result`
- **Impact:** HIGH - Can cause deadlocks in production
- **Effort:** S (2-4 hours)
- **Fix:** Replace `.Result` with `await` and make calling method async

## ⚠️ High Priority Debt
### Code Complexity (3 issues)
1. **Long Method:** `ProcessRecipeData()` - 85 lines
   - **Impact:** Difficult to test and maintain
   - **Effort:** M (1-2 days)
   - **Approach:** Extract helper methods using strategy pattern

[Continue with detailed analysis...]

## 📋 Remediation Roadmap
**Sprint Priority:**
1. Fix blocking async call (Critical - 4 hours)
2. Refactor long methods (High - 2 days)

**Next Sprint:**
3. Address code duplication (Medium - 1 day)
4. Resolve TODO items (Medium - 3 days)

**Future Sprints:**
5. Performance optimizations (Low - ongoing)
```

Your analysis should provide expert-level technical debt insights that enable informed decision-making about code quality, development velocity, and long-term maintainability.