FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine

# environment variables
ARG HTTP_PROXY
ARG HTTPS_PROXY

# work dir
WORKDIR /app

# copy the project file to /app and restore in container
COPY *.csproj ./
RUN dotnet restore

# copy the whole project to the container and publish to /app/out
COPY . ./
RUN dotnet publish -c Release -o out

# run
ENTRYPOINT ["dotnet", "out/SerilogDemo.dll"]
