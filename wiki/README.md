# Ribosoft Wiki

Welcome to the Ribosoft documentation wiki. This collection of guides provides comprehensive information for building, configuring, and deploying the Ribosoft web service for ribozyme design.

## Quick Navigation

### Getting Started
- **[Building Guide](BUILDING.md)** - Complete instructions for building Ribosoft from source
- **[Configuration Guide](CONFIGURATION.md)** - Comprehensive configuration reference for all settings

### Development & Deployment
- **[Dependency Script Guide](DEPENDENCY_SCRIPT_GUIDE.md)** - Using the Python dependency management script
- **[Local Database Guide](LOCAL_DATABASE_GUIDE.md)** - Setting up PostgreSQL or SQL Server locally
- **[Docker Guide](DOCKER_GUIDE.md)** - Containerization and Docker deployment

## Guide Overview

### [Building Guide](BUILDING.md)
Learn how to build Ribosoft from source code, including:
- Prerequisites and platform support
- C++ algorithm library compilation
- Frontend asset building with modern JavaScript
- .NET application compilation
- Development workflow and troubleshooting

**Key Topics:**
- Quick start instructions
- Detailed build process for each component
- Development environment setup
- Testing and verification
- Common build issues and solutions

### [Configuration Guide](CONFIGURATION.md)
Complete reference for configuring Ribosoft in all environments:
- Database connection strings
- HTTPS and SSL certificate setup
- Email service integration (Mailgun)
- NCBI Datasets API configuration
- BLAST database settings
- Background job processing (Hangfire)
- Logging and monitoring

**Key Topics:**
- Environment-specific configurations
- Security best practices
- Docker environment variables
- Production deployment settings
- Troubleshooting configuration issues

### [Dependency Script Guide](DEPENDENCY_SCRIPT_GUIDE.md)
Master the Python 3.13 dependency management script:
- Installing C++ libraries (ViennaRNA, Melting)
- Managing package versions and updates
- Automated dependency resolution
- Integration with build processes
- Advanced features and troubleshooting

**Key Topics:**
- Command reference and usage
- Configuration file management
- Parallel downloads and optimization
- CI/CD integration
- Error handling and debugging

### [Local Database Guide](LOCAL_DATABASE_GUIDE.md)
Set up and manage local databases for development:
- PostgreSQL installation and configuration
- SQL Server LocalDB and Express setup
- Entity Framework migrations
- Database administration and maintenance
- Performance optimization
- Backup and restore procedures

**Key Topics:**
- Dual database support (PostgreSQL/SQL Server)
- Migration management
- Development workflow
- Security configuration
- Troubleshooting database issues

### [Docker Guide](DOCKER_GUIDE.md)
Comprehensive Docker containerization and deployment:
- Multi-stage Dockerfile optimization
- Docker Compose orchestration
- Production deployment strategies
- Security hardening and best practices
- Monitoring and logging
- Scaling and performance tuning

**Key Topics:**
- Container architecture
- HTTPS configuration in containers
- Volume management and persistence
- Health checks and monitoring
- CI/CD integration
- Troubleshooting containerized deployments

## Recommended Reading Order

### For New Developers
1. **[Building Guide](BUILDING.md)** - Start here to get Ribosoft running locally
2. **[Local Database Guide](LOCAL_DATABASE_GUIDE.md)** - Set up your development database
3. **[Dependency Script Guide](DEPENDENCY_SCRIPT_GUIDE.md)** - Understand C++ dependency management
4. **[Configuration Guide](CONFIGURATION.md)** - Configure services and integrations

### For DevOps/Deployment
1. **[Docker Guide](DOCKER_GUIDE.md)** - Containerization and orchestration
2. **[Configuration Guide](CONFIGURATION.md)** - Production configuration
3. **[Building Guide](BUILDING.md)** - Understanding the build process
4. **[Local Database Guide](LOCAL_DATABASE_GUIDE.md)** - Database setup and management

### For System Administrators
1. **[Configuration Guide](CONFIGURATION.md)** - Complete configuration reference
2. **[Docker Guide](DOCKER_GUIDE.md)** - Production deployment
3. **[Local Database Guide](LOCAL_DATABASE_GUIDE.md)** - Database administration
4. **[Dependency Script Guide](DEPENDENCY_SCRIPT_GUIDE.md)** - Dependency management

## Technology Stack Reference

### Backend (.NET 8)
- **Framework**: ASP.NET Core 8.0
- **Language**: C# with Entity Framework Core
- **Database**: PostgreSQL (primary) / SQL Server (alternative)
- **Background Jobs**: Hangfire with database storage
- **Authentication**: ASP.NET Core Identity

### Frontend
- **Framework**: Modern JavaScript with Bootstrap 5.3.3
- **Build Tools**: Webpack 5.91.0, Node.js ≥20.19.0
- **UI Components**: Bootstrap, jQuery, jQuery UI
- **Visualization**: fornac (RNA structure visualization)

### Core Algorithm (C++23)
- **Language**: C++23 with native build system
- **Parallelization**: OpenMP support
- **Testing**: Catch2 framework
- **Distribution**: NuGet packages

### Dependencies
- **ViennaRNA**: RNA secondary structure prediction
- **Melting**: DNA/RNA melting temperature calculations
- **Python 3.13**: Dependency management and build scripts

## Platform Support

- ✅ **Linux (Ubuntu)**: Primary development platform with full support
- 🚧 **macOS**: Temporarily disabled during C++23 modernization (Apple Silicon support planned)
- 🚧 **Windows**: Temporarily disabled during modernization (dependency rebuild required)

## Getting Help

### Documentation
- **Main README**: [../README.md](../README.md) - Project overview and quick start
- **GitHub Issues**: [Issues](https://github.com/t-vaudry/ribosoft/issues) - Bug reports and feature requests
- **Live Demo**: https://ribosoft2.vaudryread.ca - Try the application online

### Support Resources
- **Build Issues**: Check the [Building Guide](BUILDING.md) troubleshooting section
- **Configuration Problems**: Refer to the [Configuration Guide](CONFIGURATION.md) troubleshooting
- **Database Issues**: See the [Local Database Guide](LOCAL_DATABASE_GUIDE.md) troubleshooting
- **Docker Problems**: Consult the [Docker Guide](DOCKER_GUIDE.md) troubleshooting
- **Dependency Issues**: Review the [Dependency Script Guide](DEPENDENCY_SCRIPT_GUIDE.md) troubleshooting

### Community
- **GitHub Discussions**: For questions and community support
- **Issue Tracker**: For bug reports and feature requests
- **Pull Requests**: For contributing improvements and fixes

## Contributing to Documentation

We welcome contributions to improve this documentation:

1. **Fork the repository**
2. **Create a feature branch** for your documentation changes
3. **Follow the existing style** and structure
4. **Test your changes** by following the guides
5. **Submit a pull request** with a clear description

### Documentation Standards
- Use clear, concise language
- Include code examples and command snippets
- Provide troubleshooting sections
- Test all instructions on a clean environment
- Update the wiki index when adding new guides

## License

This documentation is part of the Ribosoft project and is licensed under the [GNU General Public License v3.0](../LICENSE.md).

---

**Last Updated**: August 2025  
**Ribosoft Version**: 2.x with .NET 8 and C++23 modernization
