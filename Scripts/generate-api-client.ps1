# Generate API Client for Tests
# This script automates the generation of strongly-typed Refit client interfaces for integration testing.
# It builds the Zarichney.Server project, generates swagger.json, and creates multiple Refit client interfaces grouped by OpenAPI tags.

# Stop on first error
$ErrorActionPreference = "Stop"

Write-Host "Starting API client generation process..." -ForegroundColor Cyan

# Define paths
$rootDir = Split-Path -Parent $PSScriptRoot
$apiServerDir = Join-Path -Path $rootDir -ChildPath "Code/Zarichney.Server"
$apiServerTestsDir = Join-Path -Path $rootDir -ChildPath "Code/Zarichney.Server.Tests"
$apiClientDir = Join-Path -Path $apiServerTestsDir -ChildPath "Framework\Client"
$swaggerJsonPath = Join-Path -Path $apiServerDir -ChildPath "swagger.json"

# Ensure the ApiClient directory exists
if (-not (Test-Path -Path $apiClientDir)) {
    Write-Host "Creating API Client directory..." -ForegroundColor Yellow
    New-Item -Path $apiClientDir -ItemType Directory | Out-Null
}

# Step 1: Build the Zarichney.Server project
Write-Host "Building Zarichney.Server project..." -ForegroundColor Green
dotnet build "$apiServerDir/Zarichney.Server.csproj" -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error building Zarichney.Server project. Exiting." -ForegroundColor Red
    exit 1
}

# Step 2: Check if swagger CLI tool is installed
Write-Host "Checking if Swashbuckle CLI tool is installed..." -ForegroundColor Green
$swaggerToolInstalled = dotnet tool list --global | Select-String "swashbuckle.aspnetcore.cli"
if (-not $swaggerToolInstalled) {
    Write-Host "Installing Swashbuckle CLI tool..." -ForegroundColor Yellow
    dotnet tool install --global Swashbuckle.AspNetCore.Cli
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error installing Swashbuckle CLI tool. Exiting." -ForegroundColor Red
        exit 1
    }
}

# Step 3: Generate swagger.json by running the API server temporarily
Write-Host "Generating swagger.json by running the API server temporarily..." -ForegroundColor Green

# Start the API server in the background to generate swagger.json
Write-Host "Starting API server temporarily..." -ForegroundColor Green
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "$apiServerDir/Zarichney.Server.csproj", "--urls", "http://localhost:5000" -PassThru
$apiPid = $apiProcess.Id

# Wait for the API to start up
Write-Host "Waiting for API to start..." -ForegroundColor Green
Start-Sleep -Seconds 10

try {
    # Download the swagger.json
    Write-Host "Downloading swagger.json..." -ForegroundColor Green
    $webClient = New-Object System.Net.WebClient
    $webClient.DownloadFile("http://localhost:5000/api/swagger/swagger.json", $swaggerJsonPath)
    
    if (-not (Test-Path -Path $swaggerJsonPath) -or (Get-Item $swaggerJsonPath).Length -eq 0) {
        throw "Failed to download swagger.json or file is empty"
    }
}
catch {
    Write-Host "Error downloading swagger.json: $($_.Exception.Message). Exiting." -ForegroundColor Red
    Stop-Process -Id $apiPid -Force -ErrorAction SilentlyContinue
    exit 1
}
finally {
    # Stop the API server
    Write-Host "Stopping API server..." -ForegroundColor Green
    Stop-Process -Id $apiPid -Force -ErrorAction SilentlyContinue
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

# Step 5: Generate Refit client using refitter with .refitter settings file
Write-Host "Generating Refit client using .refitter settings file..." -ForegroundColor Green
refitter --settings-file "$PSScriptRoot/.refitter" --skip-validation
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error generating Refit client using Scripts/.refitter file. Exiting." -ForegroundColor Red
    exit 1
}

# Step 6: Clean up swagger.json if needed
if (Test-Path -Path $swaggerJsonPath) {
    Write-Host "Cleaning up swagger.json..." -ForegroundColor Green
    Remove-Item -Path $swaggerJsonPath
}

Write-Host "API client generation completed successfully!" -ForegroundColor Cyan
Write-Host "Generated client interfaces are available in: $apiClientDir/" -ForegroundColor Cyan
Write-Host "Client interfaces are grouped by OpenAPI tags (multiple files)" -ForegroundColor Cyan
Write-Host "Client namespace: Zarichney.Client" -ForegroundColor Cyan