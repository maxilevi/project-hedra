#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."

source_dir="${HEDRA_CORE_SOURCE:-$PWD/hedra-core}"
build_root="${HEDRA_NATIVE_OUTPUT:-$PWD/.ci/native}"
case "$(uname -s)" in
    Linux) platform=linux; library=libhedracore.so; architectures=(x64) ;;
    Darwin) platform=macos; library=libhedracore.dylib; architectures=(x64) ;;
    MINGW*|MSYS*|CYGWIN*) platform=windows; library=hedracore.dll; architectures=(x64 x86) ;;
    *) echo "Unsupported native build platform" >&2; exit 2 ;;
esac

for architecture in "${architectures[@]}"; do
    build="$build_root/$platform-$architecture"
    options=(-DCMAKE_BUILD_TYPE=Release -DCMAKE_POLICY_VERSION_MINIMUM=3.5 -DCMAKE_CXX_STANDARD=11 -DCMAKE_CXX_STANDARD_REQUIRED=ON)
    if [[ "$platform" == windows ]]; then
        generator_arch=x64
        [[ "$architecture" != x86 ]] || generator_arch=Win32
        options+=(-A "$generator_arch")
    elif [[ "$platform" == macos ]]; then
        options+=(-DCMAKE_OSX_ARCHITECTURES=x86_64)
    fi
    cmake -S "$source_dir" -B "$build" "${options[@]}"
    cmake --build "$build" --config Release --parallel "${CMAKE_BUILD_PARALLEL_LEVEL:-4}"
    artifact="$build/$library"
    [[ "$platform" != windows ]] || artifact="$build/Release/$library"
    test -s "$artifact"
    # Baseline builds must not replace the current game's native library.
    if [[ -z "${HEDRA_CORE_SOURCE:-}" && "$platform" != macos ]]; then
        mkdir -p "Hedra/$architecture"
        cp "$artifact" "Hedra/$architecture/$library"
    fi
done
