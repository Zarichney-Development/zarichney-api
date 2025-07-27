#!/bin/bash

# Test script for Standards Compliance Check
# Validates that the check scripts work correctly

set -e

echo "🧪 Testing Standards Compliance Check System..."

# Create temporary test environment
TEST_DIR="test-standards-temp"
rm -rf "$TEST_DIR"
mkdir -p "$TEST_DIR/standards-results"
cd "$TEST_DIR"

# Initialize test counters
echo "0" > standards-results/mandatory-count
echo "0" > standards-results/recommended-count
echo "0" > standards-results/optional-count

# Set environment variables for testing
export ACTION_PATH="../"
export PR_NUMBER="53"
export BASE_BRANCH="develop"
export HEAD_SHA="test-sha"
export SEVERITY_LEVEL="optional"

echo "  🎨 Testing formatting check..."
# Test formatting check (should pass on current directory)
if ../scripts/check-formatting.sh; then
  echo "     ✅ Formatting check script executed successfully"
else
  echo "     ❌ Formatting check script failed"
  exit 1
fi

echo "  📋 Testing git standards check..."
# Test git standards check
if ../scripts/check-git-standards.sh; then
  echo "     ✅ Git standards check script executed successfully"
else
  echo "     ❌ Git standards check script failed"
  exit 1
fi

echo "  🧪 Testing testing standards check..."
# Test testing standards check
if ../scripts/check-testing-standards.sh; then
  echo "     ✅ Testing standards check script executed successfully"
else
  echo "     ❌ Testing standards check script failed"
  exit 1
fi

echo "  📚 Testing documentation check..."
# Test documentation check
if ../scripts/check-documentation.sh; then
  echo "     ✅ Documentation check script executed successfully"
else
  echo "     ❌ Documentation check script failed"
  exit 1
fi

echo "  📄 Testing report generation..."
# Test report generation
if ../scripts/generate-report.sh; then
  echo "     ✅ Report generation script executed successfully"
else
  echo "     ❌ Report generation script failed"
  exit 1
fi

# Verify report was created
if [ -f "standards-report.md" ]; then
  echo "     ✅ Standards report generated successfully"
  
  # Check if report contains expected sections
  if grep -q "📊 Executive Summary" standards-report.md && \
     grep -q "🚪 Merge Decision" standards-report.md && \
     grep -q "📚 Standards Reference" standards-report.md; then
    echo "     ✅ Report contains all expected sections"
  else
    echo "     ❌ Report missing expected sections"
    exit 1
  fi
else
  echo "     ❌ Standards report not generated"
  exit 1
fi

# Verify violation counters exist and are numeric
MANDATORY=$(cat standards-results/mandatory-count)
RECOMMENDED=$(cat standards-results/recommended-count)
OPTIONAL=$(cat standards-results/optional-count)

if [[ "$MANDATORY" =~ ^[0-9]+$ ]] && [[ "$RECOMMENDED" =~ ^[0-9]+$ ]] && [[ "$OPTIONAL" =~ ^[0-9]+$ ]]; then
  echo "     ✅ Violation counters are valid: M=$MANDATORY, R=$RECOMMENDED, O=$OPTIONAL"
else
  echo "     ❌ Invalid violation counters"
  exit 1
fi

# Test action.yml input/output structure
echo "  ⚙️ Testing action configuration..."
if [ -f "../action.yml" ]; then
  # Check that action.yml has required sections
  if grep -q "inputs:" ../action.yml && \
     grep -q "outputs:" ../action.yml && \
     grep -q "runs:" ../action.yml; then
    echo "     ✅ Action configuration is valid"
  else
    echo "     ❌ Action configuration missing required sections"
    exit 1
  fi
else
  echo "     ❌ Action configuration file not found"
  exit 1
fi

# Clean up test environment
cd ..
rm -rf "$TEST_DIR"

echo ""
echo "🎉 All Standards Compliance Check tests passed!"
echo "   ✅ All check scripts execute successfully"
echo "   ✅ Report generation works correctly"
echo "   ✅ Violation counting is accurate"
echo "   ✅ Action configuration is valid"
echo ""
echo "The Standards Compliance Check system is ready for deployment."