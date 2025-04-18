#!/bin/bash
# Fail on any error
set -e

# Set the environment to Production
export ASPNETCORE_ENVIRONMENT=Production

# Set data directory if not already set
if [ -z "$APP_DATA_PATH" ]; then
  export APP_DATA_PATH="/var/lib/cookbook-api/data"
  echo "APP_DATA_PATH set to default: $APP_DATA_PATH"
fi

# Ensure data directory exists
if [ ! -d "$APP_DATA_PATH" ]; then
  echo "Creating data directory: $APP_DATA_PATH"
  mkdir -p "$APP_DATA_PATH"
fi

# Configure Playwright if needed
export PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1
export PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH=/usr/bin/google-chrome

# Validate that Chrome executable exists
if [ ! -x "$PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH" ]; then
  echo "Chrome executable not found at $PLAYWRIGHT_CHROMIUM_EXECUTABLE_PATH"
  exit 1
fi

# Apply database migrations if needed
if [ -f "/opt/cookbook-api/migrations/ApplyAllMigrations.sql" ] && [ -f "/opt/cookbook-api/Server/Auth/Migrations/ApplyMigrations.sh" ]; then
  echo "Applying database migrations..."
  cd /opt/cookbook-api/Server/Auth/Migrations/
  bash ./ApplyMigrations.sh
  cd /opt/cookbook-api
fi

# Start the API server
echo "Starting API server..."
dotnet /opt/cookbook-api/Zarichney.dll --urls "http://+:80"
