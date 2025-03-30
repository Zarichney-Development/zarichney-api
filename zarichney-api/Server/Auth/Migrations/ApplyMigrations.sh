#!/bin/bash
#
# ApplyMigrations.sh
# This script applies all pending Entity Framework Core migrations for the UserDbContext.

# --- Configuration ---
# Database connection parameters (used for psql checks/fallbacks)
DB_HOST=${DB_HOST:-"localhost"}
DB_PORT=${DB_PORT:-"5432"}
DB_NAME=${DB_NAME:-"zarichney_identity"}
DB_USER=${DB_USER:-"zarichney_user"}
# PGPASSWORD must be set externally for security.
# The `dotnet ef database update` command primarily uses the connection
# string from appsettings.Production.json or environment variables like
# ConnectionStrings__DefaultConnection or IDENTITY_CONNECTION_STRING.
# Ensure the connection details used by `dotnet ef` match the DB_* vars above if using psql fallbacks.
# Example using AWS Secrets Manager:
#   export PGPASSWORD=$(aws secretsmanager get-secret-value --secret-id your-secret-arn --query SecretString --output text)
# Example using AWS SSM Parameter Store (SecureString):
#   export PGPASSWORD=$(aws ssm get-parameter --name /your/db/password --with-decryption --query Parameter.Value --output text)

# Determine directories relative to the script location
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# Adjust this relative path if your project structure is different
PROJECT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)" # Assumes Migrations is in Server/Auth/Migrations
SQL_FALLBACK_SCRIPT="${SCRIPT_DIR}/ApplyMigration.sql" # Fallback SQL relative to script

# EF Core Context Name
DB_CONTEXT="UserDbContext"

# --- Helper Functions ---
execute_psql() {
    # Use -d postgres for checks that don't require the target DB yet
    local db_to_use=${1:-$DB_NAME}
    shift
    PGPASSWORD=$PGPASSWORD psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$db_to_use" "$@"
}

# --- Pre-checks ---
echo -e "\e[36mApplying EF Core migrations for context '$DB_CONTEXT'...\e[0m"
echo -e "\e[90mProject directory: $PROJECT_DIR\e[0m"
echo -e "\e[90mUsing DB (for checks/fallback): $DB_NAME on $DB_HOST:$DB_PORT as user $DB_USER\e[0m"

# Check if PGPASSWORD is set (needed for psql checks/fallback)
if [ -z "$PGPASSWORD" ]; then
    echo -e "\e[31mWarning: PGPASSWORD environment variable is not set.\e[0m"
    echo -e "\e[33mVerification steps and SQL fallback using psql might fail without it.\e[0m"
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

# Check if psql is available (for checks/fallback)
if ! command -v psql &> /dev/null; then
    echo -e "\e[31mWarning: psql command not found. Verification steps and SQL fallback cannot be performed.\e[0m"
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

# Check available migrations
echo -e "\e[33mChecking available migrations for context '$DB_CONTEXT'...\e[0m"
MIGRATIONS_LIST=$(dotnet ef migrations list --context $DB_CONTEXT --prefix-output --no-build)
if [ $? -ne 0 ]; then
    echo -e "\e[31mError checking migrations. Ensure EF Core tools are installed ('dotnet tool install --global dotnet-ef') and context '$DB_CONTEXT' is correct.\e[0m"
    exit 1
fi
echo -e "\e[90mAvailable migrations:\n$MIGRATIONS_LIST\e[0m"

# Apply migrations using dotnet ef
echo -e "\e[33mRunning: dotnet ef database update --context $DB_CONTEXT --verbose\e[0m"
MIGRATION_OUTPUT=$(dotnet ef database update --context $DB_CONTEXT --verbose --no-build 2>&1)
MIGRATION_STATUS=$?

# Display the output for debugging
echo -e "\e[90mMigration command output:\n$MIGRATION_OUTPUT\e[0m"

# Check migration status
if [ $MIGRATION_STATUS -ne 0 ]; then
    echo -e "\e[31mMigration failed with status code $MIGRATION_STATUS\e[0m"
    echo -e "\e[33mReview the output above for errors.\e[0m"

    # Offer SQL script fallback only if psql is available and password was set
    if command -v psql &> /dev/null && [ -n "$PGPASSWORD" ] && [ -f "$SQL_FALLBACK_SCRIPT" ]; then
        echo -e "\e[33mAttempting a manual fallback using the SQL script...\e[0m"
        read -p "Would you like to run the fallback SQL script '$SQL_FALLBACK_SCRIPT'? (y/n) " RUN_SQL_FILE
        if [ "$RUN_SQL_FILE" = "y" ]; then
            echo -e "\e[33mRunning SQL script to create tables...\e[0m"
            if execute_psql -v ON_ERROR_STOP=1 -f "$SQL_FALLBACK_SCRIPT"; then
                echo -e "\e[32mSQL fallback script executed successfully.\e[0m"
                # Re-run verification after fallback
            else
                echo -e "\e[31mSQL fallback script execution failed.\e[0m"
                exit 1 # Exit after failed fallback
            fi
        else
             echo -e "\e[33mSkipping SQL fallback.\e[0m"
             exit 1 # Exit if migration failed and fallback skipped
        fi
    elif [ ! -f "$SQL_FALLBACK_SCRIPT" ]; then
         echo -e "\e[33mSQL fallback script '$SQL_FALLBACK_SCRIPT' not found. Cannot attempt fallback.\e[0m"
         exit 1
    else
         echo -e "\e[33mSQL fallback not possible (psql missing or PGPASSWORD not set).\e[0m"
         exit 1
    fi
fi

echo -e "\e[32mMigrations applied or no pending migrations found.\e[0m"

# --- Verification (only if psql available and password was set) ---
if command -v psql &> /dev/null && [ -n "$PGPASSWORD" ]; then
    echo -e "\e[33mVerifying essential ASP.NET Identity tables exist...\e[0m"
    TABLES_OK=true
    for table in "AspNetUsers" "AspNetRoles" "__EFMigrationsHistory"; do
        TABLE_CHECK=$(execute_psql -tAc "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '$table');")
        if [ "$TABLE_CHECK" != "t" ]; then
            echo -e "\e[31mVerification failed: Essential table '$table' does not exist.\e[0m"
            TABLES_OK=false
        else
             echo -e "\e[90mVerified table: $table\e[0m"
        fi
    done

    if $TABLES_OK; then
        echo -e "\e[32mVerification successful: Essential tables found.\e[0m"
    else
        echo -e "\e[31mVerification failed: One or more essential tables missing.\e[0m"
        # Decide if this should be a fatal error
        # exit 1
    fi
else
    echo -e "\e[33mSkipping table verification (psql missing or PGPASSWORD not set).\e[0m"
fi


echo -e "\e[36mEF Core migration process completed.\e[0m"
exit 0 # Exit successfully even if verification failed, unless specified otherwise