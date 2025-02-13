param(
    [Parameter(Mandatory=$true)]
    [string]$ReleasePath,

    [Parameter(Mandatory=$true)]
    [string]$Version
)

$7zipPath = "$env:ProgramFiles\7-Zip\7z.exe"
if (-not (Test-Path -Path $7zipPath -PathType Leaf)) {
    throw "7 zip executable '$7zipPath' not found"
}
Set-Alias Start-SevenZip $7zipPath

# List of platform folders to process
$platforms = @("linux-x64", "win-x64", "osx-arm64", "osx-x64")

foreach ($platform in $platforms) {
    $platformPath = Join-Path $ReleasePath $platform
    $publishPath = Join-Path $platformPath "publish"

    if (-not (Test-Path $platformPath)) {
        Write-Warning "Platform folder not found: $platformPath"
        continue
    }

    if (-not (Test-Path $publishPath)) {
        Write-Warning "Publish folder not found in: $platformPath"
        continue
    }

    # Delete this files
    $filesToDelete = @(
        "appsettings.DeveLinux.json",
        "appsettings.Development.json",
        "Common.Contracts.pdb",
        "Common.Services.pdb",
        "NsxLibraryManager.Shared.pdb"
    )

    foreach ($file in $filesToDelete) {
        $filePath = Join-Path $publishPath $file
        if (Test-Path $filePath) {
            Remove-Item $filePath -Force
            Write-Host "Deleted: $filePath"
        } else {
            Write-Warning "File not found: $filePath"
        }
    }

    $newFolderName = "nsx-library-manager-$platform-v$Version"
    $newFolderPath = Join-Path $platformPath $newFolderName

    if (Test-Path $newFolderPath) {
        Remove-Item $newFolderPath -Recurse -Force
    }
    Rename-Item -Path $publishPath -NewName $newFolderName
    Write-Host "Renamed publish folder to: $newFolderName"

    # Create relese assetts
    if ($platform -eq "linux-x64") {
        $archivePath = Join-Path $ReleasePath "$newFolderName.tar.gz"
        if (Test-Path $archivePath) {
            Remove-Item $archivePath -Force
        }

        Push-Location $platformPath
        try {
            & Start-SevenZip a -ttar "$newFolderName.tar" $newFolderName
            & Start-SevenZip a -tgzip "$archivePath" "$newFolderName.tar"

            Remove-Item "$newFolderName.tar" -Force
            Write-Host "Created tar.gz file: $archivePath"
        }
        finally {
            Pop-Location
        }
    }
    else {
        $archivePath = Join-Path $ReleasePath "$newFolderName.zip"
        if (Test-Path $archivePath) {
            Remove-Item $archivePath -Force
        }
        Compress-Archive -Path $newFolderPath -DestinationPath $archivePath
        Write-Host "Created zip file: $archivePath"
    }

    Rename-Item -Path $newFolderPath -NewName "publish"
}

Write-Host "Processing complete!"