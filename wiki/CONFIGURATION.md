# Ribosoft Configuration Guide

This guide covers all configuration aspects of the Ribosoft web service, including database setup, HTTPS configuration, email services, API integrations, and deployment settings.

## Configuration Files Overview

Ribosoft uses multiple configuration files depending on the environment:

- `appsettings.json` - Base configuration settings
- `appsettings.Development.json` - Development environment overrides
- `appsettings.Production.json` - Production environment overrides
- `docker-compose.yml` - Docker environment configuration
- `Properties/launchSettings.json` - Development launch profiles

## Database Configuration

### PostgreSQL (Recommended)

Edit `appsettings.json` to configure PostgreSQL connection:

```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft;Username=your_user;Password=your_password;Pooling=true;"
  },
  "EntityFrameworkProvider": "Npgsql",
  "DB_CONTEXT": "NpgsqlDbContext"
}
```

### SQL Server (Alternative)

For SQL Server configuration:

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=(localdb)\\mssqllocaldb;Database=ribosoft;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "EntityFrameworkProvider": "SqlServer",
  "DB_CONTEXT": "SqlServerDbContext"
}
```

### Database Environment Variables

For Docker or production deployments, use environment variables:

```bash
# PostgreSQL
ConnectionStrings__NpgsqlConnection="Host=db;Port=5432;Username=postgres;Password=postgres;Database=ribosoft;Pooling=true;"
EntityFrameworkProvider="Npgsql"
DB_CONTEXT="NpgsqlDbContext"

# SQL Server
ConnectionStrings__SqlServerConnection="Server=sqlserver;Database=ribosoft;User Id=sa;Password=YourPassword123;"
EntityFrameworkProvider="SqlServer"
DB_CONTEXT="SqlServerDbContext"
```

## HTTPS Configuration

### Development HTTPS

For local development with HTTPS:

```bash
# Generate and trust development certificate
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

Configure in `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "Ribosoft": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Production HTTPS

#### Using Environment Variables

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS="https://+:443;http://+:80"
ASPNETCORE_HTTPS_PORT=443
```

#### Using appsettings.Production.json

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:80"
      },
      "Https": {
        "Url": "https://+:443",
        "Certificate": {
          "Path": "/app/certificates/certificate.pfx",
          "Password": "your_certificate_password"
        }
      }
    }
  }
}
```

#### Docker HTTPS Configuration

For Docker deployments, mount your certificate:

```yaml
# docker-compose.yml
services:
  ribosoft:
    volumes:
      - ./certificates:/app/certificates:ro
    environment:
      ASPNETCORE_URLS: "https://+:443;http://+:80"
      ASPNETCORE_HTTPS_PORT: "443"
```

## Email Configuration (Mailgun)

Ribosoft uses Mailgun for email services. Configure in `appsettings.json`:

```json
{
  "MailgunAPIKey": "your-mailgun-api-key",
  "MailgunDomain": "your-mailgun-domain.com",
  "SenderEmail": "noreply@your-domain.com",
  "SenderName": "Ribosoft"
}
```

### Environment Variables for Email

```bash
MailgunAPIKey="key-1234567890abcdef1234567890abcdef"
MailgunDomain="mg.yourdomain.com"
SenderEmail="noreply@yourdomain.com"
SenderName="Ribosoft"
```

### Mailgun Setup Steps

1. **Create Mailgun Account**: Sign up at https://www.mailgun.com/
2. **Add Domain**: Add and verify your domain in Mailgun dashboard
3. **Get API Key**: Copy your API key from the Mailgun dashboard
4. **Configure DNS**: Set up required DNS records (SPF, DKIM, MX)
5. **Test Configuration**: Use Mailgun's test endpoints to verify setup

### Email Templates

Ribosoft sends various email types:
- Account confirmation emails
- Password reset emails
- Job completion notifications
- System alerts

Email templates are located in `Views/Shared/EmailTemplates/`.

## NCBI Datasets API Configuration

Configure NCBI Datasets API for sequence data retrieval:

```json
{
  "NCBIDatasetsApi": {
    "ApiKey": "your-ncbi-api-key",
    "BaseUrl": "https://api.ncbi.nlm.nih.gov/datasets/v2",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 1
  }
}
```

### Environment Variables for NCBI API

```bash
NCBIDatasetsApi__ApiKey="your-ncbi-api-key"
NCBIDatasetsApi__BaseUrl="https://api.ncbi.nlm.nih.gov/datasets/v2"
NCBIDatasetsApi__TimeoutSeconds="30"
NCBIDatasetsApi__MaxRetryAttempts="3"
NCBIDatasetsApi__RetryDelaySeconds="1"
```

### NCBI API Key Setup

1. **Create NCBI Account**: Register at https://www.ncbi.nlm.nih.gov/account/
2. **Generate API Key**: Go to Account Settings → API Key Management
3. **Set Usage Limits**: Configure appropriate rate limits for your usage
4. **Test Connection**: Verify API key works with test requests

## BLAST Configuration

Configure BLAST database settings for sequence similarity searching:

```json
{
  "Assemblies": {
    "Path": "/app/blastdb",
    "NumThreads": 4,
    "MakeBlastDbPath": "makeblastdb",
    "AutoCreateBlastDatabase": true,
    "MaxBlastDbFileSizeMB": 500,
    "SkipBlastDbForLargeFiles": false,
    "CleanupAfterProcessing": true,
    "CleanupZipFiles": true
  }
}
```

### Environment Variables for BLAST

```bash
Assemblies__Path="/app/blastdb"
Assemblies__NumThreads="4"
Assemblies__MakeBlastDbPath="makeblastdb"
Assemblies__AutoCreateBlastDatabase="true"
Assemblies__MaxBlastDbFileSizeMB="500"
Assemblies__SkipBlastDbForLargeFiles="false"
Assemblies__CleanupAfterProcessing="true"
Assemblies__CleanupZipFiles="true"
```

### BLAST Database Setup

1. **Install BLAST+**: Download from NCBI BLAST+ suite
2. **Create Database Directory**: Ensure the path exists and is writable
3. **Set Permissions**: Make sure the application can read/write to the directory
4. **Configure Threads**: Set `NumThreads` based on available CPU cores

## Background Jobs (Hangfire)

Configure Hangfire for background job processing:

```json
{
  "Hangfire": {
    "DashboardEnabled": true,
    "DashboardPath": "/hangfire",
    "WorkerCount": 4,
    "Queues": ["default", "critical", "background"],
    "JobExpirationTimeout": "24:00:00"
  }
}
```

### Hangfire Environment Variables

```bash
Hangfire__DashboardEnabled="true"
Hangfire__DashboardPath="/hangfire"
Hangfire__WorkerCount="4"
Hangfire__JobExpirationTimeout="24:00:00"
```

## Logging Configuration

Configure NLog for structured logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Hangfire": "Information"
    }
  },
  "NLog": {
    "autoReload": true,
    "throwConfigExceptions": true,
    "targets": {
      "async": true,
      "logfile": {
        "type": "File",
        "fileName": "logs/ribosoft-${shortdate}.log",
        "layout": "${longdate} ${level:uppercase=true} ${logger} ${message} ${exception:format=tostring}"
      }
    },
    "rules": [
      {
        "logger": "*",
        "minLevel": "Info",
        "writeTo": "logfile"
      }
    ]
  }
}
```

### Environment Variables for Logging

```bash
Logging__LogLevel__Default="Information"
Logging__LogLevel__Microsoft="Warning"
Logging__LogLevel__Microsoft.Hosting.Lifetime="Information"
```

## Frontend Build Configuration

Configure Node.js and Webpack settings:

```json
{
  "Frontend": {
    "NODE_ENV": "production",
    "BuildMode": "production",
    "EnableSourceMaps": false,
    "EnableMinification": true
  }
}
```

### Environment Variables for Frontend

```bash
NODE_ENV="production"
BuildMode="production"
EnableSourceMaps="false"
EnableMinification="true"
```

## Security Configuration

### Authentication Settings

```json
{
  "Authentication": {
    "RequireConfirmedAccount": true,
    "RequireConfirmedEmail": true,
    "RequireUniqueEmail": true,
    "SignIn": {
      "RequireConfirmedEmail": true
    },
    "Password": {
      "RequiredLength": 8,
      "RequireNonAlphanumeric": true,
      "RequireDigit": true,
      "RequireUppercase": true,
      "RequireLowercase": true
    },
    "Lockout": {
      "DefaultLockoutTimeSpan": "00:05:00",
      "MaxFailedAccessAttempts": 5,
      "AllowedForNewUsers": true
    }
  }
}
```

### CORS Configuration

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://ribosoft2.vaudryread.ca",
      "https://localhost:5001"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  }
}
```

## Complete Docker Configuration Example

Here's a complete `docker-compose.yml` with all configuration options:

```yaml
version: '3.8'

volumes:
  pgdata:
  ribosoft_logs:
  ribosoft_certificates:
  ribosoft_blastdb:

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
    volumes:
      - pgdata:/var/lib/postgresql/data

  ribosoft:
    container_name: ribosoft_app
    restart: unless-stopped
    ports:
      - "5000:80"    # HTTP port
      - "5001:443"   # HTTPS port
    build:
      context: .
      dockerfile: Ribosoft/Dockerfile
    environment:
      # ASP.NET Core Configuration
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: "https://+:443;http://+:80"
      ASPNETCORE_HTTPS_PORT: "443"
      
      # Database Configuration
      ConnectionStrings__NpgsqlConnection: "Host=db;Port=5432;Username=postgres;Password=postgres;Database=ribosoft;Pooling=true;"
      DB_CONTEXT: "NpgsqlDbContext"
      EntityFrameworkProvider: "Npgsql"
      
      # Frontend Build Configuration
      NODE_ENV: "production"
      
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
      
    volumes:
      - ribosoft_logs:/app/logs
      - ribosoft_certificates:/app/certificates
      - ribosoft_blastdb:/app/blastdb
      # Mount your certificate directory (uncomment for production):
      # - ./certificates:/app/certificates:ro
      # Mount BLAST database directory (uncomment if needed):
      # - ./blastdb:/app/blastdb
    depends_on:
      - "db"
```

## Environment-Specific Configuration

### Development Environment

Create `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft_dev;Username=dev_user;Password=dev_password;Pooling=true;"
  },
  "Frontend": {
    "NODE_ENV": "development",
    "EnableSourceMaps": true,
    "EnableMinification": false
  }
}
```

### Production Environment

Create `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error"
    }
  },
  "Frontend": {
    "NODE_ENV": "production",
    "EnableSourceMaps": false,
    "EnableMinification": true
  },
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 52428800
    }
  }
}
```

## Configuration Validation

### Required Settings Checklist

Before deploying, ensure these settings are configured:

- [ ] Database connection string
- [ ] Entity Framework provider
- [ ] Mailgun API key and domain
- [ ] NCBI API key
- [ ] BLAST database path
- [ ] HTTPS certificate (production)
- [ ] Logging configuration
- [ ] Frontend build settings

### Testing Configuration

Test your configuration with these commands:

```bash
# Test database connection
dotnet ef database update

# Test email configuration
# (Use built-in test endpoints or create test controller)

# Test NCBI API
# (Use built-in API test endpoints)

# Test BLAST configuration
# (Verify BLAST database creation)

# Test HTTPS
curl -k https://localhost:5001/health

# Test application startup
dotnet run --environment Production
```

## Troubleshooting Configuration Issues

### Common Configuration Problems

#### Database Connection Issues
```bash
# Check PostgreSQL status
sudo systemctl status postgresql

# Test connection manually
psql -h localhost -p 5432 -U your_user -d ribosoft

# Verify connection string format
# Ensure special characters in passwords are URL-encoded
```

#### Email Configuration Issues
```bash
# Test Mailgun API key
curl -s --user 'api:YOUR_API_KEY' \
    https://api.mailgun.net/v3/YOUR_DOMAIN/messages \
    -F from='test@YOUR_DOMAIN' \
    -F to='test@example.com' \
    -F subject='Test' \
    -F text='Test message'
```

#### HTTPS Certificate Issues
```bash
# Check certificate validity
openssl x509 -in certificate.crt -text -noout

# Verify certificate permissions
ls -la /app/certificates/

# Test HTTPS endpoint
curl -k -v https://localhost:5001/
```

#### NCBI API Issues
```bash
# Test API key
curl -H "api-key: YOUR_API_KEY" \
    "https://api.ncbi.nlm.nih.gov/datasets/v2/genome/accession/GCF_000001405.26"
```

### Configuration File Precedence

ASP.NET Core loads configuration in this order (later sources override earlier ones):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. User secrets (development only)
4. Environment variables
5. Command-line arguments

### Security Best Practices

1. **Never commit secrets** to version control
2. **Use environment variables** for sensitive data in production
3. **Enable HTTPS** in production
4. **Use strong passwords** for database connections
5. **Regularly rotate API keys**
6. **Limit CORS origins** to trusted domains
7. **Enable request size limits**
8. **Configure proper logging levels** (avoid logging sensitive data)

## Getting Help

If you encounter configuration issues:

1. Check the application logs in `/app/logs/` or `logs/` directory
2. Verify environment variables are set correctly
3. Test individual components (database, email, APIs) separately
4. Review the [GitHub Issues](https://github.com/t-vaudry/ribosoft/issues) for similar problems
5. Ensure all prerequisites are installed and configured

This configuration guide should cover all aspects of setting up Ribosoft for development and production environments.
