#!/bin/bash

set -e


if [[ -f /.dockerenv ]]; then
    echo " ❗  You are inside a Docker container right now. Run this in WSL instead."
    exit 1
fi


ENSURE_SCRIPT="$HOME/.ssh/ensure-ssh-agent.sh"
CRON_LINE="@reboot $ENSURE_SCRIPT >> $HOME/.ssh/ensure-ssh-agent.log 2>&1"


# Remove any existing SSH Agent block (both legacy marker styles), then
# trim trailing blank lines so re-runs do not grow the file
sed -i "/Start SSH Agent/,/End SSH Agent/d" "$HOME/.bashrc"
sed -i "/SSH Agent - Start/,/SSH Agent - End/d" "$HOME/.bashrc"
sed -i -e :a -e '/^[[:space:]]*$/{$d;N;ba' -e '}' "$HOME/.bashrc"


# Append the current block
cat >> "$HOME/.bashrc" <<'BASHRC_EOF'


# ==== SSH Agent - Start ====
SSH_AGENT_SOCK="/tmp/ssh-agent.sock"
SSH_AGENT_ENV="$HOME/.ssh/ssh-agent"

# Prefer GPG agent if available and has keys
GPG_SSH_SOCK=""
if command -v gpgconf >/dev/null 2>&1; then
    GPG_SSH_SOCK=$(gpgconf --list-dirs agent-ssh-socket 2>/dev/null)
fi
if [[ -z "$GPG_SSH_SOCK" ]]; then
    GPG_SSH_SOCK="/run/user/$(id -u)/gnupg/S.gpg-agent.ssh"
fi

USE_GPG=0
if [[ -S "$GPG_SSH_SOCK" ]] && SSH_AUTH_SOCK="$GPG_SSH_SOCK" ssh-add -l &>/dev/null; then
    USE_GPG=1
fi

if [[ $USE_GPG -eq 1 ]]; then
    export SSH_AUTH_SOCK="$GPG_SSH_SOCK"
else
    # Classic ssh-agent fallback.
    # Only eval the env file if it looks like ssh-agent output; a failed
    # ssh-agent once wrote its stderr here and every shell eval'd garbage.
    if [[ -f "$SSH_AGENT_ENV" ]] && grep -q "SSH_AUTH_SOCK" "$SSH_AGENT_ENV"; then
        eval "$(cat "$SSH_AGENT_ENV")" > /dev/null
    fi

    # ssh-add -l: 0 = agent has keys, 1 = agent reachable but empty,
    # 2 = agent unreachable. Only 2 warrants replacing the agent; an empty
    # agent may be the boot-pinned one that containers already mounted.
    ssh-add -l &>/dev/null
    AGENT_STATUS=$?

    # Unreachable via env file does not mean the pinned agent is dead (the
    # env file may be missing or stale). Probe the socket directly and adopt
    # a live agent rather than killing it out from under mounted containers.
    if [[ $AGENT_STATUS -eq 2 && -S "$SSH_AGENT_SOCK" ]]; then
        SSH_AUTH_SOCK="$SSH_AGENT_SOCK" ssh-add -l &>/dev/null
        if [[ $? -ne 2 ]]; then
            export SSH_AUTH_SOCK="$SSH_AGENT_SOCK"
            printf 'SSH_AUTH_SOCK=%s; export SSH_AUTH_SOCK;\n' "$SSH_AGENT_SOCK" > "$SSH_AGENT_ENV"
            ssh-add -l &>/dev/null
            AGENT_STATUS=$?
        fi
    fi

    if [[ $AGENT_STATUS -eq 2 ]]; then
        if [[ -e "$SSH_AGENT_SOCK" && ! -S "$SSH_AGENT_SOCK" ]]; then
            # Docker creates missing bind-mount sources as root-owned dirs,
            # and sticky /tmp means only root can remove one.
            echo " ❌  $SSH_AGENT_SOCK exists but is not a socket (a container start likely beat the agent to it)."
            echo "     Fix:  sudo rm -rf $SSH_AGENT_SOCK   then open a new terminal."
        else
            # Kill stale agent and start fresh with pinned socket
            pkill -U "$UID" -x ssh-agent 2>/dev/null
            rm -f "$SSH_AGENT_SOCK"
            AGENT_ENV_OUT=$(ssh-agent -a "$SSH_AGENT_SOCK" -s 2>/dev/null)
            if [[ -n "$AGENT_ENV_OUT" ]]; then
                printf '%s\n' "$AGENT_ENV_OUT" > "$SSH_AGENT_ENV"
                eval "$AGENT_ENV_OUT" > /dev/null
                ssh-add -l &>/dev/null
                AGENT_STATUS=$?
            else
                echo " ❌  Failed to start ssh-agent on $SSH_AGENT_SOCK"
            fi
        fi
    fi

    # Add the default key once per agent lifetime
    if [[ $AGENT_STATUS -eq 1 ]]; then
        RESULT_ADD=$(ssh-add 2>&1)
        if [[ $? -ne 0 ]]; then
            echo " ❌  Failed to add SSH key to agent"
            echo "$RESULT_ADD"
        fi
    fi
fi

if [[ -n "$SSH_AUTH_SOCK" ]]; then
    export HOST_SSH_AUTH_SOCK="$SSH_AUTH_SOCK"
fi
# ==== SSH Agent - End ====
BASHRC_EOF


# Install the boot-time socket pinner. If a devcontainer starts while
# /tmp/ssh-agent.sock does not exist, dockerd creates the missing bind-mount
# source as a root-owned directory and SSH forwarding is broken until
# "sudo rm -rf /tmp/ssh-agent.sock". Pinning the socket at boot prevents it.
mkdir -p "$HOME/.ssh"
chmod 700 "$HOME/.ssh"

cat > "$ENSURE_SCRIPT" <<'ENSURE_EOF'
#!/bin/bash
#
# Pin a host ssh-agent to /tmp/ssh-agent.sock so devcontainer bind mounts
# always find a real socket. If a container starts while this path does not
# exist, dockerd creates it as a root-owned directory and breaks SSH
# forwarding until "sudo rm -rf /tmp/ssh-agent.sock".
#
# Runs from cron at boot (and from init-home.sh before devcontainer up):
#   @reboot $HOME/.ssh/ensure-ssh-agent.sh >> $HOME/.ssh/ensure-ssh-agent.log 2>&1
#
# Idempotent and safe to run concurrently. Keys are added later by the
# bashrc block; containers only need the socket to exist.

SSH_AGENT_SOCK="/tmp/ssh-agent.sock"
SSH_AGENT_ENV="$HOME/.ssh/ssh-agent"

# A root-owned socket at the sticky /tmp path would be unusable by the
# real user and undeletable without root. Refuse to pin via sudo.
if [[ $EUID -eq 0 && -n "$SUDO_USER" ]]; then
    echo "ensure-ssh-agent: run as your normal user, not via sudo." >&2
    exit 1
fi

umask 077
mkdir -p "$HOME/.ssh"

if command -v flock >/dev/null 2>&1; then
    exec 9>>"$SSH_AGENT_ENV.lock"
    if ! flock -w 30 9; then
        echo "ensure-ssh-agent: timed out waiting for $SSH_AGENT_ENV.lock" >&2
        exit 1
    fi
fi

if [[ -S "$SSH_AGENT_SOCK" ]]; then
    SSH_AUTH_SOCK="$SSH_AGENT_SOCK" ssh-add -l >/dev/null 2>&1
    if [[ $? -ne 2 ]]; then
        # A live agent already owns the socket
        exit 0
    fi
fi

if [[ -e "$SSH_AGENT_SOCK" && ! -S "$SSH_AGENT_SOCK" ]]; then
    echo "ensure-ssh-agent: $SSH_AGENT_SOCK exists but is not a socket." >&2
    echo "ensure-ssh-agent: fix with:  sudo rm -rf $SSH_AGENT_SOCK" >&2
    exit 1
fi

# Dead socket owned by someone else: rm -f would fail on sticky /tmp,
# so surface the real fix instead of a generic spawn failure.
if [[ -S "$SSH_AGENT_SOCK" ]]; then
    SOCK_OWNER=$(stat -c %u "$SSH_AGENT_SOCK" 2>/dev/null)
    if [[ -n "$SOCK_OWNER" && "$SOCK_OWNER" != "$EUID" ]]; then
        echo "ensure-ssh-agent: dead socket at $SSH_AGENT_SOCK is owned by uid $SOCK_OWNER." >&2
        echo "ensure-ssh-agent: fix with:  sudo rm -rf $SSH_AGENT_SOCK" >&2
        exit 1
    fi
fi

rm -f "$SSH_AGENT_SOCK"
# 9>&- closes the lock fd for the spawned agent: the daemonized ssh-agent
# would otherwise inherit it and hold the flock for its entire lifetime,
# deadlocking every later invocation. Do not remove.
AGENT_ENV_OUT=$(ssh-agent -a "$SSH_AGENT_SOCK" -s 2>/dev/null 9>&-)
if [[ -z "$AGENT_ENV_OUT" ]]; then
    echo "ensure-ssh-agent: failed to start ssh-agent on $SSH_AGENT_SOCK" >&2
    exit 1
fi
printf '%s\n' "$AGENT_ENV_OUT" > "$SSH_AGENT_ENV"
echo "ensure-ssh-agent: agent pinned to $SSH_AGENT_SOCK"
ENSURE_EOF

chmod 755 "$ENSURE_SCRIPT"


# Register the boot crontab entry (idempotent; ignores commented-out lines)
if command -v crontab >/dev/null 2>&1; then
    if crontab -l 2>/dev/null | grep -v '^[[:space:]]*#' | grep -qF "ensure-ssh-agent.sh"; then
        echo " ⏰  Crontab already runs ensure-ssh-agent.sh"
    else
        if { crontab -l 2>/dev/null || true; echo "$CRON_LINE"; } | crontab -; then
            echo " ⏰  Added crontab entry: $CRON_LINE"
        else
            echo " ⚠️   Failed to install crontab entry. Add it manually:  $CRON_LINE"
        fi
    fi
else
    echo " ⚠️   crontab not available. Arrange for this to run at boot:"
    echo "      $CRON_LINE"
fi


# Pin the agent right now (prints the sudo fix if the path is unusable)
"$ENSURE_SCRIPT" || true


echo " ⚙️   Your agent has been configured in bashrc. Please restart VSCode."


exit 0
