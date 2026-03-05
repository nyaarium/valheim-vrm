#!/bin/bash

set -e

# This script runs on the host machine before the dev container is started.

WORKSPACE_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd .. && pwd )"

VOLUME_HOME="${WORKSPACE_ROOT}/volumes/home"

mkdir -p "${VOLUME_HOME}"

if [ ! -f "${VOLUME_HOME}/.gitconfig" ]; then
    cat > "${VOLUME_HOME}/.gitconfig" <<EOF
[core]
	editor = cursor
[commit]
	gpgSign = false
[safe]
	directory = /workspace
EOF

    chmod 644 "${VOLUME_HOME}/.gitconfig"

    HOST_NAME=$(git config --global user.name)
    HOST_EMAIL=$(git config --global user.email)
    if [ -n "${HOST_NAME}" ] && [ -n "${HOST_EMAIL}" ]; then
        cat >> "${VOLUME_HOME}/.gitconfig" <<EOF
[user]
	name = ${HOST_NAME}
	email = ${HOST_EMAIL}
EOF
    fi
fi
