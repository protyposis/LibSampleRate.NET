param(
    [Parameter(Mandatory = $true)][string]$ApiKey,
    [string]$Configuration = "Release",
    [string]$Version,
    [string]$Source = "https://api.nuget.org/v3/index.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path -Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "src/LibSampleRate/LibSampleRate.csproj"
$outputDir = Join-Path $repoRoot "temp/publish"

if (Test-Path -Path $outputDir) {
    Remove-Item -Recurse -Force -Path $outputDir
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

$packArgs = @("pack", $projectPath, "-c", $Configuration, "-o", $outputDir)
if ($Version) {
    $packArgs += "-p:PackageVersion=$Version"
}

Write-Host "Packing LibSampleRate..."
& dotnet @packArgs | Write-Host

$package = Get-ChildItem -Path $outputDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $package) {
    throw "No nupkg found in $outputDir"
}

$symbolPackage = Get-ChildItem -Path $outputDir -Filter "*.snupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

Write-Host "Found package: $($package.FullName)"
if ($symbolPackage) {
    Write-Host "Found symbols package: $($symbolPackage.FullName)"
}

Write-Host "Pushing package to $Source..."
& dotnet nuget push $package.FullName --api-key $ApiKey --source $Source --skip-duplicate | Write-Host

if ($symbolPackage) {
    Write-Host "Pushing symbols package..."
    & dotnet nuget push $symbolPackage.FullName --api-key $ApiKey --source $Source --skip-duplicate | Write-Host
}

Write-Host "Publish completed."
