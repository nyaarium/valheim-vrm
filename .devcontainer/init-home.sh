#!/bin/bash

set -e

# This script runs on the host machine before the dev container is started.

# Pin the host SSH agent socket before the container starts; compose
# bind-mounts /tmp/ssh-agent.sock, and dockerd creates a missing mount
# source as a root-owned directory (see docker-workspace README).
if [ -x "$HOME/.ssh/ensure-ssh-agent.sh" ]; then
    "$HOME/.ssh/ensure-ssh-agent.sh" || echo "init-home.sh: SSH agent socket unavailable; container SSH forwarding may not work" >&2
else
    echo "init-home.sh: ~/.ssh/ensure-ssh-agent.sh not installed; run .devcontainer/install-ssh-agent.sh or /tmp/ssh-agent.sock may be created as a root-owned directory" >&2
fi

WORKSPACE_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd .. && pwd )"

VOLUME_HOME="${WORKSPACE_ROOT}/volumes/home"

mkdir -p "${VOLUME_HOME}"

# When host is root (UID 0), updateRemoteUserUID can't remap vscode to UID 0.
# Ensure vscode (default UID 1001) can write to its home volume.
if [ "$(id -u)" = "0" ]; then
    chown 1001:1001 "${VOLUME_HOME}"
fi

# Seed user identity from host git config (only on first creation)
if [ ! -f "${VOLUME_HOME}/.gitconfig" ]; then
    HOST_NAME=$(git config --global user.name || true)
    HOST_EMAIL=$(git config --global user.email || true)
    if [ -n "${HOST_NAME}" ] && [ -n "${HOST_EMAIL}" ]; then
        cat > "${VOLUME_HOME}/.gitconfig" <<EOF
[user]
	name = ${HOST_NAME}
	email = ${HOST_EMAIL}
EOF
        chmod 644 "${VOLUME_HOME}/.gitconfig"
    fi
fi

# Ensure vscode user's .claude.json has the nyaascripts MCP server entry.
# Requires jq on the host.
CLAUDE_JSON="${VOLUME_HOME}/.claude.json"
if command -v jq >/dev/null 2>&1; then
    if [ ! -f "${CLAUDE_JSON}" ]; then
        echo '{}' > "${CLAUDE_JSON}"
        if [ "$(id -u)" = "0" ]; then
            chown 1001:1001 "${CLAUDE_JSON}"
        fi
    fi
    if ! jq -e '.mcpServers.nyaascripts' "${CLAUDE_JSON}" >/dev/null 2>&1; then
        TMP_JSON=$(mktemp)
        jq '.mcpServers.nyaascripts = {
            "type": "stdio",
            "command": "/home/vscode/scripts/nyaascripts",
            "args": [],
            "env": {}
        }' "${CLAUDE_JSON}" > "${TMP_JSON}" && mv "${TMP_JSON}" "${CLAUDE_JSON}"
        if [ "$(id -u)" = "0" ]; then
            chown 1001:1001 "${CLAUDE_JSON}"
        fi
    fi
else
    echo "init-home.sh: jq not found on host; skipping .claude.json nyaascripts seed" >&2
fi
