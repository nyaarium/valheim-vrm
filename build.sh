#!/bin/bash
set -e

echo "Building ValheimVRM..."

cd "$(dirname "$0")"

ROOT="$(pwd)"
VALHEIM_INSTALL_PATH=/var/build-dlls
UNIVRM_UNITY_LIBS="$ROOT/Libs"
PROJECT_DIR="$ROOT/ValheimVRM"
OUT_DLL="$PROJECT_DIR/bin/Release/net48/ValheimVRM.dll"

cd "$PROJECT_DIR"

dotnet build --configuration Release

# Extract version info
VERSION=$(grep -oP 'PluginVersion = "([^"]+)"' VersionInfo.g.cs | cut -d'"' -f2)
FILENAME=$(ls "$ROOT"/release/*.zip | head -1 | xargs basename)
CHECKSUM=$(sha256sum "$ROOT/release/$FILENAME" | cut -d' ' -f1)

# Write info to files
echo -n "$VERSION" > "$ROOT/release/version.txt"
echo -n "$CHECKSUM" > "$ROOT/release/sha256.txt"
cp "$ROOT/release-notes.md" "$ROOT/release/body.md"

echo "Release: $FILENAME"
echo "Version: $VERSION"
echo "sha256: $CHECKSUM"
