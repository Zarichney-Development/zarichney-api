#!/bin/bash

# Standards Compliance Check: Git & Task Management Standards
# Validates adherence to TaskManagementStandards.md requirements

set -e

echo "📋 Checking Git & Task Management Standards..."

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
      echo "🚫 **MANDATORY:** $title" >> standards-results/git-violations.md
      ;;
    "recommended")
      RECOMMENDED_VIOLATIONS=$((RECOMMENDED_VIOLATIONS + 1))
      echo "⚠️ **RECOMMENDED:** $title" >> standards-results/git-violations.md
      ;;
    "optional")
      OPTIONAL_VIOLATIONS=$((OPTIONAL_VIOLATIONS + 1))
      echo "💡 **OPTIONAL:** $title" >> standards-results/git-violations.md
      ;;
  esac
  
  echo "   - **Issue:** $description" >> standards-results/git-violations.md
  echo "   - **Fix:** $fix" >> standards-results/git-violations.md
  echo "   - **Reference:** [TaskManagementStandards.md](../Docs/Standards/TaskManagementStandards.md)" >> standards-results/git-violations.md
  echo "" >> standards-results/git-violations.md
}

# Initialize violations file
echo "## 📋 Git & Task Management Violations" > standards-results/git-violations.md
echo "" >> standards-results/git-violations.md

# Get PR information
if [ -n "$PR_NUMBER" ]; then
  echo "  Analyzing PR #$PR_NUMBER..."
  
  # Get PR details using GitHub CLI
  PR_TITLE=$(gh pr view "$PR_NUMBER" --json title --jq '.title' 2>/dev/null || echo "")
  PR_BODY=$(gh pr view "$PR_NUMBER" --json body --jq '.body' 2>/dev/null || echo "")
  
  # Check 1: PR title follows conventional commit format (MANDATORY)
  echo "  Checking PR title format..."
  if [[ ! "$PR_TITLE" =~ ^(feat|fix|docs|style|refactor|perf|test|build|ci|chore)(\(.+\))?:\ .+\ \(#[0-9]+\)$ ]]; then
    add_violation "mandatory" \
      "PR title does not follow conventional commit format" \
      "PR title '$PR_TITLE' does not match required format: <type>: <description> (#ISSUE_ID)" \
      "Update PR title to follow format: 'feat: implement feature (#53)'"
  fi
  
  # Check 2: PR body contains issue reference (MANDATORY)
  echo "  Checking PR body for issue reference..."
  if [[ ! "$PR_BODY" =~ (Closes|Fixes|Refs)\ #[0-9]+ ]]; then
    add_violation "mandatory" \
      "PR body missing issue reference" \
      "PR body does not contain required issue reference with closing keywords" \
      "Add 'Closes #53' or similar reference to the PR body"
  fi
  
  # Check 3: PR targets correct base branch (RECOMMENDED)
  echo "  Checking target branch..."
  PR_BASE=$(gh pr view "$PR_NUMBER" --json baseRefName --jq '.baseRefName' 2>/dev/null || echo "")
  if [[ "$PR_BASE" != "develop" && "$PR_BASE" != "main" ]]; then
    add_violation "recommended" \
      "Unusual target branch" \
      "PR targets '$PR_BASE' instead of standard 'develop' or 'main'" \
      "Verify target branch is correct or retarget to 'develop'"
  fi
fi

# Check 4: Branch naming convention (MANDATORY)
echo "  Checking branch naming convention..."
CURRENT_BRANCH=$(git branch --show-current)
if [[ ! "$CURRENT_BRANCH" =~ ^(feature|test|hotfix|bugfix)/issue-[0-9]+-[a-z0-9-]+$ ]]; then
  add_violation "mandatory" \
    "Branch name does not follow naming convention" \
    "Branch '$CURRENT_BRANCH' does not match required format: feature/issue-{ID}-{description}" \
    "Create branches with format: feature/issue-53-standards-compliance-check"
fi

# Check 5: Commit message format (MANDATORY)
echo "  Checking commit message format..."
# Get commits in this branch compared to base
commits=$(git log --oneline origin/"$BASE_BRANCH"..HEAD --pretty=format:"%s" 2>/dev/null || git log --oneline -n 5 --pretty=format:"%s")

invalid_commits=0
while IFS= read -r commit_msg; do
  if [[ -n "$commit_msg" && ! "$commit_msg" =~ ^(feat|fix|docs|style|refactor|perf|test|build|ci|chore)(\(.+\))?:\ .+ ]]; then
    invalid_commits=$((invalid_commits + 1))
  fi
done <<< "$commits"

if [ "$invalid_commits" -gt 0 ]; then
  add_violation "mandatory" \
    "Invalid commit message format" \
    "Found $invalid_commits commit(s) not following conventional commit format" \
    "Use format: 'type: description' (e.g., 'feat: add new feature')"
fi

# Check 6: Issue references in commits (RECOMMENDED)
echo "  Checking for issue references in commits..."
commits_without_refs=0
while IFS= read -r commit_msg; do
  if [[ -n "$commit_msg" && ! "$commit_msg" =~ \(#[0-9]+\) ]]; then
    commits_without_refs=$((commits_without_refs + 1))
  fi
done <<< "$commits"

if [ "$commits_without_refs" -gt 0 ]; then
  add_violation "recommended" \
    "Commits missing issue references" \
    "Found $commits_without_refs commit(s) without issue references in format (#123)" \
    "Include issue reference in commit messages: 'feat: add feature (#53)'"
fi

# Check 7: Commit granularity (OPTIONAL)
echo "  Checking commit granularity..."
total_commits=$(echo "$commits" | wc -l)
if [ "$total_commits" -gt 10 ]; then
  add_violation "optional" \
    "Large number of commits" \
    "Branch contains $total_commits commits - consider squashing related commits" \
    "Group related changes into logical commits before merging"
fi

# Check 8: Files changed analysis (OPTIONAL)
echo "  Analyzing changed files..."
files_changed=$(git diff --name-only origin/"$BASE_BRANCH"...HEAD 2>/dev/null | wc -l || echo "0")
if [ "$files_changed" -gt 50 ]; then
  add_violation "optional" \
    "Large changeset" \
    "PR modifies $files_changed files - consider breaking into smaller PRs" \
    "Split large changes into focused, reviewable pull requests"
fi

# Update violation counters
current_mandatory=$(cat standards-results/mandatory-count)
current_recommended=$(cat standards-results/recommended-count)
current_optional=$(cat standards-results/optional-count)

echo $((current_mandatory + MANDATORY_VIOLATIONS)) > standards-results/mandatory-count
echo $((current_recommended + RECOMMENDED_VIOLATIONS)) > standards-results/recommended-count
echo $((current_optional + OPTIONAL_VIOLATIONS)) > standards-results/optional-count

echo "  ✅ Git & task management check completed"
echo "     Mandatory violations: $MANDATORY_VIOLATIONS"
echo "     Recommended violations: $RECOMMENDED_VIOLATIONS"  
echo "     Optional violations: $OPTIONAL_VIOLATIONS"