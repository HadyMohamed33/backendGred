# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY AlNady.Backend.sln ./
COPY src/AlNady.Domain/AlNady.Domain.csproj             src/AlNady.Domain/
COPY src/AlNady.Shared/AlNady.Shared.csproj             src/AlNady.Shared/
COPY src/AlNady.Application/AlNady.Application.csproj   src/AlNady.Application/
COPY src/AlNady.Infrastructure/AlNady.Infrastructure.csproj src/AlNady.Infrastructure/
COPY src/AlNady.API/AlNady.API.csproj                   src/AlNady.API/
COPY tests/AlNady.UnitTests/AlNady.UnitTests.csproj     tests/AlNady.UnitTests/
COPY tests/AlNady.IntegrationTests/AlNady.IntegrationTests.csproj tests/AlNady.IntegrationTests/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build and publish Release
RUN dotnet publish src/AlNady.API/AlNady.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create uploads directory
RUN mkdir -p /app/uploads && chmod 777 /app/uploads

# Create logs directory
RUN mkdir -p /app/logs && chmod 777 /app/logs

# Copy published output
COPY --from=build /app/publish .

# Non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AlNady.API.dll"]
