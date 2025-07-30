# BLAST Database Creation Configuration Guide

## 🚀 **Performance Issues Resolved**

The BLAST database creation process has been enhanced to handle large genomic datasets without hanging or timing out.

## ⚙️ **Configuration Options**

### **appsettings.json Configuration:**
```json
{
  "DatasetDownloads": {
    "CreateBlastDatabase": true,
    "SkipBlastDbForLargeFiles": false,
    "MaxBlastDbFileSizeMB": 500,
    "CleanupAfterProcessing": true
  },
  "Blast": {
    "BLASTDB": "/path/to/blast/databases",
    "NumThreads": 4,
    "MakeBlastDbPath": "makeblastdb"
  }
}
```

## 🔧 **Configuration Parameters**

### **DatasetDownloads Section:**

#### **CreateBlastDatabase** (default: `true`)
- **Purpose**: Enable/disable BLAST database creation entirely
- **Use Case**: Disable for testing or when only downloading sequence files
- **Example**: `"CreateBlastDatabase": false`

#### **SkipBlastDbForLargeFiles** (default: `false`)
- **Purpose**: Skip BLAST database creation for files larger than specified size
- **Use Case**: Avoid long processing times for very large genomes
- **Example**: `"SkipBlastDbForLargeFiles": true`

#### **MaxBlastDbFileSizeMB** (default: `500`)
- **Purpose**: Maximum file size (MB) for BLAST database creation when skipping is enabled
- **Use Case**: Set threshold for large file handling
- **Example**: `"MaxBlastDbFileSizeMB": 200` (skip files > 200MB)

#### **CleanupAfterProcessing** (default: `true`)
- **Purpose**: Delete extracted ZIP contents after BLAST database creation
- **Use Case**: Save disk space by removing temporary files
- **Example**: `"CleanupAfterProcessing": false` (keep extracted files)

### **Blast Section:**

#### **MakeBlastDbPath** (default: `"makeblastdb"`)
- **Purpose**: Path to makeblastdb executable
- **Use Case**: Custom BLAST installation location
- **Example**: `"MakeBlastDbPath": "/usr/local/bin/makeblastdb"`

## ⏱️ **Timeout Management**

### **Automatic Timeout Calculation:**
- **Base timeout**: 5 minutes
- **Size-based**: +1 minute per 10MB of FASTA file
- **Example**: 100MB file = 15 minute timeout (5 + 10)

### **Timeout Examples:**
| File Size | Estimated Timeout |
|-----------|------------------|
| 10 MB     | 6 minutes        |
| 50 MB     | 10 minutes       |
| 100 MB    | 15 minutes       |
| 200 MB    | 25 minutes       |
| 500 MB    | 55 minutes       |

## 🧬 **File Type Handling**

### **Automatic Database Type Detection:**
- **Protein files** (`.faa`, contains "protein"): `dbtype prot`
- **Nucleotide files** (`.fna`, genomic/RNA/CDS): `dbtype nucl`

### **FASTA File Types Processed:**
1. **Genomic FASTA** (`.fna`) - Complete genome sequences
2. **RNA FASTA** (`.fna`) - RNA sequences  
3. **Protein FASTA** (`.faa`) - Protein sequences
4. **CDS FASTA** (`.fna`) - Coding sequences

## 📊 **Progress Monitoring**

### **Progress Stages:**
- **0-70%**: Download and extraction
- **70-90%**: BLAST database creation
- **90-100%**: Finalization and cleanup

### **Logging Information:**
- File sizes and processing times
- Timeout calculations
- Skip decisions for large files
- Success/failure status for each database

## 🛠️ **Troubleshooting**

### **Common Issues:**

#### **"BLAST database creation timed out"**
**Solutions:**
1. Increase file size limit: `"MaxBlastDbFileSizeMB": 1000`
2. Enable large file skipping: `"SkipBlastDbForLargeFiles": true`
3. Disable BLAST creation: `"CreateBlastDatabase": false`

#### **"makeblastdb command not found"**
**Solutions:**
1. Install BLAST+: `sudo apt-get install ncbi-blast+`
2. Set custom path: `"MakeBlastDbPath": "/usr/local/bin/makeblastdb"`
3. Add to PATH: `export PATH=$PATH:/path/to/blast/bin`

#### **"Permission denied creating BLAST database"**
**Solutions:**
1. Fix permissions: `sudo chown -R $USER:$USER /path/to/blastdb`
2. Create directory: `sudo mkdir -p /path/to/blastdb && sudo chmod 755 /path/to/blastdb`

### **Performance Optimization:**

#### **For Large Datasets (>500MB):**
```json
{
  "DatasetDownloads": {
    "SkipBlastDbForLargeFiles": true,
    "MaxBlastDbFileSizeMB": 200,
    "CleanupAfterProcessing": true
  }
}
```

#### **For Fast Processing:**
```json
{
  "DatasetDownloads": {
    "CreateBlastDatabase": false
  }
}
```

#### **For Development/Testing:**
```json
{
  "DatasetDownloads": {
    "CreateBlastDatabase": true,
    "SkipBlastDbForLargeFiles": true,
    "MaxBlastDbFileSizeMB": 50,
    "CleanupAfterProcessing": false
  }
}
```

## 📈 **Monitoring**

### **Log Messages to Watch:**
- `"Starting BLAST database creation for X FASTA files"`
- `"Creating BLAST database X from Y (Z MB)"`
- `"Skipping BLAST database creation for large file"`
- `"BLAST database creation timed out after X minutes"`
- `"Successfully created BLAST database X in Y minutes"`

### **Progress Tracking:**
- Check download progress in the web interface
- Monitor log files for detailed progress information
- Use Hangfire dashboard to see job status

## 🎯 **Recommended Settings**

### **Production (Balanced):**
```json
{
  "DatasetDownloads": {
    "CreateBlastDatabase": true,
    "SkipBlastDbForLargeFiles": true,
    "MaxBlastDbFileSizeMB": 300,
    "CleanupAfterProcessing": true
  }
}
```

### **High Performance (Skip Large Files):**
```json
{
  "DatasetDownloads": {
    "CreateBlastDatabase": true,
    "SkipBlastDbForLargeFiles": true,
    "MaxBlastDbFileSizeMB": 100,
    "CleanupAfterProcessing": true
  }
}
```

### **Complete Processing (Patient):**
```json
{
  "DatasetDownloads": {
    "CreateBlastDatabase": true,
    "SkipBlastDbForLargeFiles": false,
    "CleanupAfterProcessing": true
  }
}
```

The enhanced BLAST database creation system now handles large datasets gracefully with proper timeouts, progress monitoring, and flexible configuration options!
