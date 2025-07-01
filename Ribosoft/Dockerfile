# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

# Install Node.js 20 and build tools
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    ca-certificates \
    && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install Mono for legacy compatibility if needed
RUN apt-get update && apt-get install -y \
    dirmngr \
    gnupg \
    apt-transport-https \
    software-properties-common \
    && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
    && apt-add-repository 'deb https://download.mono-project.com/repo/ubuntu stable-jammy main' \
    && apt-get update \
    && apt-get install -y mono-complete \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy package files first for better caching
COPY package*.json ./
RUN npm ci --only=production

# Copy project files
COPY *.csproj ./
RUN dotnet restore

# Copy source code
COPY . .

# Copy Docker-specific configuration files
COPY .docker/appsettings.json ./appsettings.json
COPY .docker/20210327162902_PreloadRibozymes.cs ./Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.cs
COPY .docker/20210327162902_PreloadRibozymes.Designer.cs ./Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.Designer.cs

# Build frontend assets
RUN npm run build:vendor
RUN npm run build

# Build .NET application
RUN dotnet build "Ribosoft.csproj" -c Release --no-restore

# Publish application
RUN dotnet publish "Ribosoft.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime

# Install runtime dependencies
RUN apt-get update && apt-get install -y \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r ribosoft && useradd -r -g ribosoft ribosoft

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Copy migration script
COPY .docker/migrations.sh .
RUN chmod +x ./migrations.sh

# Set ownership
RUN chown -R ribosoft:ribosoft /app

# Switch to non-root user
USER ribosoft

EXPOSE 80
EXPOSE 5432

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

CMD ["/bin/bash", "./migrations.sh"]