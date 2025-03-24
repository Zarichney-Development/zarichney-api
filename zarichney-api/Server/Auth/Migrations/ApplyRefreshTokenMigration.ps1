<#
.SYNOPSIS
    Applies the RefreshToken migration for ASP.NET Identity with PostgreSQL
.DESCRIPTION
    This script applies the RefreshToken migration for ASP.NET Identity with PostgreSQL
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
Write-Host "Applying RefreshToken migration for ASP.NET Identity in PostgreSQL..." -ForegroundColor Cyan
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

# Look for any migration containing "AddRefreshTokenSchema" (after timestamp)
$migrationExists = $migrations -match "AddRefreshTokenSchema"
$exactTimestampedMigration = $null

if ($migrationExists) {
    # Extract the full migration name with timestamp to use for updates
    foreach ($migration in $migrations) {
        if ($migration -match ".*_AddRefreshTokenSchema") {
            # Strip the "(Pending)" suffix if it exists
            $exactTimestampedMigration = $migration.Trim() -replace ' \(Pending\)$', ''
            Write-Host "Found existing migration: $exactTimestampedMigration" -ForegroundColor Green
            break
        }
    }
}

if (-not $migrationExists) {
    Write-Host "The migration 'AddRefreshTokenSchema' does not appear to exist in the migrations list." -ForegroundColor Red
    Write-Host "You may need to create this migration first with:" -ForegroundColor Yellow
    Write-Host "dotnet ef migrations add AddRefreshTokenSchema --context UserDbContext" -ForegroundColor Yellow
    
    $createMigration = Read-Host "Would you like to create this migration now? (y/n)"
    if ($createMigration -eq 'y') {
        Write-Host "Creating migration 'AddRefreshTokenSchema'..." -ForegroundColor Yellow
        $createResult = dotnet ef migrations add AddRefreshTokenSchema --context UserDbContext
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to create migration:" -ForegroundColor Red
            Write-Host $createResult -ForegroundColor Red
            exit $LASTEXITCODE
        }
        else {
            Write-Host "Migration created successfully. Now attempting to apply it..." -ForegroundColor Green
        }
    }
    else {
        Write-Host "Migration not created. Exiting script." -ForegroundColor Yellow
        exit 1
    }
}
else {
    # Delete the non-timestamped file to prevent conflicts if it exists
    $nonTimestampedFile = "$ScriptDir\AddRefreshTokenSchema.cs"
    if (Test-Path $nonTimestampedFile) {
        Write-Host "Found non-timestamped migration file. Will rename to avoid conflicts." -ForegroundColor Yellow
        Rename-Item -Path $nonTimestampedFile -NewName "AddRefreshTokenSchema.cs.bak" -Force
        Write-Host "Renamed $nonTimestampedFile to AddRefreshTokenSchema.cs.bak" -ForegroundColor Yellow
    }
}

# Apply the migration with verbose output - use the exact timestamped name if available
$migrationToApply = if ($exactTimestampedMigration) { $exactTimestampedMigration } else { "AddRefreshTokenSchema" }
Write-Host "Running: dotnet ef database update $migrationToApply --context UserDbContext --verbose" -ForegroundColor Yellow
try {
    # Capture the full output of the migration command
    $migrationOutput = dotnet ef database update $migrationToApply --context UserDbContext --verbose 2>&1
    
    # Display the output for debugging
    Write-Host "Migration command output:" -ForegroundColor Gray
    foreach ($line in $migrationOutput) {
        Write-Host " $line" -ForegroundColor Gray
    }
    
    # Check if the migration was successful
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Migration failed with exit code $LASTEXITCODE" -ForegroundColor Red
        
        # Try to identify common error patterns
        if ($migrationOutput -match "connection") {
            Write-Host "Database connection issue detected." -ForegroundColor Red
            Write-Host "Check your connection string in appsettings.json and ensure PostgreSQL is running." -ForegroundColor Yellow
        }
        elseif ($migrationOutput -match "table") {
            Write-Host "Table-related issue detected." -ForegroundColor Red
            Write-Host "There might be conflicts with existing tables or schema issues." -ForegroundColor Yellow
        }
        
        # Provide guidance based on error type
        Write-Host "Check that:" -ForegroundColor Yellow
        Write-Host "1. PostgreSQL is running" -ForegroundColor Yellow
        Write-Host "2. Connection string in appsettings.json is correct" -ForegroundColor Yellow
        Write-Host "3. Database 'zarichney_identity' exists (create it manually if needed)" -ForegroundColor Yellow
        Write-Host "4. EF Core tools are installed (run: dotnet tool install --global dotnet-ef)" -ForegroundColor Yellow
        Write-Host "5. The User model and DbContext are properly configured for RefreshToken" -ForegroundColor Yellow
        
        # Offer to create the database if it doesn't exist
        Write-Host "Would you like to attempt to create the database if it doesn't exist? (y/n)" -ForegroundColor Yellow
        $createDb = Read-Host
        if ($createDb -eq 'y') {
            Write-Host "Attempting to create the database 'zarichney_identity'..." -ForegroundColor Yellow
            try {
                psql -h localhost -U postgres -c "CREATE DATABASE zarichney_identity;" 2>&1
                Write-Host "Database creation attempted. You may now try running the migration script again." -ForegroundColor Green
            }
            catch {
                Write-Host "Failed to create database. You may need to create it manually." -ForegroundColor Red
            }
        }
        
        exit $LASTEXITCODE
    }
    else {
        Write-Host "RefreshToken migration successfully applied." -ForegroundColor Green
        
        # Verify table was created - Note: This requires psql to be in the PATH
        Write-Host "Verifying RefreshTokens table was created..." -ForegroundColor Yellow
        try {
            $tables = psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t
            if ($tables -like "*refreshtokens*") {
                Write-Host "RefreshTokens table was successfully created!" -ForegroundColor Green
            }
            else {
                Write-Host "RefreshTokens table may not have been created correctly." -ForegroundColor Yellow
                Write-Host "Please check the database manually." -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "Could not verify table creation. Please check the database manually." -ForegroundColor Yellow
            Write-Host "Error: $_" -ForegroundColor Red
        }
    }
}
catch {
    Write-Host "An error occurred while running the migration command:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}