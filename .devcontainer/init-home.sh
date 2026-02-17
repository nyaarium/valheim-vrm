#!/bin/bash

set -e

# This script runs on the host machine before the dev container is started.
# It will initialize the home directory's .bashrc file

WORKSPACE_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd .. && pwd )"

DEV_CONTAINER="${WORKSPACE_ROOT}/.devcontainer"
VOLUME_HOME="${WORKSPACE_ROOT}/volumes/home"

mkdir -p "${VOLUME_HOME}"

VERSION_STRING=$(grep -o "Devcontainer: v.*" "${DEV_CONTAINER}/.bashrc-user" | head -n1)

if [ ! -f "${VOLUME_HOME}/.bashrc" ]; then
    cp "${DEV_CONTAINER}/.bashrc-user" "${VOLUME_HOME}/.bashrc"
    chmod 644 "${VOLUME_HOME}/.bashrc"
elif ! grep -q "${VERSION_STRING}" "${VOLUME_HOME}/.bashrc" 2>/dev/null; then
    rm "${VOLUME_HOME}/.bashrc" "${VOLUME_HOME}/.gitconfig"
    cp "${DEV_CONTAINER}/.bashrc-user" "${VOLUME_HOME}/.bashrc"
    chmod 644 "${VOLUME_HOME}/.bashrc"
fi

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
