<#
.SYNOPSIS
    Applies all Entity Framework Core migrations for ASP.NET Identity with PostgreSQL
.DESCRIPTION
    This script applies all pending Entity Framework Core migrations for ASP.NET Identity with PostgreSQL
.NOTES
    Prerequisites:
    1. Ensure PostgreSQL is installed and running
    2. Ensure the database specified in appsettings.json exists
    3. Ensure Npgsql.EntityFrameworkCore.PostgreSQL package is installed
    4. Ensure the EF Core CLI tools are installed globally:
       dotnet tool install --global dotnet-ef
#>

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
# Navigate to project root (assuming it's 3 levels up from the Migrations directory)
$ProjectDir = (Get-Item "$ScriptDir\..\..\..").FullName

# Display current info
Write-Host "Applying all migrations for ASP.NET Identity in PostgreSQL..." -ForegroundColor Cyan
Write-Host "Script directory: $ScriptDir" -ForegroundColor Gray
Write-Host "Project directory: $ProjectDir" -ForegroundColor Gray

# Change to the project directory
Set-Location -Path $ProjectDir
Write-Host "Current working directory: $(Get-Location)" -ForegroundColor Gray

# First, ensure the project is properly built
Write-Host "Building project..." -ForegroundColor Yellow
$buildResult = dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Check if the migration exists - improved to detect migrations with the name in it
Write-Host "Checking available migrations..." -ForegroundColor Yellow
$migrations = dotnet ef migrations list --context UserDbContext
Write-Host "Available migrations:" -ForegroundColor Gray
foreach ($migration in $migrations) {
    Write-Host " - $migration" -ForegroundColor Gray
}

# Verify if the EF Migrations History table exists
Write-Host "Checking migration history..." -ForegroundColor Yellow
try {
    $historyCheck = psql -h localhost -U postgres -d zarichney_identity -c 'SELECT * FROM "__EFMigrationsHistory"' -t
    Write-Host "Found existing migration history." -ForegroundColor Green
    
    # Apply all migrations
    Write-Host "Running: dotnet ef database update --context UserDbContext --verbose" -ForegroundColor Yellow
    $migrationOutput = dotnet ef database update --context UserDbContext --verbose 2>&1
}
catch {
    Write-Host "Migration history table not found or is empty. Forcing initial migration..." -ForegroundColor Yellow
    
    # Create the migration history table first if needed
    try {
        psql -h localhost -U postgres -d zarichney_identity -c 'CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" ("MigrationId" character varying(150) NOT NULL, "ProductVersion" character varying(32) NOT NULL, CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"))'
    }
    catch {
        Write-Host "Could not create migration history table. Make sure PostgreSQL is running and database exists." -ForegroundColor Red
    }
    
    # Find initial migration
    $initialMigration = $null
    foreach ($migration in $migrations) {
        if ($migration -match ".*_AddIdentitySchema") {
            $initialMigration = $migration -replace ' \(Pending\)$', ''
            Write-Host "Found initial migration: $initialMigration" -ForegroundColor Green
            break
        }
    }
    
    if (-not $initialMigration) {
        $initialMigration = "20250322000001_AddIdentitySchema"
        Write-Host "Using hardcoded initial migration: $initialMigration" -ForegroundColor Yellow
    }
    
    # Apply initial migration
    Write-Host "Running: dotnet ef database update $initialMigration --context UserDbContext --verbose" -ForegroundColor Yellow
    $migrationOutput = dotnet ef database update $initialMigration --context UserDbContext --verbose 2>&1
}

# Display the output for debugging
Write-Host "Migration command output:" -ForegroundColor Gray
foreach ($line in $migrationOutput) {
    Write-Host " $line" -ForegroundColor Gray
}

# Check if the migration was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
    
    # ...existing error handling code...
    
    # Offer SQL script fallback
    Write-Host "You can try running the provided SQL script directly:" -ForegroundColor Yellow
    Write-Host "psql -h localhost -U postgres -d zarichney_identity -f ApplyMigration.sql" -ForegroundColor Gray
    
    $runSqlFile = Read-Host "Would you like to run the SQL script now? (y/n)"
    if ($runSqlFile -eq 'y') {
        Write-Host "Running SQL script to create tables..." -ForegroundColor Yellow
        try {
            psql -h localhost -U postgres -d zarichney_identity -f "$ScriptDir\ApplyMigration.sql"
            
            # Check if SQL script was successful
            $tables = psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t
            if ($tables -like "*AspNetUsers*") {
                Write-Host "SQL script successfully created ASP.NET Identity tables!" -ForegroundColor Green
            }
            else {
                Write-Host "SQL script execution failed or didn't create the expected tables." -ForegroundColor Red
            }
        }
        catch {
            Write-Host "Failed to run SQL script: $_" -ForegroundColor Red
        }
    }
    
    exit $LASTEXITCODE
}
else {
    Write-Host "Migrations successfully applied." -ForegroundColor Green
    
    # Verify tables were created
    Write-Host "Verifying tables were created..." -ForegroundColor Yellow
    try {
        $tables = psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t
        
        if ($tables -like "*AspNetUsers*") {
            Write-Host "ASP.NET Identity tables were successfully created!" -ForegroundColor Green
            
            # Check for RefreshTokens table
            if ($tables -like "*refreshtokens*") {
                Write-Host "RefreshTokens table was also successfully created!" -ForegroundColor Green
            }
            else {
                $hasRefreshTokenMigration = $migrations -match "AddRefreshTokenSchema"
                if ($hasRefreshTokenMigration) {
                    Write-Host "RefreshTokens table was not found, but the migration exists." -ForegroundColor Yellow
                    Write-Host "You may need to run the refresh token migration separately." -ForegroundColor Yellow
                }
            }
        }
        else {
            Write-Host "ASP.NET Identity tables may not have been created correctly." -ForegroundColor Yellow
            Write-Host "Please check the database manually." -ForegroundColor Yellow
            
            # Offer to run the SQL script if the tables weren't found
            $runSqlFile = Read-Host "Would you like to run the SQL script to ensure tables are created? (y/n)"
            if ($runSqlFile -eq 'y') {
                Write-Host "Running SQL script to create tables..." -ForegroundColor Yellow
                psql -h localhost -U postgres -d zarichney_identity -f "$ScriptDir\ApplyMigration.sql"
            }
        }
    }
    catch {
        Write-Host "Could not verify table creation. Please check the database manually." -ForegroundColor Yellow
        Write-Host "Error: $_" -ForegroundColor Red
    }
}