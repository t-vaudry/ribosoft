# Download Directory Permission Issues - Troubleshooting Guide

## 🔧 **Quick Fix Commands**

### **For Custom Download Directory:**
```bash
# Replace /your/custom/download/path with your actual path
DOWNLOAD_PATH="/your/custom/download/path"

# Create directory if it doesn't exist
sudo mkdir -p "$DOWNLOAD_PATH"

# Set proper ownership (replace with your application user)
sudo chown -R $USER:$USER "$DOWNLOAD_PATH"

# Set proper permissions
sudo chmod -R 755 "$DOWNLOAD_PATH"

# Verify permissions
ls -la "$DOWNLOAD_PATH"
```

### **For Default Directory (Development):**
```bash
cd /path/to/ribosoft/Ribosoft
mkdir -p Downloads/Datasets
chmod -R 755 Downloads/Datasets
```

## ⚙️ **Configuration**

### **Set Custom Download Path:**
Add to `appsettings.json`:
```json
{
  "DatasetDownloads": {
    "Path": "/your/custom/download/path",
    "AutoCreateBlastDatabase": true
  }
}
```

### **Production Recommendations:**
```json
{
  "DatasetDownloads": {
    "Path": "/var/lib/ribosoft/downloads",
    "AutoCreateBlastDatabase": true
  }
}
```

## 🚨 **Common Permission Errors**

### **Error: "Permission denied creating download directory" (Directory Already Exists)**
**Cause:** Application tries to create a directory that already exists, but lacks permissions to the parent directory.

**Solution:**
```bash
# The directory exists, so just fix permissions on the existing directory
sudo chown -R $USER:$USER /path/to/downloads
sudo chmod -R 755 /path/to/downloads

# Verify the directory is accessible
ls -la /path/to/downloads
```

**Note:** Even when a directory exists, `Directory.CreateDirectory()` may fail if the application doesn't have read permissions to check if the directory exists.

### **Error: "Permission denied accessing existing download directory"**
**Cause:** Application user doesn't have write permissions to the directory.

**Solution:**
```bash
# Fix ownership
sudo chown -R ribosoft:ribosoft /path/to/downloads

# Fix permissions
sudo chmod -R 755 /path/to/downloads
```

### **Error: "Cannot create download directory"**
**Cause:** Application doesn't have permissions to create parent directories.

**Solution:**
```bash
# Create parent directories with proper permissions
sudo mkdir -p /path/to/downloads
sudo chown -R ribosoft:ribosoft /path/to/downloads
sudo chmod -R 755 /path/to/downloads
```

### **Error: "Download directory path not found"**
**Cause:** Parent directories in the path don't exist or aren't accessible.

**Solution:**
```bash
# Ensure all parent directories exist and are accessible
sudo mkdir -p /path/to/downloads
sudo chmod 755 /path
sudo chmod 755 /path/to
sudo chmod 755 /path/to/downloads
```

## 🔍 **Diagnostic Commands**

### **Check Current Permissions:**
```bash
# Check directory permissions
ls -la /path/to/downloads

# Check parent directory permissions
ls -la /path/to/

# Check disk space
df -h /path/to/downloads
```

### **Test Write Permissions:**
```bash
# Test if you can write to the directory
touch /path/to/downloads/test.txt && rm /path/to/downloads/test.txt
echo "Write test successful"
```

### **Check Application User:**
```bash
# Find out which user the application runs as
ps aux | grep ribosoft

# Check user permissions
id ribosoft
```

## 🐳 **Docker Considerations**

### **Volume Permissions:**
```yaml
# docker-compose.yml
services:
  ribosoft:
    volumes:
      - /host/downloads:/app/downloads:rw
    environment:
      DatasetDownloads__Path: "/app/downloads"
```

### **Fix Docker Volume Permissions:**
```bash
# On host system
sudo chown -R 1000:1000 /host/downloads
sudo chmod -R 755 /host/downloads
```

## 🔧 **Application Features**

### **Automatic Validation:**
The application now validates download directory permissions at startup and provides helpful error messages.

### **Enhanced Error Handling:**
- Detailed permission error messages
- Automatic directory creation attempts
- Write permission testing
- Helpful command suggestions

### **Logging:**
Check application logs for detailed permission error information:
```bash
# Check logs
tail -f /path/to/ribosoft/logs/ribosoft.log

# Or check console output during startup
```

## 📁 **Directory Structure**

### **Recommended Production Setup:**
```
/var/lib/ribosoft/
├── downloads/           # Downloaded datasets
│   ├── GCA_123456.1.zip
│   └── GCA_789012.1.zip
└── blastdb/            # BLAST databases
    ├── GCA_123456.1/
    └── GCA_789012.1/
```

### **Development Setup:**
```
ribosoft/
├── Ribosoft/
│   └── Downloads/
│       └── Datasets/    # Default download location
└── blastdb/            # BLAST databases
```

## 🚀 **Best Practices**

1. **Use Absolute Paths:** Always configure absolute paths for production
2. **Separate User:** Run application with dedicated user account
3. **Proper Permissions:** Use 755 for directories, 644 for files
4. **Monitor Disk Space:** Large genomic datasets can consume significant space
5. **Backup Strategy:** Include downloads in backup plans if needed
6. **Log Monitoring:** Monitor logs for permission issues

## 🆘 **Still Having Issues?**

1. **Check SELinux/AppArmor:** Security policies may block file access
2. **Check Disk Space:** Ensure sufficient space for downloads
3. **Check File System:** Some file systems have special permission requirements
4. **Check Network Mounts:** NFS/CIFS mounts may have special permission handling

### **Get Help:**
```bash
# Check system logs
sudo journalctl -u ribosoft

# Check application logs
tail -f logs/ribosoft.log

# Test with minimal permissions
sudo -u ribosoft touch /path/to/downloads/test.txt
```
