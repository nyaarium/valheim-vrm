#!/bin/bash

set -e

# This script runs on the host machine before the dev container is started.

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
