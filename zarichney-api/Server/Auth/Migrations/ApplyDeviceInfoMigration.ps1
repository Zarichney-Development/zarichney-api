<#
.SYNOPSIS
    Applies the Device Info migration for ASP.NET Identity with PostgreSQL
.DESCRIPTION
    This script applies the Device Info migration for refresh tokens in ASP.NET Identity with PostgreSQL
.NOTES
    Prerequisites:
    1. Ensure PostgreSQL is installed and running
    2. Ensure the database specified in appsettings.json exists
    3. Ensure the PostgreSQL is properly configured for authentication
#>

# Database connection parameters
$DB_HOST = if ($env:DB_HOST) { $env:DB_HOST } else { "localhost" }
$DB_PORT = if ($env:DB_PORT) { $env:DB_PORT } else { "5432" }
$DB_NAME = if ($env:DB_NAME) { $env:DB_NAME } else { "zarichney_identity" }
$DB_USER = if ($env:DB_USER) { $env:DB_USER } else { "postgres" }

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
# Navigate to project root (assuming it's 3 levels up from the Migrations directory)
$ProjectDir = (Get-Item "$ScriptDir\..\..\..").FullName
$MIGRATION_SQL = Join-Path $ScriptDir "ApplyDeviceInfoMigration.sql"

# Display current info
Write-Host "Applying Device Info migration for Refresh Tokens in PostgreSQL..." -ForegroundColor Cyan
Write-Host "Script directory: $ScriptDir" -ForegroundColor Gray
Write-Host "Project directory: $ProjectDir" -ForegroundColor Gray
Write-Host "SQL file: $MIGRATION_SQL" -ForegroundColor Gray
Write-Host "Database host: $DB_HOST" -ForegroundColor Gray
Write-Host "Database name: $DB_NAME" -ForegroundColor Gray
Write-Host "Database user: $DB_USER" -ForegroundColor Gray

# Check if the SQL file exists
if (-not (Test-Path $MIGRATION_SQL)) {
    Write-Host "Error: Migration SQL file not found: $MIGRATION_SQL" -ForegroundColor Red
    exit 1
}

# Verify if the database exists
Write-Host "Checking if database exists..." -ForegroundColor Yellow
try {
    $dbCheck = psql -h $DB_HOST -p $DB_PORT -U $DB_USER -c "SELECT 1 FROM pg_database WHERE datname='$DB_NAME'" -t
    if ($dbCheck.Trim() -ne "1") {
        Write-Host "Database '$DB_NAME' does not exist." -ForegroundColor Red
        $createDb = Read-Host "Would you like to create the database? (y/n)"
        if ($createDb -eq 'y') {
            Write-Host "Creating database '$DB_NAME'..." -ForegroundColor Yellow
            psql -h $DB_HOST -p $DB_PORT -U $DB_USER -c "CREATE DATABASE $DB_NAME" 
            if ($LASTEXITCODE -ne 0) {
                Write-Host "Failed to create database." -ForegroundColor Red
                exit 1
            }
            Write-Host "Database created successfully." -ForegroundColor Green
        }
        else {
            Write-Host "Exiting script." -ForegroundColor Yellow
            exit 1
        }
    }
    else {
        Write-Host "Database '$DB_NAME' exists." -ForegroundColor Green
    }
}
catch {
    Write-Host "Error checking database: $_" -ForegroundColor Red
    Write-Host "Make sure PostgreSQL is running and connection parameters are correct." -ForegroundColor Yellow
    exit 1
}

# Apply the migration
Write-Host "Applying migration to add device info to refresh tokens..." -ForegroundColor Yellow
$psqlCmd = "psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f `"$MIGRATION_SQL`""

# Execute the psql command and capture output
try {
    $migrationOutput = Invoke-Expression $psqlCmd 2>&1

    # Display the output for debugging
    Write-Host "Migration command output:" -ForegroundColor Gray
    foreach ($line in $migrationOutput) {
        Write-Host " $line" -ForegroundColor Gray
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Migration applied successfully!" -ForegroundColor Green
        
        # Verify the changes were applied
        Write-Host "Verifying migration was applied..." -ForegroundColor Yellow
        try {
            $columnCheck = psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "SELECT column_name FROM information_schema.columns WHERE table_name='refreshtokens' AND column_name IN ('deviceid', 'devicename', 'browser', 'operatingsystem')" -t
            
            if ($columnCheck.Trim().Length -gt 0) {
                Write-Host "Device info columns were successfully added to RefreshTokens table!" -ForegroundColor Green
                Write-Host "Found columns:" -ForegroundColor Gray
                foreach ($line in $columnCheck) {
                    if ($line.Trim().Length -gt 0) {
                        Write-Host " - $($line.Trim())" -ForegroundColor Gray
                    }
                }
            }
            else {
                Write-Host "Device info columns may not have been added correctly." -ForegroundColor Yellow
                Write-Host "Please check the database manually." -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "Could not verify column creation. Please check the database manually." -ForegroundColor Yellow
            Write-Host "Error: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "Error applying migration." -ForegroundColor Red
        Write-Host "Check that:" -ForegroundColor Yellow
        Write-Host "1. PostgreSQL is running" -ForegroundColor Yellow
        Write-Host "2. Database '$DB_NAME' exists" -ForegroundColor Yellow
        Write-Host "3. The SQL script is valid" -ForegroundColor Yellow
        Write-Host "4. The refresh tokens table already exists" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "An error occurred while running the migration command:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}