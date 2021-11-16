#!/bin/bash
cd "$(dirname "$0")" && cd ".."

echo -e "Building hedra-core"
./hedra-core/build-linux.sh

echo -e "Copying files..."
cp ./hedra-core/cmake-build-x32-linux/libhedracore.so ./Hedra/x86/libhedracore.so
cp ./hedra-core/cmake-build-x64-linux/libhedracore.so ./Hedra/x64/libhedracore.so