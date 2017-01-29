#!/usr/bin/env bash

RESULTCODE=0

# Download dotnet cli
echo "Installing dotnet"
mkdir -p .cli
curl -o .cli/dotnet-install.sh https://raw.githubusercontent.com/dotnet/cli/58b0566d9ac399f5fa973315c6827a040b7aae1f/scripts/obtain/dotnet-install.sh

# Run install.sh
chmod +x .cli/dotnet-install.sh
.cli/dotnet-install.sh -i .cli -c preview -v 1.0.0-rc4-004706

# Display info
DOTNET="$(pwd)/.cli/dotnet"
$DOTNET --info

# clean up
rm -rf $(pwd)/artifacts
mkdir $(pwd)/artifacts

# restore
$DOTNET restore $(pwd)

if [ $? -ne 0 ]; then
    echo "Restore FAILED!"
    exit 1
fi

# build
$DOTNET build $(pwd) -c release

if [ $? -ne 0 ]; then
    echo "Build FAILED!"
    exit 1
fi

# run all test projects under test/
for testProject in `find test -type f -name *.csproj`
do
	testDir="$(pwd)/$(dirname $testProject)"

	$DOTNET test testDir -f netcoreapp1.0 --no-build -r $(pwd)/artifacts

	if [ $? -ne 0 ]; then
	    echo "$testProject FAILED!"
	    RESULTCODE=1
	fi
done

if [ $RESULTCODE -ne 0 ]; then
    echo "tests FAILED!"
    exit 1
fi

# pack
$DOTNET pack --no-build -o $(pwd)/artifacts

if [ $RESULTCODE -ne 0 ]; then
    echo "pack FAILED!"
    exit 1
fi

exit $RESULTCODE
