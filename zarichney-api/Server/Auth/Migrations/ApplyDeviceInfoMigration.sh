#!/bin/bash
#
# ApplyDeviceInfoMigration.sh
# This script applies the migration to add device info columns to the RefreshTokens table.

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
MIGRATION_SQL="${SCRIPT_DIR}/ApplyDeviceInfoMigration.sql" # SQL file relative to script

# --- Helper Functions ---
execute_psql() {
    PGPASSWORD=$PGPASSWORD psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" "$@"
}

# --- Pre-checks ---
echo -e "\e[36mApplying migration to add device info to RefreshTokens...\e[0m"
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
    echo -e "\e[33mThis script requires the database and the RefreshTokens table to exist already.\e[0m"
    exit 1
else
    echo -e "\e[32mDatabase '$DB_NAME' found.\e[0m"
fi

# Check if RefreshTokens table exists before attempting to alter it
echo -e "\e[33mChecking if 'RefreshTokens' table exists...\e[0m"
TABLE_CHECK=$(execute_psql -tAc "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'RefreshTokens');")
if [ "$TABLE_CHECK" != "t" ]; then
    echo -e "\e[31mError: 'RefreshTokens' table does not exist in database '$DB_NAME'. Cannot apply device info migration.\e[0m"
    exit 1
fi
echo -e "\e[32m'RefreshTokens' table found.\e[0m"

# --- Apply Migration ---
echo -e "\e[33mApplying migration SQL script...\e[0m"
if execute_psql -v ON_ERROR_STOP=1 -f "$MIGRATION_SQL"; then
    echo -e "\e[32mMigration script executed successfully.\e[0m"
else
    echo -e "\e[31mError: Failed to apply migration script '$MIGRATION_SQL'.\e[0m"
    exit 1
fi

# --- Verification ---
echo -e "\e[33mVerifying column additions to 'RefreshTokens' table...\e[0m"
COLUMNS_OK=true
for col in "DeviceName" "DeviceIp" "UserAgent" "LastUsedAt"; do
    COL_CHECK=$(execute_psql -tAc "SELECT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'public' AND table_name = 'RefreshTokens' AND column_name = '$col');")
    if [ "$COL_CHECK" != "t" ]; then
        echo -e "\e[31mVerification failed: Column '$col' does not exist in 'RefreshTokens' table after migration.\e[0m"
        COLUMNS_OK=false
    else
        echo -e "\e[90mVerified column: $col\e[0m"
    fi
done

if $COLUMNS_OK; then
    echo -e "\e[32mVerification successful: All device info columns exist.\e[0m"
else
    echo -e "\e[31mVerification failed: One or more columns missing.\e[0m"
    exit 1
fi

echo -e "\e[36mDevice Info migration completed successfully!\e[0m"
exit 0