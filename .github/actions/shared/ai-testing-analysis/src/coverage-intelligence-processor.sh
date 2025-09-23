#!/bin/bash

# Coverage Intelligence Processor for AI Testing Analysis
# Extracts coverage-specific insights and generates improvement recommendations

set -euo pipefail

# Extract coverage-specific insights from AI analysis
extract_coverage_insights() {
    echo "🧠 Extracting coverage insights from AI analysis..."

    # Parse AI analysis result for coverage-specific data
    local analysis_result="$AI_ANALYSIS_RESULT"

    if [[ -z "$analysis_result" ]]; then
        echo "❌ No AI analysis result to process"
        return 1
    fi

    # Extract coverage analysis section
    local coverage_analysis
    coverage_analysis=$(echo "$analysis_result" | jq -r '.coverage_analysis // empty' 2>/dev/null || echo "{}")

    if [[ "$coverage_analysis" == "{}" ]]; then
        # Generate basic coverage analysis if not present in AI result
        coverage_analysis=$(generate_basic_coverage_analysis)
    fi

    echo "coverage_analysis<<EOF" >> "$GITHUB_OUTPUT"
    echo "$coverage_analysis" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    debug_log "Coverage analysis extracted successfully"
    return 0
}

# Generate improvement recommendations based on coverage data
generate_improvement_recommendations() {
    echo "📋 Generating improvement recommendations..."

    local current_coverage="$COVERAGE_DATA"
    local baseline_coverage="$BASELINE_COVERAGE"
    local target_coverage="$IMPROVEMENT_TARGET"

    # Calculate coverage gap
    local coverage_gap
    coverage_gap=$(awk "BEGIN {printf \"%.2f\", $target_coverage - $current_coverage}")

    # Generate recommendations based on coverage phase and gap
    local recommendations_json
    recommendations_json=$(cat <<EOF
[
  {
    "priority": "high",
    "area": "Service Layer Coverage",
    "description": "Focus on core service methods that handle business logic",
    "impact": "high",
    "effort": "medium",
    "coverage_gain": "5-8%",
    "files": ["Services/**/*.cs"],
    "rationale": "Service layer tests provide high coverage value with moderate effort"
  },
  {
    "priority": "medium",
    "area": "API Controller Coverage",
    "description": "Add integration tests for API endpoints missing coverage",
    "impact": "medium",
    "effort": "medium",
    "coverage_gain": "3-5%",
    "files": ["Controllers/**/*.cs"],
    "rationale": "Controller tests improve integration coverage and catch API contract issues"
  },
  {
    "priority": "medium",
    "area": "Entity Validation Coverage",
    "description": "Test entity validation rules and data annotations",
    "impact": "medium",
    "effort": "low",
    "coverage_gain": "2-4%",
    "files": ["Models/**/*.cs", "Entities/**/*.cs"],
    "rationale": "Entity tests are quick wins for coverage improvement"
  }
]
EOF
    )

    # Adjust recommendations based on coverage phase
    case "$COVERAGE_PHASE" in
        "initial")
            # Focus on foundational tests
            recommendations_json=$(echo "$recommendations_json" | jq '.[0].priority = "critical" | .[0].description = "Establish foundational service layer test coverage for core business logic"')
            ;;
        "milestone")
            # Focus on closing specific gaps
            recommendations_json=$(echo "$recommendations_json" | jq 'map(if .priority == "high" then .priority = "critical" else . end)')
            ;;
    esac

    # Filter recommendations based on coverage gap
    if (( $(awk "BEGIN {print ($coverage_gap < 10)}") )); then
        # Small gap - focus on high-impact areas
        recommendations_json=$(echo "$recommendations_json" | jq 'map(select(.impact == "high"))')
    fi

    echo "improvement_recommendations<<EOF" >> "$GITHUB_OUTPUT"
    echo "$recommendations_json" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    debug_log "Improvement recommendations generated successfully"
    return 0
}

# Calculate milestone progress toward 90% coverage
calculate_milestone_progress() {
    echo "🎯 Calculating milestone progress..."

    local current_coverage="$COVERAGE_DATA"
    local baseline_coverage="$BASELINE_COVERAGE"
    local target_coverage="$IMPROVEMENT_TARGET"

    # Calculate progress metrics
    local progress_percentage
    progress_percentage=$(awk "BEGIN {printf \"%.2f\", ($current_coverage / $target_coverage) * 100}")

    local coverage_gained
    coverage_gained=$(awk "BEGIN {printf \"%.2f\", $current_coverage - $baseline_coverage}")

    local coverage_remaining
    coverage_remaining=$(awk "BEGIN {printf \"%.2f\", $target_coverage - $current_coverage}")

    # Estimate timeline based on current velocity
    local monthly_velocity=2.8  # Target velocity for 90% by Jan 2026
    local months_remaining
    months_remaining=$(awk "BEGIN {printf \"%.1f\", $coverage_remaining / $monthly_velocity}")

    # Determine status
    local status="on-track"
    if (( $(awk "BEGIN {print ($months_remaining > 15)}") )); then
        status="behind-schedule"
    elif (( $(awk "BEGIN {print ($months_remaining < 8)}") )); then
        status="ahead-of-schedule"
    fi

    # Generate milestone progress JSON
    local milestone_progress
    milestone_progress=$(cat <<EOF
{
  "current": $current_coverage,
  "target": $target_coverage,
  "progressPercentage": $progress_percentage,
  "coverageGained": $coverage_gained,
  "coverageRemaining": $coverage_remaining,
  "monthlyVelocityTarget": $monthly_velocity,
  "estimatedMonthsRemaining": $months_remaining,
  "status": "$status",
  "milestoneDate": "2026-01-31",
  "phase": "$COVERAGE_PHASE"
}
EOF
    )

    echo "milestone_progress<<EOF" >> "$GITHUB_OUTPUT"
    echo "$milestone_progress" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    debug_log "Milestone progress calculated: $status"
    return 0
}

# Identify priority areas for coverage improvement
identify_priority_areas() {
    echo "🎯 Identifying priority areas..."

    local current_coverage="$COVERAGE_DATA"
    local target_coverage="$IMPROVEMENT_TARGET"

    # Define priority areas based on coverage phase and current level
    local priority_areas_json

    if (( $(awk "BEGIN {print ($current_coverage < 20)}") )); then
        # Phase 1: Foundation building
        priority_areas_json=$(cat <<EOF
[
  {
    "area": "Core Service Layer",
    "priority": "critical",
    "impact": "high",
    "effort": "medium",
    "estimatedCoverageGain": "6-10%",
    "rationale": "Service layer contains core business logic with high coverage value",
    "suggestedActions": [
      "Create unit tests for primary service methods",
      "Test service constructor injection and dependencies",
      "Add tests for service error handling and validation"
    ]
  },
  {
    "area": "API Controllers",
    "priority": "high",
    "impact": "medium",
    "effort": "medium",
    "estimatedCoverageGain": "4-7%",
    "rationale": "Controller tests improve integration coverage and API contract validation",
    "suggestedActions": [
      "Add integration tests for main API endpoints",
      "Test controller input validation and error responses",
      "Verify controller authorization and authentication"
    ]
  }
]
EOF
        )
    elif (( $(awk "BEGIN {print ($current_coverage < 50)}") )); then
        # Phase 2-3: Comprehensive coverage
        priority_areas_json=$(cat <<EOF
[
  {
    "area": "Edge Cases and Error Handling",
    "priority": "high",
    "impact": "high",
    "effort": "medium",
    "estimatedCoverageGain": "5-8%",
    "rationale": "Edge cases and error scenarios provide significant coverage improvements",
    "suggestedActions": [
      "Test exception handling paths in services",
      "Add boundary condition tests for validation logic",
      "Create tests for null/empty input scenarios"
    ]
  },
  {
    "area": "Integration Scenarios",
    "priority": "medium",
    "impact": "medium",
    "effort": "high",
    "estimatedCoverageGain": "3-6%",
    "rationale": "Integration tests validate component interactions and system behavior",
    "suggestedActions": [
      "Add database integration tests with Testcontainers",
      "Test API endpoint end-to-end scenarios",
      "Verify service layer integration with external dependencies"
    ]
  }
]
EOF
        )
    else
        # Phase 4+: Optimization and completeness
        priority_areas_json=$(cat <<EOF
[
  {
    "area": "Complex Business Scenarios",
    "priority": "medium",
    "impact": "medium",
    "effort": "high",
    "estimatedCoverageGain": "2-4%",
    "rationale": "Complex scenarios ensure comprehensive system validation",
    "suggestedActions": [
      "Test multi-step business workflows",
      "Add performance and load testing scenarios",
      "Create tests for complex data transformation logic"
    ]
  },
  {
    "area": "Cross-Cutting Concerns",
    "priority": "low",
    "impact": "low",
    "effort": "medium",
    "estimatedCoverageGain": "1-3%",
    "rationale": "Cross-cutting concerns complete comprehensive coverage",
    "suggestedActions": [
      "Test logging and monitoring functionality",
      "Add security and authorization edge cases",
      "Verify configuration and environment handling"
    ]
  }
]
EOF
        )
    fi

    echo "priority_areas<<EOF" >> "$GITHUB_OUTPUT"
    echo "$priority_areas_json" >> "$GITHUB_OUTPUT"
    echo "EOF" >> "$GITHUB_OUTPUT"

    debug_log "Priority areas identified for coverage level: $current_coverage%"
    return 0
}

# Generate basic coverage analysis when AI result doesn't contain specific coverage data
generate_basic_coverage_analysis() {
    local current_coverage="$COVERAGE_DATA"
    local baseline_coverage="$BASELINE_COVERAGE"
    local coverage_change
    coverage_change=$(awk "BEGIN {printf \"%.2f\", $current_coverage - $baseline_coverage}")

    local analysis_summary
    if (( $(awk "BEGIN {print ($coverage_change > 0)}") )); then
        analysis_summary="Coverage improved by ${coverage_change}% from baseline"
    elif (( $(awk "BEGIN {print ($coverage_change < 0)}") )); then
        analysis_summary="Coverage decreased by ${coverage_change#-}% from baseline"
    else
        analysis_summary="Coverage maintained at baseline level"
    fi

    cat <<EOF
{
  "summary": "$analysis_summary",
  "currentCoverage": $current_coverage,
  "baselineCoverage": $baseline_coverage,
  "coverageChange": $coverage_change,
  "analysisPhase": "$COVERAGE_PHASE",
  "recommendation": "Focus on service layer and API controller test coverage for maximum impact"
}
EOF
}

# Debug logging function
debug_log() {
    if [[ "$DEBUG_MODE" == "true" ]]; then
        echo "🐛 DEBUG: $1" >&2
    fi
}

# Export functions for use in action
export -f extract_coverage_insights
export -f generate_improvement_recommendations
export -f calculate_milestone_progress
export -f identify_priority_areas
export -f generate_basic_coverage_analysis
export -f debug_log