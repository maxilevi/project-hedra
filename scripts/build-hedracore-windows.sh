#!/bin/bash
cd "$(dirname "$0")" && cd ".."

echo -e "Building hedra-core"
./hedra-core/build-windows.sh

echo -e "Copying files..."
cp ./hedra-core/cmake-build-x32/Release/hedracore.dll ./Hedra/x86/hedracore.dll
cp ./hedra-core/cmake-build-x64/Release/hedracore.dll ./Hedra/x64/hedracore.dll