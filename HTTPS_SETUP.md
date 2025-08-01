# HTTPS Setup for Ribosoft

This document explains how to configure HTTPS for the Ribosoft application.

## Development Environment

For development, you can use ASP.NET Core's built-in development certificate:

```bash
# Generate and trust the development certificate
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Run the application
dotnet run --project Ribosoft
```

The application will be available at:
- HTTP: http://localhost:5000 (redirects to HTTPS)
- HTTPS: https://localhost:5001

## Production Environment

### Using Your Own Certificate

1. **Prepare your certificate** in PKCS#12 (.pfx) format
2. **Update configuration** in `appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "/path/to/your/certificate.pfx",
          "Password": "your_certificate_password"
        }
      }
    }
  }
}
```

3. **Run the application**:
```bash
dotnet run --project Ribosoft
```

### Docker Deployment

1. **Place your certificate** in a `certificates` directory
2. **Update docker-compose.yml** to mount your certificates:

```yaml
services:
  ribosoft:
    volumes:
      - ribosoft_logs:/app/logs
      - ./certificates:/app/certificates:ro  # Mount your certificates
    environment:
      # Set certificate password if needed
      ASPNETCORE_Kestrel__Certificates__Default__Password: "your_password"
```

3. **Update the Docker configuration** in `Ribosoft/.docker/appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:80"
      },
      "Https": {
        "Url": "https://*:443",
        "Certificate": {
          "Path": "/app/certificates/your_certificate.pfx",
          "Password": ""
        }
      }
    }
  }
}
```

4. **Start the containers**:
```bash
docker-compose up -d
```

The application will be available at:
- HTTP: http://localhost:5000 (redirects to HTTPS)
- HTTPS: https://localhost:5001

## Certificate Formats

The application expects certificates in PKCS#12 (.pfx) format. If you have other formats:

### Convert PEM to PKCS#12:
```bash
openssl pkcs12 -export -out certificate.pfx -inkey private.key -in certificate.crt
```

### Convert from other formats:
Consult OpenSSL documentation for converting from other certificate formats.

## Environment Variables

You can also configure certificates using environment variables:

```bash
export ASPNETCORE_Kestrel__Certificates__Default__Path="/path/to/certificate.pfx"
export ASPNETCORE_Kestrel__Certificates__Default__Password="your_password"
```

## Security Features

The application includes:
- **HTTPS Redirection**: HTTP requests automatically redirect to HTTPS
- **HSTS**: HTTP Strict Transport Security headers in production
- **Secure Cookies**: Cookies marked as secure when using HTTPS

## Troubleshooting

### Certificate Not Found
If you see "certificate not found" errors, verify:
- Certificate path is correct
- Certificate file is readable by the application
- Certificate is in PKCS#12 (.pfx) format

### Permission Issues
Ensure the application has read access to the certificate file:
```bash
chmod 644 /path/to/certificate.pfx
```

### Port Conflicts
If ports 5000/5001 are in use, you can change them in the configuration:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:8080"
      },
      "Https": {
        "Url": "https://localhost:8443",
        "Certificate": {
          "Path": "/path/to/certificate.pfx",
          "Password": "password"
        }
      }
    }
  }
}
```
