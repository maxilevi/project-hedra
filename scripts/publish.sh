#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

if [[ $# -ne 2 ]]; then
    echo "Usage: $0 {win-x64|win-x86|linux-x64} {Debug|Release}" >&2
    exit 2
fi
rid="$1"
config="$2"
case "$rid" in win-x64|win-x86|linux-x64) ;; *) echo "Unsupported RID: $rid" >&2; exit 2 ;; esac
case "$config" in Debug|Release) ;; *) echo "Unsupported configuration: $config" >&2; exit 2 ;; esac

# Explicit staging avoids depending on a target-framework output path.
output="$PWD/.ci/publish/$rid"
assets="$PWD/.ci/publish/assets"
rm -rf "$output"
dotnet publish Hedra/Hedra.csproj -c "$config" -r "$rid" \
    -p:Platform=AnyCPU -p:HedraPackAssets=false --no-restore --self-contained true --output "$output"

# Archives are shared by all Steam platform depots. Fail if any are missing.
mkdir -p "$assets"
for archive in data1.db data2.db data3.db; do
    source_archive="$PWD/.ci/assets/$config/$archive"
    test -s "$source_archive"
    cp "$source_archive" "$assets/$archive"
    rm -f "$output/$archive"
done
rm -rf "$output/Assets" "$output/Shaders" "$output/Sounds" "$output/ref"
if [[ "$rid" == linux-x64 ]]; then
    test -x "$output/Hedra"
fi
