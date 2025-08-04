# Standards Compliance for PR #{{PR_NUMBER}}

**PR Context:**
- Pull Request: #{{PR_NUMBER}} by @{{PR_AUTHOR}}
- Issue: {{ISSUE_REF}}
- Branch: {{SOURCE_BRANCH}} → {{TARGET_BRANCH}}

**Your Mission:**
Review THIS PR's changes for compliance with project standards.

**Analysis Focus:**
1. Check formatting violations in changed files (from quality-analysis-data.json)
2. Review commit messages for this PR
3. Verify documentation updates if APIs changed
4. Check naming conventions in new code

**Standards to Check:**
- Code formatting (.editorconfig compliance)
- Commit message format (conventional commits)
- Test naming conventions
- Documentation requirements for public APIs
- File organization standards

**Output Format:**
### 🎯 Compliance Summary
- Overall compliance level
- Key violations found in this PR

### 🚫 Mandatory Fixes (Blocks Merge)
- Formatting violations (file:line)
- Missing required documentation
- Invalid commit messages
- Run: `dotnet format` to fix

### ⚠️ Recommended Fixes
- Naming improvements
- Documentation enhancements
- Code organization issues

### ✅ Quick Fix Commands
```bash
# Commands to fix issues
dotnet format
# Other specific commands
```

### 💚 Compliance Wins
- Good practices followed
- Standards properly implemented

Focus on actionable fixes for THIS PR only. Reference specific files and lines from the PR changes.