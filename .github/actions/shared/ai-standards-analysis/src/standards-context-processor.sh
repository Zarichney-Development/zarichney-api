#!/bin/bash

# AI Standards Analysis - Standards Context Processor
# Build Workflow Component: Standards data validation and context preparation

set -euo pipefail

# Global variables for context processing
ENHANCED_CONTEXT_FILE="/tmp/enhanced-standards-context.json"
COMPONENT_CONTEXT_FILE="/tmp/component-context.json"
EPIC_CONTEXT_FILE="/tmp/epic-prioritization-context.json"

# Security and logging setup
SECURITY_LOG="/tmp/ai-standards-security.log"
DEBUG_LOG="/tmp/ai-standards-debug.log"

# Initialize logging
setup_logging() {
    if [[ "${DEBUG_MODE:-false}" == "true" ]]; then
        exec 3>&1 4>&2
        exec 1> >(tee -a "$DEBUG_LOG")
        exec 2> >(tee -a "$DEBUG_LOG" >&2)
        echo "[$(date -u +"%Y-%m-%d %H:%M:%S UTC")] Standards context processor initialized" | tee -a "$DEBUG_LOG"
    fi
}

# Validate standards analysis inputs
validate_standards_inputs() {
    echo "🔍 Validating standards analysis inputs..."

    # Component type validation
    local valid_types=("workflow" "action" "documentation" "backend" "frontend" "test" "core" "security")
    local type_valid=false

    for valid_type in "${valid_types[@]}"; do
        if [[ "$COMPONENT_TYPE" == "$valid_type" ]]; then
            type_valid=true
            break
        fi
    done

    if [[ "$type_valid" != "true" ]]; then
        echo "❌ Invalid component type: $COMPONENT_TYPE. Must be one of: ${valid_types[*]}" | tee -a "$SECURITY_LOG"
        return 1
    fi

    # Standards context validation
    if [[ -z "$STANDARDS_CONTEXT" ]]; then
        echo "❌ Standards context is required" | tee -a "$SECURITY_LOG"
        return 1
    fi

    # Validate standards paths exist
    IFS=',' read -ra STANDARDS_PATHS <<< "$STANDARDS_CONTEXT"
    for path in "${STANDARDS_PATHS[@]}"; do
        # Remove leading/trailing whitespace
        path=$(echo "$path" | xargs)

        # Security: Validate path doesn't contain traversal attempts
        if [[ "$path" == *".."* ]] || [[ "$path" == *"//"* ]]; then
            echo "❌ Invalid path detected (security): $path" | tee -a "$SECURITY_LOG"
            return 1
        fi

        # Check if path exists
        if [[ ! -f "$path" ]]; then
            echo "⚠️ Warning: Standards document not found: $path" | tee -a "$DEBUG_LOG"
        fi
    done

    # Change scope validation
    if [[ -z "$CHANGE_SCOPE" ]]; then
        echo "❌ Change scope is required" | tee -a "$SECURITY_LOG"
        return 1
    fi

    # Epic context validation
    if [[ -n "$EPIC_CONTEXT" ]]; then
        # Basic validation for epic context format
        if [[ ${#EPIC_CONTEXT} -gt 200 ]]; then
            echo "❌ Epic context too long (max 200 characters)" | tee -a "$SECURITY_LOG"
            return 1
        fi
    fi

    # Analysis depth validation
    local valid_depths=("surface" "detailed" "comprehensive")
    local depth_valid=false

    for valid_depth in "${valid_depths[@]}"; do
        if [[ "$ANALYSIS_DEPTH" == "$valid_depth" ]]; then
            depth_valid=true
            break
        fi
    done

    if [[ "$depth_valid" != "true" ]]; then
        echo "❌ Invalid analysis depth: $ANALYSIS_DEPTH. Must be one of: ${valid_depths[*]}" | tee -a "$SECURITY_LOG"
        return 1
    fi

    # Architecture mode validation
    local valid_modes=("component" "integration" "system")
    local mode_valid=false

    for valid_mode in "${valid_modes[@]}"; do
        if [[ "$ARCHITECTURE_MODE" == "$valid_mode" ]]; then
            mode_valid=true
            break
        fi
    done

    if [[ "$mode_valid" != "true" ]]; then
        echo "❌ Invalid architecture mode: $ARCHITECTURE_MODE. Must be one of: ${valid_modes[*]}" | tee -a "$SECURITY_LOG"
        return 1
    fi

    # Compliance threshold validation
    if [[ "$COMPLIANCE_THRESHOLD" -lt 0 ]] || [[ "$COMPLIANCE_THRESHOLD" -gt 100 ]]; then
        echo "❌ Invalid compliance threshold: $COMPLIANCE_THRESHOLD. Must be 0-100" | tee -a "$SECURITY_LOG"
        return 1
    fi

    # Priority focus validation
    local valid_focuses=("security" "maintainability" "performance" "all")
    local focus_valid=false

    for valid_focus in "${valid_focuses[@]}"; do
        if [[ "$PRIORITY_FOCUS" == "$valid_focus" ]]; then
            focus_valid=true
            break
        fi
    done

    if [[ "$focus_valid" != "true" ]]; then
        echo "❌ Invalid priority focus: $PRIORITY_FOCUS. Must be one of: ${valid_focuses[*]}" | tee -a "$SECURITY_LOG"
        return 1
    fi

    echo "✅ Standards input validation successful"
    return 0
}

# Process standards context for AI analysis
process_standards_context() {
    echo "📋 Processing standards context..."

    # Initialize base context structure
    local base_context=$(cat <<EOF
{
    "analysis_type": "standards",
    "component_type": "$COMPONENT_TYPE",
    "standards_context": "$STANDARDS_CONTEXT",
    "change_scope": "$CHANGE_SCOPE",
    "epic_context": "$EPIC_CONTEXT",
    "analysis_depth": "$ANALYSIS_DEPTH",
    "architecture_mode": "$ARCHITECTURE_MODE",
    "compliance_threshold": $COMPLIANCE_THRESHOLD,
    "priority_focus": "$PRIORITY_FOCUS",
    "timestamp": "$(date -u +"%Y-%m-%d %H:%M:%S UTC")",
    "standards_documents": [],
    "change_analysis": {},
    "component_specifications": {},
    "epic_alignment_requirements": {}
}
EOF
)

    # Write base context
    echo "$base_context" > "$ENHANCED_CONTEXT_FILE"

    # Load and analyze standards documents
    load_standards_documents

    # Analyze change scope
    analyze_change_scope

    # Validate final context
    if ! jq . "$ENHANCED_CONTEXT_FILE" >/dev/null 2>&1; then
        echo "❌ Generated invalid JSON context" | tee -a "$SECURITY_LOG"
        return 1
    fi

    # Output enhanced context for AI analysis using heredoc format for complex JSON
    {
        echo "enhanced_context<<EOF"
        cat "$ENHANCED_CONTEXT_FILE" | jq -c .
        echo "EOF"
    } >> "$GITHUB_OUTPUT"

    echo "✅ Standards context processing successful"
    return 0
}

# Load and validate standards documents
load_standards_documents() {
    echo "📚 Loading standards documents..."

    local standards_array="[]"

    IFS=',' read -ra STANDARDS_PATHS <<< "$STANDARDS_CONTEXT"
    for path in "${STANDARDS_PATHS[@]}"; do
        # Remove leading/trailing whitespace
        path=$(echo "$path" | xargs)

        if [[ -f "$path" ]]; then
            # Extract document metadata
            local doc_name=$(basename "$path")
            local doc_size=$(stat -f%z "$path" 2>/dev/null || stat -c%s "$path" 2>/dev/null || echo "0")

            # Security: Limit document size (max 1MB)
            if [[ "$doc_size" -gt 1048576 ]]; then
                echo "⚠️ Skipping large standards document: $path ($doc_size bytes)" | tee -a "$DEBUG_LOG"
                continue
            fi

            # Create document entry
            local doc_entry=$(cat <<EOF
{
    "path": "$path",
    "name": "$doc_name",
    "size": $doc_size,
    "available": true,
    "last_modified": "$(stat -f%m "$path" 2>/dev/null || stat -c%Y "$path" 2>/dev/null || echo "0")"
}
EOF
)

            # Add to standards array
            standards_array=$(echo "$standards_array" | jq --argjson doc "$doc_entry" '. + [$doc]')

        else
            # Document not found
            local doc_entry=$(cat <<EOF
{
    "path": "$path",
    "name": "$(basename "$path")",
    "size": 0,
    "available": false,
    "error": "Document not found"
}
EOF
)

            standards_array=$(echo "$standards_array" | jq --argjson doc "$doc_entry" '. + [$doc]')
        fi
    done

    # Update enhanced context with standards documents
    jq --argjson docs "$standards_array" '.standards_documents = $docs' "$ENHANCED_CONTEXT_FILE" > "${ENHANCED_CONTEXT_FILE}.tmp"
    mv "${ENHANCED_CONTEXT_FILE}.tmp" "$ENHANCED_CONTEXT_FILE"

    echo "✅ Standards documents loaded: $(echo "$standards_array" | jq 'length')"
}

# Analyze change scope and impact
analyze_change_scope() {
    echo "🔍 Analyzing change scope..."

    # Parse change scope (could be file paths or description)
    local change_analysis="{}"

    # Detect if change scope contains file paths
    if [[ "$CHANGE_SCOPE" == *"/"* ]] || [[ "$CHANGE_SCOPE" == *"."* ]]; then
        # Likely file paths
        local file_count=0
        local file_extensions=()

        # Count files and analyze extensions
        IFS=$'\n' read -ra CHANGE_FILES <<< "$(echo "$CHANGE_SCOPE" | tr ',' '\n')"
        for file in "${CHANGE_FILES[@]}"; do
            file=$(echo "$file" | xargs)  # Trim whitespace

            if [[ -n "$file" ]]; then
                ((file_count++))

                # Extract extension
                if [[ "$file" == *"."* ]]; then
                    local ext="${file##*.}"
                    file_extensions+=("$ext")
                fi
            fi
        done

        # Create file-based analysis
        change_analysis=$(cat <<EOF
{
    "type": "file_paths",
    "file_count": $file_count,
    "file_extensions": $(printf '%s\n' "${file_extensions[@]}" | sort | uniq | jq -R . | jq -s .),
    "scope_description": "File-based change analysis",
    "analysis_focus": "component_specific"
}
EOF
)
    else
        # Descriptive change scope
        change_analysis=$(cat <<EOF
{
    "type": "description",
    "description": "$CHANGE_SCOPE",
    "scope_description": "Descriptive change analysis",
    "analysis_focus": "contextual"
}
EOF
)
    fi

    # Update enhanced context with change analysis
    jq --argjson analysis "$change_analysis" '.change_analysis = $analysis' "$ENHANCED_CONTEXT_FILE" > "${ENHANCED_CONTEXT_FILE}.tmp"
    mv "${ENHANCED_CONTEXT_FILE}.tmp" "$ENHANCED_CONTEXT_FILE"

    echo "✅ Change scope analysis completed"
}

# Generate component-specific context
generate_component_context() {
    echo "🏗️ Generating component-specific context..."

    # Component-specific specifications based on type
    local component_specs="{}"

    case "$COMPONENT_TYPE" in
        "workflow")
            component_specs=$(cat <<EOF
{
    "type": "workflow",
    "focus_areas": ["yaml_syntax", "job_dependencies", "security_controls", "performance", "maintainability"],
    "standards_emphasis": ["github_actions_best_practices", "workflow_security", "resource_optimization"],
    "architectural_patterns": ["composite_actions", "reusable_workflows", "conditional_logic"],
    "quality_gates": ["syntax_validation", "security_scanning", "performance_benchmarks"]
}
EOF
)
            ;;
        "action")
            component_specs=$(cat <<EOF
{
    "type": "action",
    "focus_areas": ["interface_design", "input_validation", "output_consistency", "error_handling"],
    "standards_emphasis": ["action_metadata", "security_controls", "documentation_completeness"],
    "architectural_patterns": ["composite_structure", "shell_scripting", "input_sanitization"],
    "quality_gates": ["interface_validation", "security_testing", "integration_testing"]
}
EOF
)
            ;;
        "backend")
            component_specs=$(cat <<EOF
{
    "type": "backend",
    "focus_areas": ["dotnet8_patterns", "api_design", "service_architecture", "dependency_injection"],
    "standards_emphasis": ["coding_standards", "solid_principles", "async_patterns", "error_handling"],
    "architectural_patterns": ["clean_architecture", "repository_pattern", "service_layer"],
    "quality_gates": ["unit_testing", "integration_testing", "performance_testing", "security_testing"]
}
EOF
)
            ;;
        "frontend")
            component_specs=$(cat <<EOF
{
    "type": "frontend",
    "focus_areas": ["angular19_patterns", "typescript_standards", "component_design", "state_management"],
    "standards_emphasis": ["component_lifecycle", "reactive_patterns", "accessibility", "performance"],
    "architectural_patterns": ["smart_dumb_components", "ngrx_patterns", "module_structure"],
    "quality_gates": ["unit_testing", "e2e_testing", "accessibility_testing", "performance_testing"]
}
EOF
)
            ;;
        "test")
            component_specs=$(cat <<EOF
{
    "type": "test",
    "focus_areas": ["test_structure", "naming_conventions", "test_categories", "coverage_patterns"],
    "standards_emphasis": ["xunit_patterns", "fluent_assertions", "test_data_builders", "async_testing"],
    "architectural_patterns": ["arrange_act_assert", "test_fixtures", "mock_patterns"],
    "quality_gates": ["test_coverage", "test_performance", "test_maintainability"]
}
EOF
)
            ;;
        "documentation")
            component_specs=$(cat <<EOF
{
    "type": "documentation",
    "focus_areas": ["readme_standards", "api_documentation", "architecture_diagrams", "linking_patterns"],
    "standards_emphasis": ["template_compliance", "content_structure", "navigation_patterns"],
    "architectural_patterns": ["hierarchical_documentation", "cross_referencing", "diagram_maintenance"],
    "quality_gates": ["template_validation", "link_checking", "content_accuracy"]
}
EOF
)
            ;;
        *)
            component_specs=$(cat <<EOF
{
    "type": "$COMPONENT_TYPE",
    "focus_areas": ["general_standards", "code_quality", "maintainability"],
    "standards_emphasis": ["coding_standards", "documentation", "testing"],
    "architectural_patterns": ["solid_principles", "clean_code"],
    "quality_gates": ["code_review", "testing", "documentation"]
}
EOF
)
            ;;
    esac

    # Write component context to file
    echo "$component_specs" > "$COMPONENT_CONTEXT_FILE"

    # Update enhanced context with component specifications
    jq --argjson specs "$component_specs" '.component_specifications = $specs' "$ENHANCED_CONTEXT_FILE" > "${ENHANCED_CONTEXT_FILE}.tmp"
    mv "${ENHANCED_CONTEXT_FILE}.tmp" "$ENHANCED_CONTEXT_FILE"

    {
        echo "component_context<<EOF"
        cat "$COMPONENT_CONTEXT_FILE" | jq -c .
        echo "EOF"
    } >> "$GITHUB_OUTPUT"
    echo "✅ Component-specific context generated"
    return 0
}

# Generate epic-aware prioritization context
generate_epic_prioritization_context() {
    echo "🎯 Generating epic-aware prioritization context..."

    # Epic-specific prioritization patterns
    local epic_requirements="{}"

    case "$EPIC_CONTEXT" in
        *"build-workflow-improvements"* | *"build-workflows"*)
            epic_requirements=$(cat <<EOF
{
    "epic": "build-workflow-improvements",
    "focus": "workflow_modernization",
    "priorities": ["component_extraction", "security_controls", "performance_optimization", "maintainability"],
    "quality_emphasis": ["modular_design", "reusable_components", "comprehensive_testing"],
    "architectural_goals": ["foundation_components", "ai_framework_integration", "canonical_patterns"],
    "success_criteria": ["behavioral_parity", "security_excellence", "team_efficiency"]
}
EOF
)
            ;;
        *"testing-coverage"* | *"coverage"*)
            epic_requirements=$(cat <<EOF
{
    "epic": "testing-coverage-to-90",
    "focus": "coverage_improvement",
    "priorities": ["test_quality", "coverage_patterns", "automation", "maintainability"],
    "quality_emphasis": ["test_coverage", "test_maintainability", "automation_efficiency"],
    "architectural_goals": ["systematic_testing", "coverage_intelligence", "quality_gates"],
    "success_criteria": ["90_percent_coverage", "sustainable_testing", "automated_validation"]
}
EOF
)
            ;;
        *)
            epic_requirements=$(cat <<EOF
{
    "epic": "$EPIC_CONTEXT",
    "focus": "general_improvement",
    "priorities": ["code_quality", "maintainability", "performance", "security"],
    "quality_emphasis": ["standards_compliance", "best_practices", "documentation"],
    "architectural_goals": ["clean_architecture", "solid_principles", "testability"],
    "success_criteria": ["standards_compliance", "code_quality", "maintainability"]
}
EOF
)
            ;;
    esac

    # Write epic context to file
    echo "$epic_requirements" > "$EPIC_CONTEXT_FILE"

    # Update enhanced context with epic alignment requirements
    jq --argjson reqs "$epic_requirements" '.epic_alignment_requirements = $reqs' "$ENHANCED_CONTEXT_FILE" > "${ENHANCED_CONTEXT_FILE}.tmp"
    mv "${ENHANCED_CONTEXT_FILE}.tmp" "$ENHANCED_CONTEXT_FILE"

    {
        echo "epic_prioritization_context<<EOF"
        cat "$EPIC_CONTEXT_FILE" | jq -c .
        echo "EOF"
    } >> "$GITHUB_OUTPUT"
    echo "✅ Epic-aware prioritization context generated"
    return 0
}

# Main execution
main() {
    setup_logging

    echo "🛡️ AI Standards Analysis - Context Processor Starting..."
    echo "Component Type: $COMPONENT_TYPE"
    echo "Analysis Depth: $ANALYSIS_DEPTH"
    echo "Architecture Mode: $ARCHITECTURE_MODE"
    echo "Epic Context: $EPIC_CONTEXT"

    # Execute processing pipeline
    validate_standards_inputs || exit 1
    process_standards_context || exit 1
    generate_component_context || exit 1
    generate_epic_prioritization_context || exit 1

    echo "✅ Standards context processing completed successfully"
}

# Execute main function if script is run directly
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi