<#
.SYNOPSIS
    This script extracts NuGet package information from .csproj files, installs the packages,
    and copies the .nupkg files to a specified directory.

.DESCRIPTION
    The script performs the following actions:
    1. Extracts all package and version information from .csproj files located in the source directory.
    2. Uses nuget.exe to install these packages into a temporary install directory.
    3. Scans the install directory for all .nupkg files and copies them to a specified packages directory.
    4. Deletes the temporary install directory after the operation, even if the script is stopped or encounters an error.

.PARAMETER SourceDirectory
    Specifies the relative or absolute path to the directory containing the .csproj files.
    Default value is "../src".

.PARAMETER InstallDirectory
    Specifies the relative or absolute path to the temporary directory where the packages will be installed.
    Default value is "../install".

.PARAMETER CopyDirectory
    Specifies the relative or absolute path to the directory where the .nupkg files will be copied.
    Default value is "../packages".

.PARAMETER NugetExePath
    Specifies the relative or absolute path to the nuget.exe executable.
    Default value is "../utilities/nuget.exe".

.EXAMPLE
    .\Install-Packages.ps1 -SourceDirectory "../src" -InstallDirectory "../install" -CopyDirectory "../packages" -NugetExePath "../utilities/nuget.exe"

    This command runs the script using custom paths for the source directory, install directory, copy directory, and nuget.exe path.

.EXAMPLE
    .\Install-Packages.ps1

    This command runs the script using the default paths for the source directory, install directory, copy directory, and nuget.exe path.

.NOTES
    Ensure that nuget.exe is available at the specified path or download it from the NuGet website (https://www.nuget.org/downloads).
    Adjust the paths in the script parameters to match your directory structure if necessary.
#>
param (
    [string]$SourceDirectory  = "../src",
    [string]$InstallDirectory = "../install",
    [string]$CopyDirectory    = "../packages",
    [string]$NugetExePath     = "../utilities/nuget.exe"
)

# Get the current script directory
$currentScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Define paths based on the current script directory
$sourceDirectory  = Join-Path -Path $currentScriptDirectory -ChildPath $SourceDirectory
$installDirectory = Join-Path -Path $currentScriptDirectory -ChildPath $InstallDirectory
$copyDirectory    = Join-Path -Path $currentScriptDirectory -ChildPath $CopyDirectory
$nugetExePath     = Join-Path -Path $currentScriptDirectory -ChildPath $NugetExePath

# Ensure paths exist before resolving
if (!(Test-Path $sourceDirectory -Verbose)) {
    New-Item -Path $sourceDirectory -ItemType Directory -Verbose | Out-Null
}
$sourceDirectory = (Resolve-Path $sourceDirectory -Verbose).Path

if (!(Test-Path $installDirectory -Verbose)) {
    New-Item -Path $installDirectory -ItemType Directory -Verbose | Out-Null
}
$installDirectory = (Resolve-Path $installDirectory -Verbose).Path

if (!(Test-Path $copyDirectory -Verbose)) {
    New-Item -Path $copyDirectory -ItemType Directory -Verbose | Out-Null
}
$copyDirectory = (Resolve-Path $copyDirectory -Verbose).Path

if (!(Test-Path $nugetExePath -Verbose)) {
    Write-Error "NuGet executable does not exist: $nugetExePath"
    exit 1
}
$nugetExePath = (Resolve-Path $nugetExePath -Verbose).Path

# Define the cleanup script
$cleanupScript = {
    param ($installDirectoryPath)
    if (Test-Path -Path $installDirectoryPath -Verbose) {
        Remove-Item -Path $installDirectoryPath -Recurse -Force -Verbose
        Write-Host "Temporary install folder has been deleted."
    }
}

# Register an event to clean up the install directory on script termination
Register-EngineEvent -SourceIdentifier "PowerShell.Exiting" -Action {
    $installDirectoryPath = $event.MessageData
    if (Test-Path -Path $installDirectoryPath -Verbose) {
        Remove-Item -Path $installDirectoryPath -Recurse -Force -Verbose
        Write-Host "Temporary install folder has been deleted."
    }
} -MessageData $installDirectory

# Main script logic
# Step 1: Extract all package and version information from .csproj files
$packages = @()
$csprojFiles = Get-ChildItem -Path $sourceDirectory -Recurse -Filter *.csproj -Verbose

foreach ($csprojFile in $csprojFiles) {
    [xml]$csprojXml = Get-Content $csprojFile.FullName -Verbose
    $packageReferences = $csprojXml.Project.ItemGroup.PackageReference

    foreach ($packageReference in $packageReferences) {
        $packageName = $packageReference.Include
        $packageVersion = $packageReference.Version

        # Add validation to ensure packageName and packageVersion are not null or empty
        if (![string]::IsNullOrEmpty($packageName) -and ![string]::IsNullOrEmpty($packageVersion)) {
            $packages += [PSCustomObject]@{Name = $packageName; Version = $packageVersion}
        }
    }
}

# Ensure the package list is distinct
$packages = $packages | Sort-Object Name, Version -Unique

# Step 2: Use nuget.exe to install these packages into the install folder
$packages | ForEach-Object {
    $packageName = $_.Name
    $packageVersion = $_.Version
    & $nugetExePath install $packageName -Version $packageVersion -OutputDirectory $installDirectory -Verbosity detailed -NonInteractive
}

# Step 3: Scan the install folder for all .nupkg files and copy them to the packages folder
$nupkgFiles = Get-ChildItem -Path $installDirectory -Recurse -Filter *.nupkg -Verbose

foreach ($file in $nupkgFiles) {
    $destination = Join-Path -Path $copyDirectory -ChildPath $file.Name
    Copy-Item -Path $file.FullName -Destination $destination -Force -Verbose
}

Write-Host "Completed processing. All .nupkg files have been copied to $copyDirectory."

# Manually trigger cleanup in case script completes successfully
& $cleanupScript -installDirectoryPath $installDirectory

# Unregister the cleanup event as it's no longer needed
Unregister-Event -SourceIdentifier "PowerShell.Exiting" -Verbose