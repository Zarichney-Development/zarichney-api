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

# Change to the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR"

# Display current info
echo -e "\e[36mApplying migrations for ASP.NET Identity in PostgreSQL...\e[0m"
echo -e "\e[37mCurrent directory: $SCRIPT_DIR\e[0m"

# First, ensure the project is properly built
echo -e "\e[33mBuilding project...\e[0m"
dotnet build
if [ $? -ne 0 ]; then
    echo -e "\e[31mBuild failed!\e[0m"
    exit 1
fi

# Verify if the EF Migrations History table exists and has any entries
echo -e "\e[33mChecking migration history...\e[0m"
MIGRATION_CHECK=$(psql -h localhost -U postgres -d zarichney_identity -c 'SELECT * FROM "__EFMigrationsHistory"' -t 2>&1)
if [[ $MIGRATION_CHECK == *"ERROR"* ]]; then
    echo -e "\e[33mMigration history table not found or is empty. Forcing initial migration...\e[0m"
    
    # Create the migration history table first if needed
    psql -h localhost -U postgres -d zarichney_identity -c 'CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" ("MigrationId" character varying(150) NOT NULL, "ProductVersion" character varying(32) NOT NULL, CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"))'
    
    # Force the migration through EF Core
    echo -e "\e[33mRunning: dotnet ef database update 20250322000001_AddIdentitySchema --context UserDbContext --project . --verbose\e[0m"
    dotnet ef database update 20250322000001_AddIdentitySchema --context UserDbContext --project . --verbose
else
    # Regular update if history table exists
    echo -e "\e[33mRunning: dotnet ef database update --context UserDbContext --project .\e[0m"
    dotnet ef database update --context UserDbContext --project .
fi

# Check if the migration was successful
if [ $? -ne 0 ]; then
    echo -e "\e[31mMigration failed!\e[0m"
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
        psql -h localhost -U postgres -d zarichney_identity -f ApplyMigration.sql
        
        # Check if SQL script was successful
        TABLES=$(psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t)
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
    TABLES=$(psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t)
    
    if [[ $TABLES == *"AspNetUsers"* ]]; then
        echo -e "\e[32mASP.NET Identity tables were successfully created!\e[0m"
    else
        echo -e "\e[33mASP.NET Identity tables may not have been created correctly.\e[0m"
        echo -e "\e[33mPlease check the database manually.\e[0m"
        
        # Offer to run the SQL script if the tables weren't found
        read -p "Would you like to run the SQL script to ensure tables are created? (y/n) " RUN_SQL_FILE
        if [ "$RUN_SQL_FILE" = "y" ]; then
            echo -e "\e[33mRunning SQL script to create tables...\e[0m"
            psql -h localhost -U postgres -d zarichney_identity -f ApplyMigration.sql
        fi
    fi
fi