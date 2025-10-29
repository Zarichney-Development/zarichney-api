#!/bin/bash
# Validate all frontmatter (skills + commands)
# Usage: ./validate-all-frontmatter.sh [file1.md file2.md ...] (optional - validates all if not provided)

set -euo pipefail

# Colors for output
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly BLUE='\033[0;34m'
readonly NC='\033[0m'

# Paths
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

log_info() {
    echo -e "${BLUE}[FRONTMATTER]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[FRONTMATTER]${NC} $1"
}

log_error() {
    echo -e "${RED}[FRONTMATTER]${NC} $1"
}

main() {
    local skills_failed=0
    local commands_failed=0

    log_info "🔍 Validating All Frontmatter (Skills + Commands)"
    echo ""
    log_info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""

    # Separate files into skills and commands based on path patterns
    local skill_files=()
    local command_files=()

    if [[ $# -gt 0 ]]; then
        # Filter provided files by type
        for file in "$@"; do
            if [[ "$file" == *"/skills/"*"/SKILL.md" ]] || [[ "$file" == *"SKILL.md" ]]; then
                skill_files+=("$file")
            elif [[ "$file" == *"/commands/"*".md" ]] && [[ "$file" != *"README.md" ]]; then
                command_files+=("$file")
            fi
        done

        # Validate skills if any skill files provided
        if [[ ${#skill_files[@]} -gt 0 ]]; then
            if ! "$SCRIPT_DIR/validate-skills-frontmatter.sh" "${skill_files[@]}"; then
                skills_failed=1
            fi
        fi

        # Validate commands if any command files provided
        if [[ ${#command_files[@]} -gt 0 ]]; then
            echo ""
            if ! "$SCRIPT_DIR/validate-commands-frontmatter.sh" "${command_files[@]}"; then
                commands_failed=1
            fi
        fi

        # If no relevant files, skip validation
        if [[ ${#skill_files[@]} -eq 0 ]] && [[ ${#command_files[@]} -eq 0 ]]; then
            log_info "No skill or command files to validate"
            return 0
        fi
    else
        # Validate all skills
        if ! "$SCRIPT_DIR/validate-skills-frontmatter.sh"; then
            skills_failed=1
        fi

        echo ""

        # Validate all commands
        if ! "$SCRIPT_DIR/validate-commands-frontmatter.sh"; then
            commands_failed=1
        fi
    fi

    # Overall result
    echo ""
    log_info "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

    if [[ $skills_failed -eq 0 ]] && [[ $commands_failed -eq 0 ]]; then
        log_success "✅ All frontmatter validation PASSED"
        return 0
    else
        log_error "❌ Frontmatter validation FAILED"
        [[ $skills_failed -eq 1 ]] && log_error "   - Skills validation failed"
        [[ $commands_failed -eq 1 ]] && log_error "   - Commands validation failed"
        return 1
    fi
}

# Run validation
main "$@"
