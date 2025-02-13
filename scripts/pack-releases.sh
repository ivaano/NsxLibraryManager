#!/usr/bin/bash

# Parameters
ReleasePath="$1"
Version="$2"

if [ -z "$ReleasePath" ] || [ -z "$Version" ]; then
  echo "Usage: $0 <ReleasePath> <Version>"
  exit 1
fi

platforms=("linux-x64" "win-x64" "osx-arm64" "osx-x64")

for platform in "${platforms[@]}"; do
  platformPath="$ReleasePath/$platform"
  publishPath="$platformPath/publish"

  if [ ! -d "$platformPath" ]; then
    echo "Warning: Platform folder not found: $platformPath"
    continue
  fi

  if [ ! -d "$publishPath" ]; then
    echo "Warning: Publish folder not found in: $platformPath"
    continue
  fi

   filesToDelete=(
    "appsettings.DeveLinux.json"
    "appsettings.Development.json"
    "Common.Contracts.pdb"
    "Common.Services.pdb"
    "NsxLibraryManager.Shared.pdb"
  )

  for file in "${filesToDelete[@]}"; do
    filePath="$publishPath/$file"
    if [ -f "$filePath" ]; then
      rm -f "$filePath"
      echo "Deleted: $filePath"
    else
      echo "Warning: File not found: $filePath"
    fi
  done

  newFolderName="nsx-library-manager-$platform-v$Version"
  newFolderPath="$platformPath/$newFolderName"

  if [ -d "$newFolderPath" ]; then
    rm -rf "$newFolderPath"
  fi
  mv "$publishPath" "$newFolderPath"
  echo "Renamed publish folder to: $newFolderName"

  # Create release assets
  if [[ "$platform" == "linux-x64" ]]; then
    archivePath="$ReleasePath/$newFolderName.tar.gz"
    if [ -f "$archivePath" ]; then
      rm -f "$archivePath"
    fi

    pushd "$platformPath" || exit 1  

    tar -cf "$newFolderName.tar" "$newFolderName"
    gzip "$newFolderName.tar"
    mv "$newFolderName.tar.gz" "$archivePath"

    rm -f "$newFolderName.tar"
    echo "Created tar.gz file: $archivePath"

    popd || exit 1 

  else
    archivePath="$ReleasePath/$newFolderName.zip"
    if [ -f "$archivePath" ]; then
      rm -f "$archivePath"
    fi
    pushd "$newFolderPath" || exit 1 
    zip -r "$archivePath" .
    popd || exit 1
    echo "Created zip file: $archivePath"
  fi

  mv "$newFolderPath" "$platformPath/publish"
done

echo "Processing complete!"