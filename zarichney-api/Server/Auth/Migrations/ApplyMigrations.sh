#!/bin/bash
#
# ApplyMigrations.sh
# Executes the pre-generated idempotent SQL migration script using psql.

# --- Configuration ---
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-"5432"}
DB_NAME=${DB_NAME:-"zarichney_identity"}
DB_USER=${DB_USER:-"zarichney_user"} # Use appropriate production user
# PGPASSWORD MUST be set externally via environment variable (e.g., from Secrets Manager/SSM)

# Location where the generated idempotent SQL script is deployed on EC2
# Adjust path if your GitHub Action copies it elsewhere
EC2_GENERATED_MIGRATION_DIR="/opt/cookbook-api/migrations" # Directory containing the SQL file
MIGRATION_SQL_FILE="ApplyAllMigrations.sql"                # Name of the generated SQL file
MIGRATION_SQL_FULL_PATH="${EC2_GENERATED_MIGRATION_DIR}/${MIGRATION_SQL_FILE}"

# --- Helper Functions ---
execute_psql() {
    local db_to_use=${1:-$DB_NAME}
    shift
    # Check if PGPASSWORD is set before attempting
    if [ -z "$PGPASSWORD" ]; then
        echo -e "\e[31mError: PGPASSWORD is not set in the environment.\e[0m"
        return 1
    fi
    PGPASSWORD=$PGPASSWORD psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$db_to_use" "$@"
    return $? # Return the exit code of psql
}

# --- Pre-checks ---
echo -e "\e[36mApplying all migrations using SQL script via psql...\e[0m"
echo -e "\e[90mTarget DB: $DB_NAME on $DB_HOST:$DB_PORT as user $DB_USER\e[0m"
echo -e "\e[90mSQL Script: $MIGRATION_SQL_FULL_PATH\e[0m"

# Check PGPASSWORD (already checked in helper, but good to double-check)
if [ -z "$PGPASSWORD" ]; then
    echo -e "\e[31mError: PGPASSWORD environment variable is not set.\e[0m"
    exit 1
fi

# Check if SQL file exists
if [ ! -f "$MIGRATION_SQL_FULL_PATH" ]; then
    echo -e "\e[31mError: Migration SQL file '$MIGRATION_SQL_FULL_PATH' not found.\e[0m"
    echo -e "\e[33mEnsure it was generated and deployed correctly by the GitHub Action.\e[0m"
    exit 1
fi

# Check if psql is available
if ! command -v psql &> /dev/null; then
    echo -e "\e[31mError: psql command not found. Please install PostgreSQL client tools.\e[0m"
    exit 1
fi

# Check database existence (optional but good practice)
echo -e "\e[33mChecking if database '$DB_NAME' exists...\e[0m"
if ! execute_psql postgres -lqt | cut -d \| -f 1 | grep -qw "$DB_NAME"; then
    echo -e "\e[31mDatabase '$DB_NAME' does not exist on $DB_HOST:$DB_PORT.\e[0m"
    # You might want to automatically create it in production, or fail if it's missing
    echo -e "\e[33mAttempting to create database '$DB_NAME'...\e[0m"
    if PGPASSWORD=$PGPASSWORD createdb -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" "$DB_NAME"; then
         echo -e "\e[32mDatabase '$DB_NAME' created successfully.\e[0m"
    else
         echo -e "\e[31mFailed to create database '$DB_NAME'. Exiting.\e[0m"
         exit 1
    fi
else
    echo -e "\e[32mDatabase '$DB_NAME' found.\e[0m"
fi

# --- Apply Migration SQL Script ---
echo -e "\e[33mApplying migration SQL script '$MIGRATION_SQL_FILE'...\e[0m"
# Use ON_ERROR_STOP to ensure the script halts on any SQL error
# Execute command targeting the specific database
if execute_psql $DB_NAME -v ON_ERROR_STOP=1 -f "$MIGRATION_SQL_FULL_PATH"; then
    echo -e "\e[32mMigration script executed successfully.\e[0m"
else
    # psql returns non-zero on error when ON_ERROR_STOP is set
    echo -e "\e[31mError: Failed to apply migration script '$MIGRATION_SQL_FILE'. Check psql output for details.\e[0m"
    exit 1
fi

# --- Verification (Optional but Recommended) ---
echo -e "\e[33mVerifying essential tables exist...\e[0m"
TABLES_OK=true
# Add any other crucial tables created by your migrations here
for table in "AspNetUsers" "AspNetRoles" "__EFMigrationsHistory" "RefreshTokens" "ApiKeys"; do
    # Execute verification targeting the specific database
    TABLE_CHECK=$(execute_psql $DB_NAME -tAc "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '$table');")
    if [ "$TABLE_CHECK" != "t" ]; then
        echo -e "\e[31mVerification failed: Table '$table' does not exist.\e[0m"
        TABLES_OK=false
    else
         echo -e "\e[90mVerified table: $table\e[0m"
    fi
done

if $TABLES_OK; then
    echo -e "\e[32mVerification successful: Essential tables found.\e[0m"
else
    echo -e "\e[31mVerification failed: One or more essential tables missing.\e[0m"
    # exit 1 # Make this fatal if necessary
fi

echo -e "\e[36mApplyMigrations.sh script completed.\e[0m"
exit 0