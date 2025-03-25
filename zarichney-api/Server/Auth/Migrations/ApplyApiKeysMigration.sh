#!/bin/bash

# This script applies the migration to add the ApiKeys table to the database
# It should be run from the root directory of the project

# Get the connection string from the environment variable
CONNECTION_STRING="${IDENTITY_CONNECTION_STRING}"

if [ -z "$CONNECTION_STRING" ]; then
    echo "Error: IDENTITY_CONNECTION_STRING environment variable is not set"
    exit 1
fi

# Extract host, database, username, and password from the connection string
HOST=$(echo $CONNECTION_STRING | grep -oP 'Host=\K[^;]+')
DATABASE=$(echo $CONNECTION_STRING | grep -oP 'Database=\K[^;]+')
USERNAME=$(echo $CONNECTION_STRING | grep -oP 'Username=\K[^;]+')
PASSWORD=$(echo $CONNECTION_STRING | grep -oP 'Password=\K[^;]+')

if [ -z "$HOST" ] || [ -z "$DATABASE" ] || [ -z "$USERNAME" ]; then
    echo "Error: Could not extract database connection details from connection string"
    exit 1
fi

# Define the migration SQL file path
MIGRATION_SQL="./zarichney-api/Server/Auth/Migrations/AddApiKeysTable.sql"

if [ ! -f "$MIGRATION_SQL" ]; then
    echo "Error: Migration SQL file not found at $MIGRATION_SQL"
    exit 1
fi

echo "Applying migration to add ApiKeys table to database $DATABASE on $HOST..."

# Run the migration SQL script
PGPASSWORD=$PASSWORD psql -h $HOST -d $DATABASE -U $USERNAME -f "$MIGRATION_SQL"

if [ $? -eq 0 ]; then
    echo "Migration applied successfully!"
else
    echo "Error: Failed to apply migration"
    exit 1
fi

echo "Done!"