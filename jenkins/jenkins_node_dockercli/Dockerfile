FROM jenkins/ssh-agent:latest

# It's beter to match docker cli version with the host docker daemon version.
ENV DOCKER_CLI_VERSION=5:20.10.5~3-0~debian-buster

# Use if docker daemon version does not match the client's version
# ENV DOCKER_API_VERSION=20.10.5

RUN ln -s /usr/local/openjdk-8/bin/java /usr/local/bin/java && \
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
    apt-get install -y docker-ce-cli=${DOCKER_CLI_VERSION}
