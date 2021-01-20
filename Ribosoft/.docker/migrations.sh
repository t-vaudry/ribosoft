#!/bin/bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools/"
run_cmd="dotnet run --configuration Release --urls http://*:80"
# test the db connection
dotnet restore
until dotnet-ef database update -c $DB_CONTEXT --configuration Release; do
	>&2 echo "Waiting for database..."
	sleep 1
done
>&2 echo "Database is started - migrations applied..."
exec $run_cmd