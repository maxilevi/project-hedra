#!/usr/bin/bash

cd "$(dirname "$0")"

RID=$1
FRAMEWORK=netcoreapp6.0

valid_rids=("win-x64" "win-x86" "linux-x64")

if ! [[ $(echo ${valid_rids[@]} | grep -o $RID | wc -w) ]]; then
	exit 1
fi

(cd ../Hedra && dotnet publish -c Release -r $RID --self-contained true)

rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/Assets
rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/Shaders
rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/Sounds
rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/ref

