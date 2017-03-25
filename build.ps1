param (
    [switch]$SkipTests,
    [switch]$SkipPack
)

$RepoName = "NuGet.Test.Helpers"
$RepoRoot = $PSScriptRoot


# Load common build script helper methods
. "$PSScriptRoot\build\common\common.ps1"

# Remove artifacts
Remove-Artifacts $RepoRoot

# Ensure dotnet.exe exists in .cli
Install-DotnetCLI $RepoRoot

# Ensure packages.config packages
Install-NuGetExe $RepoRoot

$dotnetExe = Get-DotnetCLIExe $RepoRoot

# Restore
Write-Host "msbuild build\build.proj /t:Restore"
& $dotnetExe msbuild build\build.proj /t:Restore

if (-not $?)
{
    Write-Error "Restore failed!"
    exit 1
}

# Build, Pack, Test
$buildTargets = "Build"

if (-not $SkipTests)
{
    $buildTargets += ";Test"
}

if (-not $SkipPack)
{
    $buildTargets += ";Pack"
}

$buildArgs = , "msbuild"
$buildArgs += "build\build.proj"
$buildArgs += "/t:$buildTargets"

# Run build.proj
Write-Host $dotnetExe $buildArgs
& $dotnetExe $buildArgs

if (-not $?)
{
    Write-Error "Build failed!"
    exit 1
}

# run additional CI steps if on the CI machine
Start-CIBuild $RepoRoot $RepoName 

Write-Host "Success!"