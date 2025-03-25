<#
.SYNOPSIS
    Applies the API Keys migration for ASP.NET Identity with PostgreSQL
.DESCRIPTION
    This script applies the migration to add the ApiKeys table to the database
.NOTES
    Prerequisites:
    1. Ensure PostgreSQL is installed and running
    2. Ensure the database specified in the connection string exists
    3. Ensure the PostgreSQL is properly configured for authentication
#>

# Database connection parameters with fallback values
$DB_HOST = if ($env:DB_HOST) { $env:DB_HOST } else { "localhost" }
$DB_PORT = if ($env:DB_PORT) { $env:DB_PORT } else { "5432" }
$DB_NAME = if ($env:DB_NAME) { $env:DB_NAME } else { "zarichney_identity" }
$DB_USER = if ($env:DB_USER) { $env:DB_USER } else { "postgres" }

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
# Define the migration SQL file path
$MIGRATION_SQL = Join-Path $ScriptDir "AddApiKeysTable.sql"

# Display current info
Write-Host "Applying migration to add ApiKeys table to the database..." -ForegroundColor Cyan
Write-Host "Script directory: $ScriptDir" -ForegroundColor Gray
Write-Host "SQL file: $MIGRATION_SQL" -ForegroundColor Gray
Write-Host "Database host: $DB_HOST" -ForegroundColor Gray
Write-Host "Database name: $DB_NAME" -ForegroundColor Gray
Write-Host "Database user: $DB_USER" -ForegroundColor Gray

# Check if the SQL file exists
if (-not (Test-Path $MIGRATION_SQL)) {
  Write-Host "Error: Migration SQL file not found at $MIGRATION_SQL" -ForegroundColor Red
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

Write-Host "Applying migration to add ApiKeys table to database $DB_NAME on $DB_HOST..." -ForegroundColor Yellow

# Run the migration SQL script
try {
  $psqlCmd = "psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f `"$MIGRATION_SQL`""
  $migrationOutput = Invoke-Expression $psqlCmd 2>&1

  # Display the output for debugging
  Write-Host "Migration command output:" -ForegroundColor Gray
  foreach ($line in $migrationOutput) {
    Write-Host " $line" -ForegroundColor Gray
  }

  if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration applied successfully!" -ForegroundColor Green
        
    # Verify the ApiKeys table was created
    Write-Host "Verifying ApiKeys table was created..." -ForegroundColor Yellow
    try {
      $tableCheck = psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -c "\dt apikeys" -t
            
      if ($tableCheck -match "apikeys") {
        Write-Host "ApiKeys table was successfully created!" -ForegroundColor Green
      }
      else {
        Write-Host "ApiKeys table may not have been created correctly." -ForegroundColor Yellow
        Write-Host "Please check the database manually." -ForegroundColor Yellow
      }
    }
    catch {
      Write-Host "Could not verify table creation. Please check the database manually." -ForegroundColor Yellow
      Write-Host "Error: $_" -ForegroundColor Red
    }
  }
  else {
    Write-Host "Error: Failed to apply migration" -ForegroundColor Red
    exit 1
  }
}
catch {
  Write-Host "An error occurred while running the migration command:" -ForegroundColor Red
  Write-Host $_.Exception.Message -ForegroundColor Red
  exit 1
}

Write-Host "Done!" -ForegroundColor Green
