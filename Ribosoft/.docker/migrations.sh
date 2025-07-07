#!/bin/bash
# Set environment variables for .NET
export DOTNET_CLI_HOME=/app
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
export DOTNET_NOLOGO=true
export HOME=/app
export PATH="$PATH:/app/.dotnet/tools"

run_cmd="dotnet Ribosoft.dll --urls http://*:80"

# Wait for database to be ready
echo "Waiting for database to be ready..."
until pg_isready -h db -p 5432 -U postgres; do
    echo "Database is unavailable - sleeping"
    sleep 1
done
echo "Database is ready!"

# Run database migrations using EF tools
echo "Attempting to run database migrations..."

# Check if dotnet ef is available
if dotnet ef --version >/dev/null 2>&1; then
    echo "Running EF migrations for NpgsqlDbContext..."
    # Run migrations for the Npgsql context
    if dotnet ef database update --connection "$ConnectionStrings__NpgsqlConnection" --verbose; then
        echo "Migrations completed successfully"
    else
        echo "EF migrations failed, but continuing to start application..."
        echo "Note: Application may handle migrations on startup"
    fi
else
    echo "dotnet ef tool not found, skipping migrations..."
    echo "Note: Application will need to handle migrations on startup"
fi

echo "Starting Ribosoft application..."
exec $run_cmd