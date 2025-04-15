<#
.SYNOPSIS
    Applies all pending EF Core migrations to the local development database.
.DESCRIPTION
    This script runs 'dotnet ef database update' targeting the UserDbContext.
    It assumes connection string is configured via user secrets or appsettings.Development.json.
.NOTES
    Prerequisites:
    1. EF Core tools installed: dotnet tool install --global dotnet-ef
    2. You are in a directory containing the project/solution file.
#>

# Define the project path relative to the script if needed, or run from project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = (Get-Item "$ScriptDir\..\..\..").FullName # Adjust if needed

Write-Host "Applying pending migrations for UserDbContext..." -ForegroundColor Cyan
Write-Host "Project directory: $ProjectDir" -ForegroundColor Gray

# Change to the project directory if the script isn't run from there
# Set-Location -Path $ProjectDir

# Apply all migrations using EF Core CLI
Write-Host "Running: dotnet ef database update --context UserDbContext --verbose" -ForegroundColor Yellow
$migrationOutput = dotnet ef database update --context UserDbContext --verbose 2>&1

# Display the output
Write-Host "Command output:" -ForegroundColor Gray
foreach ($line in $migrationOutput) {
    Write-Host " $line" -ForegroundColor Gray
}

# Check if the migration was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}
else {
    Write-Host "Migrations applied successfully (or no pending migrations)." -ForegroundColor Green
}

Write-Host "Done!" -ForegroundColor Green