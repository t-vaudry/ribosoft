# Ribosoft

Ribosoft is a web service to design different types of trans-acting conventional and allosteric ribozymes. Ribosoft uses template secondary structures that can be submitted by users to design ribozymes in accordance with parameters provided by the user. The generated designs specifically target a transcript (or, generally, an RNA sequence) given by the user.

**Live URL**: https://ribosoft2.vaudryread.ca

[![CodeQL](https://github.com/t-vaudry/ribosoft/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/t-vaudry/ribosoft/actions/workflows/github-code-scanning/codeql)
[![codecov](https://codecov.io/gh/t-vaudry/ribosoft/branch/develop/graph/badge.svg?token=nExWlRXew4)](https://codecov.io/gh/t-vaudry/ribosoft)
[![CodeFactor](https://www.codefactor.io/repository/github/t-vaudry/ribosoft/badge?s=f9a38174a3b592c768d1bfefc6316c85fb6925d3)](https://www.codefactor.io/repository/github/t-vaudry/ribosoft)
[![Publish Docker Image to GitHub Container Registry](https://github.com/t-vaudry/ribosoft/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/t-vaudry/ribosoft/actions/workflows/docker-publish.yml)
[![Publish RibosoftAlgo NuGet Package](https://github.com/t-vaudry/ribosoft/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/t-vaudry/ribosoft/actions/workflows/publish-nuget.yml)
[![Ribosoft .NET 8 Build and Test (Ubuntu)](https://github.com/t-vaudry/ribosoft/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/t-vaudry/ribosoft/actions/workflows/dotnet-core.yml)
[![RibosoftAlgo Build and Test](https://github.com/t-vaudry/ribosoft/actions/workflows/ribosoft-algo-build.yml/badge.svg)](https://github.com/t-vaudry/ribosoft/actions/workflows/ribosoft-algo-build.yml)

## Technology Stack

### Backend (.NET 8)
- **Framework**: ASP.NET Core 8.0
- **Language**: C# with Entity Framework Core
- **Database**: PostgreSQL (primary) / SQL Server (alternative)
- **Background Jobs**: Hangfire with PostgreSQL/SQL Server storage
- **Authentication**: ASP.NET Core Identity
- **Logging**: NLog
- **Email**: Mailgun integration

### Frontend
- **Framework**: Modern JavaScript with Bootstrap 5.3.3
- **Build Tools**: Webpack 5.91.0, Node.js ≥20.19.0, npm ≥9.0.0
- **UI Components**: Bootstrap, jQuery, jQuery UI
- **Visualization**: fornac (RNA structure visualization)
- **Additional Libraries**: QR code generation (qrious), structured filtering

### Core Algorithm (C++23)
- **Language**: C++23 with native build system
- **Parallelization**: OpenMP support
- **Testing**: Catch2 framework
- **Distribution**: NuGet packages (RibosoftAlgo 2.2.0)

## Platform Support

- ✅ **Linux (Ubuntu)**: Primary development platform with C++23 modernization and automated builds
- 🚧 **macOS**: Disabled for modernization - will be rebuilt for Apple Silicon (arm64) natively  
- 🚧 **Windows**: Disabled for modernization - requires ViennaRNA dependency rebuild

> **Development Focus**: Currently focusing on Ubuntu/Linux builds with C++23 modernization. macOS and Windows support will be reintroduced with proper native architecture support (Apple Silicon arm64 for macOS) and updated dependencies as part of the modernization effort.

## Prerequisites

- **.NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
- **Node.js ≥20.19.0**: https://nodejs.org/ (Node.js 24.4.0 recommended)
- **npm ≥9.0.0**: Included with Node.js
- **C++ Compiler**: g++ with C++23 support (g++-11 or newer)
- **Python 3.5+**: For dependency management
- **PostgreSQL**: Recommended database (or SQL Server)

## Quick Start

### 1. Clone and Setup
```bash
git clone https://github.com/t-vaudry/ribosoft.git
cd ribosoft/Ribosoft
```

### 2. Setup HTTPS (Optional)
For development with HTTPS:
```bash
# Generate and trust development certificate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### 3. Install Dependencies
```bash
# Install Node.js dependencies
npm install

# Install Python dependencies (optional, for advanced features)
pip install pipenv
pipenv install
```

### 4. Install C++ Dependencies and Build Algorithm Library
```bash
# Install Python dependencies (required for C++ dependencies)
pip install pipenv
pipenv install

# Install C++ dependencies (ViennaRNA, etc.)
pipenv run python ribosoft.py deps install --yes

# Build C++ algorithm library
cd RibosoftAlgo
./build-native.sh linux-x64 Release
cd ../Ribosoft
```

### 5. Build Frontend (Critical Step)
```bash
# Development build (recommended for local development)
npm run build:all:dev

# OR Production build
npm run build:all
```

### 6. Configure Database
Edit `appsettings.json` with your database connection:
```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft;Username=your_user;Password=your_password;Pooling=true;"
  },
  "EntityFrameworkProvider": "Npgsql"
}
```

### 7. Run Application
```bash
# Build and run the .NET application
dotnet build
dotnet run

# Application will be available at:
# HTTPS: https://localhost:5001/ (if certificate configured)
# HTTP:  http://localhost:5000/ (redirects to HTTPS if available)
```

## Development Workflow

### Frontend Development
```bash
# Watch mode for continuous frontend rebuilding
npm run dev

# Clean and rebuild everything
npm run clean
npm run build:all:dev

# Individual builds
npm run build:vendor:dev    # Build vendor libraries (Bootstrap, Vue, etc.)
npm run build:dev          # Build application code
```

### Backend Development
```bash
# Run with hot reload
dotnet watch run

# Run tests
dotnet test
```

### Available npm Scripts
- `npm run dev` - Development watch mode
- `npm run build` - Production build (main bundle)
- `npm run build:dev` - Development build (main bundle)
- `npm run build:vendor` - Production vendor bundle
- `npm run build:vendor:dev` - Development vendor bundle
- `npm run build:all` - Build both vendor and main (production)
- `npm run build:all:dev` - Build both vendor and main (development)
- `npm run clean` - Clean build artifacts
- `npm run lint` - ESLint code checking
- `npm run format` - Prettier code formatting

## Testing

### C++ Unit Tests (Catch2)
```bash
# C++ tests are run through the .NET test framework
dotnet test RibosoftAlgo.Tests/RibosoftAlgo.Tests.csproj --configuration Release
```

### .NET Unit Tests (xUnit)
```bash
dotnet test Ribosoft.Tests
```

## Troubleshooting

### Frontend Build Issues
```bash
# If styles are missing or console errors occur
npm run clean
npm run build:all:dev

# Check that vendor-manifest.json exists
ls -la wwwroot/dist/vendor-manifest.json
```

### Database Connection Issues
- Ensure PostgreSQL is running: `sudo systemctl status postgresql`
- Verify connection string in `appsettings.json`
- Check database exists and user has proper permissions

### C++ Library Issues
```bash
# Rebuild C++ components
cd RibosoftAlgo
./build-native.sh linux-x64 Release
```

### Port Conflicts
- Default application port: `50273`
- Check `Properties/launchSettings.json` for configuration
- Verify port availability: `ss -tlnp | grep :50273`

## Architecture Overview

### Project Structure
```
ribosoft/
├── Ribosoft/                    # Main ASP.NET Core web application
│   ├── ClientApp/               # Modern JavaScript frontend application
│   ├── Controllers/             # MVC controllers
│   ├── Models/                  # Data models and view models
│   ├── Views/                   # Razor views
│   ├── Data/                    # Entity Framework contexts
│   ├── Services/                # Business logic services
│   ├── Jobs/                    # Hangfire background jobs
│   └── wwwroot/dist/            # Built frontend assets
├── RibosoftAlgo/                # C++23 core algorithm library
│   ├── src/                     # C++ source files
│   ├── build/                   # Build directory
│   └── build-native.sh          # Native build script
├── Ribosoft.Tests/              # xUnit test project
├── RibosoftAlgo.Tests/          # C++ algorithm test project
├── NCBI.Datasets.API/           # NCBI integration library
└── docs/                        # Documentation
```

### Key Features
- **Ribozyme Design**: Template-based ribozyme generation
- **Multi-Objective Optimization**: Advanced optimization algorithms
- **BLAST Integration**: Sequence similarity searching
- **GenBank Integration**: Sequence data retrieval
- **Background Processing**: Hangfire job queue system
- **User Management**: ASP.NET Core Identity
- **RNA Visualization**: Interactive structure visualization
- **Enhanced Accessibility Scoring**: Section-based analysis of binding site accessibility (improved from binary approach)

## Docker Support

Ribosoft has been published as a docker application that can be used locally to run the service for larger jobs. This can help have control over the resources allocated to your service, and succeed in larger queries.

To achieve the proper configuration, Docker Compose is used.

### Docker Compose with HTTPS

Below is an example docker-compose.yml file with HTTPS support and all required environment variables.

```yaml
version: '3.8'

volumes:
  pgdata:
  ribosoft_logs:
  ribosoft_certificates:

services:
  db:
     image: postgres:latest
     container_name: ribosoft_db
     restart: unless-stopped
     ports:
       - 5432:5432
     environment:
       POSTGRES_USER: postgres
       POSTGRES_PASSWORD: postgres
       POSTGRES_DB: ribosoft
     volumes:
       - pgdata:/var/lib/postgresql/data

  ribosoft:
    container_name: ribosoft_app
    restart: unless-stopped
    ports:
      - "5000:80"    # HTTP port (redirects to HTTPS)
      - "5001:443"   # HTTPS port
    build:
      context: .
      dockerfile: Ribosoft/Dockerfile
    environment:
      # ASP.NET Core Configuration
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: https://+:443;http://+:80
      
      # Database Configuration
      ConnectionStrings__NpgsqlConnection: "Host=db;Port=5432;Username=postgres;Password=postgres;Database=ribosoft;Pooling=true;"
      DB_CONTEXT: NpgsqlDbContext
      EntityFrameworkProvider: Npgsql
      
      # Frontend Build Configuration
      NODE_ENV: production
      
      # Email Configuration (Mailgun)
      MailgunAPIKey: "your-mailgun-api-key"
      MailgunDomain: "your-mailgun-domain.com"
      SenderEmail: "noreply@your-domain.com"
      SenderName: "Ribosoft"
      
      # NCBI Datasets API Configuration
      NCBIDatasetsApi__ApiKey: "your-ncbi-api-key"
      NCBIDatasetsApi__BaseUrl: "https://api.ncbi.nlm.nih.gov/datasets/v2"
      NCBIDatasetsApi__TimeoutSeconds: "30"
      NCBIDatasetsApi__MaxRetryAttempts: "3"
      NCBIDatasetsApi__RetryDelaySeconds: "1"
      
      # BLAST Configuration
      Assemblies__Path: "/app/blastdb"
      Assemblies__NumThreads: "4"
      Assemblies__MakeBlastDbPath: "makeblastdb"
      Assemblies__AutoCreateBlastDatabase: "true"
      Assemblies__MaxBlastDbFileSizeMB: "500"
      Assemblies__SkipBlastDbForLargeFiles: "false"
      Assemblies__CleanupAfterProcessing: "true"
      Assemblies__CleanupZipFiles: "true"
      
      # Logging Configuration
      Logging__LogLevel__Default: "Information"
      Logging__LogLevel__Microsoft: "Warning"
      Logging__LogLevel__Microsoft.Hosting.Lifetime: "Information"
      
      # Security Configuration
      ASPNETCORE_HTTPS_PORT: "443"
      
    volumes:
      - ribosoft_logs:/app/logs
      - ribosoft_certificates:/app/certificates
      # Mount your certificate directory:
      # - ./certificates:/app/certificates:ro
      # Mount BLAST database directory:
      # - ./blastdb:/app/blastdb
    depends_on:
      - "db"
```

### Quick Docker Setup

```bash
# Start the services
docker-compose up -d

# The service will run at:
# HTTP:  http://localhost:5000 (redirects to HTTPS)
# HTTPS: https://localhost:5001 (if certificate is provided)
```

### Production Deployment with Your Certificate

1. **Place your certificate** (in .pfx format) in a `certificates` directory
2. **Uncomment the certificate volume mount** in docker-compose.yml:
   ```yaml
   volumes:
     - ./certificates:/app/certificates:ro
   ```
3. **Update certificate path** in `Ribosoft/.docker/appsettings.json`
4. **Start the containers**:
   ```bash
   docker-compose up -d
   ```

**Note**: For production, use certificates from trusted Certificate Authorities (Let's Encrypt, etc.).

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes following the modernized architecture
4. Ensure all tests pass (`npm test`, `dotnet test`)
5. Run code formatting (`npm run format`, `npm run lint`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## License
[GNU General Public License v3.0](https://choosealicense.com/licenses/gpl-3.0/)

## Acknowledgments

- Built with modern web technologies for optimal performance
- Scientific computing powered by C++23 algorithms
- Responsive design with Bootstrap 5 and modern JavaScript
- Continuous integration and deployment via GitHub Actions
