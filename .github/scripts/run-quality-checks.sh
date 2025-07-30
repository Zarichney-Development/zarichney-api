#!/bin/bash

# Quality Checks Script
# Consolidated standards compliance and tech debt analysis
# Version: 1.0
# Last Updated: 2025-07-27

# Source common functions
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/common-functions.sh"

# Script configuration
readonly QUALITY_DIR="quality-analysis"
readonly STANDARDS_PROMPT="$SCRIPT_DIR/../Prompts/standards-compliance.md"
readonly TECH_DEBT_PROMPT="$SCRIPT_DIR/../Prompts/tech-debt-analysis.md"

# Help function
show_help() {
    cat << EOF
Quality Checks Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    --standards-only    Run only standards compliance checks
    --tech-debt-only    Run only tech debt analysis
    --skip-analysis     Skip AI analysis (checks only)
    --severity LEVEL    Minimum severity level (critical, high, medium, low)
    --pr-number NUM     Pull request number for analysis
    --base-branch BRANCH Base branch for comparison (default: develop)
    --head-sha SHA      Head commit SHA for analysis
    --claude-token TOKEN Claude OAuth token for AI analysis
    --help              Show this help message

ENVIRONMENT VARIABLES:
    GITHUB_TOKEN            GitHub API token
    CLAUDE_CODE_OAUTH_TOKEN Claude OAuth token for Claude Max plan
    BASE_BRANCH             Base branch for comparison (default: develop)

EXAMPLES:
    $0                      # Run all quality checks with AI analysis
    $0 --standards-only     # Run only standards compliance
    $0 --tech-debt-only     # Run only tech debt analysis
    $0 --severity high      # Report only high and critical issues
EOF
}

# Parse command line arguments
STANDARDS_ONLY=false
TECH_DEBT_ONLY=false
SKIP_ANALYSIS=false
SEVERITY_LEVEL="low"
PR_NUMBER=""
BASE_BRANCH="${BASE_BRANCH:-develop}"
HEAD_SHA="${HEAD_SHA:-$(git rev-parse HEAD)}"
CLAUDE_TOKEN="${CLAUDE_CODE_OAUTH_TOKEN:-}"

while [[ $# -gt 0 ]]; do
    case $1 in
        --standards-only)
            STANDARDS_ONLY=true
            shift
            ;;
        --tech-debt-only)
            TECH_DEBT_ONLY=true
            shift
            ;;
        --skip-analysis)
            SKIP_ANALYSIS=true
            shift
            ;;
        --severity)
            SEVERITY_LEVEL="$2"
            shift 2
            ;;
        --pr-number)
            PR_NUMBER="$2"
            shift 2
            ;;
        --base-branch)
            BASE_BRANCH="$2"
            shift 2
            ;;
        --head-sha)
            HEAD_SHA="$2"
            shift 2
            ;;
        --claude-token)
            CLAUDE_TOKEN="$2"
            shift 2
            ;;
        --help)
            show_help
            exit 0
            ;;
        *)
            log_error "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# Main execution
main() {
    local start_time
    start_time=$(start_timer)
    
    # Initialize environment
    init_pipeline
    
    log_section "Quality Checks Execution"
    
    # Validate prerequisites
    validate_quality_prereqs
    
    # Prepare quality analysis directory
    prepare_quality_directory
    
    # Run quality checks based on options
    if [[ "$STANDARDS_ONLY" == "true" ]]; then
        run_standards_compliance
    elif [[ "$TECH_DEBT_ONLY" == "true" ]]; then
        run_tech_debt_analysis
    else
        # Run both checks
        run_standards_compliance
        run_tech_debt_analysis
    fi
    
    # Prepare comprehensive quality data
    prepare_quality_data
    
    # Run AI analysis unless skipped
    if [[ "$SKIP_ANALYSIS" != "true" ]]; then
        run_ai_quality_analysis
    fi
    
    # Generate quality report
    generate_quality_report
    
    # Create quality artifacts
    create_quality_artifacts
    
    end_timer "$start_time" "quality-checks"
    log_success "Quality checks completed successfully"
}

validate_quality_prereqs() {
    log_section "Validating Quality Prerequisites"
    
    # Check required tools
    check_required_tools git jq dotnet
    
    # Check required environment variables
    local required_vars=("GITHUB_TOKEN")
    
    if [[ "$SKIP_ANALYSIS" != "true" && -z "$CLAUDE_TOKEN" ]]; then
        log_warning "Claude token not provided, AI analysis will be skipped"
        SKIP_ANALYSIS=true
    fi
    
    check_required_env "${required_vars[@]}"
    
    log_success "Prerequisites validation passed"
}

prepare_quality_directory() {
    log_section "Preparing Quality Analysis Directory"
    
    # Create quality analysis directory
    rm -rf "$QUALITY_DIR"
    mkdir -p "$QUALITY_DIR"
    
    # Create artifact directory
    create_artifact_dir "artifacts/quality"
    
    log_success "Quality directory prepared"
}

run_standards_compliance() {
    log_section "Running Standards Compliance Checks"
    
    local violations=0
    local mandatory_violations=0
    local recommended_violations=0
    local optional_violations=0
    
    # Code formatting checks
    log_info "Checking code formatting..."
    if ! dotnet format --verify-no-changes --verbosity diagnostic > "$QUALITY_DIR/format-check.log" 2>&1; then
        local format_issues=0
        if [[ -f "$QUALITY_DIR/format-check.log" ]]; then
            # Use temp file approach to avoid command substitution issues
            grep -c "Formatted code file" "$QUALITY_DIR/format-check.log" 2>/dev/null > "$QUALITY_DIR/format-count.tmp" || echo "0" > "$QUALITY_DIR/format-count.tmp"
            format_issues=$(cat "$QUALITY_DIR/format-count.tmp")
            # Ensure format_issues is a valid number
            if ! [[ "$format_issues" =~ ^[0-9]+$ ]]; then
                format_issues=0
            fi
        fi
        mandatory_violations=$((mandatory_violations + format_issues))
        violations=$((violations + format_issues))
        log_warning "Code formatting issues found: $format_issues"
    else
        log_success "Code formatting check passed"
    fi
    
    # Check .editorconfig compliance
    log_info "Checking .editorconfig compliance..."
    if [[ ! -f ".editorconfig" ]]; then
        recommended_violations=$((recommended_violations + 1))
        violations=$((violations + 1))
        log_warning ".editorconfig file not found"
    fi
    
    # Check Git standards
    log_info "Checking Git standards..."
    local git_violations=0
    
    # Check commit message format (last 5 commits)
    git log --oneline -5 --format="%s" > "$QUALITY_DIR/recent-commits.txt"
    while read -r commit_msg; do
        if [[ ! "$commit_msg" =~ ^(feat|fix|docs|style|refactor|test|chore|perf|ci|build|revert)(\(.+\))?: ]]; then
            git_violations=$((git_violations + 1))
        fi
    done < "$QUALITY_DIR/recent-commits.txt"
    
    if [[ $git_violations -gt 0 ]]; then
        recommended_violations=$((recommended_violations + git_violations))
        violations=$((violations + git_violations))
        log_warning "Git commit message format violations: $git_violations"
    fi
    
    # Check testing standards
    log_info "Checking testing standards..."
    local test_violations=0
    
    # Check for test files without proper naming
    find . -name "*.Tests.cs" -o -name "*Tests.cs" -o -name "*Test.cs" | \
        grep -v "/bin/" | grep -v "/obj/" > "$QUALITY_DIR/test-files.txt"
    
    local test_files_count
    test_files_count=$(wc -l < "$QUALITY_DIR/test-files.txt")
    
    if [[ $test_files_count -eq 0 ]]; then
        mandatory_violations=$((mandatory_violations + 1))
        violations=$((violations + 1))
        log_warning "No test files found"
    fi
    
    # Check for missing XML documentation
    log_info "Checking XML documentation..."
    local missing_docs=0
    
    # Count public methods without XML docs in C# files
    find Code/ -name "*.cs" -not -path "*/bin/*" -not -path "*/obj/*" | \
        while read -r file; do
            if grep -q "public.*(" "$file" && ! grep -q "///" "$file"; then
                missing_docs=$((missing_docs + 1))
            fi
        done > "$QUALITY_DIR/doc-check.tmp"
    
    if [[ -f "$QUALITY_DIR/doc-check.tmp" ]]; then
        missing_docs=$(cat "$QUALITY_DIR/doc-check.tmp" | wc -l)
        # Ensure missing_docs is a valid number
        if ! [[ "$missing_docs" =~ ^[0-9]+$ ]]; then
            missing_docs=0
        fi
        if [[ $missing_docs -gt 0 ]]; then
            recommended_violations=$((recommended_violations + missing_docs))
            violations=$((violations + missing_docs))
            log_warning "Files with missing XML documentation: $missing_docs"
        fi
    fi
    
    # Get code formatting count safely
    local code_formatting_count=0
    if [[ -f "$QUALITY_DIR/format-check.log" ]]; then
        # Use temp file approach to avoid command substitution issues
        grep -c "Formatted code file" "$QUALITY_DIR/format-check.log" 2>/dev/null > "$QUALITY_DIR/format-count-json.tmp" || echo "0" > "$QUALITY_DIR/format-count-json.tmp"
        code_formatting_count=$(cat "$QUALITY_DIR/format-count-json.tmp")
        # Ensure it's a valid number
        if ! [[ "$code_formatting_count" =~ ^[0-9]+$ ]]; then
            code_formatting_count=0
        fi
    fi
    
    # Create standards compliance summary
    cat > "$QUALITY_DIR/standards-compliance-summary.json" << EOF
{
    "timestamp": "$(date -Iseconds)",
    "total_violations": $violations,
    "mandatory_violations": $mandatory_violations,
    "recommended_violations": $recommended_violations,
    "optional_violations": $optional_violations,
    "categories": {
        "code_formatting": $code_formatting_count,
        "git_standards": $git_violations,
        "testing_standards": $test_violations,
        "documentation": $missing_docs
    },
    "compliance_score": $(( 100 - (mandatory_violations * 10) - (recommended_violations * 5) - (optional_violations * 2) )),
    "scan_completed": true
}
EOF
    
    log_info "Standards compliance violations: $violations (mandatory: $mandatory_violations, recommended: $recommended_violations)"
    log_success "Standards compliance check completed"
}

run_tech_debt_analysis() {
    log_section "Running Tech Debt Analysis"
    
    local critical_debt=0
    local high_debt=0
    local medium_debt=0
    local low_debt=0
    local total_debt=0
    
    # Code complexity analysis
    log_info "Analyzing code complexity..."
    
    # Find long methods (>50 lines)
    local long_methods=0
    find Code/ -name "*.cs" -not -path "*/bin/*" -not -path "*/obj/*" | \
        while read -r file; do
            awk '/^[[:space:]]*public|^[[:space:]]*private|^[[:space:]]*protected|^[[:space:]]*internal/ && /\(.*\).*{/ {
                start=NR; brace_count=1; 
                while ((getline > 0) && brace_count > 0) {
                    if (/\{/) brace_count++; 
                    if (/\}/) brace_count--; 
                } 
                if (NR-start > 50) print FILENAME":"start":Method too long ("(NR-start)" lines)"
            }' "$file" 2>/dev/null
        done > "$QUALITY_DIR/long-methods.txt"
    
    long_methods=$(wc -l < "$QUALITY_DIR/long-methods.txt")
    medium_debt=$((medium_debt + long_methods))
    
    # Find large classes (>500 lines)
    local large_classes=0
    find Code/ -name "*.cs" -not -path "*/bin/*" -not -path "*/obj/*" | \
        while read -r file; do
            local line_count
            line_count=$(wc -l < "$file")
            if [[ $line_count -gt 500 ]]; then
                echo "$file:$line_count lines" >> "$QUALITY_DIR/large-classes.txt"
            fi
        done
    
    if [[ -f "$QUALITY_DIR/large-classes.txt" ]]; then
        large_classes=$(wc -l < "$QUALITY_DIR/large-classes.txt")
        medium_debt=$((medium_debt + large_classes))
    fi
    
    # Find TODO/FIXME/HACK comments
    local todo_comments=0
    grep -r -i --include="*.cs" --include="*.js" --include="*.ts" \
        "TODO\|FIXME\|HACK\|XXX" Code/ 2>/dev/null | \
        grep -v "/bin/" | grep -v "/obj/" | \
        wc -l > "$QUALITY_DIR/todo-count.tmp" 2>/dev/null || echo "0" > "$QUALITY_DIR/todo-count.tmp"
    
    todo_comments=$(cat "$QUALITY_DIR/todo-count.tmp")
    low_debt=$((low_debt + todo_comments))
    
    # Find code duplication (simple heuristic)
    log_info "Checking for potential code duplication..."
    local duplicate_lines=0
    
    # Look for repeated string literals (potential constants)
    grep -r -h --include="*.cs" '"[^"]{10,}"' Code/ 2>/dev/null | \
        sort | uniq -c | sort -nr | head -20 | \
        awk '$1 > 3 {count++} END {print count+0}' > "$QUALITY_DIR/duplicate-strings.tmp" 2>/dev/null || echo "0" > "$QUALITY_DIR/duplicate-strings.tmp"
    
    duplicate_lines=$(cat "$QUALITY_DIR/duplicate-strings.tmp")
    low_debt=$((low_debt + duplicate_lines))
    
    # Performance debt analysis
    log_info "Analyzing performance debt..."
    local perf_debt=0
    
    # Find potential performance issues
    grep -r --include="*.cs" "\.Result\|\.Wait(" Code/ 2>/dev/null | \
        grep -v "/bin/" | grep -v "/obj/" | \
        wc -l > "$QUALITY_DIR/blocking-async.tmp" 2>/dev/null || echo "0" > "$QUALITY_DIR/blocking-async.tmp"
    
    local blocking_async
    blocking_async=$(cat "$QUALITY_DIR/blocking-async.tmp")
    if [[ $blocking_async -gt 0 ]]; then
        high_debt=$((high_debt + blocking_async))
        perf_debt=$((perf_debt + blocking_async))
    fi
    
    # Calculate total debt
    total_debt=$((critical_debt + high_debt + medium_debt + low_debt))
    
    # Calculate debt score (0-100, higher is better)
    local debt_score=100
    debt_score=$((debt_score - (critical_debt * 25) - (high_debt * 15) - (medium_debt * 10) - (low_debt * 5)))
    if [[ $debt_score -lt 0 ]]; then
        debt_score=0
    fi
    
    # Create tech debt summary
    cat > "$QUALITY_DIR/tech-debt-summary.json" << EOF
{
    "timestamp": "$(date -Iseconds)",
    "total_debt_items": $total_debt,
    "critical_debt": $critical_debt,
    "high_debt": $high_debt,
    "medium_debt": $medium_debt,
    "low_debt": $low_debt,
    "debt_score": $debt_score,
    "categories": {
        "code_complexity": {
            "long_methods": $long_methods,
            "large_classes": $large_classes
        },
        "maintenance_debt": {
            "todo_comments": $todo_comments,
            "code_duplication": $duplicate_lines
        },
        "performance_debt": {
            "blocking_async_calls": $blocking_async,
            "total_performance_issues": $perf_debt
        }
    },
    "scan_completed": true
}
EOF
    
    log_info "Tech debt analysis: $total_debt items (critical: $critical_debt, high: $high_debt, medium: $medium_debt, low: $low_debt)"
    log_info "Tech debt score: $debt_score/100"
    log_success "Tech debt analysis completed"
}

prepare_quality_data() {
    log_section "Preparing Comprehensive Quality Data"
    
    # Gather change context
    if [[ -n "$PR_NUMBER" && -n "$GITHUB_TOKEN" ]]; then
        log_info "Gathering PR context for quality analysis..."
        gh pr view "$PR_NUMBER" --json title,body,commits,additions,deletions > "$QUALITY_DIR/pr-info.json" 2>/dev/null || true
        
        # Get changed files
        git fetch origin "$BASE_BRANCH:$BASE_BRANCH" 2>/dev/null || true
        git diff --name-only "$BASE_BRANCH..HEAD" > "$QUALITY_DIR/changed-files.txt" 2>/dev/null || true
        
        # Get detailed diff for analysis
        git diff "$BASE_BRANCH..HEAD" > "$QUALITY_DIR/changes.diff" 2>/dev/null || true
    fi
    
    # Process quality check results
    local standards_violations=0
    local compliance_score=100
    local tech_debt_items=0
    local debt_score=100
    
    # Process standards compliance results
    if [[ -f "$QUALITY_DIR/standards-compliance-summary.json" ]]; then
        standards_violations=$(jq -r '.total_violations // 0' "$QUALITY_DIR/standards-compliance-summary.json")
        compliance_score=$(jq -r '.compliance_score // 100' "$QUALITY_DIR/standards-compliance-summary.json")
    fi
    
    # Process tech debt results
    if [[ -f "$QUALITY_DIR/tech-debt-summary.json" ]]; then
        tech_debt_items=$(jq -r '.total_debt_items // 0' "$QUALITY_DIR/tech-debt-summary.json")
        debt_score=$(jq -r '.debt_score // 100' "$QUALITY_DIR/tech-debt-summary.json")
    fi
    
    # Calculate overall quality score
    local overall_score=$(( (compliance_score + debt_score) / 2 ))
    
    # Determine quality level
    local quality_level="EXCELLENT"
    if [[ $overall_score -lt 95 ]]; then
        quality_level="GOOD"
    fi
    if [[ $overall_score -lt 85 ]]; then
        quality_level="FAIR"
    fi
    if [[ $overall_score -lt 70 ]]; then
        quality_level="POOR"
    fi
    if [[ $overall_score -lt 50 ]]; then
        quality_level="CRITICAL"
    fi
    
    # Calculate PR context values safely
    local changed_files_count=0
    local commits_count=0
    if [[ -f "$QUALITY_DIR/changed-files.txt" ]]; then
        changed_files_count=$(wc -l < "$QUALITY_DIR/changed-files.txt" 2>/dev/null || echo "0")
        # Ensure it's a valid number
        if ! [[ "$changed_files_count" =~ ^[0-9]+$ ]]; then
            changed_files_count=0
        fi
    fi
    if [[ -f "$QUALITY_DIR/pr-info.json" ]]; then
        commits_count=$(jq -r '.commits | length' "$QUALITY_DIR/pr-info.json" 2>/dev/null || echo "0")
        # Ensure it's a valid number
        if ! [[ "$commits_count" =~ ^[0-9]+$ ]]; then
            commits_count=0
        fi
    fi
    
    # Create comprehensive quality analysis data
    cat > "$QUALITY_DIR/quality-analysis-data.json" << EOF
{
    "project": "zarichney-api",
    "analysis_type": "comprehensive_quality_analysis",
    "build_context": {
        "pr_number": "${PR_NUMBER:-unknown}",
        "base_branch": "$BASE_BRANCH",
        "head_sha": "$HEAD_SHA",
        "timestamp": "$(date -Iseconds)"
    },
    "standards_compliance": {
        "total_violations": $standards_violations,
        "compliance_score": $compliance_score,
        "scan_completed": true
    },
    "tech_debt_analysis": {
        "total_debt_items": $tech_debt_items,
        "debt_score": $debt_score,
        "scan_completed": true
    },
    "quality_assessment": {
        "overall_score": $overall_score,
        "quality_level": "$quality_level",
        "severity_threshold": "$SEVERITY_LEVEL"
    },
    "pr_context": {
        "changed_files_count": ${changed_files_count:-0},
        "commits_count": ${commits_count:-0}
    }
}
EOF
    
    log_success "Quality data preparation completed"
    log_info "  - Standards Violations: $standards_violations"
    log_info "  - Compliance Score: $compliance_score/100"
    log_info "  - Tech Debt Items: $tech_debt_items"
    log_info "  - Debt Score: $debt_score/100"
    log_info "  - Overall Quality: $quality_level ($overall_score/100)"
}

run_ai_quality_analysis() {
    log_section "Running AI Quality Analysis"
    
    if [[ -z "$CLAUDE_TOKEN" ]]; then
        log_warning "Claude token not provided, skipping AI analysis"
        return 0
    fi
    
    log_info "Real Claude AI analysis will be handled by workflow Claude action"
    log_info "This script prepares data for Claude action to analyze using prompts:"
    log_info "  - Standards compliance: $SCRIPT_DIR/../Prompts/standards-compliance.md"
    log_info "  - Tech debt analysis: $SCRIPT_DIR/../Prompts/tech-debt-analysis.md"
    
    # Note: The actual Claude AI analysis is performed by the GitHub workflow
    # using the Claude action with the prepared quality data and appropriate prompts.
    # This function just ensures the data is ready for analysis.
    
    log_success "Quality data prepared for Claude AI analysis"
}

generate_quality_report() {
    log_section "Generating Quality Report"
    
    local quality_data="$QUALITY_DIR/quality-analysis-data.json"
    
    if [[ ! -f "$quality_data" ]]; then
        log_error "Quality analysis data not found"
        return 1
    fi
    
    # Extract metrics from quality data
    local standards_violations compliance_score tech_debt_items debt_score overall_score quality_level
    standards_violations=$(jq -r '.standards_compliance.total_violations // 0' "$quality_data")
    compliance_score=$(jq -r '.standards_compliance.compliance_score // 100' "$quality_data")
    tech_debt_items=$(jq -r '.tech_debt_analysis.total_debt_items // 0' "$quality_data")
    debt_score=$(jq -r '.tech_debt_analysis.debt_score // 100' "$quality_data")
    overall_score=$(jq -r '.quality_assessment.overall_score // 100' "$quality_data")
    quality_level=$(jq -r '.quality_assessment.quality_level // "UNKNOWN"' "$quality_data")
    
    # Create quality report (internal metrics only - no fake AI analysis)
    cat > "quality-report.md" << EOF
# 📊 Quality Metrics Report

**Pull Request:** #${PR_NUMBER:-unknown} | **Commit:** \`$HEAD_SHA\` | **Base:** \`$BASE_BRANCH\`

## Quality Metrics Summary
- **Overall Quality Score:** $overall_score/100
- **Quality Level:** $quality_level
- **Standards Violations:** $standards_violations
- **Compliance Score:** $compliance_score/100
- **Tech Debt Items:** $tech_debt_items
- **Debt Score:** $debt_score/100

*Note: AI-powered analysis provided by Claude in separate PR comments*

EOF
    
    # Note: Real AI analysis will be added by Claude action in the workflow
    # The Claude action will use the quality data and prompts to generate genuine insights
    
    # Set GitHub Actions outputs
    if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
        {
            echo "standards_violations=$standards_violations"
            echo "compliance_score=$compliance_score"
            echo "tech_debt_items=$tech_debt_items"
            echo "debt_score=$debt_score"
            echo "overall_score=$overall_score"
            echo "quality_level=$quality_level"
        } >> "$GITHUB_OUTPUT"
    fi
    
    log_success "Quality report generated"
}

create_quality_artifacts() {
    log_section "Creating Quality Artifacts"
    
    local artifact_dir="artifacts/quality"
    
    # Copy all quality analysis files
    cp -r "$QUALITY_DIR"/* "$artifact_dir/" 2>/dev/null || true
    
    # Copy quality report
    cp "quality-report.md" "$artifact_dir/" 2>/dev/null || true
    
    # Generate artifact summary
    cat > "$artifact_dir/artifact-summary.json" << EOF
{
    "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%S.%3NZ")",
    "commit_sha": "$HEAD_SHA",
    "base_branch": "$BASE_BRANCH",
    "pr_number": "${PR_NUMBER:-unknown}",
    "analysis_types": ["standards_compliance", "tech_debt"],
    "severity_level": "$SEVERITY_LEVEL",
    "files_included": [
        "quality-analysis-data.json",
        "quality-report.md",
        "*-summary.json",
        "ai-quality-analysis.md"
    ]
}
EOF
    
    log_info "Quality artifacts created in $artifact_dir"
    upload_artifact "quality-analysis" "$artifact_dir"
}

# Error handling
set -euo pipefail
trap cleanup EXIT

# Check if script is being sourced or executed
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi