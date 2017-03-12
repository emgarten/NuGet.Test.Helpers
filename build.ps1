param (
    [switch]$SkipTests,
    [switch]$SkipPack,
    [switch]$Push
)

$RepoRoot = $PSScriptRoot
$SleetFeedId = "packages"

# Load common build script helper methods
. "$PSScriptRoot\build\common\common.ps1"

# Ensure dotnet.exe exists in .cli
Install-DotnetCLI $RepoRoot

# Ensure packages.config packages
Install-PackagesConfig $RepoRoot

$ArtifactsDir = Join-Path $RepoRoot 'artifacts'
$dotnetExe = Get-DotnetCLIExe $RepoRoot
$sleetExe = Join-Path $RepoRoot "packages\Sleet.2.0.0-beta.2017.3.12.4.50\tools\Sleet.exe"

# Clean
& $dotnetExe msbuild build\build.proj /t:Clean

if (-not $?)
{
    Write-Error "Clean failed!"
    exit 1
}

# Restore
& $dotnetExe msbuild build\build.proj /t:Restore

if (-not $?)
{
    Write-Error "Restore failed!"
    exit 1
}

# Build, Pack, Test
$buildArgs = , "msbuild"
$buildArgs += "build\build.proj"

if ($SkipTests)
{
    $buildArgs += "/p:SkipTest=true"
}

if ($SkipPack)
{
    $buildArgs += "/p:SkipPack=true"
}

& $dotnetExe $buildArgs

if (-not $?)
{
    Write-Error "Build failed!"
    exit 1
}

if ($Push)
{
    & $sleetExe push --source $SleetFeedId $ArtifactsDir

    if (-not $?)
    {
       Write-Error "Push failed!"
       exit 1
    }

    & $sleetExe validate --source $SleetFeedId

    if (-not $?)
    {
       Write-Error "Feed corrupt!"
       exit 1
    }
}

Write-Host "Success!"