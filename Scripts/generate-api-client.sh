#!/bin/bash
# Generate API Client for Tests
# This script automates the generation of a strongly-typed Refit client for integration testing.
# It builds the api-server project, generates swagger.json, and creates the Refit client.

# Exit on error
set -e

echo -e "\e[36mStarting API client generation process...\e[0m"

# Define paths
ROOT_DIR="$(dirname "$(dirname "$(readlink -f "$0")")")"
API_SERVER_DIR="$ROOT_DIR/api-server"
API_SERVER_TESTS_DIR="$ROOT_DIR/api-server.Tests"
API_CLIENT_DIR="$API_SERVER_TESTS_DIR/Framework/Client"
SWAGGER_JSON_PATH="$API_SERVER_DIR/swagger.json"

# Ensure the ApiClient directory exists
if [ ! -d "$API_CLIENT_DIR" ]; then
    echo -e "\e[33mCreating API Client directory...\e[0m"
    mkdir -p "$API_CLIENT_DIR"
fi

# Step 1: Build the api-server project
echo -e "\e[32mBuilding api-server project...\e[0m"
dotnet build "$API_SERVER_DIR/api-server.csproj" -c Debug
if [ $? -ne 0 ]; then
    echo -e "\e[31mError building api-server project. Exiting.\e[0m"
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

# Start the API server in the background to generate swagger.json
echo -e "\e[32mStarting API server temporarily...\e[0m"
dotnet run --project "$API_SERVER_DIR/api-server.csproj" --urls "http://localhost:5000" > /dev/null 2>&1 &
API_PID=$!

# Wait for the API to start up
echo -e "\e[32mWaiting for API to start...\e[0m"
sleep 10

# Download the swagger.json
echo -e "\e[32mDownloading swagger.json...\e[0m"
curl -s "http://localhost:5000/swagger/swagger.json" -o "$SWAGGER_JSON_PATH"
if [ $? -ne 0 ] || [ ! -s "$SWAGGER_JSON_PATH" ]; then
    echo -e "\e[31mError downloading swagger.json. Exiting.\e[0m"
    kill $API_PID
    exit 1
fi

# Stop the API server
echo -e "\e[32mStopping API server...\e[0m"
kill $API_PID

# Step 4: Install refitter if not already installed
echo -e "\e[32mChecking if refitter is installed...\e[0m"
if ! dotnet tool list -g | grep -q "refitter"; then
    echo -e "\e[33mInstalling refitter...\e[0m"
    dotnet tool install -g refitter
    if [ $? -ne 0 ]; then
        echo -e "\e[31mError installing refitter. Exiting.\e[0m"
        exit 1
    fi
fi

# Step 5: Generate Refit client using refitter with .refitter settings file
echo -e "\e[32mGenerating Refit client using .refitter settings file...\e[0m"
refitter --settings-file "$ROOT_DIR/.refitter" --skip-validation
if [ $? -ne 0 ]; then
    echo -e "\e[31mError generating Refit client using .refitter file. Exiting.\e[0m"
    exit 1
fi

# Step 6: Clean up swagger.json if needed
if [ -f "$SWAGGER_JSON_PATH" ]; then
    echo -e "\e[32mCleaning up swagger.json...\e[0m"
    rm "$SWAGGER_JSON_PATH"
fi

echo -e "\e[36mAPI client generation completed successfully!\e[0m"
echo -e "\e[36mGenerated client is available at: $API_CLIENT_DIR/IZarichneyAPI.cs\e[0m"
echo -e "\e[36mClient interface name: IZarichneyAPI\e[0m"
echo -e "\e[36mClient namespace: Zarichney.Client\e[0m"