#!/bin/bash
dotnet tool install --global dotnet-ef
export PATH="$PATH:$HOME/.dotnet/tools/"
run_cmd="dotnet run --configuration Release --urls http://*:80"
# test the db connection
dotnet restore
until dotnet-ef database update -c NpgsqlDbContext --configuration Release; do
	>&2 echo "DB is starting up"
	sleep 1
done
>&2 echo "DB is up - executing command"
exec $run_cmd