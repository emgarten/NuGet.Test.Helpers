param (
    [switch]$SkipTests,
    [switch]$SkipPack,
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release"
)

$RepoName = "NuGet.Test.Helpers"
$RepoRoot = $PSScriptRoot
Push-Location $RepoRoot

# Load common build script helper methods
. "$PSScriptRoot\build\common\common.ps1"

# Download tools
Install-CommonBuildTools $RepoRoot

# Clean and write git info
Remove-Artifacts $RepoRoot
Invoke-DotnetMSBuild $RepoRoot ("build\build.proj", "/t:Clean;WriteGitInfo", "/p:Configuration=$Configuration")

# Restore
Invoke-DotnetMSBuild $RepoRoot ("build\build.proj", "/t:Restore", "/p:Configuration=$Configuration")

# Run main build
$buildTargets = "Build"

if (-not $SkipPack)
{
    $buildTargets += ";Pack"
}

if (-not $SkipTests)
{
    $buildTargets += ";Test"
}

# Run build.proj
Invoke-DotnetMSBuild $RepoRoot ("build\build.proj", "/t:$buildTargets", "/p:Configuration=$Configuration /v:d")

Pop-Location
Write-Host "Success!"
