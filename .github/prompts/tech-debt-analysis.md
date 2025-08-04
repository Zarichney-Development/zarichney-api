# Tech Debt Analysis for PR #{{PR_NUMBER}}

**PR Context:**
- Pull Request: #{{PR_NUMBER}} by @{{PR_AUTHOR}}
- Issue: {{ISSUE_REF}}
- Branch: {{SOURCE_BRANCH}} → {{TARGET_BRANCH}}

**Your Mission:**
Assess technical debt introduced or resolved by THIS PR.

**Analysis Focus:**
1. Complexity added by new code
2. Code duplication introduced
3. Performance implications of changes
4. Debt RESOLVED by refactoring

**What to Check:**
- Method complexity in new/modified code (from quality-analysis-data.json)
- New dependencies or coupling
- TODOs or FIXMEs added
- Refactoring improvements
- Performance patterns in changed code

**Output Format:**
### 🎯 Debt Impact Summary
- Net debt change: [Increased/Decreased/Neutral]
- Key concerns in new code

### 🚨 New Debt Introduced
- Complex methods (file:line, complexity score)
- Code duplication detected
- Performance concerns
- New TODOs/FIXMEs

### 🎉 Debt Resolved
- Refactoring wins
- Complexity reduced
- Performance improved
- Old TODOs resolved

### 📋 Recommendations
1. Immediate fixes for this PR
2. Future refactoring opportunities created

Be constructive and focus on THIS PR's impact. Celebrate debt reduction!