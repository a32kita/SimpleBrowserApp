# PowerShell script to package the Release build into a versioned zip file.
# - Zips all files under src\SimpleBrowserApp\bin\Release\net8.0-windows
# - Zip file name: SimpleBrowserApp-build-x.x.x.x.zip (version from SimpleBrowserApp.exe)
# - Inside the zip, files are under SimpleBrowserApp\ (e.g., SimpleBrowserApp\SimpleBrowserApp.exe)

# Set variables
$SourceDir = "src\SimpleBrowserApp\bin\Release\net8.0-windows"
$ExePath = Join-Path $SourceDir "SimpleBrowserApp.exe"

if (!(Test-Path $ExePath)) {
    Write-Error "SimpleBrowserApp.exe not found: $ExePath"
    exit 1
}

# Get version info from exe
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($ExePath).FileVersion
if (-not $version) {
    Write-Error "Failed to get version from $ExePath"
    exit 1
}

# Build zip file name
$zipName = "SimpleBrowserApp-build-$version.zip"
$zipPath = Join-Path (Get-Location) build $zipName

# Create a temporary directory for packaging
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ("SimpleBrowserApp_pack_" + [System.Guid]::NewGuid().ToString())
$targetDir = Join-Path $tempDir "SimpleBrowserApp"

New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

# Copy all files to temp\SimpleBrowserApp\
Copy-Item -Path (Join-Path $SourceDir "*") -Destination $targetDir -Recurse -Force

# Remove old zip if exists
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Create the zip archive
Compress-Archive -Path $targetDir -DestinationPath $zipPath -CompressionLevel Optimal -Force

# Clean up temp directory
Remove-Item $tempDir -Recurse -Force

Write-Host "Packaged to $zipPath"
