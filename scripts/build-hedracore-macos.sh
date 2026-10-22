#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")/.."
arch="${HEDRA_ARCH:-$(uname -m)}"
case "$arch" in
  arm64) rid=osx-arm64 ;;
  x86_64) rid=osx-x64 ;;
  *) echo "Unsupported macOS architecture: $arch" >&2; exit 1 ;;
esac
build="$PWD/.macos-build"
mkdir -p "$build" "Hedra/native/$rid"
cmake -S scripts/macos/hedracore -B "$build/hedracore-macos-$arch" \
  -DCMAKE_POLICY_VERSION_MINIMUM=3.5 -DCMAKE_OSX_ARCHITECTURES="$arch"
cmake --build "$build/hedracore-macos-$arch" --parallel 8
cp "$build/hedracore-macos-$arch/core/libhedracore.dylib" "Hedra/native/$rid/"

fetch_source() {
  local name="$1" url="$2" revision="$3"
  if [[ ! -d "$build/$name/.git" ]]; then
    git init "$build/$name"
    git -C "$build/$name" remote add origin "$url"
    git -C "$build/$name" fetch --depth 1 origin "$revision"
    git -C "$build/$name" checkout --detach FETCH_HEAD
  fi
  [[ "$(git -C "$build/$name" rev-parse HEAD)" == "$revision" ]] || {
    echo "Unexpected revision in $build/$name" >&2; exit 1;
  }
}
fetch_source BulletSharpPInvoke https://github.com/AndresTraks/BulletSharpPInvoke.git 1d2377ad74fe4d0396cceb2ec9f4ffa1bdd37aca
fetch_source bullet3 https://github.com/bulletphysics/bullet3.git 2c204c49e56ed15ec5fcfa71d199ab6d6570b3f5
cmake -S "$build/BulletSharpPInvoke/libbulletc" -B "$build/bullet-$arch" \
  -DCMAKE_POLICY_VERSION_MINIMUM=3.5 -DCMAKE_OSX_ARCHITECTURES="$arch" \
  -DBULLET_INCLUDE_DIR="$build/bullet3/src" -DBUILD_SHARED_LIBS=OFF \
  -DBUILD_EXTRAS=ON -DBUILD_OPENGL3_DEMOS=OFF
cmake --build "$build/bullet-$arch" --target bulletc --parallel 8
cp "$build/bullet-$arch/libbulletc.dylib" "Hedra/native/$rid/"
