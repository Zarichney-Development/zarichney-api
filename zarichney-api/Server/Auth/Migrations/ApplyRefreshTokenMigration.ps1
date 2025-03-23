# ApplyRefreshTokenMigration.ps1
#
# This script applies the RefreshToken migration for ASP.NET Identity with PostgreSQL
# 
# Prerequisites:
# 1. Ensure PostgreSQL is installed and running
# 2. Ensure the database specified in appsettings.json exists
# 3. Ensure Npgsql.EntityFrameworkCore.PostgreSQL package is installed
# 4. Ensure the EF Core CLI tools are installed globally:
#    dotnet tool install --global dotnet-ef
#
# Usage:
# .\ApplyRefreshTokenMigration.ps1

# Change to the project directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

# Display current info
Write-Host "Applying RefreshToken migration for ASP.NET Identity in PostgreSQL..." -ForegroundColor Cyan
Write-Host "Current directory: $pwd" -ForegroundColor Gray

# First, ensure the project is properly built
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Apply the migration
Write-Host "Running: dotnet ef database update AddRefreshTokenSchema --context UserDbContext --project ." -ForegroundColor Yellow
dotnet ef database update AddRefreshTokenSchema --context UserDbContext --project .

# Check if the migration was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
    Write-Host "Check that:" -ForegroundColor Yellow
    Write-Host "1. PostgreSQL is running" -ForegroundColor Yellow
    Write-Host "2. Connection string in appsettings.json is correct" -ForegroundColor Yellow
    Write-Host "3. Database exists (create it manually if needed)" -ForegroundColor Yellow
    Write-Host "4. EF Core tools are installed (run: dotnet tool install --global dotnet-ef)" -ForegroundColor Yellow
    
    exit $LASTEXITCODE
}
else {
    Write-Host "RefreshToken migration successfully applied." -ForegroundColor Green
    
    # Verify table was created
    Write-Host "Verifying RefreshTokens table was created..." -ForegroundColor Yellow
    $checkTablesCmd = "psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t"
    $tables = Invoke-Expression $checkTablesCmd
    
    if ($tables -match "RefreshTokens") {
        Write-Host "RefreshTokens table was successfully created!" -ForegroundColor Green
    }
    else {
        Write-Host "RefreshTokens table may not have been created correctly." -ForegroundColor Yellow
        Write-Host "Please check the database manually." -ForegroundColor Yellow
    }
}