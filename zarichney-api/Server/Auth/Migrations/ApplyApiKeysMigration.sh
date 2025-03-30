#!/bin/bash
#
# ApplyApiKeysMigration.sh
# This script applies the migration to add the ApiKeys table to the database.

# --- Configuration ---
# Database connection parameters (use environment variables with defaults)
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-"5432"}
DB_NAME=${DB_NAME:-"zarichney_identity"}
DB_USER=${DB_USER:-"zarichney_user"}
# PGPASSWORD must be set externally for security.
# Example using AWS Secrets Manager:
#   export PGPASSWORD=$(aws secretsmanager get-secret-value --secret-id your-secret-arn --query SecretString --output text)
# Example using AWS SSM Parameter Store (SecureString):
#   export PGPASSWORD=$(aws ssm get-parameter --name /your/db/password --with-decryption --query Parameter.Value --output text)

# Get the script's directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIGRATION_SQL="${SCRIPT_DIR}/AddApiKeysTable.sql" # SQL file relative to script

# --- Helper Functions ---
execute_psql() {
    PGPASSWORD=$PGPASSWORD psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" "$@"
}

# --- Pre-checks ---
echo -e "\e[36mApplying migration to add ApiKeys table...\e[0m"
echo -e "\e[90mUsing DB: $DB_NAME on $DB_HOST:$DB_PORT as user $DB_USER\e[0m"
echo -e "\e[90mSQL Script: $MIGRATION_SQL\e[0m"

# Check if PGPASSWORD is set
if [ -z "$PGPASSWORD" ]; then
    echo -e "\e[31mError: PGPASSWORD environment variable is not set.\e[0m"
    echo -e "\e[33mPlease set it securely before running this script (see comments in script).\e[0m"
    exit 1
fi

# Check if SQL file exists
if [ ! -f "$MIGRATION_SQL" ]; then
    echo -e "\e[31mError: Migration SQL file not found at $MIGRATION_SQL\e[0m"
    exit 1
fi

# Check if psql is available
if ! command -v psql &> /dev/null; then
    echo -e "\e[31mError: psql command not found. Please install PostgreSQL client tools.\e[0m"
    exit 1
fi

# Check database existence (connect to 'postgres' or 'template1' db to check)
echo -e "\e[33mChecking if database '$DB_NAME' exists...\e[0m"
if ! PGPASSWORD=$PGPASSWORD psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d postgres -lqt | cut -d \| -f 1 | grep -qw "$DB_NAME"; then
    echo -e "\e[31mDatabase '$DB_NAME' does not exist on $DB_HOST:$DB_PORT.\e[0m"
    # In a production script, you might want to exit here or have specific logic.
    # For this example, we'll prompt (though automatic creation might be preferred in CICD).
    read -p "Database not found. Would you like to attempt to create it? (y/n) " CREATE_DB
    if [ "$CREATE_DB" = "y" ]; then
        echo -e "\e[33mAttempting to create database '$DB_NAME'...\e[0m"
        if PGPASSWORD=$PGPASSWORD createdb -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" "$DB_NAME"; then
            echo -e "\e[32mDatabase '$DB_NAME' created successfully.\e[0m"
        else
            echo -e "\e[31mFailed to create database '$DB_NAME'. Please check permissions or create it manually.\e[0m"
            exit 1
        fi
    else
        echo -e "\e[33mExiting script as database does not exist.\e[0m"
        exit 1
    fi
else
    echo -e "\e[32mDatabase '$DB_NAME' found.\e[0m"
fi

# --- Apply Migration ---
echo -e "\e[33mApplying migration SQL script...\e[0m"
if execute_psql -v ON_ERROR_STOP=1 -f "$MIGRATION_SQL"; then
    echo -e "\e[32mMigration script executed successfully.\e[0m"
else
    echo -e "\e[31mError: Failed to apply migration script '$MIGRATION_SQL'.\e[0m"
    exit 1
fi

# --- Verification ---
echo -e "\e[33mVerifying 'ApiKeys' table creation...\e[0m"
TABLE_CHECK=$(execute_psql -tAc "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'ApiKeys');")

if [ "$TABLE_CHECK" = "t" ]; then
    echo -e "\e[32mVerification successful: 'ApiKeys' table exists.\e[0m"
else
    echo -e "\e[31mVerification failed: 'ApiKeys' table does not exist after migration.\e[0m"
    exit 1
fi

echo -e "\e[36mApiKeys migration completed successfully!\e[0m"
exit 0