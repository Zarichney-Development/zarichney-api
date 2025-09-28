# AI Standards Analysis Component

## Overview

The AI Standards Analysis component provides comprehensive standards compliance analysis with component-specific intelligence, epic-aware prioritization, and architectural validation. Built on the ai-sentinel-base foundation, this component transforms basic compliance checking into intelligent standards guidance for build workflow modernization goals.

## Purpose

- **Component-Specific Analysis**: Targeted standards analysis based on component type and architectural context
- **Epic-Aware Prioritization**: Compliance assessment aligned with build workflow modernization objectives
- **Architectural Validation**: Deep analysis of component architecture and design patterns
- **Improvement Roadmaps**: Actionable guidance for achieving standards compliance excellence

## Key Features

### 1. Component-Specific Intelligence
- **Workflow Components**: GitHub Actions workflow standards, composite action patterns, security controls
- **Backend Components**: .NET 8/C# 12 standards, API design patterns, service architecture
- **Frontend Components**: Angular 19 standards, TypeScript patterns, component design
- **Test Components**: Testing standards, coverage requirements, quality patterns
- **Documentation Components**: README standards, documentation patterns, template compliance

### 2. Epic-Aware Prioritization
- **Build Workflow Integration**: Alignment with workflow modernization goals
- **Component Extraction**: Standards for modular component design and implementation
- **Foundation Patterns**: Adherence to established foundation component standards
- **Modernization Impact**: Assessment of standards compliance impact on epic progression

### 3. Standards Framework Integration
- **Project Standards Coverage**: Integration with `/Docs/Standards/` documentation
- **Template Compliance**: Validation against established templates and patterns
- **Quality Gates**: Alignment with project quality gates and compliance thresholds
- **Best Practices**: Enforcement of industry and project-specific best practices

### 4. Architectural Validation
- **Component Mode**: Analysis of individual component architecture and design
- **Integration Mode**: Validation of component integration patterns and interfaces
- **System Mode**: Assessment of system-wide architectural impact and consistency

## Usage

### Basic Usage

```yaml
- name: Execute Standards Analysis
  uses: ./.github/actions/shared/ai-standards-analysis
  with:
    component_type: 'workflow'
    standards_context: '/Docs/Standards/CodingStandards.md,/Docs/Standards/TaskManagementStandards.md'
    change_scope: ${{ steps.pr-context.outputs.changed_files }}
    github_token: ${{ secrets.GITHUB_TOKEN }}
    openai_api_key: ${{ secrets.OPENAI_API_KEY }}
```

### Advanced Configuration

```yaml
- name: Comprehensive Standards Analysis
  uses: ./.github/actions/shared/ai-standards-analysis
  with:
    component_type: 'backend'
    standards_context: '/Docs/Standards/CodingStandards.md,/Docs/Standards/TestingStandards.md,/Docs/Standards/DocumentationStandards.md'
    change_scope: 'Code/Zarichney.Server/Services'
    epic_context: 'build-workflow-improvements'
    analysis_depth: 'comprehensive'
    architecture_mode: 'integration'
    compliance_threshold: '85'
    priority_focus: 'maintainability'
    debug_mode: 'true'
    github_token: ${{ secrets.GITHUB_TOKEN }}
    openai_api_key: ${{ secrets.OPENAI_API_KEY }}
```

## Inputs

### Required Inputs

| Input | Description | Example |
|-------|-------------|---------|
| `component_type` | Type of component being analyzed | `workflow`, `backend`, `frontend`, `test`, `documentation` |
| `standards_context` | Relevant standards documentation paths | `/Docs/Standards/CodingStandards.md,/Docs/Standards/TestingStandards.md` |
| `change_scope` | Scope of changes for targeted analysis | File paths or change description |
| `github_token` | GitHub token for repository access | `${{ secrets.GITHUB_TOKEN }}` |
| `openai_api_key` | OpenAI API key for AI analysis | `${{ secrets.OPENAI_API_KEY }}` |

### Optional Inputs

| Input | Description | Default | Options |
|-------|-------------|---------|---------|
| `epic_context` | Epic progression context | `build-workflow-improvements` | Any epic identifier |
| `analysis_depth` | Analysis depth level | `detailed` | `surface`, `detailed`, `comprehensive` |
| `architecture_mode` | Architectural validation mode | `component` | `component`, `integration`, `system` |
| `compliance_threshold` | Minimum compliance score | `75` | `0-100` |
| `priority_focus` | Focus area for analysis | `all` | `security`, `maintainability`, `performance`, `all` |
| `skip_duplicate` | Enable duplicate prevention | `true` | `true`, `false` |
| `debug_mode` | Enable debug logging | `false` | `true`, `false` |

## Outputs

### Standards Intelligence Outputs

| Output | Description |
|--------|-------------|
| `standards_analysis` | Structured compliance analysis with component-specific insights |
| `compliance_score` | Standards compliance assessment (0-100) with improvement areas |
| `priority_violations` | JSON array of high-priority violations with remediation steps |
| `improvement_roadmap` | Actionable roadmap for compliance improvement |
| `epic_alignment` | Assessment of alignment with epic modernization goals |
| `architectural_recommendations` | Component-specific architectural guidance |

### Standard AI Analysis Outputs

| Output | Description |
|--------|-------------|
| `analysis_result` | Complete analysis in structured JSON format |
| `analysis_summary` | Human-readable summary for PR comments |
| `recommendations` | Standard recommendations for improvements |
| `analysis_metadata` | Execution metadata with metrics and timing |

### Status Outputs

| Output | Description |
|--------|-------------|
| `skip_reason` | Reason for skipping analysis (if applicable) |
| `error_details` | Detailed error information for failures |

## Component Architecture

### Structure

```
ai-standards-analysis/
├── action.yml                           # Component interface definition
├── README.md                            # This documentation
└── src/
    ├── standards-context-processor.sh   # Standards data validation and structuring
    ├── standards-intelligence-processor.sh # AI insight extraction and scoring
    └── epic-alignment-analyzer.sh       # Epic alignment and architectural analysis
```

### Processing Flow

1. **Standards Context Preparation**: Validate inputs and prepare component-specific context
2. **AI Standards Analysis**: Execute AI analysis using ai-sentinel-base foundation
3. **Standards Intelligence Processing**: Extract compliance insights and calculate scores
4. **Epic Alignment Analysis**: Generate epic-aware recommendations and architectural guidance

## Security Framework

### Inherited Security Controls (via ai-sentinel-base)

- **Input Validation**: Standards data sanitization and format validation
- **Context Security**: Secure template processing with integrity checks
- **API Protection**: Secure AI service communication with audit logging
- **Error Handling**: Graceful failure recovery without information disclosure

### Component-Specific Security

- **Path Validation**: Standards documentation path validation and security
- **Component Validation**: Component type whitelist enforcement
- **Scope Validation**: Change scope validation and sanitization
- **Output Security**: Standards intelligence output validation and format checking

## Build Workflow Integration

### Build Workflow Enhancement

- **Component Extraction**: Standards for modular component design and implementation
- **Foundation Integration**: Compliance with ai-sentinel-base and foundation components
- **Workflow Modernization**: Standards alignment with build workflow modernization goals
- **Quality Excellence**: Comprehensive standards framework for workflow quality

### Coverage Workflow Integration

```yaml
- name: Execute Standards Analysis for Coverage Workflow
  uses: ./.github/actions/shared/ai-standards-analysis
  with:
    component_type: 'workflow'
    standards_context: '/Docs/Standards/TaskManagementStandards.md,/Docs/Standards/TestingStandards.md'
    change_scope: '.github/workflows/testing-coverage-build-review.yml'
    epic_context: 'build-workflow-improvements'
    analysis_depth: 'comprehensive'
    github_token: ${{ secrets.GITHUB_TOKEN }}
    openai_api_key: ${{ secrets.OPENAI_API_KEY }}
```

## Team Integration

### WorkflowEngineer Integration

- **Workflow Standards**: GitHub Actions workflow standards and best practices
- **Composite Actions**: Standards for reusable action design and implementation
- **Security Controls**: Workflow security and access control standards
- **Performance**: Workflow performance and efficiency standards

### Other Agent Coordination

- **CodeChanger**: Backend and frontend component standards validation
- **TestEngineer**: Testing standards compliance and coverage requirements
- **SecurityAuditor**: Security standards enforcement and validation
- **DocumentationMaintainer**: Documentation standards and template compliance

## Quality Gates

### Compliance Scoring

- **Excellent (90-100)**: Outstanding compliance with all standards
- **Good (75-89)**: Strong compliance with minor improvement areas
- **Acceptable (60-74)**: Adequate compliance with focused improvement needed
- **Needs Improvement (40-59)**: Significant compliance gaps requiring attention
- **Critical (0-39)**: Major compliance violations requiring immediate action

### Priority Violation Classification

- **🚨 Critical**: Blocking violations requiring immediate attention
- **⚠️ High**: Important violations that should be addressed
- **📋 Medium**: Recommended improvements for better compliance
- **💡 Low**: Optional enhancements for excellence

## Performance Characteristics

### Operational Metrics

- **Analysis Time**: 30-180 seconds based on analysis depth and component complexity
- **Memory Usage**: <200MB during standards intelligence processing
- **Template Processing**: <5 seconds for standards-enhanced templates
- **API Efficiency**: Optimized prompts with component-specific context injection

### Optimization Features

- **Context Caching**: Efficient repeated analysis support for similar components
- **Intelligent Retry Logic**: Robust handling of transient failures with exponential backoff
- **Parallel Processing**: Support for concurrent team coordination scenarios
- **Resource Management**: Automatic cleanup and memory optimization

## Troubleshooting

### Common Issues

1. **Standards Context Processing Failed**
   - Verify standards_context paths exist and are accessible
   - Check component_type is valid (workflow, backend, frontend, test, documentation)
   - Validate change_scope format and content

2. **Compliance Score Calculation Failed**
   - Ensure AI analysis result contains valid standards data
   - Check compliance_threshold is within valid range (0-100)
   - Verify component_type matches expected standards framework

3. **Epic Alignment Analysis Failed**
   - Validate epic_context format and content
   - Check architecture_mode is valid (component, integration, system)
   - Ensure analysis_depth is appropriate for epic analysis

### Debug Mode

Enable debug mode for detailed troubleshooting:

```yaml
debug_mode: 'true'
```

This provides:
- Detailed processing logs in `/tmp/ai-standards-debug.log`
- Input validation debugging information
- AI analysis request/response debugging
- Standards intelligence processing traces

## Examples

### Workflow Component Analysis

```yaml
- name: Analyze Workflow Standards
  uses: ./.github/actions/shared/ai-standards-analysis
  with:
    component_type: 'workflow'
    standards_context: '/Docs/Standards/TaskManagementStandards.md'
    change_scope: '.github/workflows/build.yml'
    epic_context: 'build-workflow-improvements'
    analysis_depth: 'detailed'
```

### Backend Service Analysis

```yaml
- name: Analyze Backend Standards
  uses: ./.github/actions/shared/ai-standards-analysis
  with:
    component_type: 'backend'
    standards_context: '/Docs/Standards/CodingStandards.md,/Docs/Standards/TestingStandards.md'
    change_scope: 'Code/Zarichney.Server/Services/UserService.cs'
    architecture_mode: 'integration'
    compliance_threshold: '85'
    priority_focus: 'maintainability'
```

### Test Component Analysis

```yaml
- name: Analyze Test Standards
  uses: ./.github/actions/shared/ai-standards-analysis
  with:
    component_type: 'test'
    standards_context: '/Docs/Standards/TestingStandards.md'
    change_scope: 'Code/Zarichney.Server.Tests'
    epic_context: 'epic-testing-coverage-to-90'
    analysis_depth: 'comprehensive'
```

---

This component provides comprehensive standards compliance analysis that supports build workflow modernization goals while maintaining the highest quality and architectural standards through AI-powered insights.
