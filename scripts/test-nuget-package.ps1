# Script to test if the NuGet package packs the required files and can be consumed.
param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$PackageVersion = ("0.0.0-test." + [DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path -Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "src/LibSampleRate/LibSampleRate.csproj"
$outputDir = Join-Path $repoRoot "temp/nuget-package-test"

if (Test-Path -Path $outputDir) {
    Remove-Item -Recurse -Force -Path $outputDir
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

dotnet pack "$projectPath" -c $Configuration -o "$outputDir" -p:PackageVersion=$PackageVersion

$package = Get-ChildItem -Path $outputDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $package) {
    throw "No nupkg found in $outputDir"
}

Write-Host "`nPackage contents ($($package.Name)):"
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
try {
    foreach ($entry in $zip.Entries) {
        Write-Host " - $($entry.FullName)"
    }
}
finally {
    $zip.Dispose()
}

$testProjectDir = Join-Path $outputDir "TestNugetPackage"
if (Test-Path -Path $testProjectDir) {
    Remove-Item -Recurse -Force -Path $testProjectDir
}

dotnet new console --name TestNugetPackage --output $testProjectDir
$testProjectPath = Join-Path $testProjectDir "TestNugetPackage.csproj"

$programPath = Join-Path $testProjectDir "Program.cs"
$demoProgram = Join-Path $repoRoot "src/LibSampleRate.Demo/Program.cs"
Copy-Item -Path $demoProgram -Destination $programPath -Force

# Prevent pollution of global NuGet cache
Write-Host "`nUsing local NuGet cache..."
$testPackages = Join-Path $outputDir ".nuget-packages"
$env:NUGET_PACKAGES = $testPackages

# Restore and add package using local source
dotnet add "$testProjectPath" package LibSampleRate --version $PackageVersion --source "$outputDir" --no-restore
dotnet restore "$testProjectPath" --source "$outputDir" --disable-parallel --force
dotnet build "$testProjectPath" -c $Configuration -r $Runtime

Write-Host "`nBuild directory contents:"
Get-ChildItem -Path $outputDir -Recurse | Select-Object -ExpandProperty FullName

Write-Host "`nCheck existence of files from package:"
$buildRoot = Join-Path $testProjectDir "bin/$Configuration"
if (-not (Test-Path -Path $buildRoot)) {
    throw "Build output root not found: $buildRoot"
}
$candidateDirs = Get-ChildItem -Path $buildRoot -Directory -Recurse | Where-Object {
    [System.IO.Path]::GetFileName($_.FullName) -eq $Runtime
} | Select-Object -First 1
if (-not $candidateDirs) {
    throw "Could not locate build output for runtime '$Runtime' under $buildRoot"
}
$buildDir = $candidateDirs.FullName

$expectedFiles = @(
    "libsamplerate.windows.x64.dll",
    "libsamplerate/COPYING"
)

foreach ($relative in $expectedFiles) {
    $fullPath = Join-Path $buildDir $relative
    if (-not (Test-Path -Path $fullPath)) {
        throw "Expected file missing from build output: $relative"
    }
    Write-Host "Verified: $relative"
}

Write-Host "`nRunning the console app..."
dotnet run --project "$testProjectPath" -c $Configuration -r $Runtime --no-build

Write-Host "`nLocal NuGet package test completed successfully."
