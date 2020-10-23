#
# Common
#
export PS1='\w $ '
alias l='ls -F'
alias ls='ls -F'
alias la='ls -Fa'
alias ll='ls -Fal'

#
# NVM
#
export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"  # This loads nvm
[ -s "$NVM_DIR/bash_completion" ] && \. "$NVM_DIR/bash_completion"  # This loads nvm bash_completion

#
# Git
#
source /usr/local/etc/bash_completion.d/git-prompt.sh

#
# AWS
#
export AWS_REGION=ap-northeast-1
export AWS_ACCOUNT_ID=372853230800
export AWS_ECR_URL=${AWS_ACCOUNT_ID}.dkr.ecr.ap-northeast-1.amazonaws.com

# eksctl command completion
. <(eksctl completion bash)
