#!/usr/bin/env bash

# Helper function to run a command with logging
run_command()
{
  echo ">> $@"
  "$@"
}

run_standard_tests()
{
  pushd $(pwd)

  # Download dotnet cli
  REPO_ROOT=$(pwd)
  DOTNET=${DOTNET_EXE_PATH:-$(pwd)/.cli/dotnet}

  if [ -n "${DOTNET_EXE_PATH:-}" ]; then
    if ! command -v "$DOTNET" >/dev/null 2>&1; then
      echo "Unable to find dotnet executable: $DOTNET"
      exit 1
    fi
  elif [ ! -f "$DOTNET" ]; then
    echo ""
    echo "===> Installing .NET SDK..."
    echo ""
    mkdir -p .cli
    run_command curl -L -o .cli/dotnet-install.sh https://dot.net/v1/dotnet-install.sh

    # Run install.sh
    chmod +x .cli/dotnet-install.sh
    run_command .cli/dotnet-install.sh -i .cli --channel 8.0
    run_command .cli/dotnet-install.sh -i .cli --channel 9.0
    run_command .cli/dotnet-install.sh -i .cli --channel 10.0
  fi

  # Display info
  echo ""
  echo "===> Displaying .NET SDK info..."
  echo ""
  run_command $DOTNET --info

  # Clean, restore, build, and pack
  echo ""
  echo "===> Building projects and creating NuGet packages..."
  echo ""
  run_command $DOTNET msbuild build/build.proj /t:Clean\;WriteGitInfo\;Restore\;Build\;Pack /p:Configuration=Release /nologo /v:m /nr:false /m

  if [ $? -ne 0 ]; then
    echo "Build FAILED!"
    exit 1
  fi

  # test
  echo ""
  echo "===> Running tests..."
  echo ""
  
  # Find all solution files in the repo root and run dotnet test on each
  SLN_FILES=$(find "$REPO_ROOT" -maxdepth 1 -name "*.sln")
  
  if [ -z "$SLN_FILES" ]; then
    echo "No solution files found in $REPO_ROOT. Missing solution for tests!"
    exit 1
  fi
  
  for sln in $SLN_FILES; do
    echo "Running tests for solution: $sln"
    run_command $DOTNET test "$sln" --configuration Release --no-build --no-restore
    
    if [ $? -ne 0 ]; then
      echo "Test FAILED for $sln!"
      exit 1
    fi
  done

  popd
}
