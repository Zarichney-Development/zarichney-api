# Generate API Client for Tests
# This script automates the generation of a strongly-typed Refit client for integration testing.
# It builds the api-server project, generates swagger.json, and creates the Refit client.

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Starting API client generation process..." -ForegroundColor Cyan

# Define paths
$rootDir = Split-Path -Parent $PSScriptRoot
$apiServerDir = Join-Path -Path $rootDir -ChildPath "api-server"
$apiServerTestsDir = Join-Path -Path $rootDir -ChildPath "api-server.Tests"
$apiClientDir = Join-Path -Path $apiServerTestsDir -ChildPath "Client"
$swaggerJsonPath = Join-Path -Path $apiServerDir -ChildPath "swagger.json"

# Ensure the ApiClient directory exists
if (-not (Test-Path -Path $apiClientDir)) {
    Write-Host "Creating API Client directory..." -ForegroundColor Yellow
    New-Item -Path $apiClientDir -ItemType Directory | Out-Null
}

# Step 1: Build the api-server project
Write-Host "Building api-server project..." -ForegroundColor Green
dotnet build "$apiServerDir/api-server.csproj" -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error building api-server project. Exiting." -ForegroundColor Red
    exit 1
}

# Step 2: Check if swagger CLI tool is installed
Write-Host "Checking if Swashbuckle CLI tool is installed..." -ForegroundColor Green
$swaggerToolInstalled = dotnet tool list --global | Select-String "swashbuckle.aspnetcore.cli"
$dotnetToolsPath = Join-Path $env:USERPROFILE ".dotnet\\tools"
$swaggerExe = Join-Path $dotnetToolsPath "swagger.exe"
if (-not $swaggerToolInstalled) {
    Write-Host "Installing Swashbuckle CLI tool..." -ForegroundColor Yellow
    dotnet tool install --global Swashbuckle.AspNetCore.Cli
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error installing Swashbuckle CLI tool. Exiting." -ForegroundColor Red
        exit 1
    }
}

# Step 3: Generate swagger.json using swagger CLI (full path)
Write-Host "Generating swagger.json..." -ForegroundColor Green
$swashbuckleVersion = "8.1.1"  # Should match the version in api-server.csproj
& $swaggerExe tofile --output "$swaggerJsonPath" "$apiServerDir/bin/Debug/net8.0/Zarichney.dll" swagger
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generating swagger.json. Exiting." -ForegroundColor Red
    exit 1
}

# Step 4: Install refitter if not already installed
Write-Host "Checking if refitter is installed..." -ForegroundColor Green
$refitterInstalled = dotnet tool list -g | Select-String "refitter"
if (-not $refitterInstalled) {
    Write-Host "Installing refitter..." -ForegroundColor Yellow
    dotnet tool install -g refitter
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error installing refitter. Exiting." -ForegroundColor Red
        exit 1
    }
}

# Step 5: Generate Refit client using refitter
Write-Host "Generating Refit client..." -ForegroundColor Green
refitter "$swaggerJsonPath" `
    --namespace "Zarichney.Client" `
    --output "$apiClientDir/ZarichneyAPI.cs" `
    --interface-name "IZarichneyAPI" `
    --use-api-response-for-all-responses `
    --use-nullable-reference-types `
    --use-api-response-for-successful-responses
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generating Refit client. Exiting." -ForegroundColor Red
    exit 1
}

# Step 6: Clean up swagger.json if needed
if (Test-Path -Path $swaggerJsonPath) {
    Write-Host "Cleaning up swagger.json..." -ForegroundColor Green
    Remove-Item -Path $swaggerJsonPath
}

Write-Host "API client generation completed successfully!" -ForegroundColor Cyan
Write-Host "Generated client is available at: $apiClientDir/ZarichneyAPI.cs" -ForegroundColor Cyan
Write-Host "Client interface name: IZarichneyAPI" -ForegroundColor Cyan
Write-Host "Client namespace: Zarichney.Client" -ForegroundColor Cyan
