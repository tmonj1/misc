FROM jenkins/ssh-agent:latest

# It's beter to match docker cli version with the host docker daemon version.
ENV DOCKER_CLI_VERSION=5:20.10.5~3-0~debian-buster

# Settings for AWS configure
# ARG aws_accesskey_id
# ARG aws_secretkey
# ARG aws_default_region=ap-northeast-1

# Use if docker daemon version does not match the client's version
# ENV DOCKER_API_VERSION=20.10.5

# install sudo (and add jenkins in sudoers), basic tools and docker CLI
RUN apt-get update && \
    apt-get install sudo && \
    echo 'jenkins ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers && \
    ln -s /usr/local/openjdk-8/bin/java /usr/local/bin/java && \
    apt-get update && \
    apt-get install -y \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg \
    lsb-release && \
    curl -fsSL https://download.docker.com/linux/debian/gpg | \
    gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg && \
    echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] \
    https://download.docker.com/linux/debian $(lsb_release -cs) stable" | \
    tee /etc/apt/sources.list.d/docker.list > /dev/null && \
    apt-get update && \
    apt-get install -y docker-ce-cli=${DOCKER_CLI_VERSION} && \
    sudo curl -L "https://github.com/docker/compose/releases/download/1.29.1/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose && \
    sudo chmod +x /usr/local/bin/docker-compose

USER jenkins
SHELL ["/bin/bash", "--login", "-c"]

# install NVM and Node.js v14
RUN cd ~ && \
    touch ~/.bashrc && \
    wget -qO- https://raw.githubusercontent.com/nvm-sh/nvm/v0.38.0/install.sh | bash && \
    export NVM_DIR="$HOME/.nvm" && \
    [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh" && \
    [ -s "$NVM_DIR/bash_completion" ] && \. "$NVM_DIR/bash_completion" && \
    nvm install v14

# install AWS CLI v2
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && \
    unzip awscliv2.zip && \
    sudo ./aws/install
#    aws configure set aws_access_key_id $aws_accesskey_id && \
#    aws configure set aws_secret_access_key $aws_secretkey && \
#    aws configure set default.region aws_default_region

# a permission error occurs if not change back to root user.
USER root
