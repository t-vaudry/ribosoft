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

# Install Python 3.13 from deadsnakes PPA
RUN apt-get update && apt-get install -y software-properties-common \
    && add-apt-repository ppa:deadsnakes/ppa \
    && apt-get update \
    && apt-get install -y python3.13 python3.13-dev python3.13-venv \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Set up pip for Python 3.13
RUN python3.13 -m ensurepip --upgrade \
    && python3.13 -m pip install --upgrade pip

# Install Mono from Ubuntu repositories
RUN apt-get update && apt-get install -y \
    mono-complete \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy Python dependency files and install Python dependencies
COPY Pipfile Pipfile.lock ./

# Install Python dependencies directly with pip
RUN python3.13 -m pip install \
    'click>=8.1,<9.0' \
    'click-log>=0.4,<0.5' \
    'requests>=2.31,<3.0' \
    'urllib3>=2.0,<3.0' \
    'aiohttp>=3.9,<4.0' \
    'aiofiles>=23.0,<24.0'

# Install C++ dependencies using Python 3.13
COPY ribosoft.py ./
RUN python3.13 ribosoft.py deps install --yes

# Copy package files first for better caching
COPY Ribosoft/package*.json ./
RUN npm ci

# Copy project files
COPY Ribosoft/*.csproj ./
RUN dotnet restore

# Copy source code
COPY Ribosoft/ .

# Copy Docker-specific configuration files
COPY Ribosoft/.docker/appsettings.json ./appsettings.json
COPY Ribosoft/.docker/20210327162902_PreloadRibozymes.cs ./Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.cs
COPY Ribosoft/.docker/20210327162902_PreloadRibozymes.Designer.cs ./Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.Designer.cs

# Build frontend assets
RUN npm run build:vendor
RUN npm run build

# Build .NET application
RUN dotnet build "Ribosoft.csproj" -c Release --no-restore

# Publish application (skip npm build since we already built frontend assets)
RUN dotnet publish "Ribosoft.csproj" -c Release -o /app/publish --no-restore -p:SkipNpmBuild=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS runtime

# Install runtime dependencies
RUN apt-get update && apt-get install -y \
    curl \
    postgresql-client \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user first
RUN groupadd -r ribosoft && useradd -r -g ribosoft ribosoft

WORKDIR /app

# Install Entity Framework tools in /app/.dotnet/tools (accessible to ribosoft user)
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
EXPOSE 5432

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

CMD ["/bin/bash", "./migrations.sh"]