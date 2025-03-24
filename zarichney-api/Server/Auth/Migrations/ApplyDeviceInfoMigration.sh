#!/bin/bash

# Database connection parameters
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
DB_NAME=${DB_NAME:-zarichney_identity}
DB_USER=${DB_USER:-postgres}

# Script location
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIGRATION_SQL="${SCRIPT_DIR}/ApplyDeviceInfoMigration.sql"

# Check if PGPASSWORD is set
if [ -z "$PGPASSWORD" ]; then
    echo "Error: PGPASSWORD environment variable is not set."
    echo "Please set it to the PostgreSQL password before running this script."
    exit 1
fi

# Check if the SQL file exists
if [ ! -f "$MIGRATION_SQL" ]; then
    echo "Error: Migration SQL file not found: $MIGRATION_SQL"
    exit 1
fi

echo "Applying migration to add device info to refresh tokens..."
psql -h $DB_HOST -p $DB_PORT -U $DB_USER -d $DB_NAME -f "$MIGRATION_SQL"

if [ $? -eq 0 ]; then
    echo "Migration applied successfully!"
else
    echo "Error applying migration."
    exit 1
fi