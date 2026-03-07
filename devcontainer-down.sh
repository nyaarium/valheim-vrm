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

echo "Stopping devcontainer for ${PROJECT_NAME}..."
docker compose -p "$DEVCONTAINER_NAME" down --remove-orphans 2>/dev/null || true

echo "Devcontainer ${PROJECT_NAME} stopped."
