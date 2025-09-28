#!/bin/bash

# Test Suite Health Dashboard Generator
# Generates comprehensive health dashboard with historical trends and visualizations
# Part of Phase 4: Documentation & Standards consolidation

set -euo pipefail

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
RESULTS_DIR="$PROJECT_ROOT/TestResults"
DASHBOARD_FILE="$RESULTS_DIR/test_health_dashboard.html"
HISTORICAL_DIR="$RESULTS_DIR/historical"

# Color codes for terminal output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Logging functions
log() {
    echo -e "${GREEN}[INFO]${NC} $1" >&2
}

warn() {
    echo -e "${YELLOW}[WARN]${NC} $1" >&2
}

error() {
    echo -e "${RED}[ERROR]${NC} $1" >&2
}

# Create necessary directories
create_directories() {
    mkdir -p "$RESULTS_DIR" "$HISTORICAL_DIR"
}

# Generate HTML dashboard
generate_dashboard() {
    local current_results="$RESULTS_DIR/baseline_validation.json"
    local timestamp=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
    
    if [[ ! -f "$current_results" ]]; then
        error "Current baseline validation results not found. Run test suite first."
        exit 1
    fi
    
    log "Generating test suite health dashboard..."
    
    # Extract current metrics
    local total_tests=$(jq -r '.metrics.totalTests // 0' "$current_results")
    local skipped_tests=$(jq -r '.metrics.skippedTests // 0' "$current_results")
    local failed_tests=$(jq -r '.metrics.failedTests // 0' "$current_results")
    local skip_percentage=$(jq -r '.metrics.skipPercentage // 0' "$current_results")
    local line_coverage=$(jq -r '.metrics.lineCoverage // 0' "$current_results")
    local environment=$(jq -r '.environment.classification // "unknown"' "$current_results")
    local passes_thresholds=$(jq -r '.validation.passesThresholds // false' "$current_results")
    
    # Progressive coverage metrics
    local current_phase=$(jq -r '.progressiveCoverage.currentPhase // "Unknown"' "$current_results")
    local next_target=$(jq -r '.progressiveCoverage.nextTarget // 0' "$current_results")
    local coverage_gap=$(jq -r '.progressiveCoverage.coverageGap // 0' "$current_results")
    local is_on_track=$(jq -r '.progressiveCoverage.isOnTrack // false' "$current_results")
    local required_velocity=$(jq -r '.progressiveCoverage.requiredVelocity // 0' "$current_results")
    local months_to_target=$(jq -r '.progressiveCoverage.monthsToTarget // 0' "$current_results")
    
    # Generate historical trend data
    local historical_data=$(generate_historical_trends)
    
    # Determine status values for cleaner HTML generation
    if [ "$passes_thresholds" = "true" ]; then
        status_class="pass"
        status_text="BASELINE PASSED"
    else
        status_class="fail"
        status_text="BASELINE FAILED"
    fi
    
    # Create HTML dashboard
    cat > "$DASHBOARD_FILE" << EOF
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Test Suite Health Dashboard - Zarichney API</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
            color: #333;
        }
        
        .container {
            max-width: 1400px;
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            padding: 30px;
        }
        
        .header {
            text-align: center;
            margin-bottom: 40px;
            border-bottom: 2px solid #e0e0e0;
            padding-bottom: 20px;
        }
        
        .header h1 {
            color: #2c3e50;
            margin: 0 0 10px 0;
            font-size: 2.5em;
        }
        
        .header .subtitle {
            color: #7f8c8d;
            font-size: 1.1em;
        }
        
        .status-indicator {
            display: inline-block;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: bold;
            text-transform: uppercase;
            font-size: 0.9em;
            margin-left: 15px;
        }
        
        .status-pass {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        
        .status-fail {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        
        .metrics-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 20px;
            margin-bottom: 40px;
        }
        
        .metric-card {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            border-left: 4px solid;
        }
        
        .metric-card.primary {
            border-left-color: #007bff;
        }
        
        .metric-card.success {
            border-left-color: #28a745;
        }
        
        .metric-card.warning {
            border-left-color: #ffc107;
        }
        
        .metric-card.danger {
            border-left-color: #dc3545;
        }
        
        .metric-value {
            font-size: 2.5em;
            font-weight: bold;
            margin-bottom: 5px;
        }
        
        .metric-label {
            font-size: 0.9em;
            color: #6c757d;
            text-transform: uppercase;
            letter-spacing: 1px;
        }
        
        .progressive-section {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border-radius: 12px;
            padding: 30px;
            margin: 40px 0;
        }
        
        .progressive-section h2 {
            margin-top: 0;
            font-size: 1.8em;
        }
        
        .phase-info {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin-top: 20px;
        }
        
        .phase-card {
            background: rgba(255,255,255,0.1);
            border-radius: 8px;
            padding: 15px;
            backdrop-filter: blur(10px);
        }
        
        .charts-container {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 30px;
            margin: 40px 0;
        }
        
        .chart-container {
            background: white;
            border-radius: 8px;
            padding: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        
        .chart-title {
            font-size: 1.3em;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 20px;
            text-align: center;
        }
        
        .environment-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 0.8em;
            font-weight: bold;
            text-transform: uppercase;
        }
        
        .env-unconfigured {
            background-color: #fff3cd;
            color: #856404;
        }
        
        .env-configured {
            background-color: #d1ecf1;
            color: #0c5460;
        }
        
        .env-production {
            background-color: #d4edda;
            color: #155724;
        }
        
        .violations-section {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
        }
        
        .violation-item {
            background: #fff;
            border-left: 4px solid #dc3545;
            padding: 10px 15px;
            margin: 10px 0;
            border-radius: 4px;
        }
        
        .recommendation-item {
            background: #fff;
            border-left: 4px solid #28a745;
            padding: 10px 15px;
            margin: 10px 0;
            border-radius: 4px;
        }
        
        @media (max-width: 768px) {
            .charts-container {
                grid-template-columns: 1fr;
            }
            
            .container {
                padding: 15px;
            }
            
            .header h1 {
                font-size: 2em;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Test Suite Health Dashboard</h1>
            <div class="subtitle">
                Generated: $timestamp
                <span class="environment-badge env-$environment">$environment Environment</span>
                <span class="status-indicator status-$status_class">
                    $status_text
                </span>
            </div>
        </div>
        
        <!-- Current Metrics -->
        <div class="metrics-grid">
            <div class="metric-card primary">
                <div class="metric-value">$total_tests</div>
                <div class="metric-label">Total Tests</div>
            </div>
            <div class="metric-card warning">
                <div class="metric-value">$skipped_tests</div>
                <div class="metric-label">Skipped Tests</div>
            </div>
            <div class="metric-card $([ "$failed_tests" -eq 0 ] && echo "success" || echo "danger")">
                <div class="metric-value">$failed_tests</div>
                <div class="metric-label">Failed Tests</div>
            </div>
            <div class="metric-card primary">
                <div class="metric-value">${line_coverage}%</div>
                <div class="metric-label">Line Coverage</div>
            </div>
            <div class="metric-card warning">
                <div class="metric-value">${skip_percentage}%</div>
                <div class="metric-label">Skip Rate</div>
            </div>
        </div>
        
        <!-- Progressive Coverage Section -->
        <div class="progressive-section">
            <h2>🎯 Progressive Coverage Journey to Continuous Testing Excellence</h2>
            <div class="phase-info">
                <div class="phase-card">
                    <h3>Current Phase</h3>
                    <p>$current_phase</p>
                </div>
                <div class="phase-card">
                    <h3>Next Target</h3>
                    <p>${next_target}% Coverage</p>
                </div>
                <div class="phase-card">
                    <h3>Coverage Gap</h3>
                    <p>${coverage_gap}% to go</p>
                </div>
                <div class="phase-card">
                    <h3>Timeline Status</h3>
                    <p>$([ "$is_on_track" = "true" ] && echo "✅ On Track" || echo "⚠️ Behind Schedule")</p>
                </div>
                <div class="phase-card">
                    <h3>Required Velocity</h3>
                    <p>${required_velocity}% per month</p>
                </div>
                <div class="phase-card">
                    <h3>Months to Target</h3>
                    <p>${months_to_target} months</p>
                </div>
            </div>
        </div>
        
        <!-- Charts -->
        <div class="charts-container">
            <div class="chart-container">
                <div class="chart-title">Coverage Trend (Last 30 Days)</div>
                <canvas id="coverageChart"></canvas>
            </div>
            <div class="chart-container">
                <div class="chart-title">Skip Rate Analysis</div>
                <canvas id="skipChart"></canvas>
            </div>
        </div>
        
        <!-- Validation Results -->
EOF

    # Add violations section if there are any
    local violations=$(jq -r '.validation.violations[]? // empty' "$current_results")
    if [[ -n "$violations" ]]; then
        cat >> "$DASHBOARD_FILE" << EOF
        <div class="violations-section">
            <h3>❌ Current Issues</h3>
EOF
        while IFS= read -r violation; do
            echo "            <div class=\"violation-item\">$violation</div>" >> "$DASHBOARD_FILE"
        done <<< "$violations"
        echo "        </div>" >> "$DASHBOARD_FILE"
    fi
    
    # Add recommendations section
    local recommendations=$(jq -r '.validation.recommendations[]? // empty' "$current_results")
    if [[ -n "$recommendations" ]]; then
        cat >> "$DASHBOARD_FILE" << EOF
        <div class="violations-section">
            <h3>💡 Recommendations</h3>
EOF
        while IFS= read -r recommendation; do
            echo "            <div class=\"recommendation-item\">$recommendation</div>" >> "$DASHBOARD_FILE"
        done <<< "$recommendations"
        echo "        </div>" >> "$DASHBOARD_FILE"
    fi
    
    # Add JavaScript for charts
    cat >> "$DASHBOARD_FILE" << EOF
    </div>
    
    <script>
        // Historical data
        const historicalData = $historical_data;
        
        // Coverage Trend Chart
        const coverageCtx = document.getElementById('coverageChart').getContext('2d');
        new Chart(coverageCtx, {
            type: 'line',
            data: {
                labels: historicalData.dates,
                datasets: [{
                    label: 'Line Coverage %',
                    data: historicalData.coverage,
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0, 123, 255, 0.1)',
                    tension: 0.4,
                    fill: true
                }, {
                    label: 'Target Progression',
                    data: historicalData.targets,
                    borderColor: '#28a745',
                    backgroundColor: 'transparent',
                    borderDash: [5, 5],
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100,
                        ticks: {
                            callback: function(value) {
                                return value + '%';
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top'
                    }
                }
            }
        });
        
        // Skip Rate Chart
        const skipCtx = document.getElementById('skipChart').getContext('2d');
        new Chart(skipCtx, {
            type: 'doughnut',
            data: {
                labels: ['Passed Tests', 'Skipped Tests', 'Failed Tests'],
                datasets: [{
                    data: [$((total_tests - skipped_tests - failed_tests)), $skipped_tests, $failed_tests],
                    backgroundColor: ['#28a745', '#ffc107', '#dc3545'],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = Math.round((context.parsed / total) * 100);
                                return context.label + ': ' + context.parsed + ' (' + percentage + '%)';
                            }
                        }
                    }
                }
            }
        });
    </script>
</body>
</html>
EOF
    
    log "Dashboard generated: $DASHBOARD_FILE"
}

# Generate historical trends data for charts
generate_historical_trends() {
    local json_data='{"dates":[],"coverage":[],"targets":[],"skipRates":[]}'
    
    # If historical data exists, process it
    if [[ -d "$HISTORICAL_DIR" ]]; then
        local dates=()
        local coverage=()
        local targets=()
        local skip_rates=()
        
        # Process historical files (last 30 entries) - using safe file iteration
        local historical_files=()
        if [[ -d "$HISTORICAL_DIR" ]]; then
            # Use mapfile with null-delimited find output for safe iteration
            mapfile -t historical_files < <(find "$HISTORICAL_DIR" -name "baseline_validation_*.json" -type f -print0 2>/dev/null | xargs -0 ls -t 2>/dev/null | head -30 | sort)
        fi
        
        for file in "${historical_files[@]}"; do
            if [[ -f "$file" ]]; then
                local date=$(jq -r '.timestamp // empty' "$file" 2>/dev/null)
                local cov=$(jq -r '.metrics.lineCoverage // 0' "$file" 2>/dev/null)
                local skip=$(jq -r '.metrics.skipPercentage // 0' "$file" 2>/dev/null)
                local target=$(jq -r '.progressiveCoverage.nextTarget // 20' "$file" 2>/dev/null)
                
                if [[ -n "$date" ]]; then
                    dates+=("\"$date\"")
                    coverage+=("$cov")
                    targets+=("$target")
                    skip_rates+=("$skip")
                fi
            fi
        done
        
        # Add current data point
        local current_file="$RESULTS_DIR/baseline_validation.json"
        if [[ -f "$current_file" ]]; then
            local current_date=$(jq -r '.timestamp // empty' "$current_file" 2>/dev/null)
            local current_cov=$(jq -r '.metrics.lineCoverage // 0' "$current_file" 2>/dev/null)
            local current_skip=$(jq -r '.metrics.skipPercentage // 0' "$current_file" 2>/dev/null)
            local current_target=$(jq -r '.progressiveCoverage.nextTarget // 20' "$current_file" 2>/dev/null)
            
            if [[ -n "$current_date" ]]; then
                dates+=("\"$current_date\"")
                coverage+=("$current_cov")
                targets+=("$current_target")
                skip_rates+=("$current_skip")
            fi
        fi
        
        # Build JSON
        if [[ ${#dates[@]} -gt 0 ]]; then
            local dates_json="[$(IFS=','; echo "${dates[*]}")]"
            local coverage_json="[$(IFS=','; echo "${coverage[*]}")]"
            local targets_json="[$(IFS=','; echo "${targets[*]}")]"
            local skip_json="[$(IFS=','; echo "${skip_rates[*]}")]"
            
            json_data="{\"dates\":$dates_json,\"coverage\":$coverage_json,\"targets\":$targets_json,\"skipRates\":$skip_json}"
        fi
    fi
    
    echo "$json_data"
}

# Archive current results to historical
archive_current_results() {
    local current_file="$RESULTS_DIR/baseline_validation.json"
    if [[ -f "$current_file" ]]; then
        local timestamp=$(jq -r '.timestamp // empty' "$current_file")
        if [[ -n "$timestamp" ]]; then
            local formatted_timestamp=$(echo "$timestamp" | tr ':' '-' | tr 'T' '_' | cut -d'.' -f1)
            local archive_file="$HISTORICAL_DIR/baseline_validation_${formatted_timestamp}.json"
            cp "$current_file" "$archive_file"
            log "Results archived to $archive_file"
        fi
    fi
}

# Open dashboard in browser
open_dashboard() {
    if command -v xdg-open > /dev/null; then
        xdg-open "$DASHBOARD_FILE"
    elif command -v open > /dev/null; then
        open "$DASHBOARD_FILE"
    elif command -v start > /dev/null; then
        start "$DASHBOARD_FILE"
    else
        log "Dashboard ready at: $DASHBOARD_FILE"
        log "Open the file in your web browser to view the dashboard."
    fi
}

# Main function
main() {
    local auto_open=false
    
    # Parse arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --open)
                auto_open=true
                shift
                ;;
            -h|--help)
                echo "Usage: $0 [OPTIONS]"
                echo ""
                echo "Generate a comprehensive HTML dashboard for test suite health."
                echo ""
                echo "Options:"
                echo "  --open    Automatically open dashboard in browser"
                echo "  -h, --help Show this help message"
                exit 0
                ;;
            *)
                error "Unknown option: $1"
                exit 1
                ;;
        esac
    done
    
    log "Starting test suite health dashboard generation..."
    
    create_directories
    archive_current_results
    generate_dashboard
    
    if [[ "$auto_open" == "true" ]]; then
        open_dashboard
    else
        log "Dashboard generated successfully!"
        log "File: $DASHBOARD_FILE"
        log "Run with --open to automatically open in browser."
    fi
}

# Run main function
main "$@"