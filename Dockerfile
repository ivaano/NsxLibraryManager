FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends gosu && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
WORKDIR /src
COPY src/. .
RUN dotnet restore "NsxLibraryManager/NsxLibraryManager.csproj"
WORKDIR "/src/NsxLibraryManager"
RUN dotnet build "NsxLibraryManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NsxLibraryManager.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
COPY --from=publish /app/publish /app
RUN mkdir -p /app/backup
RUN mkdir -p /app/renamer/in /app/renamer/out
RUN mkdir -p /app/wwwroot/images/icon

COPY entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh

WORKDIR /app
ENTRYPOINT ["/app/entrypoint.sh"]