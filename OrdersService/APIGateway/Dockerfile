FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only the APIGateway project files first for restore
COPY APIGateway/APIGateway.csproj ./APIGateway/

# If your APIGateway project depends on other projects (e.g. BusinessLogicLayer), copy those too
COPY BusinessLogicLayer/ ./BusinessLogicLayer/
COPY OrdersMicroserviceAPI/ ./OrdersMicroserviceAPI/

# Restore packages for the solution or project
RUN dotnet restore ./APIGateway/APIGateway.csproj

# Now copy all source code of APIGateway (and dependencies if needed)
COPY APIGateway/ ./APIGateway/
COPY BusinessLogicLayer/ ./BusinessLogicLayer/
COPY OrdersMicroserviceAPI/ ./OrdersMicroserviceAPI/

# Build the APIGateway project
RUN dotnet build ./APIGateway/APIGateway.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish ./APIGateway/APIGateway.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APIGateway.dll"]
