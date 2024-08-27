# Define the base image for the container
FROM mcr.microsoft.com/dotnet/sdk:8.0

ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# ENV TZ="Europe/Brussels"

# Create the user
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    #
    # [Optional] Add sudo support. Omit if you don't need to install software after connecting.
    && apt-get update \
    && apt-get install -y sudo \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

RUN apt-get install ghostscript -y sudo
# # Install Docker CE CLI
# RUN apt-get update \
#     && apt-get install -y apt-transport-https ca-certificates curl gnupg2 lsb-release \
#     && curl -fsSL https://download.docker.com/linux/$(lsb_release -is | tr '[:upper:]' '[:lower:]')/gpg | apt-key add - 2>/dev/null \
#     && echo "deb [arch=amd64] https://download.docker.com/linux/$(lsb_release -is | tr '[:upper:]' '[:lower:]') $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list \
#     && apt-get update \
#     && apt-get install -y docker-ce-cli

# # Install Docker Compose
# RUN LATEST_COMPOSE_VERSION=$(curl -sSL "https://api.github.com/repos/docker/compose/releases/latest" | grep -o -P '(?<="tag_name": ").+(?=")') \
#     && curl -sSL "https://github.com/docker/compose/releases/download/${LATEST_COMPOSE_VERSION}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose \
#     && chmod +x /usr/local/bin/docker-compose
    
RUN dotnet workload update \
    && dotnet workload install aspire \
    && dotnet tool install --global dotnet-ef \
    && dotnet tool install -g dotnet-aspnet-codegenerator \
    && dotnet new install MudBlazor.Templates
    
# Set the working directory in the container
WORKDIR /app

# ********************************************************
# * Anything else you want to do like clean up goes here *
# ********************************************************

# [Optional] Set the default user. Omit if you want to keep the default as root.
USER $USERNAME

# Expose port 80 to the outside world
EXPOSE 80



