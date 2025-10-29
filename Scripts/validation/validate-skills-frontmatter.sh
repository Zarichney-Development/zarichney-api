#!/bin/bash
# Validate SKILL.md YAML frontmatter against JSON schema
# Usage: ./validate-skills-frontmatter.sh [file1.md file2.md ...] (optional - validates all if not provided)

set -euo pipefail

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m'

# Paths
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly ROOT_DIR="$(cd "${SCRIPT_DIR}/../.." && pwd)"
readonly SCHEMA_FILE="$ROOT_DIR/Docs/Templates/schemas/skill-metadata.schema.json"

# Test results
declare -i TOTAL_FILES=0
declare -i VALID_FILES=0
declare -i INVALID_FILES=0

log_info() {
    echo -e "${BLUE}[SKILL-VALIDATE]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SKILL-VALIDATE]${NC} $1"
}

log_error() {
    echo -e "${RED}[SKILL-VALIDATE]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[SKILL-VALIDATE]${NC} $1"
}

# Check dependencies
check_dependencies() {
    local missing_deps=()

    if ! command -v python3 >/dev/null 2>&1; then
        missing_deps+=("python3")
    fi

    # Check Python libraries
    if ! python3 -c "import jsonschema" 2>/dev/null; then
        missing_deps+=("python3-jsonschema")
    fi

    if ! python3 -c "import yaml" 2>/dev/null; then
        missing_deps+=("python3-yaml")
    fi

    if [ ${#missing_deps[@]} -gt 0 ]; then
        log_error "Missing required dependencies: ${missing_deps[*]}"
        log_error "Install with: pip3 install jsonschema PyYAML"
        return 1
    fi

    return 0
}

# Verify schema file exists
verify_schema() {
    if [[ ! -f "$SCHEMA_FILE" ]]; then
        log_error "Schema file not found: $SCHEMA_FILE"
        return 1
    fi
    return 0
}

# Extract YAML frontmatter from markdown file
extract_frontmatter() {
    local file="$1"
    local in_frontmatter=false
    local frontmatter=""
    local line_count=0

    while IFS= read -r line; do
        line_count=$((line_count + 1))

        # First line should be ---
        if [[ $line_count -eq 1 ]]; then
            if [[ "$line" != "---" ]]; then
                echo "ERROR: File does not start with frontmatter delimiter (---)" >&2
                return 1
            fi
            in_frontmatter=true
            continue
        fi

        # Check for closing delimiter
        if [[ "$line" == "---" ]] && [[ $in_frontmatter == true ]]; then
            # Found closing delimiter
            echo "$frontmatter"
            return 0
        fi

        # Accumulate frontmatter content
        if [[ $in_frontmatter == true ]]; then
            frontmatter+="$line"$'\n'
        fi
    done < "$file"

    # If we get here, no closing delimiter found
    echo "ERROR: No closing frontmatter delimiter (---) found" >&2
    return 1
}

# Validate frontmatter against schema
validate_frontmatter() {
    local file="$1"
    local frontmatter="$2"

    # Create temporary file for frontmatter YAML
    local temp_yaml=$(mktemp)
    echo "$frontmatter" > "$temp_yaml"

    # Run Python validation
    local validation_result
    validation_result=$(python3 -c "
import json
import jsonschema
import yaml
import sys
from pathlib import Path

try:
    # Load schema
    with open('$SCHEMA_FILE', 'r') as schema_file:
        schema = json.load(schema_file)

    # Load YAML frontmatter
    with open('$temp_yaml', 'r') as yaml_file:
        data = yaml.safe_load(yaml_file)

    # Handle empty frontmatter
    if data is None:
        print('ERROR: Frontmatter is empty')
        sys.exit(1)

    # Validate against schema
    jsonschema.validate(data, schema)
    print('VALID')
    sys.exit(0)

except yaml.YAMLError as e:
    print(f'YAML_ERROR: Invalid YAML syntax - {str(e)}')
    sys.exit(1)
except jsonschema.ValidationError as e:
    # Extract field path
    field_path = '.'.join(str(p) for p in e.absolute_path) if e.absolute_path else 'root'
    print(f'VALIDATION_ERROR: Field \"{field_path}\" - {e.message}')
    sys.exit(1)
except Exception as e:
    print(f'ERROR: {str(e)}')
    sys.exit(1)
" 2>&1)

    local exit_code=$?

    # Clean up temp file
    rm -f "$temp_yaml"

    # Process validation result
    if [[ $exit_code -eq 0 ]] && [[ "$validation_result" == "VALID" ]]; then
        return 0
    else
        echo "$validation_result"
        return 1
    fi
}

# Validate single skill file
validate_skill_file() {
    local file="$1"
    local relative_path="${file#$ROOT_DIR/}"

    TOTAL_FILES=$((TOTAL_FILES + 1))

    # Check file exists
    if [[ ! -f "$file" ]]; then
        log_error "❌ $relative_path - File not found"
        INVALID_FILES=$((INVALID_FILES + 1))
        return 1
    fi

    # Extract frontmatter
    local frontmatter
    if ! frontmatter=$(extract_frontmatter "$file" 2>&1); then
        log_error "❌ $relative_path - $frontmatter"
        INVALID_FILES=$((INVALID_FILES + 1))
        return 1
    fi

    # Validate frontmatter
    local validation_error
    if validation_error=$(validate_frontmatter "$file" "$frontmatter" 2>&1); then
        log_success "✅ $relative_path"
        VALID_FILES=$((VALID_FILES + 1))
        return 0
    else
        log_error "❌ $relative_path"
        log_error "   $validation_error"
        INVALID_FILES=$((INVALID_FILES + 1))
        return 1
    fi
}

# Find all SKILL.md files
find_all_skills() {
    find "$ROOT_DIR/.claude/skills" -name "SKILL.md" -type f 2>/dev/null | sort
}

# Main validation logic
main() {
    log_info "🔍 Skills Frontmatter Validation"
    echo ""

    # Check dependencies
    if ! check_dependencies; then
        return 1
    fi

    # Verify schema exists
    if ! verify_schema; then
        return 1
    fi

    # Determine files to validate
    local files_to_validate=()

    if [[ $# -gt 0 ]]; then
        # Validate specific files provided as arguments
        for file in "$@"; do
            # Convert to absolute path if relative
            if [[ "$file" != /* ]]; then
                file="$ROOT_DIR/$file"
            fi
            files_to_validate+=("$file")
        done
        log_info "Validating ${#files_to_validate[@]} specified file(s)"
    else
        # Validate all SKILL.md files
        while IFS= read -r file; do
            files_to_validate+=("$file")
        done < <(find_all_skills)

        if [[ ${#files_to_validate[@]} -eq 0 ]]; then
            log_warning "No SKILL.md files found in $ROOT_DIR/.claude/skills"
            return 0
        fi

        log_info "Validating all ${#files_to_validate[@]} SKILL.md file(s)"
    fi

    echo ""

    # Validate each file
    for file in "${files_to_validate[@]}"; do
        validate_skill_file "$file"
    done

    # Report results
    echo ""
    log_info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    log_info "Validation Summary:"
    log_info "  Total files:   $TOTAL_FILES"
    log_success "  Valid files:   $VALID_FILES"

    if [[ $INVALID_FILES -gt 0 ]]; then
        log_error "  Invalid files: $INVALID_FILES"
        log_info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo ""
        log_error "❌ Skill frontmatter validation FAILED"
        log_info "Fix validation errors or consult schema: $SCHEMA_FILE"
        return 1
    else
        log_info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
        echo ""
        log_success "✅ All skill frontmatter validation PASSED"
        return 0
    fi
}

# Run validation
main "$@"
