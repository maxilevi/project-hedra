#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../.."

# Compare only when a PR/push actually changes the native submodule revision.
if [[ ! "${BASE_SHA:-}" =~ ^[0-9a-f]{40}$ || "$BASE_SHA" =~ ^0+$ ]]; then
    echo 'No previous revision to compare.'
    exit 0
fi
git fetch --no-tags --depth 1 origin "$BASE_SHA"
baseline_revision="$(git ls-tree "$BASE_SHA" hedra-core | awk '{print $3}')"
current_revision="$(git -C hedra-core rev-parse HEAD)"
if [[ "$baseline_revision" == "$current_revision" ]]; then
    echo 'Native revision unchanged; no second build required.'
    exit 0
fi
[[ "$baseline_revision" =~ ^[0-9a-f]{40}$ ]] || { echo 'Missing baseline native revision' >&2; exit 1; }
git -C hedra-core fetch --no-tags --depth 1 origin "$baseline_revision"
git -C hedra-core worktree add --detach "$PWD/.ci/native-baseline-source" "$baseline_revision"
HEDRA_CORE_SOURCE="$PWD/.ci/native-baseline-source" \
HEDRA_NATIVE_OUTPUT="$PWD/.ci/native-baseline" ./scripts/build-hedracore.sh
case "$(uname -s)" in
    Linux) library="$PWD/.ci/native-baseline/linux-x64/libhedracore.so" ;;
    Darwin) library="$PWD/.ci/native-baseline/macos-x64/libhedracore.dylib" ;;
    *) library="$PWD/.ci/native-baseline/windows-x64/Release/hedracore.dll" ;;
esac
python scripts/ci/check_native.py --baseline "$library"
# The managed equivalence fixtures consume these when present in the test suite.
if command -v cygpath >/dev/null 2>&1; then
    library="$(cygpath -m "$library")"
fi
if [[ -n "${GITHUB_ENV:-}" ]]; then
    echo "HEDRA_NOISE_BASELINE_NATIVE=$library" >> "$GITHUB_ENV"
    echo "HEDRA_MESH_BASELINE_NATIVE=$library" >> "$GITHUB_ENV"
fi
