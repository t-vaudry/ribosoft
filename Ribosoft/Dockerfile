# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

# Install Node.js 24.x for frontend build (required for Vite 7)
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    ca-certificates \
    && curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Verify Node.js version
RUN node --version && npm --version

WORKDIR /src

# Copy and restore .NET dependencies for Ribosoft project only
COPY Ribosoft/Ribosoft.csproj ./
RUN dotnet restore "Ribosoft.csproj"

# Copy Node.js dependencies and install
COPY Ribosoft/package*.json ./
RUN npm ci

# Copy Ribosoft source code
COPY Ribosoft/ ./

# Copy Docker-specific configuration files
COPY Ribosoft/.docker/appsettings.json ./appsettings.json
COPY Ribosoft/.docker/20210327162902_PreloadRibozymes.cs ./Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.cs
COPY Ribosoft/.docker/20210327162902_PreloadRibozymes.Designer.cs ./Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.Designer.cs

# Build frontend assets with Vite 7
RUN npm run build:vendor
RUN npm run build

# Build and publish .NET application in Release mode
RUN dotnet build "Ribosoft.csproj" --configuration Release --no-restore
RUN SkipNpmBuild=true dotnet publish "Ribosoft.csproj" --configuration Release --no-build --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS runtime

# Install minimal runtime dependencies
RUN apt-get update && apt-get install -y \
    curl \
    postgresql-client \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r ribosoft && useradd -r -g ribosoft ribosoft

WORKDIR /app

# Install Entity Framework tools for migrations
RUN dotnet tool install dotnet-ef --tool-path /app/.dotnet/tools
ENV PATH="${PATH}:/app/.dotnet/tools"

# Copy published application
COPY --from=build /app/publish .

# Copy migration script
COPY Ribosoft/.docker/migrations.sh .
RUN chmod +x ./migrations.sh

# Set ownership
RUN chown -R ribosoft:ribosoft /app

# Switch to non-root user
USER ribosoft

EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

CMD ["/bin/bash", "./migrations.sh"]