#!/bin/bash

set -e

if ! command -v devcontainer &>/dev/null; then
	echo "devcontainer CLI not found. Install it with:"
	echo "  curl -fsSL https://raw.githubusercontent.com/devcontainers/cli/main/scripts/install.sh | sh"
	exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_NAME="$(basename "$SCRIPT_DIR")"
DEVCONTAINER_NAME="${PROJECT_NAME}_devcontainer"

# Tear down existing container first (ignore errors if already down)
echo "Stopping existing devcontainer for ${PROJECT_NAME} (if any)..."
docker compose -p "$DEVCONTAINER_NAME" down 2>/dev/null || true

echo "Building devcontainer for ${PROJECT_NAME}..."
devcontainer build --workspace-folder "$SCRIPT_DIR"

echo "Starting devcontainer for ${PROJECT_NAME}..."
devcontainer up --workspace-folder "$SCRIPT_DIR"

echo "Attaching shell..."
exec devcontainer exec --workspace-folder "$SCRIPT_DIR" bash
