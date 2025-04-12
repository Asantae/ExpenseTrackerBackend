#!/bin/sh
set -e

DB_FILE="/home/asantae/expense_tracker.db"

# Check if the database file exists; if not, initialize it.
if [ ! -f "$DB_FILE" ]; then
    echo "Database file not found. Initializing database schema..."
    # Run the init.sql script to create the schema.
    sqlite3 $DB_FILE < /app/init.sql
else
    echo "Database file found; skipping initialization."
fi

# Execute the dotnet application.
exec dotnet ExpenseTrackerBackend.dll
