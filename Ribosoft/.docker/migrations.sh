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

# Try to run migrations using the application itself
# Many ASP.NET Core apps support this pattern
echo "Attempting to run database migrations..."

# First, try to see if the application has a migrate command or similar
if dotnet Ribosoft.dll --migrate 2>/dev/null; then
    echo "Migrations completed successfully using --migrate flag"
elif dotnet Ribosoft.dll migrate 2>/dev/null; then
    echo "Migrations completed successfully using migrate command"
else
    echo "No built-in migration command found, trying EF tools..."
    # Fallback to EF tools if available
    if command -v dotnet-ef >/dev/null 2>&1; then
        # Try with the DLL directly
        if dotnet ef database update --assembly Ribosoft.dll --startup-assembly Ribosoft.dll --context NpgsqlDbContext --verbose 2>/dev/null; then
            echo "Migrations completed using EF tools"
        else
            echo "EF tools migration failed, starting application anyway..."
            echo "Note: Application will need to handle migrations on startup"
        fi
    else
        echo "EF tools not available, starting application..."
        echo "Note: Application will need to handle migrations on startup"
    fi
fi

echo "Starting Ribosoft application..."
exec $run_cmd