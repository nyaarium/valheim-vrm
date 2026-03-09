#!/bin/bash

set -e

# This script runs on the host machine before the dev container is started.

WORKSPACE_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd .. && pwd )"

VOLUME_HOME="${WORKSPACE_ROOT}/volumes/home"

mkdir -p "${VOLUME_HOME}"

# Seed user identity from host git config (only on first creation)
if [ ! -f "${VOLUME_HOME}/.gitconfig" ]; then
    HOST_NAME=$(git config --global user.name)
    HOST_EMAIL=$(git config --global user.email)
    if [ -n "${HOST_NAME}" ] && [ -n "${HOST_EMAIL}" ]; then
        cat > "${VOLUME_HOME}/.gitconfig" <<EOF
[user]
	name = ${HOST_NAME}
	email = ${HOST_EMAIL}
EOF
        chmod 644 "${VOLUME_HOME}/.gitconfig"
    fi
fi
