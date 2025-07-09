# Ribosoft

Ribosoft is a web service to design different types of trans-acting conventional and allosteric ribozymes. Ribosoft uses template secondary structures that can be submitted by users to design ribozymes in accordance with parameters provided by the user. The generated designs specifically target a transcript (or, generally, an RNA sequence) given by the user.

URL : http://ribosoft2.fungalgenomics.ca

![Ribosoft2 FungalGenomics Uptime](https://github.com/t-vaudry/ribosoft/workflows/Ribosoft2%20FungalGenomics%20Uptime/badge.svg)
![Deployment to FungalGenomics](https://github.com/t-vaudry/ribosoft/workflows/Deployment%20to%20FungalGenomics/badge.svg?branch=master)
[![codecov](https://codecov.io/gh/t-vaudry/ribosoft/branch/develop/graph/badge.svg?token=nExWlRXew4)](https://codecov.io/gh/t-vaudry/ribosoft)
[![CodeFactor](https://www.codefactor.io/repository/github/t-vaudry/ribosoft/badge?s=f9a38174a3b592c768d1bfefc6316c85fb6925d3)](https://www.codefactor.io/repository/github/t-vaudry/ribosoft)
![Publish Docker image to GitHub Packages](https://github.com/t-vaudry/ribosoft/workflows/Publish%20Docker%20image%20to%20GitHub%20Packages/badge.svg)
![Publish RibosoftAlgo Nuget Package](https://github.com/t-vaudry/ribosoft/workflows/Publish%20RibosoftAlgo%20Nuget%20Package/badge.svg)
![Ribosoft .NET Core Builds](https://github.com/t-vaudry/ribosoft/workflows/Ribosoft%20.NET%20Core%20Builds/badge.svg)
![RibosoftAlgo CMake Builds](https://github.com/t-vaudry/ribosoft/workflows/RibosoftAlgo%20CMake%20Builds/badge.svg)
[![Robot Framework Testing](https://github.com/t-vaudry/ribosoft/actions/workflows/robot.yml/badge.svg)](https://github.com/t-vaudry/ribosoft/actions/workflows/robot.yml)

## Technology Stack (Modernized)

### Backend (.NET 8)
- **Framework**: ASP.NET Core 8.0
- **Language**: C# with Entity Framework Core
- **Database**: PostgreSQL (primary) / SQL Server (alternative)
- **Background Jobs**: Hangfire with PostgreSQL storage
- **Authentication**: ASP.NET Core Identity

### Frontend (Modern)
- **Framework**: Vue.js 3.4.21 with Bootstrap 5.3.3
- **Build Tools**: Webpack 5.99.9, Node.js ≥18.0.0, npm ≥9.0.0
- **UI Components**: Bootstrap-Vue-Next, modern ES2024 support
- **Visualization**: fornac (RNA structure visualization)
- **Bundling**: Modern DLL-based vendor/app bundle splitting

### Core Algorithm (C++23)
- **Language**: C++23 with CMake build system (≥3.5)
- **Parallelization**: OpenMP support
- **Testing**: Catch2 framework
- **Distribution**: NuGet packages (RibosoftAlgo)

## Platform Support

- ✅ **Linux (Ubuntu)**: Primary development platform with C++23 modernization and automated builds
- 🚧 **macOS**: Disabled for modernization - will be rebuilt for Apple Silicon (arm64) natively  
- 🚧 **Windows**: Disabled for modernization - requires ViennaRNA dependency rebuild

> **Development Focus**: Currently focusing on Ubuntu/Linux builds with C++23 modernization. macOS and Windows support will be reintroduced with proper native architecture support (Apple Silicon arm64 for macOS) and updated dependencies as part of the modernization effort.

## Prerequisites

- **.NET 8 SDK**: https://dotnet.microsoft.com/download/dotnet/8.0
- **Node.js ≥18.0.0**: https://nodejs.org/
- **npm ≥9.0.0**: Included with Node.js
- **CMake ≥3.5**: https://cmake.org/download/
- **Python 3.5+**: For dependency management
- **PostgreSQL**: Recommended database (or SQL Server)

## Quick Start

### 1. Clone and Setup
```bash
git clone https://github.com/t-vaudry/ribosoft.git
cd ribosoft/Ribosoft
```

### 2. Install Dependencies
```bash
# Install Node.js dependencies
npm install

# Install Python dependencies (optional, for advanced features)
pip install pipenv
pipenv install
```

### 3. Build C++ Algorithm Library
```bash
cd ../RibosoftAlgo/build
cmake .. -G "Unix Makefiles"
make
cd ../../Ribosoft
```

### 4. Build Frontend (Critical Step)
```bash
# Development build (recommended for local development)
npm run build:all:dev

# OR Production build
npm run build:all
```

### 5. Configure Database
Edit `appsettings.json` with your database connection:
```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft;Username=your_user;Password=your_password;Pooling=true;"
  },
  "EntityFrameworkProvider": "Npgsql"
}
```

### 6. Run Application
```bash
# Build and run the .NET application
dotnet build
dotnet run

# Application will be available at: http://localhost:50273/
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
cd RibosoftAlgo/build
make tests
./bin/tests
```

### .NET Unit Tests (xUnit)
```bash
dotnet test Ribosoft.Tests
```

### Integration Tests (Robot Framework)
```bash
cd test
pipenv install
pipenv run robot --outputdir reports tests/startup.robot
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
cd RibosoftAlgo/build
make clean
cmake .. && make
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
│   ├── ClientApp/               # Vue.js 3 frontend application
│   ├── Controllers/             # MVC controllers
│   ├── Models/                  # Data models and view models
│   ├── Views/                   # Razor views
│   ├── Data/                    # Entity Framework contexts
│   ├── Services/                # Business logic services
│   ├── Jobs/                    # Hangfire background jobs
│   └── wwwroot/dist/            # Built frontend assets
├── RibosoftAlgo/                # C++23 core algorithm library
│   ├── src/                     # C++ source files
│   ├── include/                 # Header files
│   ├── test/                    # Catch2 unit tests
│   └── build/                   # CMake build directory
├── Ribosoft.Tests/              # xUnit test project
└── test/                        # Robot Framework integration tests
```

### Key Features
- **Ribozyme Design**: Template-based ribozyme generation
- **Multi-Objective Optimization**: Advanced optimization algorithms
- **BLAST Integration**: Sequence similarity searching
- **GenBank Integration**: Sequence data retrieval
- **Background Processing**: Hangfire job queue system
- **User Management**: ASP.NET Core Identity
- **RNA Visualization**: Interactive structure visualization

## Docker Support

Ribosoft has been published as a docker application that can be used locally to run the service for larger jobs. This can help have control over the resources allocated to your service, and succeed in larger queries.

To achieve the proper configuration, Docker Compose is used.

### Docker Compose

Below is an example docker-compose.yml file.

```yaml
version: '3.8'

volumes:
  pgdata:

services:
  db:
     image: postgres:latest
     container_name: db
     restart: always
     ports:
       - 5432:5432
     environment:
       POSTGRES_USER: postgres
       POSTGRES_PASSWORD: postgres
       POSTGRES_DB: ribosoft
     volumes:
       - pgdata:/var/lib/postgresql/data

  ribosoft:
    image: tvaudryread/ribosoft:latest
    container_name: ribosoft
    ports:
      - 5001:80
    environment:
      ConnectionStrings__NpgsqlConnection: "Host=db;Port=5432;Username=postgres;Password=postgres;Database=ribosoft;Pooling=true;"
      DB_CONTEXT: NpgsqlDbContext
      EntityFrameworkProvider: Npgsql
      NODE_ENV: production
    depends_on:
      - "db"
```

The docker compose command to start the service is;

`docker-compose up`

The service will run at http://localhost:5001/

## Development Notes

### Frontend Modernization
The frontend has been completely modernized with:
- **Vue.js 3.4.21**: Composition API, improved TypeScript support
- **Bootstrap 5.3.3**: Modern CSS framework with improved accessibility
- **Webpack 5.99.9**: Modern bundling with improved tree-shaking and caching
- **ES2024**: Latest JavaScript features and syntax
- **Modern Build Pipeline**: Separate vendor and application bundles for optimal loading

### C++23 Modernization
The core algorithm library has been updated to:
- **C++23 Standard**: Latest language features and improvements
- **Modern CMake**: Improved build system configuration
- **Enhanced Testing**: Comprehensive Catch2 test suite
- **Cross-platform**: Focus on Linux with planned macOS/Windows support

### Performance Optimizations
- **DLL-based Bundling**: Vendor libraries cached separately from application code
- **Code Splitting**: Lazy loading of route-specific components
- **Modern Caching**: Webpack 5 persistent caching for faster rebuilds
- **OpenMP Parallelization**: Multi-threaded C++ algorithm execution

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes following the modernized architecture
4. Ensure all tests pass (`npm test`, `dotnet test`, `make tests`)
5. Run code formatting (`npm run format`, `npm run lint`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## License
[GNU General Public License v3.0](https://choosealicense.com/licenses/gpl-3.0/)

## Acknowledgments

- Built with modern web technologies for optimal performance
- Scientific computing powered by C++23 algorithms
- Responsive design with Bootstrap 5 and Vue.js 3
- Continuous integration and deployment via GitHub Actions
