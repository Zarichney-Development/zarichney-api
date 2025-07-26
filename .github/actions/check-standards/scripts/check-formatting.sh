#!/bin/bash

# Standards Compliance Check: Code Formatting & Style
# Validates adherence to CodingStandards.md formatting requirements

set -e

echo "🎨 Checking Code Formatting & Style Standards..."

# Initialize violation tracking
MANDATORY_VIOLATIONS=0
RECOMMENDED_VIOLATIONS=0
OPTIONAL_VIOLATIONS=0

# Function to add violation
add_violation() {
  local severity="$1"
  local title="$2"
  local description="$3"
  local fix="$4"
  
  case "$severity" in
    "mandatory")
      MANDATORY_VIOLATIONS=$((MANDATORY_VIOLATIONS + 1))
      echo "🚫 **MANDATORY:** $title" >> standards-results/formatting-violations.md
      ;;
    "recommended")
      RECOMMENDED_VIOLATIONS=$((RECOMMENDED_VIOLATIONS + 1))
      echo "⚠️ **RECOMMENDED:** $title" >> standards-results/formatting-violations.md
      ;;
    "optional")
      OPTIONAL_VIOLATIONS=$((OPTIONAL_VIOLATIONS + 1))
      echo "💡 **OPTIONAL:** $title" >> standards-results/formatting-violations.md
      ;;
  esac
  
  echo "   - **Issue:** $description" >> standards-results/formatting-violations.md
  echo "   - **Fix:** $fix" >> standards-results/formatting-violations.md
  echo "   - **Reference:** [CodingStandards.md](../Docs/Standards/CodingStandards.md)" >> standards-results/formatting-violations.md
  echo "" >> standards-results/formatting-violations.md
}

# Initialize violations file
echo "## 🎨 Code Formatting & Style Violations" > standards-results/formatting-violations.md
echo "" >> standards-results/formatting-violations.md

# Check 1: .NET Format Compliance (MANDATORY)
echo "  Checking .NET format compliance..."
if ! dotnet format --verify-no-changes --verbosity diagnostic > format-check.log 2>&1; then
  add_violation "mandatory" \
    "Code formatting violations detected" \
    "Code does not comply with .editorconfig formatting rules" \
    "Run \`dotnet format\` to automatically fix formatting issues"
  
  # Include specific formatting violations in output
  echo "   - **Details:**" >> standards-results/formatting-violations.md
  echo "   \`\`\`" >> standards-results/formatting-violations.md
  tail -20 format-check.log >> standards-results/formatting-violations.md
  echo "   \`\`\`" >> standards-results/formatting-violations.md
  echo "" >> standards-results/formatting-violations.md
fi

# Check 2: File-scoped namespaces (RECOMMENDED)
echo "  Checking namespace conventions..."
cs_files_with_traditional_namespaces=$(find . -name "*.cs" -path "./Code/*" -exec grep -l "^namespace.*{" {} \; 2>/dev/null | wc -l || echo "0")
if [ "$cs_files_with_traditional_namespaces" -gt 0 ]; then
  add_violation "recommended" \
    "Traditional namespace declarations found" \
    "Found $cs_files_with_traditional_namespaces C# files using traditional namespace declarations instead of file-scoped namespaces" \
    "Convert to file-scoped namespaces (namespace MyNamespace; instead of namespace MyNamespace { })"
fi

# Check 3: Primary constructor usage (OPTIONAL)
echo "  Checking modern C# feature usage..."
cs_files_with_verbose_constructors=$(find . -name "*.cs" -path "./Code/*" -exec grep -l "public.*class.*" {} \; | \
  xargs grep -l "private readonly.*_.*;" 2>/dev/null | \
  xargs grep -L "public.*class.*(" 2>/dev/null | wc -l || echo "0")

if [ "$cs_files_with_verbose_constructors" -gt 5 ]; then
  add_violation "optional" \
    "Consider using primary constructors" \
    "Found multiple classes with traditional constructor patterns that could benefit from primary constructors" \
    "Consider refactoring to use primary constructors where appropriate for cleaner code"
fi

# Check 4: Collection expressions (RECOMMENDED for new code)
echo "  Checking collection expression usage..."
new_cs_files_with_old_collections=$(git diff --name-only origin/develop...HEAD | grep "\.cs$" | \
  xargs grep -l "new List<.*>{" 2>/dev/null | wc -l || echo "0")

if [ "$new_cs_files_with_old_collections" -gt 0 ]; then
  add_violation "recommended" \
    "Use collection expressions in new code" \
    "Found $new_cs_files_with_old_collections new/modified files using traditional collection initialization" \
    "Use collection expressions: [\"item1\", \"item2\"] instead of new List<string> { \"item1\", \"item2\" }"
fi

# Check 5: XML Documentation (RECOMMENDED)
echo "  Checking XML documentation coverage..."
public_members_without_docs=$(find . -name "*.cs" -path "./Code/*" -exec grep -l "public.*class\|public.*interface\|public.*enum" {} \; | \
  xargs grep -L "/// <summary>" 2>/dev/null | wc -l || echo "0")

if [ "$public_members_without_docs" -gt 0 ]; then
  add_violation "recommended" \
    "Missing XML documentation" \
    "Found $public_members_without_docs files with public types lacking XML documentation comments" \
    "Add /// <summary> XML documentation comments to all public types and members"
fi

# Check 6: Logging patterns (RECOMMENDED)
echo "  Checking logging standards..."
files_with_wrong_logger_pattern=$(find . -name "*.cs" -path "./Code/*" -exec grep -l "ILogger.*logger.*ForContext" {} \; 2>/dev/null | wc -l || echo "0")

if [ "$files_with_wrong_logger_pattern" -gt 0 ]; then
  add_violation "recommended" \
    "Incorrect logger injection pattern" \
    "Found $files_with_wrong_logger_pattern files using ILogger with .ForContext<T>() instead of ILogger<T>" \
    "Use constructor injection with ILogger<T> instead of ILogger followed by .ForContext<T>()"
fi

# Update violation counters
current_mandatory=$(cat standards-results/mandatory-count)
current_recommended=$(cat standards-results/recommended-count)
current_optional=$(cat standards-results/optional-count)

echo $((current_mandatory + MANDATORY_VIOLATIONS)) > standards-results/mandatory-count
echo $((current_recommended + RECOMMENDED_VIOLATIONS)) > standards-results/recommended-count
echo $((current_optional + OPTIONAL_VIOLATIONS)) > standards-results/optional-count

# Clean up temp files
rm -f format-check.log

echo "  ✅ Code formatting check completed"
echo "     Mandatory violations: $MANDATORY_VIOLATIONS"
echo "     Recommended violations: $RECOMMENDED_VIOLATIONS"  
echo "     Optional violations: $OPTIONAL_VIOLATIONS"