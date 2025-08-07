# Validate Test Suite Baselines Action

This GitHub Action validates test results against configured baseline standards with environment-aware thresholds. It integrates with the test suite baseline framework established in Phase 1 of issue #86.

## Features

- **Environment-Aware Validation**: Automatically detects test environment and applies appropriate thresholds
- **Skip Categorization**: Analyzes skipped tests by category (external services, infrastructure, hardcoded)
- **Coverage Baseline Validation**: Ensures coverage meets the 14.22% baseline with regression tolerance
- **Structured Output**: Generates JSON artifacts for downstream analysis and AI integration
- **Flexible Failure Modes**: Can run in warning mode or strict validation mode

## Environment Classifications

| Environment | Skip Threshold | Description |
|-------------|----------------|-------------|
| `unconfigured` | 26.7% | Local dev / CI without external services |
| `configured` | 1.2% | Full external service configuration |
| `production` | 0.0% | Production deployment - no failures acceptable |

## Usage

### Basic Usage

```yaml
- name: Validate test suite baselines
  uses: ./.github/actions/shared/validate-test-suite
  with:
    test-results-path: './TestResults'
    fail-on-violations: 'false'  # Warning mode
```

### Strict Validation Mode

```yaml
- name: Validate test suite baselines (strict)
  uses: ./.github/actions/shared/validate-test-suite
  with:
    test-results-path: './TestResults'
    fail-on-violations: 'true'   # Fail on violations
    environment-override: 'configured'  # Force environment
```

### With Artifact Upload

```yaml
- name: Validate test suite baselines
  uses: ./.github/actions/shared/validate-test-suite
  with:
    upload-artifacts: 'true'     # Upload validation results
    
- name: Use validation results
  run: |
    echo "Validation passed: ${{ steps.validation.outputs.validation-passed }}"
    echo "Skip percentage: ${{ steps.validation.outputs.skip-percentage }}%"
    echo "Environment: ${{ steps.validation.outputs.environment-classification }}"
```

## Inputs

| Input | Description | Required | Default |
|-------|-------------|----------|---------|
| `test-results-path` | Path to test results directory | No | `./TestResults` |
| `solution-file` | Path to solution file | No | `./zarichney-api.sln` |
| `fail-on-violations` | Fail action when violations detected | No | `false` |
| `upload-artifacts` | Upload validation artifacts | No | `true` |
| `environment-override` | Force environment classification | No | `` |

## Outputs

| Output | Description |
|--------|-------------|
| `validation-passed` | Whether baseline validation passed (`true`/`false`) |
| `skip-percentage` | Calculated skip percentage (e.g., `26.7`) |
| `environment-classification` | Detected environment (`unconfigured`/`configured`/`production`) |
| `violations-count` | Number of baseline violations detected |
| `baseline-validation-file` | Path to generated validation results JSON |

## Generated Artifacts

### Baseline Validation JSON

```json
{
  "timestamp": "2025-08-07T10:30:00Z",
  "environment": {
    "classification": "unconfigured",
    "description": "CI environment without external services",
    "expectedSkipPercentage": 26.7
  },
  "metrics": {
    "totalTests": 86,
    "skippedTests": 23,
    "failedTests": 0,
    "skipPercentage": 26.7,
    "lineCoverage": 15.1
  },
  "baselines": {
    "skipThreshold": 26.7,
    "coverageBaseline": 14.22,
    "coverageRegressionTolerance": 1.0
  },
  "validation": {
    "passesThresholds": true,
    "violationsCount": 0,
    "violations": [],
    "recommendations": [
      "Configure external services to reduce skip rate to target 1.2%"
    ]
  },
  "skipAnalysis": {
    "externalServices": {
      "categoryType": "expected",
      "skippedCount": 16,
      "isExpected": true,
      "reason": "External service dependencies not configured"
    },
    "infrastructure": {
      "categoryType": "expected", 
      "skippedCount": 6,
      "isExpected": true,
      "reason": "Infrastructure dependencies unavailable"
    },
    "hardcodedSkips": {
      "categoryType": "problematic",
      "skippedCount": 1,
      "isExpected": false,
      "reason": "Hardcoded Skip attributes requiring review"
    }
  }
}
```

## Integration Examples

### Backend Build Integration

```yaml
- name: Build and test backend
  run: ./Scripts/run-test-suite.sh report json

- name: Validate baselines
  uses: ./.github/actions/shared/validate-test-suite
  with:
    test-results-path: './TestResults'

- name: Upload test results
  uses: actions/upload-artifact@v4
  with:
    name: test-results-with-baselines
    path: |
      TestResults/
      CoverageReport/
```

### AI Analysis Integration

```yaml
- name: Validate baselines
  id: baseline-validation
  uses: ./.github/actions/shared/validate-test-suite

- name: AI Analysis with baseline context
  if: always()
  run: |
    # Pass baseline validation results to AI analysis
    export BASELINE_VALIDATION_FILE="${{ steps.baseline-validation.outputs.baseline-validation-file }}"
    export ENVIRONMENT_CLASSIFICATION="${{ steps.baseline-validation.outputs.environment-classification }}"
    # Run AI analysis with baseline awareness
```

## Environment Detection Logic

1. **Environment Override**: Use `environment-override` input if provided
2. **Production Check**: `ASPNETCORE_ENVIRONMENT=Production` → production classification
3. **CI Detection**: `CI=true` → unconfigured classification (default for CI/CD)
4. **Default**: unconfigured classification

## Validation Logic

### Skip Percentage Validation
- Compare actual skip % vs environment-specific threshold
- Account for test categorization (expected vs problematic skips)
- Generate recommendations for improvement

### Coverage Validation  
- Check against 14.22% baseline with 1% regression tolerance
- Flag coverage degradation
- Support for progressive coverage targets

### Test Failure Validation
- Always critical - any failing tests flag validation failure
- Provide detailed failure information when available

## Error Handling

- **Missing Test Results**: Action fails with clear error message
- **Invalid JSON**: Graceful degradation with error reporting  
- **Missing Dependencies**: Checks for required tools (jq, bc)
- **File Access**: Robust file existence and permission checking

## Dependencies

- **jq**: Required for JSON processing
- **bc**: Required for floating-point calculations
- **bash**: Shell environment with standard utilities

## Related Documentation

- [Issue #86: Test Suite Baselines](https://github.com/Zarichney-Development/zarichney-api/issues/86)
- [TestingStandards.md](../../../Docs/Standards/TestingStandards.md)
- [TestSuiteStandardsHelper.cs](../../../Code/Zarichney.Server.Tests/Framework/Helpers/TestSuiteStandardsHelper.cs)
- [Test Suite Script](../../../Scripts/run-test-suite.sh)

## Changelog

### v1.0.0 (2025-08-07)
- Initial implementation for Phase 2 of issue #86
- Environment-aware validation with configurable thresholds  
- Skip categorization and coverage baseline validation
- JSON artifact generation for AI analysis integration
- Flexible failure modes (warning vs strict validation)