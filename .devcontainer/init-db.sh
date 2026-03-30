#!/usr/bin/env bash
set -euo pipefail

WORKSPACE_DIR="$(pwd)"

echo "Workspace: ${WORKSPACE_DIR}"
echo "Waiting for SQL Server on db:1433..."

for i in {1..60}; do
  if (echo > /dev/tcp/db/1433) >/dev/null 2>&1; then
    echo "SQL Server port is reachable."
    break
  fi

  if [ "$i" -eq 60 ]; then
    echo "SQL Server did not become reachable in time."
    exit 1
  fi

  sleep 2
done

export PATH="$PATH:/home/vscode/.dotnet/tools"

echo "Ensuring dotnet-ef is installed..."
if ! command -v dotnet-ef >/dev/null 2>&1; then
  dotnet tool install --global dotnet-ef
else
  dotnet tool update --global dotnet-ef
fi

echo "Restoring NuGet packages..."
if [ -f "MESS.slnx" ]; then
  dotnet restore MESS.slnx
else
  dotnet restore
fi

echo "Applying ApplicationContext migration..."
until dotnet ef database update \
  --context ApplicationContext \
  --project MESS.Data/MESS.Data.csproj \
  --startup-project MESS.Blazor/MESS.Blazor.csproj
do
  echo "ApplicationContext migration failed, retrying in 5 seconds..."
  sleep 5
done

echo "Applying UserContext migration..."
until dotnet ef database update \
  --context UserContext \
  --project MESS.Data/MESS.Data.csproj \
  --startup-project MESS.Blazor/MESS.Blazor.csproj
do
  echo "UserContext migration failed, retrying in 5 seconds..."
  sleep 5
done

echo "Database initialization complete."