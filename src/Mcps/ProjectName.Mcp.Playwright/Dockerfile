# ═══════════════════════════════════════════════════════════════
# Playwright MCP Actuator — Dockerfile
# Multi-stage build mirroring the phase service pattern.
# Playwright requires system dependencies + browser binaries.
# ═══════════════════════════════════════════════════════════════

# ── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution-level build files first (layer caching — changes rarely)
COPY ["Directory.Build.props",    "./"]
COPY ["Directory.Build.targets",  "./"]
COPY ["Directory.Packages.props", "./"]

# Copy csproj files and restore (cached unless csproj or Packages changes)
COPY ["src/Mcps/ProjectName.Mcp.Playwright/ProjectName.Mcp.Playwright.csproj",
      "src/Mcps/ProjectName.Mcp.Playwright/"]
COPY ["src/ProjectNmae.ServiceDefaults/ProjectNmae.ServiceDefaults.csproj",
      "src/ProjectNmae.ServiceDefaults/"]

RUN dotnet restore "src/Mcps/ProjectName.Mcp.Playwright/ProjectName.Mcp.Playwright.csproj"

# Copy remaining source and publish
COPY . .
WORKDIR "/src/src/Mcps/ProjectName.Mcp.Playwright"
RUN dotnet publish "ProjectName.Mcp.Playwright.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
# Playwright base image — includes all required system libs for Chromium.
FROM mcr.microsoft.com/playwright/dotnet:v1.50.0-noble AS runtime

WORKDIR /home/app

# Copy published output
COPY --from=build /app/publish .

# Install Chromium browser binaries.
# pwsh (PowerShell Core) is included in the Playwright base image.
RUN pwsh playwright.ps1 install chromium

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_HTTPS_PORTS=8081
# Set to "false" only for local headed debugging — always headless in containers.
ENV PLAYWRIGHT_HEADLESS=true

ENTRYPOINT ["dotnet", "ProjectName.Mcp.Playwright.dll"]