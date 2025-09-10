#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

declare -A FILE_HEADERS=(
  [".github/prompts/standards-compliance.md"]="## Code Review Report - Standards Compliance Analysis"
  [".github/prompts/tech-debt-analysis.md"]="## Code Review Report - Tech Debt Analysis"
  [".github/prompts/testing-analysis.md"]="## Code Review Report - Testing Analysis"
  [".github/prompts/security-analysis.md"]="## Code Review Report - Security Analysis"
  [".github/prompts/merge-orchestrator-analysis.md"]="## Code Review Report - PR Merge Review Analysis"
)

echo "🔍 Validating canonical AI header keys in prompts and workflow detection..."

errors=0

for file in "${!FILE_HEADERS[@]}"; do
  header="${FILE_HEADERS[$file]}"
  if ! grep -Fq "$header" "$ROOT_DIR/$file"; then
    echo "❌ Missing header in $file: $header"
    ((errors++)) || true
  else
    echo "✅ Found header in $file"
  fi
done

BUILD_YML="$ROOT_DIR/.github/workflows/build.yml"
for header in "${FILE_HEADERS[@]}"; do
  if ! grep -Fq "$header" "$BUILD_YML"; then
    echo "❌ build.yml does not reference header: $header"
    ((errors++)) || true
  else
    echo "✅ build.yml references header: $header"
  fi
done

if [[ $errors -gt 0 ]]; then
  echo "❌ Validation failed with $errors error(s)."
  exit 1
fi

echo "✅ All headers validated successfully."
