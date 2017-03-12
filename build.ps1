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
$sleetExe = Join-Path $RepoRoot "packages\Sleet.1.1.0-beta.296\tools\Sleet.exe"

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

$SleetConfig = Get-SleetConfig $RepoRoot

if ($Push -and (Test-Path $SleetConfig))
{
    & $sleetExe push --source $SleetFeedId --config $SleetConfig $ArtifactsDir

    if (-not $?)
    {
       Write-Host "Push failed!"
       exit 1
    }

    & $sleetExe validate --source $SleetFeedId --config $SleetConfig

    if (-not $?)
    {
       Write-Host "Feed corrupt!"
       exit 1
    }
}

Write-Host "Success!"