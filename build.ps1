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

$buildTargets = "Clean;WriteGitInfo;Restore;Build"

if (-not $SkipPack)
{
    $buildTargets += ";Pack"
}

Invoke-DotnetMSBuild $RepoRoot ("build\build.proj", "/t:$buildTargets", "/p:Configuration=$Configuration")

if (-not $SkipTests)
{
    Invoke-DotnetExe $RepoRoot ("test", "NuGet.Test.Helpers.sln", "--configuration", $Configuration, "--no-build", "--no-restore")
}

Pop-Location
Write-Host "Success!"
