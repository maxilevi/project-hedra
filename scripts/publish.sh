#!/usr/bin/bash

RID=$1
FRAMEWORK=netcoreapp6.0

valid_rids=("win-x64" "win-x86" "linux-x64")

if ! [[ $(echo ${valid_rids[@]} | grep -o $RID | wc -w) ]]; then
	exit 1
fi

dotnet publish Hedra/Hedra.csproj -c Release -r $RID --self-contained true

rm -rf Hedra/bin/Release/$FRAMEWORK/$RID/publish/Assets
rm -rf Hedra/bin/Release/$FRAMEWORK/$RID/publish/Shaders
rm -rf Hedra/bin/Release/$FRAMEWORK/$RID/publish/Sounds
rm -rf Hedra/bin/Release/$FRAMEWORK/$RID/publish/ref
chmod +x Hedra/bin/Release/$FRAMEWORK/$RID/publish/Hedra

