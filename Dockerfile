# ============================================================
# DEMO: Hardened Dockerfile (.NET 10)
# Multi-stage, chiseled base, non-root, trimmed + single file
# ============================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY dotnet/DevSumDemo.csproj .
COPY dotnet/ .
RUN dotnet publish -c Release -o /app/publish \
    && cp -r data /app/publish/data || true

# Stage 2: Runtime (chiseled = no shell, no root, no package manager)
# Same approach as hardened-6 so Trivy can scan NuGet dependencies
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "DevSumDemo.dll"]
