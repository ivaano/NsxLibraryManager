#!/bin/bash

# This script runs as root initially.
# It reads the target user ID (PUID) and group ID (PGID) from environment variables.
# If PUID/PGID are not set, it defaults to the UID/GID of the 'app' user id (1654) according to base image.
# It changes the ownership of /app/wwwroot to the determined user/group.
# Then, it uses gosu to execute the main application command as that user/group.

echo "Starting entrypoint script as root."

# Determine Target UID
if [ -z "${PUID}" ]; then
  echo "PUID environment variable not set. Attempting to use 'app' user's UID."
  if id -u app > /dev/null 2>&1; then
    TARGET_UID=$(id -u app)
    echo "Using 'app' user's UID: ${TARGET_UID}"
  else
    echo "Error: PUID not set and 'app' user does not exist. Cannot determine UID."
    exit 1
  fi
else
  TARGET_UID="${PUID}"
  echo "Using PUID environment variable: ${TARGET_UID}"
fi

# Determine Target GID
if [ -z "${PGID}" ]; then
  echo "PGID environment variable not set. Attempting to use 'app' user's GID."
  if id -g app > /dev/null 2>&1; then
    TARGET_GID=$(id -g app)
    echo "Using 'app' user's GID: ${TARGET_GID}"
  else
    echo "Error: PGID not set and 'app' user does not exist. Cannot determine GID."
    exit 1
  fi
else
  TARGET_GID="${PGID}"
  echo "Using PGID environment variable: ${TARGET_GID}"
fi


# Change ownership of /app to the target user and group
echo "Changing ownership of /app/wwwroot to ${TARGET_UID}:${TARGET_GID}"
chown -R "${TARGET_UID}":"${TARGET_GID}" /app/wwwroot

# Execute the main application command using gosu to switch to the target user/group
echo "Executing dotnet NsxLibraryManager.dll as user ${TARGET_UID}:${TARGET_GID}"
exec gosu "${TARGET_UID}":"${TARGET_GID}" dotnet NsxLibraryManager.dll