#!/bin/bash

set -e

# This script runs on the host machine before the dev container is started.
# It will initialize the home directory's .bashrc file

WORKSPACE_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd .. && pwd )"

DEV_CONTAINER="${WORKSPACE_ROOT}/.devcontainer"
VOLUME_HOME="${WORKSPACE_ROOT}/volumes/home"

mkdir -p "${VOLUME_HOME}"

if [ ! -f "${VOLUME_HOME}/.bashrc" ]; then
    cp "${DEV_CONTAINER}/.bashrc-user" "${VOLUME_HOME}/.bashrc"
    chmod 644 "${VOLUME_HOME}/.bashrc"
fi
