FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/Api/Api.csproj backend/src/Api/
COPY backend/src/Application/Application.csproj backend/src/Application/
COPY backend/src/Infrastructure/Infrastructure.csproj backend/src/Infrastructure/
COPY backend/src/Domain/Domain.csproj backend/src/Domain/
COPY backend/src/Common/Common.csproj backend/src/Common/
RUN dotnet restore backend/src/Api/Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/Api/Api.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
USER root
RUN apt-get update && apt-get install --no-install-recommends -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish/ .
USER app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Api.dll"]
