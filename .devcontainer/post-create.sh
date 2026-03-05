#!/bin/bash
set -e

/var/post-create-base.sh

# Ran as root after the Docker container creates and attaches volumes.
# Place last setup steps here that require volumes...
