#!/bin/bash

# This script runs as the user specified by the 'user' directive in docker-compose.yml
# It changes the ownership of the /app directory to the current user before starting the application.

echo "Changing ownership of /app to user $(id -u):$(id -g)"
chown -R $(id -u):$(id -g) /app

echo "Executing dotnet NsxLibraryManager.dll"
exec dotnet NsxLibraryManager.dll