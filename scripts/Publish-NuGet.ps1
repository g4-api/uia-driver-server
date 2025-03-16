$publishDirectory = "C:\Path\To\PublishResults"
$outputDirectory  = "C:\Path\To\NugetOutput"
$packageId        = "Uia.DriverServer"
$packageVersion   = "1.0.0"
$authors          = "G4™ API Community"
$description      = "NuGet package that deploys published files into 'uia-driver-server' and removes it on uninstall."

# Create a working folder structure for the package
$packageRoot      = Join-Path $outputDirectory "packageContent"
$contentDirectory = Join-Path $packageRoot "content"
$uiaDirectory     = Join-Path $contentDirectory "uia-driver-server"
$toolsDirectory   = Join-Path $packageRoot "tools"

# Ensure directories exist
New-Item -Path $uiaDirectory   -ItemType Directory -Force | Out-Null
New-Item -Path $toolsDirectory -ItemType Directory -Force | Out-Null

# Copy published files into the package's content folder
Copy-Item -Path (Join-Path $publishDirectory "*") -Destination $uiaDirectory -Recurse -Force

# Create install.ps1 script to copy files into the consuming project
$installScript = @'
param($installPath, $toolsPath, $package, $project)
$targetFolder = Join-Path $project.ProjectFullPath "uia-driver-server"
if (!(Test-Path $targetFolder)) {
    New-Item -ItemType Directory -Path $targetFolder | Out-Null
}
Copy-Item -Path (Join-Path $package.PackageRoot "content\uia-driver-server\*") -Destination $targetFolder -Recurse -Force
'@
Set-Content -Path (Join-Path $toolsDirectory "install.ps1") -Value $installScript -Encoding UTF8

# Create uninstall.ps1 script to remove the deployed folder from the consuming project
$uninstallScript = @'
param($installPath, $toolsPath, $package, $project)
$targetFolder = Join-Path $project.ProjectFullPath "uia-driver-server"
if (Test-Path $targetFolder) {
    Remove-Item -Path $targetFolder -Recurse -Force
}
'@
Set-Content -Path (Join-Path $toolsDirectory "uninstall.ps1") -Value $uninstallScript -Encoding UTF8

# Create the nuspec file dynamically
$nuspecContent = @"
<?xml version="1.0"?>
<package>
  <metadata>
    <id>$packageId</id>
    <version>$packageVersion</version>
    <authors>$authors</authors>
    <owners>$authors</owners>
    <requireLicenseAcceptance>true</requireLicenseAcceptance>
    <license type="expression">Apache-2.0</license>
    <description>$description</description>
  </metadata>
  <files>
    <file src="content\uia-driver-server\**\*" target="content\uia-driver-server\" />
    <file src="tools\install.ps1" target="tools\install.ps1" />
    <file src="tools\uninstall.ps1" target="tools\uninstall.ps1" />
  </files>
</package>
"@

$nuspecPath = Join-Path $packageRoot "$packageId.nuspec"
Set-Content -Path $nuspecPath -Value $nuspecContent -Encoding UTF8

# Change directory to the package root for packaging
Push-Location $packageRoot

# Pack the NuGet package (make sure nuget.exe is available in your PATH)
nuget pack $nuspecPath -outputDirectoryectory $outputDirectory

Pop-Location

Write-Host "NuGet package created at: $outputDirectory\$packageId.$packageVersion.nupkg"
