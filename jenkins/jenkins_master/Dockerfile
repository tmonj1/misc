FROM jenkins/jenkins:2.277.3-lts

EXPOSE 8080
EXPOSE 5000

USER root

# set JDK path, then install sudo and allow "jenkins" to use it no-password.
RUN ln -s /usr/local/openjdk-8/bin/java /usr/local/bin/java && \
    apt-get update && \
    apt-get install sudo && \
    echo 'jenkins ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers

USER jenkins
SHELL ["/bin/bash", "--login", "-c"]

# install ASP.NET 5 SDK, nvm, and Node.js 14.
RUN mkdir -p /tmp/t && \
    cd /tmp/t && \
    wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    sudo dpkg -i packages-microsoft-prod.deb && \
    sudo apt-get update; \
    sudo apt-get install -y apt-transport-https && \
    sudo apt-get update && \
    sudo apt-get install -y dotnet-sdk-5.0 && \
    cd /tmp && \
    rm -fr t && \
    cd ~ && \
    touch ~/.bashrc && \
    wget -qO- https://raw.githubusercontent.com/nvm-sh/nvm/v0.38.0/install.sh | bash && \
    source ~/.bashrc && \
    nvm install v14