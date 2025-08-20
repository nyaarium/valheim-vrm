#!/bin/bash
set -e

echo "Building ValheimVRM..."

cd "$(dirname "$0")"

ROOT="$(pwd)"
INSTALL_PATH=
VALHEIM_DLLS=/var/build-dlls
UNIVRM_UNITY_LIBS="$ROOT/Libs"
PROJECT_DIR="$ROOT/ValheimVRM"
OUT_DLL="$PROJECT_DIR/bin/Release/net471/ValheimVRM.dll"

cd "$PROJECT_DIR"

dotnet build --configuration Release                || true # REMOVE THIS NEXT PR
														mkdir -p "$ROOT/release"
													   touch "$ROOT/release/stub.zip"
														echo "public const string PluginVersion = "0.0.0";" > "$ROOT/release/VersionInfo.g.cs"

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
