# Ribosoft Docker Support Guide

This guide covers Docker containerization and deployment for the Ribosoft web service. Ribosoft provides comprehensive Docker support for both development and production environments, including multi-service orchestration with Docker Compose.

## Overview

Ribosoft's Docker implementation provides:

- **Multi-stage builds** for optimized container images
- **Docker Compose orchestration** with PostgreSQL database
- **Production-ready configuration** with security best practices
- **Health checks** and monitoring capabilities
- **Volume management** for persistent data and logs
- **HTTPS support** with certificate mounting
- **Non-root user execution** for enhanced security

## Architecture

### Container Structure

```
Ribosoft Docker Stack
├── ribosoft_app (Main Application)
│   ├── ASP.NET Core 8.0 Runtime
│   ├── Built Frontend Assets
│   ├── C++ Algorithm Libraries
│   └── BLAST Database Support
└── ribosoft_db (PostgreSQL Database)
    ├── PostgreSQL Latest
    ├── Persistent Data Volume
    └── Health Monitoring
```

### Network Architecture

- **Custom Bridge Network**: `ribosoft_network`
- **Service Discovery**: Containers communicate by service name
- **Port Mapping**: External access through host ports
- **Security**: Isolated network with minimal exposure

## Quick Start

### Prerequisites

- **Docker**: Version 20.10 or higher
- **Docker Compose**: Version 2.0 or higher
- **Available Ports**: 5000 (HTTP), 5001 (HTTPS), 5432 (PostgreSQL)

### Basic Deployment

1. **Clone the repository**:
   ```bash
   git clone https://github.com/t-vaudry/ribosoft.git
   cd ribosoft
   ```

2. **Start the services**:
   ```bash
   docker-compose up -d
   ```

3. **Access the application**:
   - HTTP: http://localhost:5000
   - HTTPS: https://localhost:5001 (if certificate configured)

4. **Check service status**:
   ```bash
   docker-compose ps
   docker-compose logs ribosoft
   ```

## Docker Compose Configuration

### Complete docker-compose.yml

The provided `docker-compose.yml` includes:

```yaml
version: '3.8'

volumes:
  pgdata:
    driver: local
  ribosoft_logs:
    driver: local
  ribosoft_certificates:
    driver: local

networks:
  ribosoft_network:
    driver: bridge

services:
  db:
    image: postgres:latest
    container_name: ribosoft_db
    restart: unless-stopped
    ports:
      - "5432:5432"
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: ribosoft
      POSTGRES_INITDB_ARGS: "--encoding=UTF8 --locale=C"
    volumes:
      - pgdata:/var/lib/postgresql/data
    networks:
      - ribosoft_network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d ribosoft"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 30s

  ribosoft:
    container_name: ribosoft_app
    restart: unless-stopped
    ports:
      - "5000:80"    # HTTP port
      - "5001:443"   # HTTPS port
    build:
      context: .
      dockerfile: Ribosoft/Dockerfile
      target: runtime
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: https://+:443;http://+:80
      ConnectionStrings__NpgsqlConnection: "Host=db;Port=5432;Username=postgres;Password=postgres;Database=ribosoft;Pooling=true;"
      EntityFrameworkProvider: Npgsql
      NODE_ENV: production
    volumes:
      - ribosoft_logs:/app/logs
      - ribosoft_certificates:/app/certificates
    networks:
      - ribosoft_network
    depends_on:
      db:
        condition: service_healthy
```

### Key Features

#### Health Checks
- **Database**: PostgreSQL readiness check
- **Application**: HTTP/HTTPS endpoint monitoring
- **Automatic Recovery**: Containers restart on health check failures

#### Security
- **Non-root User**: Application runs as `ribosoft` user
- **Security Options**: `no-new-privileges:true`
- **Minimal Attack Surface**: Only necessary ports exposed

#### Persistence
- **Database Data**: Persistent PostgreSQL data volume
- **Application Logs**: Persistent logging volume
- **Certificates**: Secure certificate storage

## Dockerfile Deep Dive

### Multi-Stage Build

The Dockerfile uses a multi-stage build for optimization:

#### Stage 1: Build Environment
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

# Install Node.js 24.x for frontend build
RUN apt-get update && apt-get install -y \
    curl gnupg ca-certificates \
    && curl -fsSL https://deb.nodesource.com/setup_24.x | bash - \
    && apt-get install -y nodejs
```

**Features:**
- .NET 8 SDK for compilation
- Node.js 24.x for frontend builds
- Dependency restoration and caching
- Frontend asset compilation
- Application publishing

#### Stage 2: Runtime Environment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy AS runtime

# Install minimal runtime dependencies
RUN apt-get update && apt-get install -y \
    curl libgomp1 \
    && apt-get clean
```

**Features:**
- Minimal ASP.NET Core runtime
- OpenMP support for C++ algorithms
- Non-root user creation
- Health check capabilities

### Build Process

1. **Dependency Restoration**: .NET and Node.js packages
2. **Frontend Build**: Webpack compilation of assets
3. **Backend Build**: .NET application compilation
4. **Publishing**: Optimized runtime artifacts
5. **Runtime Setup**: Minimal container with security hardening

## Environment Configuration

### Required Environment Variables

```bash
# Database Configuration
ConnectionStrings__NpgsqlConnection="Host=db;Port=5432;Username=postgres;Password=postgres;Database=ribosoft;Pooling=true;"
EntityFrameworkProvider="Npgsql"
DB_CONTEXT="NpgsqlDbContext"

# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="https://+:443;http://+:80"
ASPNETCORE_HTTPS_PORT="443"

# Frontend Configuration
NODE_ENV="production"
```

### Optional Configuration

```bash
# Email Configuration (Mailgun)
MailgunAPIKey="your-mailgun-api-key"
MailgunDomain="your-mailgun-domain.com"
SenderEmail="noreply@your-domain.com"
SenderName="Ribosoft"

# NCBI Datasets API
NCBIDatasetsApi__ApiKey="your-ncbi-api-key"
NCBIDatasetsApi__BaseUrl="https://api.ncbi.nlm.nih.gov/datasets/v2"

# BLAST Configuration
Assemblies__Path="/app/blastdb"
Assemblies__NumThreads="4"
Assemblies__AutoCreateBlastDatabase="true"

# Logging
Logging__LogLevel__Default="Information"
Logging__LogLevel__Microsoft="Warning"
```

## Production Deployment

### HTTPS Configuration

#### Option 1: Certificate Volume Mount

1. **Prepare certificates**:
   ```bash
   mkdir certificates
   # Copy your .pfx certificate file to certificates/
   ```

2. **Update docker-compose.yml**:
   ```yaml
   services:
     ribosoft:
       volumes:
         - ./certificates:/app/certificates:ro
       environment:
         ASPNETCORE_Kestrel__Certificates__Default__Path: "/app/certificates/certificate.pfx"
         ASPNETCORE_Kestrel__Certificates__Default__Password: "your_certificate_password"
   ```

#### Option 2: Reverse Proxy (Recommended)

Use nginx or Traefik as a reverse proxy:

```yaml
# docker-compose.prod.yml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./certificates:/etc/nginx/certificates:ro
    depends_on:
      - ribosoft

  ribosoft:
    # Remove port mappings, only internal access
    expose:
      - "80"
    # Remove HTTPS configuration
    environment:
      ASPNETCORE_URLS: "http://+:80"
```

### Production Environment Variables

Create a `.env` file for production:

```bash
# .env
POSTGRES_PASSWORD=your_secure_password
MAILGUN_API_KEY=your_mailgun_api_key
MAILGUN_DOMAIN=your_domain.com
NCBI_API_KEY=your_ncbi_api_key
CERTIFICATE_PASSWORD=your_certificate_password
```

Use in docker-compose:

```yaml
services:
  db:
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
  
  ribosoft:
    environment:
      MailgunAPIKey: ${MAILGUN_API_KEY}
      MailgunDomain: ${MAILGUN_DOMAIN}
      NCBIDatasetsApi__ApiKey: ${NCBI_API_KEY}
```

## Development with Docker

### Development Override

Create `docker-compose.override.yml` for development:

```yaml
version: '3.8'

services:
  ribosoft:
    build:
      target: build  # Use build stage for development
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:80
    volumes:
      - ./Ribosoft:/src/Ribosoft:ro  # Mount source for hot reload
      - ./logs:/app/logs
    ports:
      - "5000:80"
    command: ["dotnet", "watch", "run", "--project", "/src/Ribosoft"]
```

### Development Workflow

```bash
# Start development environment
docker-compose -f docker-compose.yml -f docker-compose.override.yml up

# View logs
docker-compose logs -f ribosoft

# Execute commands in container
docker-compose exec ribosoft bash

# Rebuild after changes
docker-compose build ribosoft
```

## Volume Management

### Persistent Volumes

```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect ribosoft_pgdata

# Backup database
docker-compose exec db pg_dump -U postgres ribosoft > backup.sql

# Restore database
docker-compose exec -T db psql -U postgres ribosoft < backup.sql
```

### Volume Locations

- **Database Data**: `ribosoft_pgdata` → `/var/lib/postgresql/data`
- **Application Logs**: `ribosoft_logs` → `/app/logs`
- **Certificates**: `ribosoft_certificates` → `/app/certificates`
- **BLAST Database**: Host mount → `/app/blastdb`

## Monitoring and Logging

### Health Monitoring

```bash
# Check container health
docker-compose ps

# View health check logs
docker inspect ribosoft_app | grep -A 10 Health

# Manual health check
curl -f http://localhost:5000/health
```

### Log Management

```bash
# View application logs
docker-compose logs ribosoft

# Follow logs in real-time
docker-compose logs -f ribosoft

# View database logs
docker-compose logs db

# Export logs
docker-compose logs ribosoft > ribosoft.log
```

### Log Configuration

Logs are structured and can be configured via environment variables:

```yaml
environment:
  Logging__LogLevel__Default: "Information"
  Logging__LogLevel__Microsoft: "Warning"
  Logging__LogLevel__Hangfire: "Information"
```

## Scaling and Performance

### Resource Limits

Add resource constraints to docker-compose.yml:

```yaml
services:
  ribosoft:
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 4G
        reservations:
          cpus: '1.0'
          memory: 2G
  
  db:
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 2G
```

### Horizontal Scaling

Scale the application (requires load balancer):

```bash
# Scale to 3 instances
docker-compose up -d --scale ribosoft=3

# Use with nginx load balancer
# Configure nginx upstream with multiple backends
```

### Performance Tuning

#### Database Optimization

```yaml
services:
  db:
    environment:
      POSTGRES_SHARED_PRELOAD_LIBRARIES: 'pg_stat_statements'
      POSTGRES_MAX_CONNECTIONS: '200'
      POSTGRES_SHARED_BUFFERS: '256MB'
      POSTGRES_EFFECTIVE_CACHE_SIZE: '1GB'
```

#### Application Optimization

```yaml
services:
  ribosoft:
    environment:
      ASPNETCORE_THREADING_THREADPOOL_MINWORKERTHREADS: '50'
      ASPNETCORE_THREADING_THREADPOOL_MINCOMPLETIONPORTTHREADS: '50'
      Assemblies__NumThreads: '8'  # Adjust based on CPU cores
```

## Troubleshooting

### Common Issues

#### Build Failures

**Issue**: Node.js build fails
```bash
# Check Node.js version in container
docker-compose exec ribosoft node --version

# Rebuild with no cache
docker-compose build --no-cache ribosoft
```

**Issue**: .NET restore fails
```bash
# Clear Docker build cache
docker system prune -a

# Check network connectivity
docker-compose exec ribosoft curl -I https://api.nuget.org/v3/index.json
```

#### Runtime Issues

**Issue**: Database connection fails
```bash
# Check database status
docker-compose exec db pg_isready -U postgres

# Verify connection string
docker-compose exec ribosoft env | grep ConnectionStrings

# Test database connectivity
docker-compose exec ribosoft curl -f http://localhost/health
```

**Issue**: HTTPS certificate problems
```bash
# Check certificate mount
docker-compose exec ribosoft ls -la /app/certificates/

# Verify certificate validity
docker-compose exec ribosoft openssl x509 -in /app/certificates/certificate.crt -text -noout
```

#### Performance Issues

**Issue**: Slow application startup
```bash
# Check resource usage
docker stats

# Increase memory limits
# Add deploy.resources.limits to docker-compose.yml

# Check logs for startup errors
docker-compose logs ribosoft | grep -i error
```

### Debugging Commands

```bash
# Enter container shell
docker-compose exec ribosoft bash

# Check running processes
docker-compose exec ribosoft ps aux

# Monitor resource usage
docker stats ribosoft_app ribosoft_db

# Check network connectivity
docker-compose exec ribosoft netstat -tlnp

# Verify file permissions
docker-compose exec ribosoft ls -la /app/

# Test internal connectivity
docker-compose exec ribosoft curl -f http://db:5432
```

## Security Best Practices

### Container Security

1. **Non-root User**: Application runs as `ribosoft` user
2. **Security Options**: `no-new-privileges:true`
3. **Minimal Base Image**: Use official ASP.NET Core runtime
4. **Regular Updates**: Keep base images updated

### Network Security

1. **Custom Network**: Isolated bridge network
2. **Minimal Exposure**: Only necessary ports exposed
3. **Internal Communication**: Services communicate by name
4. **Firewall Rules**: Configure host firewall appropriately

### Data Security

1. **Volume Encryption**: Use encrypted volumes for sensitive data
2. **Secret Management**: Use Docker secrets or external secret management
3. **Database Security**: Strong passwords and connection encryption
4. **Certificate Management**: Secure certificate storage and rotation

### Example Secure Configuration

```yaml
services:
  ribosoft:
    security_opt:
      - no-new-privileges:true
      - apparmor:docker-default
    tmpfs:
      - /tmp:noexec,nosuid,size=100m
    read_only: true
    volumes:
      - ribosoft_logs:/app/logs:rw
      - ribosoft_certificates:/app/certificates:ro
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build and Deploy Docker

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build and Deploy
        run: |
          docker-compose build
          docker-compose up -d
          
      - name: Health Check
        run: |
          sleep 30
          curl -f http://localhost:5000/health
```

### Production Deployment Script

```bash
#!/bin/bash
# deploy.sh

set -e

echo "Pulling latest changes..."
git pull origin main

echo "Building containers..."
docker-compose build --no-cache

echo "Stopping services..."
docker-compose down

echo "Starting services..."
docker-compose up -d

echo "Waiting for services to be ready..."
sleep 30

echo "Running health checks..."
docker-compose exec ribosoft curl -f http://localhost/health

echo "Deployment complete!"
```

## Getting Help

### Useful Commands

```bash
# Complete system information
docker system info

# Container resource usage
docker stats

# Network information
docker network ls
docker network inspect ribosoft_ribosoft_network

# Volume information
docker volume ls
docker volume inspect ribosoft_pgdata

# Service logs
docker-compose logs --tail=100 ribosoft
```

### Support Resources

- **GitHub Issues**: https://github.com/t-vaudry/ribosoft/issues
- **Docker Documentation**: https://docs.docker.com/
- **Docker Compose Reference**: https://docs.docker.com/compose/
- **ASP.NET Core Docker**: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/

### Common Support Scenarios

1. **Build Issues**: Check Dockerfile and build context
2. **Runtime Issues**: Verify environment variables and volumes
3. **Network Issues**: Check service connectivity and port mappings
4. **Performance Issues**: Monitor resource usage and adjust limits
5. **Security Issues**: Review security configurations and best practices

This Docker guide provides comprehensive coverage of containerization, deployment, and operational aspects of running Ribosoft in Docker environments.
