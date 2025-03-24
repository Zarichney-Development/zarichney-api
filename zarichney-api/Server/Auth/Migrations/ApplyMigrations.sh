#!/bin/bash
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
# chmod +x ApplyMigrations.sh
# ./ApplyMigrations.sh

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
# Navigate to project root (assuming it's 3 levels up from the Migrations directory)
PROJECT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)"

# Display current info
echo -e "\e[36mApplying migrations for ASP.NET Identity in PostgreSQL...\e[0m"
echo -e "\e[37mScript directory: $SCRIPT_DIR\e[0m"
echo -e "\e[37mProject directory: $PROJECT_DIR\e[0m"

# Change to the project directory
cd "$PROJECT_DIR"
echo -e "\e[37mCurrent working directory: $(pwd)\e[0m"

# First, ensure the project is properly built
echo -e "\e[33mBuilding project...\e[0m"
dotnet build
if [ $? -ne 0 ]; then
    echo -e "\e[31mBuild failed!\e[0m"
    exit 1
fi

# Check available migrations
echo -e "\e[33mChecking available migrations...\e[0m"
MIGRATIONS=$(dotnet ef migrations list --context UserDbContext)
echo -e "\e[90mAvailable migrations:\e[0m"
echo "$MIGRATIONS"

# Verify if the EF Migrations History table exists and has any entries
echo -e "\e[33mChecking migration history...\e[0m"
MIGRATION_CHECK=$(psql -h localhost -U postgres -d zarichney_identity -c 'SELECT * FROM "__EFMigrationsHistory"' -t 2>&1)
if [[ $MIGRATION_CHECK == *"ERROR"* ]]; then
    echo -e "\e[33mMigration history table not found or is empty. Forcing initial migration...\e[0m"
    
    # Create the migration history table first if needed
    psql -h localhost -U postgres -d zarichney_identity -c 'CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" ("MigrationId" character varying(150) NOT NULL, "ProductVersion" character varying(32) NOT NULL, CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"))'
    
    # Extract the initial migration name without any (Pending) suffix
    INITIAL_MIGRATION=$(echo "$MIGRATIONS" | grep -o "[0-9]*_AddIdentitySchema" | head -1)
    if [ -z "$INITIAL_MIGRATION" ]; then
        INITIAL_MIGRATION="20250322000001_AddIdentitySchema"
    fi
    
    # Force the migration through EF Core
    echo -e "\e[33mRunning: dotnet ef database update $INITIAL_MIGRATION --context UserDbContext --verbose\e[0m"
    MIGRATION_OUTPUT=$(dotnet ef database update $INITIAL_MIGRATION --context UserDbContext --verbose 2>&1)
    MIGRATION_STATUS=$?
else
    # Regular update if history table exists
    echo -e "\e[33mRunning: dotnet ef database update --context UserDbContext --verbose\e[0m"
    MIGRATION_OUTPUT=$(dotnet ef database update --context UserDbContext --verbose 2>&1)
    MIGRATION_STATUS=$?
fi

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
    
    # Manual fallback using SQL script
    echo -e "\e[33mAttempting a manual fallback using the SQL script...\e[0m"
    echo -e "\e[33mYou can try running the provided SQL script directly:\e[0m"
    echo -e "\e[37mpsql -h localhost -U postgres -d zarichney_identity -f ApplyMigration.sql\e[0m"
    
    read -p "Would you like to run the SQL script now? (y/n) " RUN_SQL_FILE
    if [ "$RUN_SQL_FILE" = "y" ]; then
        echo -e "\e[33mRunning SQL script to create tables...\e[0m"
        psql -h localhost -U postgres -d zarichney_identity -f "$SCRIPT_DIR/ApplyMigration.sql"
        
        # Check if SQL script was successful
        TABLES=$(psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t 2>&1)
        if [[ $TABLES == *"AspNetUsers"* ]]; then
            echo -e "\e[32mSQL script successfully created ASP.NET Identity tables!\e[0m"
        else
            echo -e "\e[31mSQL script execution failed or didn't create the expected tables.\e[0m"
        fi
    fi
    
    exit 1
else
    echo -e "\e[32mMigrations successfully applied.\e[0m"
    
    # Verify tables were created
    echo -e "\e[33mVerifying tables were created...\e[0m"
    TABLES=$(psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t 2>&1)
    
    if [[ $TABLES == *"AspNetUsers"* ]]; then
        echo -e "\e[32mASP.NET Identity tables were successfully created!\e[0m"
        
        # Check for RefreshTokens table if it exists in migrations
        if [[ $MIGRATIONS == *"AddRefreshTokenSchema"* ]] && [[ $TABLES == *"refreshtokens"* ]]; then
            echo -e "\e[32mRefreshTokens table was also successfully created!\e[0m"
        elif [[ $MIGRATIONS == *"AddRefreshTokenSchema"* ]]; then
            echo -e "\e[33mRefreshTokens table was not found, but the migration exists.\e[0m"
            echo -e "\e[33mYou may need to run the refresh token migration separately.\e[0m"
        fi
    else
        echo -e "\e[33mASP.NET Identity tables may not have been created correctly.\e[0m"
        echo -e "\e[33mPlease check the database manually.\e[0m"
        
        # Offer to run the SQL script if the tables weren't found
        read -p "Would you like to run the SQL script to ensure tables are created? (y/n) " RUN_SQL_FILE
        if [ "$RUN_SQL_FILE" = "y" ]; then
            echo -e "\e[33mRunning SQL script to create tables...\e[0m"
            psql -h localhost -U postgres -d zarichney_identity -f "$SCRIPT_DIR/ApplyMigration.sql"
        fi
    fi
fi