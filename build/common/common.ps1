
# install CLI
Function Install-DotnetCLI {
    param(
        [string]$RepositoryRootDir
    )

    $CLIRoot = Get-DotnetCLIRoot $RepositoryRootDir

    New-Item -ItemType Directory -Force -Path $CLIRoot | Out-Null

    $env:DOTNET_HOME=$CLIRoot
    $installDotnet = Join-Path $CLIRoot "install.ps1"

    $DotnetExe = DotnetCLIExe $RepositoryRootDir

    if (-not (Test-Path $DotnetExe)) {

        New-Item -ItemType Directory -Force -Path $CLIRoot

        Write-Host "Fetching $installDotnet"

        wget https://raw.githubusercontent.com/dotnet/cli/58b0566d9ac399f5fa973315c6827a040b7aae1f/scripts/obtain/dotnet-install.ps1 -OutFile $installDotnet

        & $installDotnet -Channel preview -i $CLIRoot -Version 1.0.1

        if (-not (Test-Path $DotnetExe)) {
            Write-Log "Missing $DotnetExe"
            exit 1
        }
    }

    & $DotnetExe --info
}

Function Get-DotnetCLIRoot {
    param(
        [string]$RepositoryRootDir
    )

    return Join-Path $RepositoryRootDir ".cli"
}

Function Get-DotnetCLIExe {
    param(
        [string]$RepositoryRootDir
    )

    $CLIRoot = Get-DotnetCLIRoot $RepositoryRootDir

    return Join-Path $CLIRoot "dotnet.exe"
}

Function Get-NuGetExePath {
    param(
        [string]$RepositoryRootDir
    )

    return Join-Path $RepositoryRootDir ".nuget/nuget.exe"
}

# download .nuget\nuget.exe
Function Install-NuGetExe {
    param(
        [string]$RepositoryRootDir
    )

    Write-Host "Downloading nuget.exe"
    $nugetExe = Get-NuGetExePath $RepositoryRootDir

    if (-not (Test-Path $nugetExe))
    {
        wget https://dist.nuget.org/win-x86-commandline/v4.0.0/NuGet.exe -OutFile $nugetExe
    }
}

# Run CI specific scripts
Function Start-CIBuild {
    param(
        [string]$RepositoryRootDir,
        [string]$RepositoryName
    )

    $repoSpecificDir = Join-Path $RepositoryRootDir $RepositoryName
    $defaultDir = Join-Path $RepositoryRootDir "default"

    $repoSpecificScript = Join-Path $repoSpecificDir "build.ps1"
    $defaultScript = Join-Path $defaultDir "build.ps1"

    if (Test-Path $repoSpecificScript)
    {
        Invoke-Expression $repoSpecificScript
    }
    elseif (Test-Path $defaultScript)
    {
        Invoke-Expression $defaultScript
    }
}

# Delete the artifacts directory
Function Remove-Artifacts {
    param(
        [string]$RepositoryRootDir
    )

    $artifactsDir = Join-Path $RepositoryRootDir "artifacts"

    if (Test-Path $artifactsDir)
    {
        Remove-Item $artifactsDir -Force -Recurse
    }
}