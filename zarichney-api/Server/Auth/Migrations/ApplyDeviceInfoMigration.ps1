# Database connection parameters
$DB_HOST = if ($env:DB_HOST) { $env:DB_HOST } else { "localhost" }
$DB_PORT = if ($env:DB_PORT) { $env:DB_PORT } else { "5432" }
$DB_NAME = if ($env:DB_NAME) { $env:DB_NAME } else { "zarichney_identity" }
$DB_USER = if ($env:DB_USER) { $env:DB_USER } else { "postgres" }

# Get the script directory and locate the SQL file
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$MIGRATION_SQL = Join-Path $SCRIPT_DIR "ApplyDeviceInfoMigration.sql"

# Check if the PGPASSWORD environment variable is set
if (-not $env:PGPASSWORD) {
    Write-Error "Error: PGPASSWORD environment variable is not set."
    Write-Host "Please set it to the PostgreSQL password before running this script."
    exit 1
}

# Check if the SQL file exists
if (-not (Test-Path $MIGRATION_SQL)) {
    Write-Error "Error: Migration SQL file not found: $MIGRATION_SQL"
    exit 1
}

# Apply the migration
Write-Host "Applying migration to add device info to refresh tokens..."
$psqlCmd = "psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f `"$MIGRATION_SQL`""

# Execute the psql command
$result = Invoke-Expression $psqlCmd

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration applied successfully!"
} else {
    Write-Error "Error applying migration."
    exit 1
}