FROM mcr.microsoft.com/dotnet/aspnet:8.0.13 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0.406-jammy AS build
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
RUN chown app:app /app -R
USER app
WORKDIR /app
ENTRYPOINT ["dotnet", "NsxLibraryManager.dll"]
