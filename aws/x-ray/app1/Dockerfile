FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY app1.csproj ./
RUN dotnet restore

# copy everything else and build app
COPY . ./
WORKDIR /app
RUN dotnet publish -c Release -o ./

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
EXPOSE 80
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "app1.dll"]