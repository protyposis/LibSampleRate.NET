param(
    [string]$Version = "0.2.2"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path -Path (Join-Path $PSScriptRoot '..')
$targetRoot = Join-Path $repoRoot 'libsamplerate'
$downloadBase = "https://github.com/libsndfile/libsamplerate/releases/download/$Version"

$packages = @(
    @{ Folder = 'win32'; Archive = "libsamplerate-$Version-win32.zip" },
    @{ Folder = 'win64'; Archive = "libsamplerate-$Version-win64.zip" }
)

if (-not (Test-Path -Path $targetRoot)) {
    New-Item -ItemType Directory -Path $targetRoot | Out-Null
}

foreach ($package in $packages) {
    $destination = Join-Path $targetRoot $package.Folder
    $archivePath = Join-Path $targetRoot $package.Archive
    $uri = "$downloadBase/$($package.Archive)"

    Write-Host "Downloading $uri..."
    Invoke-WebRequest -Uri $uri -OutFile $archivePath

    Write-Host "Extracting $($package.Archive)..."
    Expand-Archive -Path $archivePath -DestinationPath $targetRoot -Force
    Remove-Item -Path $archivePath -Force

    $extractedFolderName = [System.IO.Path]::GetFileNameWithoutExtension($package.Archive)
    $extractedPath = Join-Path $targetRoot $extractedFolderName

    if (-not (Test-Path -Path $extractedPath)) {
        throw "Expected extracted folder '$extractedFolderName' was not found."
    }

    if (Test-Path -Path $destination) {
        Remove-Item -Recurse -Force -Path $destination
    }

    Move-Item -Path $extractedPath -Destination $destination
}

# Download important packaging metadata files which are not part of the release packages
Write-Host "Downloading upstream metadata..."
$metaFiles = @("AUTHORS", "COPYING")
foreach ($meta in $metaFiles) {
    $metaUri = "https://raw.githubusercontent.com/libsndfile/libsamplerate/$Version/$meta"
    $metaPath = Join-Path $targetRoot $meta
    Write-Host "  -> $meta"
    Invoke-WebRequest -Uri $metaUri -OutFile $metaPath
}

Write-Host "libsamplerate binaries downloaded to $targetRoot"
