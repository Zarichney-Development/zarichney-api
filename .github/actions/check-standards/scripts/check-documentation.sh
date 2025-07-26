#!/bin/bash

# Standards Compliance Check: Documentation Standards
# Validates adherence to DocumentationStandards.md requirements

set -e

echo "📚 Checking Documentation Standards..."

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
      echo "🚫 **MANDATORY:** $title" >> standards-results/documentation-violations.md
      ;;
    "recommended")
      RECOMMENDED_VIOLATIONS=$((RECOMMENDED_VIOLATIONS + 1))
      echo "⚠️ **RECOMMENDED:** $title" >> standards-results/documentation-violations.md
      ;;
    "optional")
      OPTIONAL_VIOLATIONS=$((OPTIONAL_VIOLATIONS + 1))
      echo "💡 **OPTIONAL:** $title" >> standards-results/documentation-violations.md
      ;;
  esac
  
  echo "   - **Issue:** $description" >> standards-results/documentation-violations.md
  echo "   - **Fix:** $fix" >> standards-results/documentation-violations.md
  echo "   - **Reference:** [DocumentationStandards.md](../Docs/Standards/DocumentationStandards.md)" >> standards-results/documentation-violations.md
  echo "" >> standards-results/documentation-violations.md
}

# Initialize violations file
echo "## 📚 Documentation Standards Violations" > standards-results/documentation-violations.md
echo "" >> standards-results/documentation-violations.md

# Check 1: README.md files in code directories (RECOMMENDED)
echo "  Checking README.md coverage..."
code_dirs_without_readme=$(find ./Code -type d -name "*" | while read dir; do
  if [ -n "$(find "$dir" -maxdepth 1 -name "*.cs" 2>/dev/null)" ] && [ ! -f "$dir/README.md" ]; then
    echo "$dir"
  fi
done | wc -l || echo "0")

if [ "$code_dirs_without_readme" -gt 0 ]; then
  add_violation "recommended" \
    "Code directories missing README.md files" \
    "Found $code_dirs_without_readme directories with C# code but no README.md documentation" \
    "Add README.md files following the template in Docs/Templates/ReadmeTemplate.md"
fi

# Check 2: README.md template compliance (RECOMMENDED)
echo "  Checking README.md template compliance..."
readme_files_missing_sections=$(find . -name "README.md" -path "./Code/*" | while read file; do
  if ! grep -q "## 1\. Purpose & Responsibility" "$file" 2>/dev/null || \
     ! grep -q "## 2\. Architecture & Key Concepts" "$file" 2>/dev/null || \
     ! grep -q "## 3\. Interface Contract & Assumptions" "$file" 2>/dev/null; then
    echo "$file"
  fi
done | wc -l || echo "0")

if [ "$readme_files_missing_sections" -gt 0 ]; then
  add_violation "recommended" \
    "README.md files not following template structure" \
    "Found $readme_files_missing_sections README.md files missing required template sections" \
    "Update README.md files to follow the structure in Docs/Templates/ReadmeTemplate.md"
fi

# Check 3: Last Updated dates in README files (RECOMMENDED)
echo "  Checking README.md maintenance dates..."
readme_files_without_dates=$(find . -name "README.md" -path "./Code/*" | while read file; do
  if ! grep -q "Last Updated:" "$file" 2>/dev/null; then
    echo "$file"
  fi
done | wc -l || echo "0")

if [ "$readme_files_without_dates" -gt 0 ]; then
  add_violation "recommended" \
    "README.md files missing Last Updated dates" \
    "Found $readme_files_without_dates README.md files without 'Last Updated' timestamps" \
    "Add 'Last Updated: YYYY-MM-DD' to the header of each README.md file"
fi

# Check 4: Outdated README files (OPTIONAL)
echo "  Checking for potentially outdated README files..."
outdated_readme_count=0
current_date=$(date +%s)
cutoff_date=$((current_date - 180 * 24 * 3600)) # 6 months ago

find . -name "README.md" -path "./Code/*" | while read file; do
  if grep -q "Last Updated:" "$file" 2>/dev/null; then
    last_updated=$(grep "Last Updated:" "$file" | sed 's/.*Last Updated: *//' | head -1)
    if [[ "$last_updated" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}$ ]]; then
      file_date=$(date -d "$last_updated" +%s 2>/dev/null || echo "$current_date")
      if [ "$file_date" -lt "$cutoff_date" ]; then
        outdated_readme_count=$((outdated_readme_count + 1))
      fi
    fi
  fi
done

if [ "$outdated_readme_count" -gt 3 ]; then
  add_violation "optional" \
    "Potentially outdated README files" \
    "Found $outdated_readme_count README.md files that may not have been updated recently" \
    "Review and update README.md files to reflect current code state"
fi

# Check 5: XML documentation coverage (RECOMMENDED)
echo "  Checking XML documentation coverage..."
changed_cs_files=$(git diff --name-only origin/"$BASE_BRANCH"...HEAD 2>/dev/null | grep "\.cs$" | grep -v Tests || echo "")

if [ -n "$changed_cs_files" ]; then
  public_classes_without_docs=0
  for file in $changed_cs_files; do
    if [ -f "$file" ]; then
      public_classes=$(grep -c "public.*class\|public.*interface\|public.*enum" "$file" 2>/dev/null || echo "0")
      xml_docs=$(grep -c "/// <summary>" "$file" 2>/dev/null || echo "0")
      if [ "$public_classes" -gt "$xml_docs" ]; then
        public_classes_without_docs=$((public_classes_without_docs + public_classes - xml_docs))
      fi
    fi
  done
  
  if [ "$public_classes_without_docs" -gt 0 ]; then
    add_violation "recommended" \
      "New public types missing XML documentation" \
      "Found $public_classes_without_docs new public types without XML documentation comments" \
      "Add /// <summary> XML documentation to all new public classes, interfaces, and enums"
  fi
fi

# Check 6: Broken internal links (RECOMMENDED)
echo "  Checking for broken internal links..."
broken_links=0
find . -name "README.md" | while read file; do
  grep -o '\](\.\.\/[^)]*\.md)' "$file" 2>/dev/null | while read link; do
    # Extract the path from the link
    path=$(echo "$link" | sed 's/](//; s/)//')
    full_path=$(realpath -m "$(dirname "$file")/$path" 2>/dev/null || echo "")
    if [ -n "$full_path" ] && [ ! -f "$full_path" ]; then
      broken_links=$((broken_links + 1))
    fi
  done
done

if [ "$broken_links" -gt 0 ]; then
  add_violation "recommended" \
    "Broken internal documentation links" \
    "Found $broken_links broken relative links to other README.md files" \
    "Fix broken links or update paths to point to correct documentation files"
fi

# Check 7: Code changes without documentation updates (RECOMMENDED)
echo "  Checking for code changes without documentation updates..."
changed_code_dirs=$(git diff --name-only origin/"$BASE_BRANCH"...HEAD 2>/dev/null | grep "\.cs$" | xargs dirname | sort | uniq || echo "")
updated_readme_dirs=$(git diff --name-only origin/"$BASE_BRANCH"...HEAD 2>/dev/null | grep "README\.md$" | xargs dirname | sort | uniq || echo "")

undocumented_changes=0
for dir in $changed_code_dirs; do
  if [ -f "$dir/README.md" ]; then
    # Check if this directory's README was updated
    if ! echo "$updated_readme_dirs" | grep -q "^$dir$"; then
      undocumented_changes=$((undocumented_changes + 1))
    fi
  fi
done

if [ "$undocumented_changes" -gt 0 ]; then
  add_violation "recommended" \
    "Code changes without documentation updates" \
    "Found $undocumented_changes directories with code changes but no README.md updates" \
    "Review if code changes impact documented architecture, interfaces, or behavior"
fi

# Check 8: Missing parent/child links (OPTIONAL)
echo "  Checking README.md linking structure..."
readme_files_without_links=$(find ./Code -name "README.md" | while read file; do
  has_parent_link=$(grep -c "\*\*Parent:\*\*" "$file" 2>/dev/null || echo "0")
  has_links=$(grep -c "\[.*\](.*README\.md)" "$file" 2>/dev/null || echo "0")
  
  if [ "$has_parent_link" -eq 0 ] && [ "$has_links" -eq 0 ]; then
    echo "$file"
  fi
done | wc -l || echo "0")

if [ "$readme_files_without_links" -gt 3 ]; then
  add_violation "optional" \
    "README files with poor linking structure" \
    "Found $readme_files_without_links README.md files without proper parent/child links" \
    "Add navigation links to create a cohesive documentation network"
fi

# Update violation counters
current_mandatory=$(cat standards-results/mandatory-count)
current_recommended=$(cat standards-results/recommended-count)
current_optional=$(cat standards-results/optional-count)

echo $((current_mandatory + MANDATORY_VIOLATIONS)) > standards-results/mandatory-count
echo $((current_recommended + RECOMMENDED_VIOLATIONS)) > standards-results/recommended-count
echo $((current_optional + OPTIONAL_VIOLATIONS)) > standards-results/optional-count

echo "  ✅ Documentation standards check completed"
echo "     Mandatory violations: $MANDATORY_VIOLATIONS"
echo "     Recommended violations: $RECOMMENDED_VIOLATIONS"  
echo "     Optional violations: $OPTIONAL_VIOLATIONS"