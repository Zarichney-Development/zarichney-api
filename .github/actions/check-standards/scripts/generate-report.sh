#!/bin/bash

# Standards Compliance Check: Report Generation
# Generates comprehensive compliance report for PR comments

set -e

echo "📄 Generating Standards Compliance Report..."

# Read violation counts
MANDATORY_COUNT=$(cat standards-results/mandatory-count)
RECOMMENDED_COUNT=$(cat standards-results/recommended-count)
OPTIONAL_COUNT=$(cat standards-results/optional-count)

# Calculate compliance score
TOTAL_VIOLATIONS=$((MANDATORY_COUNT * 10 + RECOMMENDED_COUNT * 3 + OPTIONAL_COUNT * 1))
COMPLIANCE_SCORE=$((100 - TOTAL_VIOLATIONS))
if [ "$COMPLIANCE_SCORE" -lt 0 ]; then
  COMPLIANCE_SCORE=0
fi

# Determine overall status
if [ "$MANDATORY_COUNT" -gt 0 ]; then
  OVERALL_STATUS="🚫 **FAILED**"
  STATUS_COLOR="red"
  MERGE_STATUS="❌ **MERGE BLOCKED** - Fix mandatory violations before merging"
elif [ "$RECOMMENDED_COUNT" -gt 10 ]; then
  OVERALL_STATUS="⚠️ **NEEDS ATTENTION**"
  STATUS_COLOR="yellow"
  MERGE_STATUS="⚠️ **MERGE ALLOWED** - Consider addressing recommended violations"
else
  OVERALL_STATUS="✅ **PASSED**"
  STATUS_COLOR="green"
  MERGE_STATUS="✅ **READY TO MERGE** - All critical standards met"
fi

# Generate main report header
cat >> standards-report.md << EOF
## 📊 Executive Summary

| Metric | Value | Status |
|--------|-------|--------|
| **Overall Status** | $OVERALL_STATUS | ![Badge](https://img.shields.io/badge/Status-$(echo $OVERALL_STATUS | sed 's/.*\*\*\(.*\)\*\*.*/\1/' | tr ' ' '_')-$STATUS_COLOR) |
| **Compliance Score** | $COMPLIANCE_SCORE/100 | ![Score](https://img.shields.io/badge/Score-$COMPLIANCE_SCORE%25-$([ $COMPLIANCE_SCORE -ge 80 ] && echo "green" || [ $COMPLIANCE_SCORE -ge 60 ] && echo "yellow" || echo "red")) |
| **Mandatory Violations** | $MANDATORY_COUNT | ![Mandatory](https://img.shields.io/badge/Mandatory-$MANDATORY_COUNT-$([ $MANDATORY_COUNT -eq 0 ] && echo "green" || echo "red")) |
| **Recommended Violations** | $RECOMMENDED_COUNT | ![Recommended](https://img.shields.io/badge/Recommended-$RECOMMENDED_COUNT-$([ $RECOMMENDED_COUNT -eq 0 ] && echo "green" || [ $RECOMMENDED_COUNT -le 5 ] && echo "yellow" || echo "orange")) |
| **Optional Violations** | $OPTIONAL_COUNT | ![Optional](https://img.shields.io/badge/Optional-$OPTIONAL_COUNT-$([ $OPTIONAL_COUNT -eq 0 ] && echo "green" || [ $OPTIONAL_COUNT -le 10 ] && echo "yellow" || echo "orange")) |

### 🚪 Merge Decision
$MERGE_STATUS

EOF

# Add violation severity legend
cat >> standards-report.md << EOF
### 📋 Violation Severity Guide

- 🚫 **MANDATORY**: Critical violations that block merging. Must be fixed before deployment.
- ⚠️ **RECOMMENDED**: Important quality improvements that should be addressed.
- 💡 **OPTIONAL**: Suggestions for code quality enhancement and best practices.

EOF

# Add detailed violations if any exist
if [ "$MANDATORY_COUNT" -gt 0 ] || [ "$RECOMMENDED_COUNT" -gt 0 ] || [ "$OPTIONAL_COUNT" -gt 0 ]; then
  echo "## 🔍 Detailed Violation Reports" >> standards-report.md
  echo "" >> standards-report.md
  
  # Include each violation category if file exists and has content
  for category in formatting git testing documentation; do
    if [ -f "standards-results/${category}-violations.md" ] && [ -s "standards-results/${category}-violations.md" ]; then
      cat "standards-results/${category}-violations.md" >> standards-report.md
      echo "" >> standards-report.md
    fi
  done
else
  cat >> standards-report.md << EOF
## 🎉 Excellent Work!

No standards violations detected! Your code meets all project quality standards.

### ✅ What's Working Well:
- Code formatting follows .editorconfig rules
- Git commit messages follow conventional format
- Testing standards are properly implemented
- Documentation is up to date

EOF
fi

# Add quick fix section for mandatory violations
if [ "$MANDATORY_COUNT" -gt 0 ]; then
  cat >> standards-report.md << EOF
## 🔧 Quick Fix Guide

### Priority 1: Mandatory Violations (Blocking Merge)

To unblock this PR, address these critical issues:

1. **Code Formatting**: Run \`dotnet format\` to fix formatting violations
2. **Git Standards**: Ensure commit messages follow conventional format: \`type: description (#issue)\`
3. **Test Requirements**: Add missing test categories and ensure proper naming
4. **Integration Tests**: Use required test framework components

### Commands to Run:
\`\`\`bash
# Fix formatting issues
dotnet format

# Verify formatting compliance
dotnet format --verify-no-changes --verbosity diagnostic

# Run tests to ensure nothing is broken
/test-report summary
\`\`\`

EOF
fi

# Add improvement suggestions
if [ "$RECOMMENDED_COUNT" -gt 0 ] || [ "$OPTIONAL_COUNT" -gt 0 ]; then
  cat >> standards-report.md << EOF
## 💡 Improvement Opportunities

While not blocking merge, these improvements will enhance code quality:

### Documentation Enhancements
- Consider adding XML documentation to public APIs
- Update README.md files for modified modules
- Ensure proper linking between documentation files

### Testing Best Practices
- Use Test Data Builders for complex object creation
- Implement AAA pattern with clear comments
- Consider adding more integration test coverage

### Code Quality
- Leverage modern C# features like primary constructors
- Use collection expressions for new code
- Ensure proper logging patterns with ILogger<T>

EOF
fi

# Add standards reference links
cat >> standards-report.md << EOF
## 📚 Standards Reference

For detailed information about project standards, see:

- 📋 [Task Management Standards](../Docs/Standards/TaskManagementStandards.md)
- 🎨 [Coding Standards](../Docs/Standards/CodingStandards.md)  
- 🧪 [Testing Standards](../Docs/Standards/TestingStandards.md)
- 📚 [Documentation Standards](../Docs/Standards/DocumentationStandards.md)

## 🤖 Automated Standards Check

This report was generated automatically by the Standards Compliance Check workflow. The check analyzes:

- Code formatting compliance with .editorconfig
- Git commit message and branch naming conventions
- Testing standards and framework usage
- Documentation coverage and structure
- Modern C# feature adoption

**Next Steps**: Address mandatory violations to unblock merging, then consider recommended improvements for enhanced code quality.

---

🤖 *Generated by [Zarichney Standards Compliance Check](../../.github/workflows/standards-compliance-check.yml)*  
📊 *Analysis completed at: $(date -u +"%Y-%m-%d %H:%M:%S UTC")*  
🔄 *Workflow run: [\`$GITHUB_RUN_ID\`]($GITHUB_SERVER_URL/$GITHUB_REPOSITORY/actions/runs/$GITHUB_RUN_ID)*

EOF

echo "  ✅ Standards compliance report generated"
echo "     Report location: standards-report.md"
echo "     Total violations: $((MANDATORY_COUNT + RECOMMENDED_COUNT + OPTIONAL_COUNT))"
echo "     Compliance score: $COMPLIANCE_SCORE/100"