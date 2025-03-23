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

# Change to the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Display current info
echo -e "\e[36mApplying RefreshToken migration for ASP.NET Identity in PostgreSQL...\e[0m"
echo -e "\e[90mCurrent directory: $(pwd)\e[0m"

# First, ensure the project is properly built
echo -e "\e[33mBuilding project...\e[0m"
dotnet build
if [ $? -ne 0 ]; then
    echo -e "\e[31mBuild failed\e[0m"
    exit 1
fi

# Apply the migration
echo -e "\e[33mRunning: dotnet ef database update AddRefreshTokenSchema --context UserDbContext --project .\e[0m"
dotnet ef database update AddRefreshTokenSchema --context UserDbContext --project .

# Check if the migration was successful
if [ $? -ne 0 ]; then
    echo -e "\e[31mMigration failed\e[0m"
    echo -e "\e[33mCheck that:\e[0m"
    echo -e "\e[33m1. PostgreSQL is running\e[0m"
    echo -e "\e[33m2. Connection string in appsettings.json is correct\e[0m"
    echo -e "\e[33m3. Database exists (create it manually if needed)\e[0m"
    echo -e "\e[33m4. EF Core tools are installed (run: dotnet tool install --global dotnet-ef)\e[0m"
    
    exit 1
else
    echo -e "\e[32mRefreshToken migration successfully applied.\e[0m"
    
    # Verify table was created
    echo -e "\e[33mVerifying RefreshTokens table was created...\e[0m"
    TABLES=$(psql -h localhost -U postgres -d zarichney_identity -c '\dt' -t)
    
    if [[ $TABLES == *"refreshtokens"* ]]; then
        echo -e "\e[32mRefreshTokens table was successfully created!\e[0m"
    else
        echo -e "\e[33mRefreshTokens table may not have been created correctly.\e[0m"
        echo -e "\e[33mPlease check the database manually.\e[0m"
    fi
fi