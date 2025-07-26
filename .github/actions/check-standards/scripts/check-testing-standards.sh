#!/bin/bash

# Standards Compliance Check: Testing Standards
# Validates adherence to TestingStandards.md requirements

set -e

echo "🧪 Checking Testing Standards..."

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
      echo "🚫 **MANDATORY:** $title" >> standards-results/testing-violations.md
      ;;
    "recommended")
      RECOMMENDED_VIOLATIONS=$((RECOMMENDED_VIOLATIONS + 1))
      echo "⚠️ **RECOMMENDED:** $title" >> standards-results/testing-violations.md
      ;;
    "optional")
      OPTIONAL_VIOLATIONS=$((OPTIONAL_VIOLATIONS + 1))
      echo "💡 **OPTIONAL:** $title" >> standards-results/testing-violations.md
      ;;
  esac
  
  echo "   - **Issue:** $description" >> standards-results/testing-violations.md
  echo "   - **Fix:** $fix" >> standards-results/testing-violations.md
  echo "   - **Reference:** [TestingStandards.md](../Docs/Standards/TestingStandards.md)" >> standards-results/testing-violations.md
  echo "" >> standards-results/testing-violations.md
}

# Initialize violations file
echo "## 🧪 Testing Standards Violations" > standards-results/testing-violations.md
echo "" >> standards-results/testing-violations.md

# Check 1: Test file naming conventions (MANDATORY)
echo "  Checking test file naming conventions..."
incorrectly_named_test_files=$(find . -path "./Code/Zarichney.Server.Tests/*" -name "*.cs" | \
  grep -v "Tests\.cs$" | grep -v "TestData" | grep -v "Framework" | grep -v "Builder" | wc -l || echo "0")

if [ "$incorrectly_named_test_files" -gt 0 ]; then
  add_violation "mandatory" \
    "Test files not following naming convention" \
    "Found $incorrectly_named_test_files test files not ending with 'Tests.cs'" \
    "Rename test files to follow [SystemUnderTest]Tests.cs pattern (e.g., OrderServiceTests.cs)"
fi

# Check 2: Test method naming conventions (RECOMMENDED)
echo "  Checking test method naming conventions..."
test_methods_with_poor_names=$(find . -path "./Code/Zarichney.Server.Tests/*" -name "*Tests.cs" -exec grep -l "\[Fact\]\|\[Theory\]" {} \; | \
  xargs grep -A1 "\[Fact\]\|\[Theory\]" | grep "public.*void" | \
  grep -v "_" | wc -l || echo "0")

if [ "$test_methods_with_poor_names" -gt 0 ]; then
  add_violation "recommended" \
    "Test methods not following naming convention" \
    "Found $test_methods_with_poor_names test methods not using [Method]_[Scenario]_[Expected] pattern" \
    "Use descriptive names like CreateOrder_ValidInput_ReturnsCreatedResult"
fi

# Check 3: Test categorization with Traits (MANDATORY)
echo "  Checking test categorization..."
test_methods_without_traits=$(find . -path "./Code/Zarichney.Server.Tests/*" -name "*Tests.cs" -exec grep -l "\[Fact\]\|\[Theory\]" {} \; | \
  xargs grep -A5 "\[Fact\]\|\[Theory\]" | grep -B5 "public.*void" | \
  grep -L "\[Trait(" | wc -l || echo "0")

if [ "$test_methods_without_traits" -gt 0 ]; then
  add_violation "mandatory" \
    "Test methods missing Category traits" \
    "Found test methods without required [Trait(\"Category\", \"Unit\")] or [Trait(\"Category\", \"Integration\")] attributes" \
    "Add [Trait(\"Category\", \"Unit\")] or [Trait(\"Category\", \"Integration\")] to all test methods"
fi

# Check 4: FluentAssertions usage (MANDATORY)
echo "  Checking FluentAssertions usage..."
test_files_with_basic_asserts=$(find . -path "./Code/Zarichney.Server.Tests/*" -name "*Tests.cs" -exec grep -l "Assert\." {} \; | wc -l || echo "0")

if [ "$test_files_with_basic_asserts" -gt 0 ]; then
  add_violation "mandatory" \
    "Using basic Assert instead of FluentAssertions" \
    "Found $test_files_with_basic_asserts test files using Assert.* instead of FluentAssertions" \
    "Replace Assert.Equal() with Should().Be(), Assert.True() with Should().BeTrue(), etc."
fi

# Check 5: Moq usage for unit tests (RECOMMENDED)
echo "  Checking mocking patterns..."
unit_test_files_without_moq=$(find . -path "./Code/Zarichney.Server.Tests/Unit/*" -name "*Tests.cs" | \
  xargs grep -L "Mock<" 2>/dev/null | wc -l || echo "0")

if [ "$unit_test_files_without_moq" -gt 5 ]; then
  add_violation "recommended" \
    "Unit tests may need better isolation" \
    "Found $unit_test_files_without_moq unit test files without Moq usage - may indicate insufficient isolation" \
    "Consider using Mock<T> to isolate dependencies in unit tests"
fi

# Check 6: AAA pattern compliance (RECOMMENDED)
echo "  Checking AAA pattern usage..."
test_methods_without_aaa_comments=$(find . -path "./Code/Zarichney.Server.Tests/*" -name "*Tests.cs" -exec grep -l "\[Fact\]\|\[Theory\]" {} \; | \
  xargs grep -A20 "public.*void.*Test" | grep -L "// Arrange\|// Act\|// Assert" | wc -l || echo "0")

if [ "$test_methods_without_aaa_comments" -gt 10 ]; then
  add_violation "recommended" \
    "Test methods lacking clear AAA structure" \
    "Many test methods don't have clear Arrange-Act-Assert structure comments" \
    "Add // Arrange, // Act, // Assert comments to improve test readability"
fi

# Check 7: Integration test framework usage (MANDATORY for integration tests)
echo "  Checking integration test framework usage..."
integration_tests_without_factory=$(find . -path "./Code/Zarichney.Server.Tests/Integration/*" -name "*Tests.cs" | \
  xargs grep -L "CustomWebApplicationFactory\|IClassFixture" 2>/dev/null | wc -l || echo "0")

if [ "$integration_tests_without_factory" -gt 0 ]; then
  add_violation "mandatory" \
    "Integration tests not using test framework" \
    "Found $integration_tests_without_factory integration test files not using CustomWebApplicationFactory" \
    "Integration tests must use IClassFixture<CustomWebApplicationFactory<Program>>"
fi

# Check 8: Test coverage considerations (OPTIONAL)
echo "  Analyzing test coverage patterns..."
cs_files_in_server=$(find ./Code/Zarichney.Server -name "*.cs" | grep -v "Program.cs" | wc -l || echo "0")
test_files_count=$(find ./Code/Zarichney.Server.Tests -name "*Tests.cs" | wc -l || echo "0")

if [ "$cs_files_in_server" -gt 0 ]; then
  test_coverage_ratio=$((test_files_count * 100 / cs_files_in_server))
  if [ "$test_coverage_ratio" -lt 30 ]; then
    add_violation "optional" \
      "Low test file coverage ratio" \
      "Test file to source file ratio is ${test_coverage_ratio}% - consider adding more test coverage" \
      "Add test files for important classes and services to improve coverage"
  fi
fi

# Check 9: Test data builders usage (RECOMMENDED)
echo "  Checking test data patterns..."
test_files_with_complex_setup=$(find . -path "./Code/Zarichney.Server.Tests/*" -name "*Tests.cs" -exec grep -l "new.*{" {} \; | \
  xargs grep -L "Builder\|AutoFixture" 2>/dev/null | wc -l || echo "0")

if [ "$test_files_with_complex_setup" -gt 10 ]; then
  add_violation "recommended" \
    "Complex test data setup without builders" \
    "Found $test_files_with_complex_setup test files with complex object creation but no builders or AutoFixture" \
    "Consider using Test Data Builders or AutoFixture for cleaner test setup"
fi

# Check 10: Async test patterns (RECOMMENDED)
echo "  Checking async test patterns..."
async_tests_without_proper_await=$(find . -path "./Code/Zarichney.Server.Tests/*" -name "*Tests.cs" -exec grep -l "async.*Task" {} \; | \
  xargs grep "async.*Task" | grep -v "await" | wc -l || echo "0")

if [ "$async_tests_without_proper_await" -gt 0 ]; then
  add_violation "recommended" \
    "Async tests without await patterns" \
    "Found $async_tests_without_proper_await async test methods that may not be properly awaiting operations" \
    "Ensure async test methods properly await all async operations"
fi

# Update violation counters
current_mandatory=$(cat standards-results/mandatory-count)
current_recommended=$(cat standards-results/recommended-count)
current_optional=$(cat standards-results/optional-count)

echo $((current_mandatory + MANDATORY_VIOLATIONS)) > standards-results/mandatory-count
echo $((current_recommended + RECOMMENDED_VIOLATIONS)) > standards-results/recommended-count
echo $((current_optional + OPTIONAL_VIOLATIONS)) > standards-results/optional-count

echo "  ✅ Testing standards check completed"
echo "     Mandatory violations: $MANDATORY_VIOLATIONS"
echo "     Recommended violations: $RECOMMENDED_VIOLATIONS"  
echo "     Optional violations: $OPTIONAL_VIOLATIONS"