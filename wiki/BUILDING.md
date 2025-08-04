# Ribosoft Building Guide

This guide provides step-by-step instructions for building the Ribosoft web service for ribozyme design. Ribosoft is a multi-component application with a .NET 8 backend, modern JavaScript frontend, and C++23 core algorithms.

## Prerequisites

Before building Ribosoft, ensure you have the following installed:

### Required Software

- **.NET 8 SDK**: Download from https://dotnet.microsoft.com/download/dotnet/8.0
- **Node.js ≥20.19.0**: Download from https://nodejs.org/ (Node.js 24.4.0 recommended)
- **npm ≥9.0.0**: Included with Node.js installation
- **C++ Compiler**: g++ with C++23 support (g++-11 or newer)
- **Python 3.5+**: For dependency management and build scripts
- **PostgreSQL**: Recommended database (or SQL Server as alternative)

### Platform Support

Currently, the project focuses on **Linux (Ubuntu)** as the primary development platform with C++23 modernization:

- ✅ **Linux (Ubuntu)**: Fully supported with automated builds
- 🚧 **macOS**: Temporarily disabled during modernization (will support Apple Silicon arm64)
- 🚧 **Windows**: Temporarily disabled during modernization (requires ViennaRNA dependency rebuild)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/t-vaudry/ribosoft.git
cd ribosoft/Ribosoft
```

### 2. Setup HTTPS Development Certificate (Optional)

For development with HTTPS support:

```bash
# Clean any existing certificates
dotnet dev-certs https --clean

# Generate and trust new development certificate
dotnet dev-certs https --trust
```

### 3. Install Dependencies

#### Install Node.js Dependencies
```bash
npm install
```

#### Install Python Dependencies
```bash
pip install pipenv
pipenv install
```

### 4. Build C++ Algorithm Library

The C++ algorithm library is a critical component that must be built before the main application:

```bash
# Install C++ dependencies (ViennaRNA, etc.)
pipenv run python ribosoft.py deps install --yes

# Navigate to algorithm library directory
cd RibosoftAlgo

# Build the native C++ library
./build-native.sh linux-x64 Release

# Return to main application directory
cd ../Ribosoft
```

### 5. Build Frontend Assets (Critical Step)

The frontend build is essential and must be completed before running the application:

```bash
# Development build (recommended for local development)
npm run build:all:dev

# OR Production build
npm run build:all
```

### 6. Configure Database

Edit the `appsettings.json` file with your database connection details:

```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft;Username=your_user;Password=your_password;Pooling=true;"
  },
  "EntityFrameworkProvider": "Npgsql"
}
```

### 7. Build and Run the Application

```bash
# Build the .NET application
dotnet build

# Run the application
dotnet run

# Application will be available at:
# HTTPS: https://localhost:5001/ (if certificate configured)
# HTTP:  http://localhost:5000/ (redirects to HTTPS if available)
```

## Detailed Build Process

### Frontend Build System

Ribosoft uses a modern JavaScript build system with Webpack. The build process is divided into vendor and application bundles:

#### Available npm Scripts

```bash
# Development watch mode (continuous rebuilding)
npm run dev

# Production builds
npm run build              # Build main application bundle
npm run build:vendor       # Build vendor libraries (Bootstrap, etc.)
npm run build:all          # Build both vendor and main bundles

# Development builds
npm run build:dev          # Build main application bundle (dev mode)
npm run build:vendor:dev   # Build vendor libraries (dev mode)
npm run build:all:dev      # Build both bundles (dev mode)

# Utility commands
npm run clean              # Clean build artifacts
npm run lint               # ESLint code checking
npm run format             # Prettier code formatting
```

#### Build Order

1. **Clean previous builds** (recommended):
   ```bash
   npm run clean
   ```

2. **Build vendor bundle** (contains third-party libraries):
   ```bash
   npm run build:vendor:dev
   ```

3. **Build application bundle**:
   ```bash
   npm run build:dev
   ```

4. **Or build everything at once**:
   ```bash
   npm run build:all:dev
   ```

### Backend Build Process

#### .NET Application Build

```bash
# Build the entire solution
dotnet build Ribosoft.sln --configuration Release

# Build specific project
dotnet build Ribosoft/Ribosoft.csproj --configuration Release

# Restore NuGet packages (if needed)
dotnet restore
```

#### Development vs Release Configuration

- **Debug Configuration**: Uses local NuGet packages for RibosoftAlgo (version 0.0.0)
- **Release Configuration**: Uses platform-specific NuGet packages:
  - Linux: RibosoftAlgo 2.2.0
  - Windows: RibosoftAlgo (when re-enabled)
  - macOS: RibosoftAlgo (when re-enabled)

### C++ Algorithm Library Build

The RibosoftAlgo library is the core computational component written in C++23 with a native build system (no longer uses CMake):

#### Build Process

```bash
cd RibosoftAlgo

# Use the native build script (recommended method)
./build-native.sh linux-x64 Release

# Return to main application directory
cd ../Ribosoft
```

The build script handles all compilation, linking, and test execution automatically. The C++23 modernization has moved away from CMake to a native build system for better performance and simpler maintenance.

## Development Workflow

### Setting Up Development Environment

1. **Clone and navigate to project**:
   ```bash
   git clone https://github.com/t-vaudry/ribosoft.git
   cd ribosoft/Ribosoft
   ```

2. **Install all dependencies**:
   ```bash
   npm install
   pip install pipenv
   pipenv install
   ```

3. **Build C++ dependencies and library**:
   ```bash
   pipenv run python ribosoft.py deps install --yes
   cd RibosoftAlgo
   ./build-native.sh linux-x64 Release
   cd ../Ribosoft
   ```

4. **Build frontend in watch mode**:
   ```bash
   npm run dev
   ```

5. **Run backend with hot reload** (in separate terminal):
   ```bash
   dotnet watch run
   ```

### Testing the Build

#### Frontend Tests
```bash
# Lint JavaScript code
npm run lint

# Format code
npm run format

# Verify build artifacts exist
ls -la wwwroot/dist/
```

#### Backend Tests
```bash
# Run .NET unit tests
dotnet test Ribosoft.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

#### C++ Tests
```bash
# C++ tests are integrated with .NET test framework
dotnet test RibosoftAlgo.Tests/RibosoftAlgo.Tests.csproj --configuration Release
```

## Troubleshooting

### Common Build Issues

#### Frontend Build Problems

**Issue**: Missing styles or JavaScript console errors
```bash
# Solution: Clean and rebuild frontend
npm run clean
npm run build:all:dev

# Verify vendor manifest exists
ls -la wwwroot/dist/vendor-manifest.json
```

**Issue**: Node.js version conflicts
```bash
# Check Node.js version
node --version  # Should be ≥20.19.0

# Update npm
npm install -g npm@latest
```

#### Backend Build Problems

**Issue**: NuGet package restore failures
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force
```

**Issue**: Database connection errors
```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Verify connection string in appsettings.json
# Ensure database exists and user has permissions
```

#### C++ Build Problems

**Issue**: Missing dependencies
```bash
# Reinstall C++ dependencies
pipenv run python ribosoft.py deps install --yes --force
```

**Issue**: Build script fails
```bash
# Ensure build script is executable
chmod +x RibosoftAlgo/build-native.sh

# Try rebuilding from clean state
cd RibosoftAlgo
rm -rf build/
./build-native.sh linux-x64 Release
```

**Issue**: Compiler version
```bash
# Check g++ version (needs C++23 support)
g++ --version

# Install newer compiler if needed (Ubuntu)
sudo apt update
sudo apt install g++-11
```

### Port Conflicts

Default application port is `50273`. If you encounter port conflicts:

1. Check current port usage:
   ```bash
   ss -tlnp | grep :50273
   ```

2. Modify port in `Properties/launchSettings.json`:
   ```json
   {
     "profiles": {
       "Ribosoft": {
         "applicationUrl": "https://localhost:5001;http://localhost:5000"
       }
     }
   }
   ```

### Build Verification

After completing the build process, verify everything is working:

1. **Check build artifacts**:
   ```bash
   # Frontend assets
   ls -la wwwroot/dist/

   # .NET build output
   ls -la bin/Debug/net8.0/

   # C++ library
   ls -la RibosoftAlgo/build/
   ```

2. **Run application**:
   ```bash
   dotnet run
   ```

3. **Access application**:
   - Open browser to `https://localhost:5001` or `http://localhost:5000`
   - Verify the application loads without console errors
   - Test basic functionality

## Docker Build (Alternative)

For containerized builds, use the provided Docker configuration:

```bash
# Build and run with Docker Compose
docker-compose up --build

# The application will be available at:
# HTTP:  http://localhost:5000
# HTTPS: https://localhost:5001 (if certificate configured)
```

## Production Build

For production deployment:

1. **Build with production configuration**:
   ```bash
   # Frontend production build
   npm run build:all

   # Backend production build
   dotnet build --configuration Release
   ```

2. **Publish the application**:
   ```bash
   dotnet publish --configuration Release --output ./publish
   ```

3. **Deploy using Docker** (recommended for production):
   ```bash
   docker-compose -f docker-compose.yml up -d
   ```

## Next Steps

After successfully building Ribosoft:

1. **Configure email settings** (Mailgun integration)
2. **Set up NCBI Datasets API** credentials
3. **Configure BLAST database** paths
4. **Set up monitoring and logging**
5. **Configure SSL certificates** for production

For detailed configuration and deployment information, refer to the main README.md file.

## Getting Help

If you encounter issues during the build process:

1. Check the [GitHub Issues](https://github.com/t-vaudry/ribosoft/issues)
2. Review the troubleshooting section above
3. Ensure all prerequisites are correctly installed
4. Verify you're using the supported platform (Linux/Ubuntu)

The Ribosoft project is actively maintained with comprehensive CI/CD pipelines and automated testing to ensure build reliability.
