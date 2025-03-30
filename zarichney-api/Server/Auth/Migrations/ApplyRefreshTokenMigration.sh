#!/bin/bash
#
# ApplyRefreshTokenMigration.sh
# This script applies the specific AddRefreshTokenSchema migration.

# --- Configuration ---
# Database connection parameters (used for psql checks/fallbacks)
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-"5432"}
DB_NAME=${DB_NAME:-"zarichney_identity"}
DB_USER=${DB_USER:-"zarichney_user"}
# PGPASSWORD must be set externally for security.
# The `dotnet ef database update` command primarily uses the connection
# string from appsettings.Production.json or environment variables.
# Ensure the connection details used by `dotnet ef` match the DB_* vars above if using psql checks.
# Example using AWS Secrets Manager:
#   export PGPASSWORD=$(aws secretsmanager get-secret-value --secret-id your-secret-arn --query SecretString --output text)
# Example using AWS SSM Parameter Store (SecureString):
#   export PGPASSWORD=$(aws ssm get-parameter --name /your/db/password --with-decryption --query Parameter.Value --output text)

# Determine directories relative to the script location
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Adjust this relative path if your project structure is different
PROJECT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)" # Assumes Migrations is in Server/Auth/Migrations

# EF Core Context Name and Target Migration Name Fragment
DB_CONTEXT="UserDbContext"
TARGET_MIGRATION_FRAGMENT="_AddRefreshTokenSchema" # Used to find the specific migration

# --- Helper Functions ---
execute_psql() {
    # Use -d postgres for checks that don't require the target DB yet
    local db_to_use=${1:-$DB_NAME}
    shift
    PGPASSWORD=$PGPASSWORD psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$db_to_use" "$@"
}

# --- Pre-checks ---
echo -e "\e[36mApplying EF Core migration for Refresh Tokens ($TARGET_MIGRATION_FRAGMENT)...\e[0m"
echo -e "\e[90mProject directory: $PROJECT_DIR\e[0m"
echo -e "\e[90mUsing DB (for checks): $DB_NAME on $DB_HOST:$DB_PORT as user $DB_USER\e[0m"

# Check if PGPASSWORD is set (needed for psql checks)
if [ -z "$PGPASSWORD" ]; then
    echo -e "\e[31mWarning: PGPASSWORD environment variable is not set.\e[0m"
    echo -e "\e[33mVerification steps using psql might fail without it.\e[0m"
    # Allow script to continue as `dotnet ef` might use connection string from elsewhere.
fi

# Check if project directory exists
if [ ! -d "$PROJECT_DIR" ]; then
    echo -e "\e[31mError: Project directory not found at '$PROJECT_DIR'. Adjust PROJECT_DIR variable if needed.\e[0m"
    exit 1
fi

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo -e "\e[31mError: dotnet command not found. Please install the .NET SDK.\e[0m"
    exit 1
fi

# Check if psql is available (for checks)
if ! command -v psql &> /dev/null; then
    echo -e "\e[31mWarning: psql command not found. Verification steps cannot be performed.\e[0m"
fi

# --- Main Logic ---
# Change to the project directory
cd "$PROJECT_DIR"
echo -e "\e[90mCurrent working directory: $(pwd)\e[0m"

# Build the project first
echo -e "\e[33mBuilding project...\e[0m"
dotnet build --nologo -v q
if [ $? -ne 0 ]; then
    echo -e "\e[31mBuild failed!\e[0m"
    exit 1
fi
echo -e "\e[32mBuild successful.\e[0m"

# Check available migrations and find the target one
echo -e "\e[33mChecking available migrations for context '$DB_CONTEXT'...\e[0m"
MIGRATIONS_LIST=$(dotnet ef migrations list --context $DB_CONTEXT --prefix-output --no-build)
if [ $? -ne 0 ]; then
    echo -e "\e[31mError checking migrations. Ensure EF Core tools are installed and context '$DB_CONTEXT' is correct.\e[0m"
    exit 1
fi
echo -e "\e[90mAvailable migrations:\n$MIGRATIONS_LIST\e[0m"

TIMESTAMPED_MIGRATION=$(echo "$MIGRATIONS_LIST" | grep "$TARGET_MIGRATION_FRAGMENT" | head -1)

if [ -z "$TIMESTAMPED_MIGRATION" ]; then
    echo -e "\e[31mError: Target migration containing '$TARGET_MIGRATION_FRAGMENT' not found.\e[0m"
    echo -e "\e[33mYou may need to create it first: dotnet ef migrations add AddRefreshTokenSchema --context $DB_CONTEXT\e[0m"
    # Depending on workflow, you might offer to create it here or just exit.
    exit 1
fi

echo -e "\e[32mFound target migration: $TIMESTAMPED_MIGRATION\e[0m"

# Check if the migration is already applied (optional but good practice)
if command -v psql &> /dev/null && [ -n "$PGPASSWORD" ]; then
     APPLIED_CHECK=$(execute_psql -tAc "SELECT EXISTS (SELECT 1 FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '$TIMESTAMPED_MIGRATION');")
     if [ "$APPLIED_CHECK" = "t" ]; then
         echo -e "\e[32mMigration '$TIMESTAMPED_MIGRATION' is already applied according to history table. Skipping.\e[0m"
         exit 0 # Exit successfully as it's already done
     else
         echo -e "\e[33mMigration '$TIMESTAMPED_MIGRATION' not found in history table. Proceeding to apply...\e[0m"
     fi
else
    echo -e "\e[33mSkipping check for already applied migration (psql missing or PGPASSWORD not set).\e[0m"
fi


# Apply the specific migration
echo -e "\e[33mRunning: dotnet ef database update \"$TIMESTAMPED_MIGRATION\" --context $DB_CONTEXT --verbose\e[0m"
MIGRATION_OUTPUT=$(dotnet ef database update "$TIMESTAMPED_MIGRATION" --context $DB_CONTEXT --verbose --no-build 2>&1)
MIGRATION_STATUS=$?

# Display the output for debugging
echo -e "\e[90mMigration command output:\n$MIGRATION_OUTPUT\e[0m"

# Check migration status
if [ $MIGRATION_STATUS -ne 0 ]; then
    echo -e "\e[31mMigration failed with status code $MIGRATION_STATUS\e[0m"
    echo -e "\e[33mReview the output above for errors.\e[0m"
    exit 1
fi

echo -e "\e[32mMigration '$TIMESTAMPED_MIGRATION' applied successfully.\e[0m"

# --- Verification (only if psql available and password was set) ---
if command -v psql &> /dev/null && [ -n "$PGPASSWORD" ]; then
    echo -e "\e[33mVerifying 'RefreshTokens' table exists...\e[0m"
    TABLE_CHECK=$(execute_psql -tAc "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'RefreshTokens');")

    if [ "$TABLE_CHECK" = "t" ]; then
        echo -e "\e[32mVerification successful: 'RefreshTokens' table exists.\e[0m"
    else
        echo -e "\e[31mVerification failed: 'RefreshTokens' table does not exist after migration.\e[0m"
        # Decide if this should be a fatal error
        # exit 1
    fi
else
    echo -e "\e[33mSkipping table verification (psql missing or PGPASSWORD not set).\e[0m"
fi

echo -e "\e[36mRefresh Token migration process completed.\e[0m"
exit 0