# ApplyMigrations.ps1
#
# This script applies Entity Framework Core migrations for ASP.NET Identity with PostgreSQL
# 
# Prerequisites:
# 1. Ensure PostgreSQL 17 is installed and running
# 2. Ensure the database specified in appsettings.json exists
#    You can create it manually using pgAdmin or psql:
#    psql -U postgres -c "CREATE DATABASE zarichney_identity;"
# 3. Ensure Npgsql.EntityFrameworkCore.PostgreSQL package is installed
# 4. Ensure the EF Core CLI tools are installed globally:
#    dotnet tool install --global dotnet-ef
#
# Usage:
# .\ApplyMigrations.ps1

# Change to the project directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

# Display current info
Write-Host "Applying migrations for ASP.NET Identity in PostgreSQL..." -ForegroundColor Cyan
Write-Host "Current directory: $pwd" -ForegroundColor Gray

# First, ensure the project is properly built
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Verify if the EF Migrations History table exists and has any entries
Write-Host "Checking migration history..." -ForegroundColor Yellow
$connectionString = ((Get-Content appsettings.json -Raw) | ConvertFrom-Json).ConnectionStrings.IdentityConnection
$checkCmd = "psql -h localhost -U postgres -d zarichney_identity -c 'SELECT * FROM `"__EFMigrationsHistory`"' -t"
$migrationHistory = Invoke-Expression $checkCmd 2>&1
$error = $migrationHistory | Where-Object { $_ -match "ERROR" }

# If the table doesn't exist, we need to force the initial migration
if ($error) {
    Write-Host "Migration history table not found or is empty. Forcing initial migration..." -ForegroundColor Yellow
    
    # Create the migration history table first if needed
    $createTableCmd = "psql -h localhost -U postgres -d zarichney_identity -c 'CREATE TABLE IF NOT EXISTS `"__EFMigrationsHistory`" (`"MigrationId`" character varying(150) NOT NULL, `"ProductVersion`" character varying(32) NOT NULL, CONSTRAINT `"PK___EFMigrationsHistory`" PRIMARY KEY (`"MigrationId`"))'"
    Invoke-Expression $createTableCmd
    
    # Run SQL commands to create tables directly if needed
    Write-Host "Cleaning database and forcing initial migration..." -ForegroundColor Yellow
    
    # Force the migration through EF Core
    Write-Host "Running: dotnet ef database update 20250322000001_AddIdentitySchema --context UserDbContext --project . --verbose" -ForegroundColor Yellow
    dotnet ef database update 20250322000001_AddIdentitySchema --context UserDbContext --project . --verbose
}
else {
    # Regular update if history table exists
    Write-Host "Running: dotnet ef database update --context UserDbContext --project ." -ForegroundColor Yellow
    dotnet ef database update --context UserDbContext --project .
}

# Check if the migration was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
    Write-Host "Check that:" -ForegroundColor Yellow
    Write-Host "1. PostgreSQL is running" -ForegroundColor Yellow
    Write-Host "2. Connection string in appsettings.json is correct" -ForegroundColor Yellow
    Write-Host "3. Database exists (create it manually if needed)" -ForegroundColor Yellow
    Write-Host "4. EF Core tools are installed (run: dotnet tool install --global dotnet-ef)" -ForegroundColor Yellow
    
    # Manual fallback using SQL script
    Write-Host "Attempting a manual fallback using the SQL script..." -ForegroundColor Yellow
    Write-Host "You can try running the provided SQL script directly:" -ForegroundColor Yellow
    Write-Host "psql -h localhost -U postgres -d zarichney_identity -f ApplyMigration.sql" -ForegroundColor Gray
    
    $runSqlFile = Read-Host "Would you like to run the SQL script now? (y/n)"
    if ($runSqlFile -eq "y") {
        Write-Host "Running SQL script to create tables..." -ForegroundColor Yellow
        Invoke-Expression "psql -h localhost -U postgres -d zarichney_identity -f ApplyMigration.sql"
        
        # Check if SQL script was successful
        $checkTablesCmd = "psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t"
        $tables = Invoke-Expression $checkTablesCmd
        if ($tables -match "AspNetUsers") {
            Write-Host "SQL script successfully created ASP.NET Identity tables!" -ForegroundColor Green
        }
        else {
            Write-Host "SQL script execution failed or didn't create the expected tables." -ForegroundColor Red
        }
    }
    
    exit $LASTEXITCODE
}
else {
    Write-Host "Migrations successfully applied." -ForegroundColor Green
    
    # Verify tables were created
    Write-Host "Verifying tables were created..." -ForegroundColor Yellow
    $checkTablesCmd = "psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t"
    $tables = Invoke-Expression $checkTablesCmd
    
    if ($tables -match "AspNetUsers") {
        Write-Host "ASP.NET Identity tables were successfully created!" -ForegroundColor Green
    }
    else {
        Write-Host "ASP.NET Identity tables may not have been created correctly." -ForegroundColor Yellow
        Write-Host "Please check the database manually." -ForegroundColor Yellow
        
        # Offer to run the SQL script if the tables weren't found
        $runSqlFile = Read-Host "Would you like to run the SQL script to ensure tables are created? (y/n)"
        if ($runSqlFile -eq "y") {
            Write-Host "Running SQL script to create tables..." -ForegroundColor Yellow
            Invoke-Expression "psql -h localhost -U postgres -d zarichney_identity -f ApplyMigration.sql"
        }
    }
}