# Test Artifacts Guide

## Overview

Our CI/CD pipeline generates several artifacts that help you understand test results and code coverage. This guide explains what each artifact contains and how to use them effectively.

## Generated Artifacts

### 1. Test Results (`test-results`)
- **Format**: TRX (Test Results XML) files
- **Contents**: Detailed test execution information including:
  - Pass/fail status for each test
  - Execution time
  - Error messages and stack traces for failed tests
  - Test categories (Unit/Integration)

### 2. Coverage Results (`coverage-results`)
- **Format**: Cobertura XML
- **Contents**: Raw code coverage data showing:
  - Line coverage percentages
  - Branch coverage
  - Which lines of code were executed during tests

### 3. Coverage Report HTML (`coverage-report-html`)
- **Format**: Interactive HTML report
- **Contents**: Visual representation of code coverage with:
  - Browseable source code with coverage highlighting
  - Summary statistics by namespace/class
  - Coverage trends over time

## How to Access Artifacts

### From GitHub Actions UI

1. Navigate to the Actions tab in your repository
2. Click on a completed workflow run
3. Scroll to the bottom to find "Artifacts"
4. Download any artifact by clicking on its name

### Using GitHub CLI

```bash
# List workflow runs
gh run list

# Download artifacts from a specific run
gh run download <run-id>

# Download specific artifact
gh run download <run-id> -n coverage-report-html
```

## Understanding the Reports

### Test Results
The TRX files can be opened with:
- Visual Studio Test Explorer
- VS Code with the ".NET Core Test Explorer" extension
- Online TRX viewers

### Coverage Reports
The HTML coverage report provides:
- **Summary Page**: Overall coverage percentages
- **Detailed View**: Click through namespaces → classes → methods
- **Source View**: See exactly which lines are covered (green) or not (red)

## Setting Coverage Goals

Consider adding these to your project:

1. **Minimum Coverage Requirements**
   ```xml
   <!-- In your test project .csproj -->
   <PropertyGroup>
     <CollectCoverage>true</CollectCoverage>
     <CoverletOutputFormat>cobertura</CoverletOutputFormat>
     <Threshold>80</Threshold>
     <ThresholdType>Line,Branch,Method</ThresholdType>
   </PropertyGroup>
   ```

2. **Coverage Badges**
   After the next build, you'll find coverage badges in the artifacts that you can add to your README:
   ```markdown
   ![Coverage](./CoverageReport/badge_linecoverage.svg)
   ```

## Best Practices

1. **Review Coverage Reports in PRs**: Look for:
   - New code without tests
   - Critical paths with low coverage
   - Opportunity to add edge case tests

2. **Monitor Trends**: Coverage should generally increase or stay stable over time

3. **Focus on Meaningful Coverage**: 
   - Prioritize business logic over boilerplate
   - Test edge cases and error paths
   - Don't just aim for 100% - aim for quality tests

## Local Coverage Generation

To generate coverage reports locally:

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Install ReportGenerator tool
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"**/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"CoverageReport" \
  -reporttypes:"Html"

# Open the report
open CoverageReport/index.html  # macOS
start CoverageReport/index.html # Windows
xdg-open CoverageReport/index.html # Linux
```

## Integrating with IDEs

### Visual Studio
- Built-in coverage support in Enterprise edition
- Use "Analyze Code Coverage" from Test menu

### VS Code
- Install "Coverage Gutters" extension
- It will automatically use the coverage.cobertura.xml files

### JetBrains Rider
- Built-in coverage support
- Run tests with coverage from the Unit Tests window