# Ribosoft Local Database Setup Guide

This guide covers setting up and configuring local databases for Ribosoft development and testing. Ribosoft supports both PostgreSQL (recommended) and SQL Server as database backends, with Entity Framework Core providing database-agnostic data access.

## Overview

Ribosoft's database architecture includes:

- **Entity Framework Core**: Database-agnostic ORM with migrations
- **Dual Database Support**: PostgreSQL (primary) and SQL Server (alternative)
- **Background Jobs**: Hangfire integration with database storage
- **User Management**: ASP.NET Core Identity tables
- **Application Data**: Jobs, ribozymes, designs, and assemblies

## Database Options

### PostgreSQL (Recommended)

**Advantages:**
- Open source and free
- Excellent performance for bioinformatics workloads
- Better JSON support for complex data structures
- Cross-platform compatibility
- Primary target for production deployments

**Use Cases:**
- Development environments
- Production deployments
- Docker containerization
- Linux/macOS development

### SQL Server

**Advantages:**
- Integrated with Visual Studio
- LocalDB for development
- Familiar for Windows developers
- Rich tooling ecosystem

**Use Cases:**
- Windows development environments
- Visual Studio integration
- Legacy system compatibility

## PostgreSQL Setup

### Installation

#### Ubuntu/Debian
```bash
# Update package list
sudo apt update

# Install PostgreSQL and additional tools
sudo apt install postgresql postgresql-contrib postgresql-client

# Start and enable PostgreSQL service
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Verify installation
sudo systemctl status postgresql
```

#### macOS (using Homebrew)
```bash
# Install PostgreSQL
brew install postgresql

# Start PostgreSQL service
brew services start postgresql

# Verify installation
brew services list | grep postgresql
```

#### Windows
1. Download PostgreSQL installer from https://www.postgresql.org/download/windows/
2. Run the installer and follow the setup wizard
3. Remember the password you set for the `postgres` user
4. Ensure PostgreSQL service is running

### Database Configuration

#### 1. Create Database User

```bash
# Switch to postgres user (Linux/macOS)
sudo -u postgres psql

# Or connect directly (Windows/with password)
psql -U postgres -h localhost
```

```sql
-- Create a dedicated user for Ribosoft
CREATE USER ribosoft_user WITH PASSWORD 'your_secure_password';

-- Create the database
CREATE DATABASE ribosoft OWNER ribosoft_user;

-- Grant necessary privileges
GRANT ALL PRIVILEGES ON DATABASE ribosoft TO ribosoft_user;

-- Connect to the ribosoft database
\c ribosoft

-- Grant schema privileges
GRANT ALL ON SCHEMA public TO ribosoft_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ribosoft_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO ribosoft_user;

-- Exit psql
\q
```

#### 2. Configure Connection String

Edit `appsettings.json` in the Ribosoft project:

```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft;Username=ribosoft_user;Password=your_secure_password;Pooling=true;Timeout=30;Command Timeout=30;"
  },
  "EntityFrameworkProvider": "Npgsql",
  "DB_CONTEXT": "NpgsqlDbContext"
}
```

#### 3. Development Configuration

For development, create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft_dev;Username=ribosoft_user;Password=your_secure_password;Pooling=true;"
  },
  "EntityFrameworkProvider": "Npgsql",
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Testing PostgreSQL Connection

```bash
# Test connection from command line
psql -h localhost -p 5432 -U ribosoft_user -d ribosoft

# Test from .NET application
cd Ribosoft
dotnet ef database update --context NpgsqlDbContext
```

## SQL Server Setup

### Installation Options

#### Option 1: SQL Server LocalDB (Recommended for Development)

LocalDB is included with Visual Studio and .NET SDK:

```bash
# Verify LocalDB installation
sqllocaldb info

# Create a LocalDB instance
sqllocaldb create "RibosoftDB" -s

# List instances
sqllocaldb info
```

#### Option 2: SQL Server Express

1. Download SQL Server Express from Microsoft
2. Install with default settings
3. Enable SQL Server Browser service
4. Configure Windows Authentication or Mixed Mode

#### Option 3: SQL Server Developer Edition

Free for development use:
1. Download from Microsoft Developer Network
2. Install with SQL Server Management Studio (SSMS)
3. Configure authentication and networking

### Database Configuration

#### 1. Connection String Configuration

For LocalDB, edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=(localdb)\\MSSQLLocalDB;Database=Ribosoft;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "EntityFrameworkProvider": "SqlServer",
  "DB_CONTEXT": "SqlServerDbContext"
}
```

For SQL Server Express/Developer:

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost\\SQLEXPRESS;Database=Ribosoft;Integrated Security=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "EntityFrameworkProvider": "SqlServer",
  "DB_CONTEXT": "SqlServerDbContext"
}
```

For SQL Authentication:

```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=Ribosoft;User Id=ribosoft_user;Password=your_secure_password;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  },
  "EntityFrameworkProvider": "SqlServer",
  "DB_CONTEXT": "SqlServerDbContext"
}
```

#### 2. Create Database (if using SQL Server Express/Developer)

```sql
-- Connect using SSMS or sqlcmd
-- Create database
CREATE DATABASE Ribosoft;

-- Create user (for SQL Authentication)
USE Ribosoft;
CREATE LOGIN ribosoft_user WITH PASSWORD = 'your_secure_password';
CREATE USER ribosoft_user FOR LOGIN ribosoft_user;
ALTER ROLE db_owner ADD MEMBER ribosoft_user;
```

### Testing SQL Server Connection

```bash
# Test LocalDB connection
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Ribosoft -E

# Test SQL Server Express
sqlcmd -S "localhost\SQLEXPRESS" -d Ribosoft -E

# Test from .NET application
cd Ribosoft
dotnet ef database update --context SqlServerDbContext
```

## Entity Framework Migrations

### Understanding Migration Contexts

Ribosoft uses separate migration contexts for different databases:

- **NpgsqlDbContext**: PostgreSQL-specific migrations
- **SqlServerDbContext**: SQL Server-specific migrations

### Running Migrations

#### PostgreSQL Migrations

```bash
# Navigate to project directory
cd Ribosoft

# Add new migration (if needed)
dotnet ef migrations add InitialCreate --context NpgsqlDbContext --output-dir Data/Migrations/NpgsqlMigrations

# Update database
dotnet ef database update --context NpgsqlDbContext

# List migrations
dotnet ef migrations list --context NpgsqlDbContext
```

#### SQL Server Migrations

```bash
# Add new migration (if needed)
dotnet ef migrations add InitialCreate --context SqlServerDbContext --output-dir Data/Migrations/SqlServerMigrations

# Update database
dotnet ef database update --context SqlServerDbContext

# List migrations
dotnet ef migrations list --context SqlServerDbContext
```

### Migration Management

#### Creating New Migrations

When you modify entity models:

```bash
# For PostgreSQL
dotnet ef migrations add YourMigrationName --context NpgsqlDbContext --output-dir Data/Migrations/NpgsqlMigrations

# For SQL Server
dotnet ef migrations add YourMigrationName --context SqlServerDbContext --output-dir Data/Migrations/SqlServerMigrations
```

#### Rolling Back Migrations

```bash
# Rollback to specific migration
dotnet ef database update PreviousMigrationName --context NpgsqlDbContext

# Rollback all migrations
dotnet ef database update 0 --context NpgsqlDbContext
```

#### Removing Migrations

```bash
# Remove last migration (before applying to database)
dotnet ef migrations remove --context NpgsqlDbContext
```

## Database Schema Overview

### Core Tables

#### Identity Tables (ASP.NET Core Identity)
- `AspNetUsers`: User accounts
- `AspNetRoles`: User roles
- `AspNetUserRoles`: User-role relationships
- `AspNetUserClaims`: User claims
- `AspNetUserLogins`: External login providers

#### Application Tables
- `Jobs`: Background job tracking and results
- `Ribozymes`: Template ribozyme structures and metadata
- `Designs`: Generated ribozyme designs and parameters
- `Assemblies`: Sequence assembly data for BLAST operations

#### Hangfire Tables (Background Jobs)
- `HangfireJob`: Job definitions and state
- `HangfireJobParameter`: Job parameters
- `HangfireJobQueue`: Job queue management
- `HangfireServer`: Server registration
- `HangfireState`: Job state history

### Sample Data

The database includes preloaded ribozyme templates for common use cases:

- Hammerhead ribozymes
- Hairpin ribozymes
- HDV ribozymes
- Twister ribozymes

## Development Workflow

### Database-First Development

1. **Start with clean database**:
   ```bash
   dotnet ef database drop --context NpgsqlDbContext
   dotnet ef database update --context NpgsqlDbContext
   ```

2. **Run application**:
   ```bash
   dotnet run
   ```

3. **Verify data seeding**:
   - Check that ribozyme templates are loaded
   - Verify user registration works
   - Test job submission and processing

### Code-First Development

1. **Modify entity models** in `Models/` directory

2. **Create migration**:
   ```bash
   dotnet ef migrations add YourChanges --context NpgsqlDbContext --output-dir Data/Migrations/NpgsqlMigrations
   ```

3. **Review generated migration** in `Data/Migrations/`

4. **Apply migration**:
   ```bash
   dotnet ef database update --context NpgsqlDbContext
   ```

5. **Test changes** in application

## Database Administration

### PostgreSQL Administration

#### Using psql Command Line

```bash
# Connect to database
psql -h localhost -U ribosoft_user -d ribosoft

# Common commands
\dt                    # List tables
\d table_name         # Describe table structure
\du                   # List users
\l                    # List databases
\q                    # Quit

# Query examples
SELECT * FROM "AspNetUsers";
SELECT * FROM "Jobs" ORDER BY "CreatedAt" DESC LIMIT 10;
SELECT * FROM "Ribozymes";
```

#### Using pgAdmin (GUI Tool)

1. Install pgAdmin from https://www.pgadmin.org/
2. Add server connection:
   - Host: localhost
   - Port: 5432
   - Database: ribosoft
   - Username: ribosoft_user
3. Browse tables and data through GUI

### SQL Server Administration

#### Using SQL Server Management Studio (SSMS)

1. Install SSMS from Microsoft
2. Connect to your SQL Server instance
3. Browse database objects in Object Explorer
4. Use Query Editor for SQL commands

#### Using sqlcmd Command Line

```bash
# Connect to LocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Ribosoft -E

# Connect to SQL Server Express
sqlcmd -S "localhost\SQLEXPRESS" -d Ribosoft -E

# Common queries
SELECT * FROM AspNetUsers;
SELECT * FROM Jobs ORDER BY CreatedAt DESC;
SELECT * FROM Ribozymes;
```

## Backup and Restore

### PostgreSQL Backup/Restore

#### Backup Database

```bash
# Full database backup
pg_dump -h localhost -U ribosoft_user -d ribosoft > ribosoft_backup.sql

# Compressed backup
pg_dump -h localhost -U ribosoft_user -d ribosoft | gzip > ribosoft_backup.sql.gz

# Custom format backup (recommended)
pg_dump -h localhost -U ribosoft_user -d ribosoft -Fc > ribosoft_backup.dump
```

#### Restore Database

```bash
# From SQL file
psql -h localhost -U ribosoft_user -d ribosoft < ribosoft_backup.sql

# From compressed file
gunzip -c ribosoft_backup.sql.gz | psql -h localhost -U ribosoft_user -d ribosoft

# From custom format
pg_restore -h localhost -U ribosoft_user -d ribosoft ribosoft_backup.dump
```

### SQL Server Backup/Restore

#### Using SSMS
1. Right-click database → Tasks → Back Up...
2. Choose backup type and destination
3. Click OK to create backup

#### Using T-SQL Commands

```sql
-- Backup database
BACKUP DATABASE Ribosoft 
TO DISK = 'C:\Backups\Ribosoft.bak'
WITH FORMAT, INIT;

-- Restore database
RESTORE DATABASE Ribosoft 
FROM DISK = 'C:\Backups\Ribosoft.bak'
WITH REPLACE;
```

## Performance Optimization

### PostgreSQL Optimization

#### Configuration Tuning

Edit `postgresql.conf`:

```ini
# Memory settings
shared_buffers = 256MB
effective_cache_size = 1GB
work_mem = 4MB

# Connection settings
max_connections = 100

# Logging
log_statement = 'all'
log_duration = on
```

#### Index Optimization

```sql
-- Add indexes for common queries
CREATE INDEX idx_jobs_created_at ON "Jobs"("CreatedAt");
CREATE INDEX idx_jobs_user_id ON "Jobs"("UserId");
CREATE INDEX idx_designs_job_id ON "Designs"("JobId");

-- Analyze query performance
EXPLAIN ANALYZE SELECT * FROM "Jobs" WHERE "UserId" = 'user-id';
```

### SQL Server Optimization

#### Configuration

```sql
-- Enable query store for performance monitoring
ALTER DATABASE Ribosoft SET QUERY_STORE = ON;

-- Update statistics
UPDATE STATISTICS Jobs;
UPDATE STATISTICS Designs;
```

#### Index Management

```sql
-- Add indexes for performance
CREATE INDEX IX_Jobs_CreatedAt ON Jobs(CreatedAt);
CREATE INDEX IX_Jobs_UserId ON Jobs(UserId);
CREATE INDEX IX_Designs_JobId ON Designs(JobId);

-- Check index usage
SELECT * FROM sys.dm_db_index_usage_stats 
WHERE database_id = DB_ID('Ribosoft');
```

## Troubleshooting

### Common PostgreSQL Issues

#### Connection Refused

**Error**: `could not connect to server: Connection refused`

**Solutions**:
```bash
# Check if PostgreSQL is running
sudo systemctl status postgresql

# Start PostgreSQL if stopped
sudo systemctl start postgresql

# Check port and configuration
sudo netstat -tlnp | grep 5432

# Verify pg_hba.conf allows connections
sudo nano /etc/postgresql/*/main/pg_hba.conf
```

#### Authentication Failed

**Error**: `FATAL: password authentication failed`

**Solutions**:
```bash
# Reset user password
sudo -u postgres psql
ALTER USER ribosoft_user PASSWORD 'new_password';

# Check connection string matches credentials
# Verify pg_hba.conf authentication method
```

#### Permission Denied

**Error**: `permission denied for table`

**Solutions**:
```sql
-- Grant necessary permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ribosoft_user;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO ribosoft_user;
```

### Common SQL Server Issues

#### LocalDB Not Found

**Error**: `A network-related or instance-specific error occurred`

**Solutions**:
```bash
# Check LocalDB instances
sqllocaldb info

# Create instance if missing
sqllocaldb create "MSSQLLocalDB" -s

# Start instance
sqllocaldb start "MSSQLLocalDB"
```

#### Login Failed

**Error**: `Login failed for user`

**Solutions**:
- Verify connection string credentials
- Check SQL Server authentication mode
- Ensure user exists and has proper permissions
- For Windows Authentication, run application as appropriate user

### Entity Framework Issues

#### Migration Errors

**Error**: `Unable to create an object of type 'DbContext'`

**Solutions**:
```bash
# Ensure correct connection string in appsettings.json
# Verify EntityFrameworkProvider setting matches database type
# Check that database server is running

# Rebuild and try again
dotnet clean
dotnet build
dotnet ef database update --context NpgsqlDbContext
```

#### Context Configuration Issues

**Error**: `No database provider has been configured`

**Solutions**:
1. Verify `EntityFrameworkProvider` in appsettings.json
2. Check that correct context is specified in commands
3. Ensure connection string name matches configuration

## Environment-Specific Configuration

### Development Environment

```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft_dev;Username=ribosoft_user;Password=dev_password;Pooling=true;"
  },
  "EntityFrameworkProvider": "Npgsql",
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Testing Environment

```json
{
  "ConnectionStrings": {
    "NpgsqlConnection": "Host=localhost;Port=5432;Database=ribosoft_test;Username=ribosoft_user;Password=test_password;Pooling=true;"
  },
  "EntityFrameworkProvider": "Npgsql"
}
```

### Production Environment

Use environment variables for security:

```bash
export ConnectionStrings__NpgsqlConnection="Host=localhost;Port=5432;Database=ribosoft;Username=ribosoft_user;Password=secure_production_password;Pooling=true;"
export EntityFrameworkProvider="Npgsql"
```

## Security Best Practices

### Database Security

1. **Use Strong Passwords**: Generate complex passwords for database users
2. **Limit Permissions**: Grant only necessary privileges to application users
3. **Network Security**: Configure firewall rules to limit database access
4. **Connection Encryption**: Use SSL/TLS for database connections
5. **Regular Updates**: Keep database software updated with security patches

### Connection String Security

1. **Environment Variables**: Store sensitive connection strings in environment variables
2. **User Secrets**: Use .NET User Secrets for development
3. **Key Vaults**: Use Azure Key Vault or similar for production secrets
4. **Avoid Source Control**: Never commit connection strings with passwords

### Example Secure Configuration

```bash
# Set environment variables
export ConnectionStrings__NpgsqlConnection="Host=localhost;Port=5432;Database=ribosoft;Username=ribosoft_user;Password=${DB_PASSWORD};Pooling=true;SSL Mode=Require;"
export DB_PASSWORD="$(cat /etc/ribosoft/db_password)"
```

## Getting Help

### Useful Commands

```bash
# PostgreSQL
psql --version
pg_config --version
sudo systemctl status postgresql

# SQL Server
sqlcmd -?
sqllocaldb info

# Entity Framework
dotnet ef --version
dotnet ef dbcontext info --context NpgsqlDbContext
dotnet ef migrations list --context NpgsqlDbContext
```

### Resources

- **PostgreSQL Documentation**: https://www.postgresql.org/docs/
- **SQL Server Documentation**: https://docs.microsoft.com/en-us/sql/
- **Entity Framework Core**: https://docs.microsoft.com/en-us/ef/core/
- **ASP.NET Core Identity**: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity

### Support

If you encounter database issues:

1. **Check logs**: Application and database logs for error details
2. **Verify configuration**: Connection strings and provider settings
3. **Test connectivity**: Use database client tools to verify connections
4. **Review migrations**: Ensure all migrations are applied correctly
5. **Check permissions**: Verify database user has necessary privileges

This guide provides comprehensive coverage of local database setup and management for Ribosoft development and deployment scenarios.
