FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 5001

# Install the dotnet-ef tool for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

FROM base AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend.csproj", "/app/backend.csproj"]
RUN dotnet restore "backend.csproj"

COPY . . 
WORKDIR "/src/"
RUN dotnet build "backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage uses the same SDK image so dotnet-ef is available
FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y netcat-openbsd default-mysql-client
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "backend.dll"]
