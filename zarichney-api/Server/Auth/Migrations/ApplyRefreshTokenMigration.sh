#!/bin/bash
#
# ApplyRefreshTokenMigration.sh
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
# ./ApplyRefreshTokenMigration.sh

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Navigate to project root (assuming it's 3 levels up from the Migrations directory)
PROJECT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)"

# Display current info
echo -e "\e[36mApplying RefreshToken migration for ASP.NET Identity in PostgreSQL...\e[0m"
echo -e "\e[90mScript directory: $SCRIPT_DIR\e[0m"
echo -e "\e[90mProject directory: $PROJECT_DIR\e[0m"

# Change to the project directory
cd "$PROJECT_DIR"
echo -e "\e[90mCurrent working directory: $(pwd)\e[0m"

# First, ensure the project is properly built
echo -e "\e[33mBuilding project...\e[0m"
dotnet build
if [ $? -ne 0 ]; then
    echo -e "\e[31mBuild failed\e[0m"
    exit 1
fi

# Check available migrations
echo -e "\e[33mChecking available migrations...\e[0m"
MIGRATIONS=$(dotnet ef migrations list --context UserDbContext)
echo -e "\e[90mAvailable migrations:\e[0m"
echo "$MIGRATIONS"

# Check if the AddRefreshTokenSchema migration exists
if [[ $MIGRATIONS == *"AddRefreshTokenSchema"* ]]; then
    # Extract the timestamped migration name, removing any (Pending) suffix
    TIMESTAMPED_MIGRATION=$(echo "$MIGRATIONS" | grep -o "[0-9]*_AddRefreshTokenSchema" | head -1)
    if [ ! -z "$TIMESTAMPED_MIGRATION" ]; then
        echo -e "\e[32mFound existing migration: $TIMESTAMPED_MIGRATION\e[0m"
        MIGRATION_TO_APPLY=$TIMESTAMPED_MIGRATION
    else
        # If no timestamped format found, use the basic name
        MIGRATION_TO_APPLY="AddRefreshTokenSchema"
        echo -e "\e[33mUsing migration name: $MIGRATION_TO_APPLY\e[0m"
    fi
    
    # Check for non-timestamped duplicate file and rename if exists
    NON_TIMESTAMPED_FILE="$SCRIPT_DIR/AddRefreshTokenSchema.cs"
    if [ -f "$NON_TIMESTAMPED_FILE" ]; then
        echo -e "\e[33mFound non-timestamped migration file. Will rename to avoid conflicts.\e[0m"
        mv "$NON_TIMESTAMPED_FILE" "$NON_TIMESTAMPED_FILE.bak"
        echo -e "\e[33mRenamed $NON_TIMESTAMPED_FILE to AddRefreshTokenSchema.cs.bak\e[0m"
    fi
else
    echo -e "\e[31mThe migration 'AddRefreshTokenSchema' does not appear to exist in the migrations list.\e[0m"
    echo -e "\e[33mYou may need to create this migration first with:\e[0m"
    echo -e "\e[33mdotnet ef migrations add AddRefreshTokenSchema --context UserDbContext\e[0m"
    
    read -p "Would you like to create this migration now? (y/n) " CREATE_MIGRATION
    if [ "$CREATE_MIGRATION" = "y" ]; then
        echo -e "\e[33mCreating migration 'AddRefreshTokenSchema'...\e[0m"
        dotnet ef migrations add AddRefreshTokenSchema --context UserDbContext
        
        if [ $? -ne 0 ]; then
            echo -e "\e[31mFailed to create migration\e[0m"
            exit 1
        else
            echo -e "\e[32mMigration created successfully. Now attempting to apply it...\e[0m"
            MIGRATION_TO_APPLY="AddRefreshTokenSchema"
        fi
    else
        echo -e "\e[33mMigration not created. Exiting script.\e[0m"
        exit 1
    fi
fi

# Apply the migration
echo -e "\e[33mRunning: dotnet ef database update $MIGRATION_TO_APPLY --context UserDbContext --verbose\e[0m"
MIGRATION_OUTPUT=$(dotnet ef database update $MIGRATION_TO_APPLY --context UserDbContext --verbose 2>&1)
MIGRATION_STATUS=$?

# Display the output for debugging
echo -e "\e[90mMigration command output:\e[0m"
echo "$MIGRATION_OUTPUT"

# Check if the migration was successful
if [ $MIGRATION_STATUS -ne 0 ]; then
    echo -e "\e[31mMigration failed with status code $MIGRATION_STATUS\e[0m"
    
    # Analyze the output for common issues
    if [[ $MIGRATION_OUTPUT == *"connection"* ]]; then
        echo -e "\e[31mDatabase connection issue detected.\e[0m"
        echo -e "\e[33mCheck your connection string in appsettings.json and ensure PostgreSQL is running.\e[0m"
    elif [[ $MIGRATION_OUTPUT == *"table"* ]]; then
        echo -e "\e[31mTable-related issue detected.\e[0m"
        echo -e "\e[33mThere might be conflicts with existing tables or schema issues.\e[0m"
    fi
    
    echo -e "\e[33mCheck that:\e[0m"
    echo -e "\e[33m1. PostgreSQL is running\e[0m"
    echo -e "\e[33m2. Connection string in appsettings.json is correct\e[0m"
    echo -e "\e[33m3. Database exists (create it manually if needed)\e[0m"
    echo -e "\e[33m4. EF Core tools are installed (run: dotnet tool install --global dotnet-ef)\e[0m"
    echo -e "\e[33m5. The User model and DbContext are properly configured for RefreshToken\e[0m"
    
    # Offer to create the database if it doesn't exist
    read -p "Would you like to attempt to create the database if it doesn't exist? (y/n) " CREATE_DB
    if [ "$CREATE_DB" = "y" ]; then
        echo -e "\e[33mAttempting to create the database 'zarichney_identity'...\e[0m"
        psql -h localhost -U postgres -c "CREATE DATABASE zarichney_identity;" 2>&1
        echo -e "\e[32mDatabase creation attempted. You may now try running the migration script again.\e[0m"
    fi
    
    exit 1
else
    echo -e "\e[32mRefreshToken migration successfully applied.\e[0m"
    
    # Verify table was created
    echo -e "\e[33mVerifying RefreshTokens table was created...\e[0m"
    TABLES=$(psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t 2>&1)
    
    if [[ $TABLES == *"refreshtokens"* ]]; then
        echo -e "\e[32mRefreshTokens table was successfully created!\e[0m"
    else
        echo -e "\e[33mRefreshTokens table may not have been created correctly.\e[0m"
        echo -e "\e[33mPlease check the database manually.\e[0m"
    fi
fi