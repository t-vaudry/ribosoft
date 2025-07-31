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

# Copy project files for restore
COPY Ribosoft/Ribosoft.csproj ./Ribosoft/
COPY NCBI.Datasets.API/NCBI.Datasets.API.csproj ./NCBI.Datasets.API/

# Restore .NET dependencies in Release configuration
RUN dotnet restore "Ribosoft/Ribosoft.csproj" -p:Configuration=Release

# Copy Node.js dependencies and install
COPY Ribosoft/package*.json ./Ribosoft/
WORKDIR /src/Ribosoft
RUN npm ci

# Copy all source code
WORKDIR /src
COPY Ribosoft/ ./Ribosoft/
COPY NCBI.Datasets.API/ ./NCBI.Datasets.API/

# Create dummy appsettings.json for NCBI.Datasets.API to avoid build conflicts
RUN echo '{}' > ./NCBI.Datasets.API/appsettings.json

# Modify NCBI.Datasets.API project to exclude appsettings from publish output
RUN sed -i 's/<None Include="appsettings\.json" CopyToOutputDirectory="PreserveNewest" \/>/<None Include="appsettings.json" CopyToOutputDirectory="Never" \/>/' ./NCBI.Datasets.API/NCBI.Datasets.API.csproj
RUN sed -i 's/<None Include="appsettings\.Development\.json" CopyToOutputDirectory="PreserveNewest" Condition="Exists.*" \/>/<None Include="appsettings.Development.json" CopyToOutputDirectory="Never" Condition="Exists('\''appsettings.Development.json'\'')" \/>/' ./NCBI.Datasets.API/NCBI.Datasets.API.csproj

# Copy Docker-specific configuration files
COPY Ribosoft/.docker/appsettings.json ./Ribosoft/appsettings.json

# Copy Docker-specific migration files to the PostgreSQL migrations folder
COPY Ribosoft/.docker/20210327162902_PreloadRibozymes.cs ./Ribosoft/Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.cs
COPY Ribosoft/.docker/20210327162902_PreloadRibozymes.Designer.cs ./Ribosoft/Data/Migrations/NpgsqlMigrations/20210327162902_PreloadRibozymes.Designer.cs

# Build frontend assets
WORKDIR /src/Ribosoft
RUN npm run build:vendor
RUN npm run build

# Build and publish .NET application in Release mode
RUN dotnet build "Ribosoft.csproj" --configuration Release --no-restore
RUN SkipNpmBuild=true dotnet publish "Ribosoft.csproj" --configuration Release --no-build --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime

# Install minimal runtime dependencies
RUN apt-get update && apt-get install -y \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r ribosoft && useradd -r -g ribosoft ribosoft

WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Set ownership
RUN chown -R ribosoft:ribosoft /app

# Switch to non-root user
USER ribosoft

EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Start the application directly (migrations are handled automatically on startup)
CMD ["dotnet", "Ribosoft.dll", "--urls", "http://*:80"]