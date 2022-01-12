#!/bin/bash

cd "$(dirname "$0")"

RID=$1
FRAMEWORK=netcoreapp6.0

dotnet publish ../Hedra/Hedra.csproj -c Release -r $RID --self-contained true

rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/publish/Assets
rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/publish/Shaders
rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/publish/Sounds
rm -rf ../Hedra/bin/Release/$FRAMEWORK/$RID/publish/ref

