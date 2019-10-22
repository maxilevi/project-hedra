#!/bin/bash
cd "$(dirname "$0")"
FRAMEWORK=netstandard2.0

echo -e "Building hedra-core"
./hedra-core/build.sh

echo -e "Building Silk.NET"
(cd ./Silk.NET/ && ./generator.sh)
dotnet restore ./Silk.NET/Silk.NET.sln
dotnet msbuild ./Silk.NET/Silk.NET.sln -p:Configuration=Release

echo -e "Copying files..."
cp ./hedra-core/cmake-build-x32/Release/hedracore.dll ./Hedra/x86/hedracore.dll
cp ./hedra-core/cmake-build-x64/Release/hedracore.dll ./Hedra/x64/hedracore.dll

mkdir -p ./references/Silk.NET/
cp "./Silk.NET/src/Core/Silk.NET.Core/bin/Release/$FRAMEWORK/Silk.NET.Core.dll" "./references/Silk.NET/Silk.NET.Core.dll"
cp "./Silk.NET/src/Windowing/Silk.NET.GLFW/bin/Release/$FRAMEWORK/Silk.NET.GLFW.dll" "./references/Silk.NET/Silk.NET.GLFW.dll"
cp "./Silk.NET/src/Input/Silk.NET.Input.Common/bin/Release/$FRAMEWORK/Silk.NET.Input.Common.dll" "./references/Silk.NET/Silk.NET.Input.Common.dll"
cp "./Silk.NET/src/Input/Silk.NET.Input.Desktop/bin/Release/$FRAMEWORK/Silk.NET.Input.Desktop.dll" "./references/Silk.NET/Silk.NET.Input.Desktop.dll"
cp "./Silk.NET/src/Input/Silk.NET.Input/bin/Release/$FRAMEWORK/Silk.NET.Input.dll" "./references/Silk.NET/Silk.NET.Input.dll"
cp "./Silk.NET/src/OpenAL/Silk.NET.OpenAL/bin/Release/$FRAMEWORK/Silk.NET.OpenAL.dll" "./references/Silk.NET/Silk.NET.OpenAL.dll"
cp "./Silk.NET/src/OpenGL/Silk.NET.OpenGL/bin/Release/$FRAMEWORK/Silk.NET.OpenGL.dll" "./references/Silk.NET/Silk.NET.OpenGL.dll"
cp "./Silk.NET/src/OpenGL/Silk.NET.OpenGL.Legacy/bin/Release/$FRAMEWORK/Silk.NET.OpenGL.Legacy.dll" "./references/Silk.NET/Silk.NET.OpenGL.Legacy.dll"
cp "./Silk.NET/src/Windowing/Silk.NET.Windowing.Common/bin/Release/$FRAMEWORK/Silk.NET.Windowing.Common.dll" "./references/Silk.NET/Silk.NET.Windowing.Common.dll"
cp "./Silk.NET/src/Windowing/Silk.NET.Windowing.Desktop/bin/Release/$FRAMEWORK/Silk.NET.Windowing.Desktop.dll" "./references/Silk.NET/Silk.NET.Windowing.Desktop.dll"
cp "./Silk.NET/src/Windowing/Silk.NET.Windowing/bin/Release/$FRAMEWORK/Silk.NET.Windowing.dll" "./references/Silk.NET/Silk.NET.Windowing.dll"