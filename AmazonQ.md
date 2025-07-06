# Ribosoft Repository Analysis - Amazon Q Reference

## Project Overview
Ribosoft is a web service for designing different types of trans-acting conventional and allosteric ribozymes. It uses template secondary structures that can be submitted by users to design ribozymes according to user-provided parameters. The generated designs specifically target a transcript (or RNA sequence) given by the user.

**Live URL**: http://ribosoft2.fungalgenomics.ca

## Architecture & Technology Stack

### Backend (.NET/C#)
- **Framework**: ASP.NET Core 5.0 (net5.0)
- **Language**: C# with Entity Framework Core
- **Database**: PostgreSQL (primary) / SQL Server (alternative)
- **Background Jobs**: Hangfire with PostgreSQL storage
- **Authentication**: ASP.NET Core Identity
- **Logging**: NLog
- **Email**: SendGrid integration

### Frontend
- **Framework**: Vue.js 2.5.13 with Bootstrap 4.0.0
- **Build Tools**: Webpack 3.1.0, Node.js
- **UI Components**: Bootstrap-Vue, jQuery, jQuery UI
- **Visualization**: fornac (RNA structure visualization)
- **Additional**: QR code generation (qrious), structured filtering

### Core Algorithm (C++)
- **Language**: C++ with CMake build system (≥3.5)
- **Standard**: C++11
- **Parallelization**: OpenMP support
- **Testing**: Catch2 framework
- **Distribution**: NuGet packages (RibosoftAlgo)

### Development Tools
- **Python**: Dependency management and CLI tools
- **Testing**: Robot Framework for end-to-end testing
- **Documentation**: Doxygen
- **Containerization**: Docker with Docker Compose

## Project Structure

```
ribosoft/
├── Ribosoft/                    # Main ASP.NET Core web application
│   ├── Controllers/             # MVC controllers
│   ├── Models/                  # Data models and view models
│   ├── Views/                   # Razor views
│   ├── Data/                    # Entity Framework contexts and migrations
│   ├── Services/                # Business logic services
│   ├── Jobs/                    # Hangfire background jobs
│   ├── ClientApp/               # Vue.js frontend application
│   ├── wwwroot/                 # Static web assets
│   ├── Biology/                 # Biological computation logic
│   ├── CandidateGeneration/     # Ribozyme candidate generation
│   ├── MultiObjectiveOptimization/ # Optimization algorithms
│   ├── Blast/                   # BLAST integration
│   ├── GenbankRequests/         # GenBank API integration
│   └── RibosoftAlgo/            # C++ algorithm bindings
├── RibosoftAlgo/                # C++ core algorithm library
│   ├── src/                     # C++ source files
│   ├── include/                 # Header files
│   ├── test/                    # Catch2 unit tests
│   ├── build/                   # CMake build directory
│   ├── nuget/                   # NuGet package configuration
│   └── CMakeLists.txt           # CMake configuration
├── Ribosoft.Tests/              # xUnit test project
├── test/                        # Robot Framework integration tests
├── .github/                     # GitHub Actions workflows
├── docs/                        # Documentation
└── docker-compose.yml           # Docker orchestration
```

## Building the Project

### Prerequisites
- **Mono**: http://www.mono-project.com/download/
- **.NET 5.0**: https://www.microsoft.com/net/learn/get-started
- **CMake ≥3.5**: https://cmake.org/download/
- **Visual Studio 2019** with .NET Core development (Windows)
- **Node.js**: For frontend build process
- **Python 3.5+**: For dependency management

### Build Commands

#### C++ Algorithm Library (RibosoftAlgo)
```bash
cd RibosoftAlgo/build
cmake .. -G "Visual Studio 16 2019"  # Windows
cmake .. -G "Unix Makefiles"         # Linux/macOS
make  # or msbuild on Windows
```

#### .NET Web Application
```bash
# Build entire solution
dotnet build Ribosoft.sln --configuration Release

# Run the application
dotnet run --project Ribosoft

# Frontend assets (automatically handled in Debug mode)
cd Ribosoft
npm install
node node_modules/webpack/bin/webpack.js --config webpack.config.vendor.js
node node_modules/webpack/bin/webpack.js
```

#### Python Dependencies
```bash
# Install pipenv and dependencies
pip install pipenv
pipenv install

# Install C++ library dependencies
pipenv run python ribosoft.py deps install --yes
```

## Testing

### Unit Tests (.NET/xUnit)
```bash
dotnet test Ribosoft.Tests --configuration Release --verbosity normal
```

### C++ Tests (Catch2)
```bash
cd RibosoftAlgo/build
make tests  # Build test executable
./bin/tests  # Run tests
```

### Integration Tests (Robot Framework)
```bash
cd test
pipenv install
pipenv run robot --outputdir reports tests/startup.robot
```

### Code Coverage
- Uses coverlet for .NET code coverage
- Configuration in `coverlet.runsettings`
- Integrated with Codecov via GitHub Actions

## Deployment & DevOps

### Docker Support
The application is containerized with multi-service Docker Compose setup:

```yaml
# docker-compose.yml structure
services:
  db:          # PostgreSQL database
  ribosoft:    # Main web application
```

**Docker Commands:**
```bash
docker-compose up                    # Start all services
docker-compose up --build           # Rebuild and start
docker-compose down                  # Stop all services
```

**Published Images:**
- `tvaudryread/ribosoft:latest` (master branch)
- `tvaudryread/ribosoft:beta` (develop branch)

### GitHub Actions CI/CD Pipeline

#### Workflows:
1. **dotnet-core.yml**: .NET builds on Ubuntu/Windows
2. **cmake.yml**: C++ builds on Ubuntu/Windows/macOS
3. **robot.yml**: Robot Framework integration tests
4. **docker-publish.yml**: Docker image publishing
5. **publish-nuget.yml**: NuGet package publishing
6. **deploy.yml**: Production deployment to FungalGenomics
7. **codecov.yml**: Code coverage reporting
8. **uptime.yml**: Service uptime monitoring

#### Branch Strategy:
- **master**: Production deployments, latest Docker tags
- **develop**: Beta deployments, beta Docker tags
- **feature branches**: CI builds only

### Environment Configuration

#### Database Connections:
```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-Ribosoft-...",
    "NpgsqlConnection": "Username=postgres;Password=;Server=localhost;Port=5432;Database=ribosoft;..."
  },
  "EntityFrameworkProvider": "SqlServer" // or "Npgsql"
}
```

#### Environment Variables:
- `DB_CONTEXT`: Database context type
- `EntityFrameworkProvider`: Database provider
- `NODE_ENV`: Node.js environment
- `BLASTDB`: BLAST database path
- `NumThreads`: BLAST thread count

## Key Features & Components

### Core Functionality:
1. **Ribozyme Design**: Template-based ribozyme generation
2. **Multi-Objective Optimization**: Advanced optimization algorithms
3. **BLAST Integration**: Sequence similarity searching
4. **GenBank Integration**: Sequence data retrieval
5. **Background Processing**: Hangfire job queue system
6. **User Management**: ASP.NET Core Identity
7. **Data Visualization**: RNA structure visualization

### API Endpoints:
- `/Jobs`: Job management and monitoring
- `/Ribozymes`: Ribozyme template management
- `/Designs`: Design result management
- `/Request`: Design request submission
- `/Account`: User authentication
- `/Manage`: User profile management

### Database Schema:
- **Jobs**: Background job tracking
- **Ribozymes**: Template ribozyme structures
- **Designs**: Generated design results
- **Users**: ASP.NET Core Identity tables
- **Assemblies**: Sequence assembly data

## Development Workflow

### Local Development Setup:
1. Clone repository
2. Install prerequisites (.NET 5.0, Node.js, CMake)
3. Build C++ dependencies: `pipenv run python ribosoft.py deps install --yes`
4. Build C++ library: `cd RibosoftAlgo/build && cmake .. && make`
5. Install Node.js dependencies: `cd Ribosoft && npm install`
6. Build frontend: `npm run build`
7. Run application: `dotnet run --project Ribosoft`

### Debug Configuration:
- ✅ Uses local NuGet packages for RibosoftAlgo (version 0.0.0)
- ✅ Automatic webpack builds in Debug mode
- ✅ Local C++ library compilation and NuGet package creation
- ✅ All C++ tests passing (23 test cases, 63 assertions)
- SQL Server LocalDB for development database

### Release Configuration:
- Platform-specific NuGet packages:
  - Linux: RibosoftAlgo 1.1.0
  - Windows: RibosoftAlgo 1.1.1
  - macOS: RibosoftAlgo 1.1.2

## Security & Best Practices

### Security Features:
- ASP.NET Core Identity for authentication
- User secrets for sensitive configuration
- HTTPS enforcement
- SQL injection protection via Entity Framework
- XSS protection via Razor views

### Code Quality:
- CodeFactor integration for code quality analysis
- Comprehensive unit test coverage
- Integration testing with Robot Framework
- Doxygen documentation for C++ code
- NLog structured logging

## Troubleshooting Common Issues

### Build Issues:
1. **Node.js not found**: Ensure Node.js is installed and in PATH
2. **CMake version**: Requires CMake ≥3.5
3. **NuGet restore**: Check platform-specific package versions
4. **Webpack build**: Run `npm install` in Ribosoft directory

### Runtime Issues:
1. **Database connection**: Check connection strings in appsettings.json
2. **Missing dependencies**: Run dependency installation script
3. **Port conflicts**: Default ports 5001 (app), 5432 (PostgreSQL)

### Docker Issues:
1. **Build context**: Ensure Dockerfile context is correct
2. **Volume permissions**: Check PostgreSQL data volume permissions
3. **Network connectivity**: Verify service-to-service communication

## Performance Considerations

### Optimization Features:
- OpenMP parallelization in C++ algorithms
- Hangfire background job processing
- Database connection pooling
- Webpack asset optimization
- CDN-ready static asset structure

### Scalability:
- Stateless web application design
- Background job queue for long-running tasks
- Database-agnostic Entity Framework implementation
- Docker containerization for horizontal scaling

## License & Contributing
- **License**: GNU General Public License v3.0
- **Repository**: GitHub with comprehensive CI/CD pipeline
- **Issue Tracking**: GitHub Issues
- **Documentation**: Doxygen for C++, inline documentation for C#

This comprehensive analysis provides all necessary information for building, testing, deploying, and maintaining the Ribosoft application across all its components and environments.
