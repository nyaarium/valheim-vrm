#!/bin/bash


if [[ -f /.dockerenv ]]
then
	echo " ❗  You are inside a Docker container right now. Run this in WSL instead."
	exit
fi


# Configure SSH Agent

# Check .bashrc for "Start SSH Agent" and remove it
EXISTS=$(grep -c "SSH Agent - Start" $HOME/.bashrc)
sed -i "/SSH Agent - Start/,/SSH Agent - End/d" $HOME/.bashrc

# Write the block to .bashrc
if [[ $EXISTS -eq 0 ]]; then
    echo "" >> $HOME/.bashrc
    echo "" >> $HOME/.bashrc
fi
echo "# ==== SSH Agent - Start ====" >> $HOME/.bashrc
echo "if [[ -z \"\$SSH_AUTH_SOCK\" ]]; then" >> $HOME/.bashrc
echo "    if [[ -f \$HOME/.ssh/ssh-agent ]]; then" >> $HOME/.bashrc
echo "        eval \$(cat \$HOME/.ssh/ssh-agent) > /dev/null" >> $HOME/.bashrc
echo "    fi" >> $HOME/.bashrc
echo "" >> $HOME/.bashrc
echo "    AGENT_RUNNING=\$(ps -ax | grep 'ssh-agent -s' | grep -v grep | wc -l | tr -d '[:space:]')" >> $HOME/.bashrc
echo "    if [[ \$AGENT_RUNNING -eq 0 ]]; then" >> $HOME/.bashrc
echo "        # Start agent" >> $HOME/.bashrc
echo "        ssh-agent -s &> \$HOME/.ssh/ssh-agent" >> $HOME/.bashrc
echo "        eval \$(cat \$HOME/.ssh/ssh-agent) > /dev/null" >> $HOME/.bashrc
echo "    fi" >> $HOME/.bashrc
echo "" >> $HOME/.bashrc
echo "    RESULT_LIST=\$(ssh-add -l 2>&1)" >> $HOME/.bashrc
echo "    if [[ \$? -ne 0 ]]; then" >> $HOME/.bashrc
echo "        # Restart agent" >> $HOME/.bashrc
echo "        pkill ssh-agent" >> $HOME/.bashrc
echo "        ssh-agent -s &> \$HOME/.ssh/ssh-agent" >> $HOME/.bashrc
echo "        eval \$(cat \$HOME/.ssh/ssh-agent) > /dev/null" >> $HOME/.bashrc
echo "    fi" >> $HOME/.bashrc
echo "" >> $HOME/.bashrc
echo "    # Add key to agent" >> $HOME/.bashrc
echo "    RESULT_ADD=\$(ssh-add 2>&1)" >> $HOME/.bashrc
echo "    if [[ \$? -ne 0 ]]; then" >> $HOME/.bashrc
echo "        echo \" ❌  Failed to add SSH key to agent\"" >> $HOME/.bashrc
echo "        echo \$RESULT_ADD" >> $HOME/.bashrc
echo "    fi" >> $HOME/.bashrc
echo "fi" >> $HOME/.bashrc
echo "# ==== SSH Agent - End ====" >> $HOME/.bashrc


echo " ⚙️   Your agent has been configured in bashrc. Please restart VSCode."


exit 0


# Original Code:

# ==== Start SSH Agent ====
if [[ -z "$SSH_AUTH_SOCK" ]]; then
    if [[ -f $HOME/.ssh/ssh-agent ]]; then
        eval $(cat $HOME/.ssh/ssh-agent) > /dev/null
    fi

    AGENT_RUNNING=$(ps -ax | grep 'ssh-agent -s' | grep -v grep | wc -l | tr -d '[:space:]')
    if [[ $AGENT_RUNNING -eq 0 ]]; then
        # Start agent
        ssh-agent -s &> $HOME/.ssh/ssh-agent
        eval $(cat $HOME/.ssh/ssh-agent) > /dev/null
    fi

    RESULT_LIST=$(ssh-add -l 2>&1)
    if [[ $? -ne 0 ]]; then
        # Restart agent
        pkill ssh-agent
        ssh-agent -s &> $HOME/.ssh/ssh-agent
        eval $(cat $HOME/.ssh/ssh-agent) > /dev/null
    fi

    # Add key to agent
    RESULT_ADD=$(ssh-add 2>&1)
    if [[ $? -ne 0 ]]; then
        echo " ❌  Failed to add SSH key to agent"
        echo $RESULT_ADD
    fi
fi
# ==== End SSH Agent ====
