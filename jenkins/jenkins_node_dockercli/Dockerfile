FROM jenkins/ssh-agent:latest

# It's beter to match docker cli version with the host docker daemon version.
ENV DOCKER_CLI_VERSION=5:20.10.5~3-0~debian-buster

# Use if docker daemon version does not match the client's version
# ENV DOCKER_API_VERSION=20.10.5

# install sudo and allow "jenkins" to use it no-password.
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

RUN cd ~ && \
    touch ~/.bashrc && \
    wget -qO- https://raw.githubusercontent.com/nvm-sh/nvm/v0.38.0/install.sh | bash && \
    export NVM_DIR="$HOME/.nvm" && \
    [ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh" && \
    [ -s "$NVM_DIR/bash_completion" ] && \. "$NVM_DIR/bash_completion" && \
    nvm install v14

# a permission error occurs if not change back to root user.
USER root
