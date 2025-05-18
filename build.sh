#!/usr/bin/env bash
set -x

pushd $(pwd)

# Download dotnet cli and run tests
. build/common/common.sh
run_standard_tests