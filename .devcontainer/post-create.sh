#!/bin/bash

# Post-create script for devcontainer

set -e

# Restore Cursor/Claude agent install from image into mounted home when missing
if [ -d /var/agent-install/.local ]; then
	if [ ! -d /home/vscode/.local ]; then
		cp -a /var/agent-install/.local /home/vscode/.local
	elif [ ! -d /home/vscode/.local/share/cursor-agent ] || [ ! -d /home/vscode/.local/share/claude ]; then
		mkdir -p /home/vscode/.local/share /home/vscode/.local/bin
		[ -d /var/agent-install/.local/share/cursor-agent ] && cp -a /var/agent-install/.local/share/cursor-agent /home/vscode/.local/share/
		[ -d /var/agent-install/.local/share/claude ] && cp -a /var/agent-install/.local/share/claude /home/vscode/.local/share/
		[ -L /var/agent-install/.local/bin/agent ] && cp -a /var/agent-install/.local/bin/agent /var/agent-install/.local/bin/cursor-agent /var/agent-install/.local/bin/claude /home/vscode/.local/bin/ 2>/dev/null || true
	fi
fi
if [ -d /var/agent-install/.claude ] && [ ! -d /home/vscode/.claude ]; then
	cp -a /var/agent-install/.claude /home/vscode/.claude
fi
if [ -f /var/agent-install/.claude.json ] && [ ! -f /home/vscode/.claude.json ]; then
	cp -a /var/agent-install/.claude.json /home/vscode/.claude.json
fi
[ -d /home/vscode/.local ] && chown -R vscode:vscode /home/vscode/.local
[ -d /home/vscode/.claude ] && chown -R vscode:vscode /home/vscode/.claude
[ -f /home/vscode/.claude.json ] && chown vscode:vscode /home/vscode/.claude.json

sudo chown vscode:vscode /workspace
