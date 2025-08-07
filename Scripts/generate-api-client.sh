#!/bin/bash
# Generate API Client for Tests
# This script automates the generation of strongly-typed Refit client interfaces for integration testing.
# It builds the Zarichney.Server project, generates swagger.json, and creates multiple Refit client interfaces grouped by OpenAPI tags.

# Exit on error
set -e

echo -e "\e[36mStarting API client generation process...\e[0m"

# Define paths
ROOT_DIR="$(dirname "$(dirname "$(readlink -f "$0")")")"
API_SERVER_DIR="$ROOT_DIR/Code/Zarichney.Server"
API_SERVER_TESTS_DIR="$ROOT_DIR/Code/Zarichney.Server.Tests"
API_CLIENT_DIR="$API_SERVER_TESTS_DIR/Framework/Client"
SWAGGER_JSON_PATH="$API_SERVER_DIR/swagger.json"

# Ensure the ApiClient directory exists
if [ ! -d "$API_CLIENT_DIR" ]; then
    echo -e "\e[33mCreating API Client directory...\e[0m"
    mkdir -p "$API_CLIENT_DIR"
fi

# Step 1: Build the Zarichney.Server project
echo -e "\e[32mBuilding Zarichney.Server project...\e[0m"
dotnet build "$API_SERVER_DIR/Zarichney.Server.csproj" -c Debug
if [ $? -ne 0 ]; then
    echo -e "\e[31mError building Zarichney.Server project. Exiting.\e[0m"
    exit 1
fi

# Step 2: Check if swagger CLI tool is installed
echo -e "\e[32mChecking if Swashbuckle CLI tool is installed...\e[0m"
if ! dotnet tool list --global | grep -q "swashbuckle.aspnetcore.cli"; then
    echo -e "\e[33mInstalling Swashbuckle CLI tool...\e[0m"
    dotnet tool install --global Swashbuckle.AspNetCore.Cli
    if [ $? -ne 0 ]; then
        echo -e "\e[31mError installing Swashbuckle CLI tool. Exiting.\e[0m"
        exit 1
    fi
fi

# Step 3: Run the API server with Swagger enabled and get the swagger.json directly
echo -e "\e[32mGenerating swagger.json by running the API server temporarily...\e[0m"

# Ensure port 5000 is available
if lsof -Pi :5000 -sTCP:LISTEN -t >/dev/null ; then
    echo -e "\e[33mPort 5000 is already in use. Attempting to find and stop the process...\e[0m"
    PID_ON_PORT=$(lsof -Pi :5000 -sTCP:LISTEN -t)
    if [ ! -z "$PID_ON_PORT" ]; then
        echo -e "\e[33mKilling process $PID_ON_PORT on port 5000...\e[0m"
        kill -9 $PID_ON_PORT 2>/dev/null || true
        sleep 2
    fi
fi

# Start the API server in the background to generate swagger.json
echo -e "\e[32mStarting API server temporarily...\e[0m"
# Create a temporary log file for debugging startup issues
TEMP_LOG=$(mktemp)
echo -e "\e[33mAPI server logs will be captured in: $TEMP_LOG\e[0m"

# Set environment variables for more permissive startup
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://localhost:5000

dotnet run --project "$API_SERVER_DIR/Zarichney.Server.csproj" > "$TEMP_LOG" 2>&1 &
API_PID=$!

# Function to safely stop the API server
cleanup_api_server() {
    if ps -p $API_PID > /dev/null 2>&1; then
        echo -e "\e[32mStopping API server (PID: $API_PID)...\e[0m"
        kill $API_PID
        # Wait for graceful shutdown
        sleep 2
        # Force kill if still running
        if ps -p $API_PID > /dev/null 2>&1; then
            echo -e "\e[33mForce stopping API server...\e[0m"
            kill -9 $API_PID 2>/dev/null || true
        fi
    else
        echo -e "\e[33mAPI server process not found (may have already stopped)\e[0m"
    fi
    
    # Clean up temporary log file and show recent logs if startup failed
    if [ -f "$TEMP_LOG" ]; then
        if [ -s "$TEMP_LOG" ]; then
            echo -e "\e[33mAPI server startup logs:\e[0m"
            tail -20 "$TEMP_LOG"
        fi
        rm -f "$TEMP_LOG"
    fi
}

# Set up trap for cleanup on script exit
trap cleanup_api_server EXIT

# Wait for the API to start up with health checking
echo -e "\e[32mWaiting for API to start...\e[0m"
for i in {1..45}; do
    # Check if process is still running
    if ! ps -p $API_PID > /dev/null 2>&1; then
        echo -e "\e[31mAPI server process has stopped unexpectedly.\e[0m"
        echo -e "\e[33mLast few lines from server logs:\e[0m"
        if [ -f "$TEMP_LOG" ]; then
            tail -20 "$TEMP_LOG"
        fi
        exit 1
    fi
    
    # Try to reach the swagger endpoint
    if curl -s -f --connect-timeout 5 --max-time 10 "http://localhost:5000/api/swagger/swagger.json" > /dev/null 2>&1; then
        echo -e "\e[32mAPI is ready!\e[0m"
        break
    fi
    
    # Also try health check endpoint if available
    if curl -s -f --connect-timeout 5 --max-time 10 "http://localhost:5000/health" > /dev/null 2>&1; then
        echo -e "\e[32mAPI health check passed, waiting for swagger...\e[0m"
    fi
    
    if [ $i -eq 45 ]; then
        echo -e "\e[31mTimeout waiting for API to start (90 seconds).\e[0m"
        echo -e "\e[33mServer logs:\e[0m"
        if [ -f "$TEMP_LOG" ]; then
            cat "$TEMP_LOG"
        fi
        exit 1
    fi
    echo -e "\e[33mWaiting... (attempt $i/45)\e[0m"
    sleep 2
done

# Download the swagger.json
echo -e "\e[32mDownloading swagger.json...\e[0m"
curl -s "http://localhost:5000/api/swagger/swagger.json" -o "$SWAGGER_JSON_PATH"
if [ $? -ne 0 ] || [ ! -s "$SWAGGER_JSON_PATH" ]; then
    echo -e "\e[31mError downloading swagger.json. Exiting.\e[0m"
    exit 1
fi

# Validate swagger.json is valid JSON
echo -e "\e[32mValidating swagger.json...\e[0m"
if ! python3 -m json.tool "$SWAGGER_JSON_PATH" > /dev/null 2>&1; then
    echo -e "\e[31mInvalid JSON in swagger.json. Exiting.\e[0m"
    exit 1
fi

# Check if swagger.json contains expected OpenAPI structure
if ! grep -q '"openapi"' "$SWAGGER_JSON_PATH"; then
    echo -e "\e[31mswagger.json does not appear to be a valid OpenAPI document. Exiting.\e[0m"
    exit 1
fi

echo -e "\e[32mswagger.json validated successfully!\e[0m"

# Stop the API server (trap will handle cleanup)

# Step 4: Ensure refitter is available as local tool
echo -e "\e[32mSetting up refitter tool...\e[0m"

# Check if tool manifest exists, create if not
if [ ! -f ".config/dotnet-tools.json" ]; then
    echo -e "\e[33mCreating dotnet tool manifest...\e[0m"
    dotnet new tool-manifest --force > /dev/null 2>&1
fi

# Check if refitter is installed as local tool, install if not
if ! dotnet tool list | grep -q "refitter"; then
    echo -e "\e[33mInstalling refitter as local tool...\e[0m"
    dotnet tool install refitter
    if [ $? -ne 0 ]; then
        echo -e "\e[31mError installing refitter. Exiting.\e[0m"
        exit 1
    fi
else
    echo -e "\e[32mRefitter is already installed as local tool\e[0m"
fi

# Step 5: Generate Refit client using refitter with .refitter settings file
echo -e "\e[32mGenerating Refit client using .refitter settings file...\e[0m"
dotnet refitter --settings-file "$ROOT_DIR/Scripts/.refitter" --skip-validation
if [ $? -ne 0 ]; then
    echo -e "\e[31mError generating Refit client using Scripts/.refitter file. Exiting.\e[0m"
    exit 1
fi

# Step 6: Clean up swagger.json if needed
if [ -f "$SWAGGER_JSON_PATH" ]; then
    echo -e "\e[32mCleaning up swagger.json...\e[0m"
    rm "$SWAGGER_JSON_PATH"
fi

echo -e "\e[36mAPI client generation completed successfully!\e[0m"
echo -e "\e[36mGenerated client interfaces are available in: $API_CLIENT_DIR/\e[0m"
echo -e "\e[36mClient interfaces are grouped by OpenAPI tags (multiple files)\e[0m"
echo -e "\e[36mClient namespace: Zarichney.Client\e[0m"
