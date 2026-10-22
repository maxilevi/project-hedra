#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."
[[ "$(uname -s)" == Darwin ]] || { echo 'Run this script on macOS.' >&2; exit 1; }
./build-deps-macos.sh
arch="${HEDRA_ARCH:-$(uname -m)}"
case "$arch" in arm64) rid=osx-arm64 ;; x86_64) rid=osx-x64 ;; esac
# The asset compiler targets .NET 6; allow a newer installed SDK's runtime for this build tool.
DOTNET_ROLL_FORWARD=Major dotnet publish Hedra/Hedra.csproj -c Release -r "$rid" \
  -p:Platform=AnyCPU --self-contained true
output="Hedra/bin/Release/netcoreapp6.0/$rid"
app="$PWD/.macos-build/Project Hedra.app"
mkdir -p "$app/Contents/MacOS"
rsync -a --delete --exclude Assets --exclude Shaders --exclude Sounds --exclude x86 --exclude x64 --exclude Characters --exclude Crashes --exclude log.txt \
  "$output/publish/" "$app/Contents/MacOS/"
cp "$output"/data{1,2,3}.db "$app/Contents/MacOS/"
"$app/Contents/MacOS/Hedra" --bake-generation "$app/Contents/MacOS/generation.bin"
# Remove loose development assets from an earlier local bundle, if present.
for folder in Assets Shaders Sounds x86 x64; do
  rm -rf "$app/Contents/MacOS/$folder"
done
cat > "$app/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
<key>CFBundleExecutable</key><string>Hedra</string>
<key>CFBundleIdentifier</key><string>com.leastsquares.hedra</string>
<key>CFBundleName</key><string>Project Hedra</string>
<key>CFBundlePackageType</key><string>APPL</string>
<key>NSHighResolutionCapable</key><true/>
</dict></plist>
PLIST
printf 'Built %s\n' "$app"
