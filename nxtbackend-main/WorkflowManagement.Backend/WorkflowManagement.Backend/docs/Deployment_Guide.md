# WorkflowManagement Deployment Guide

## Overview

This guide provides comprehensive instructions for deploying the WorkflowManagement application across different environments (Development, Staging, Production) using various deployment methods including Docker, Kubernetes, and traditional server deployments.

## Prerequisites

### System Requirements

#### Minimum Requirements (Development)
- **CPU**: 2 cores
- **RAM**: 4 GB
- **Storage**: 20 GB SSD
- **Network**: 100 Mbps

#### Recommended Requirements (Production)
- **CPU**: 8 cores
- **RAM**: 16 GB
- **Storage**: 100 GB SSD (application) + 500 GB SSD (database)
- **Network**: 1 Gbps
- **Load Balancer**: Required for high availability

### Software Dependencies

#### Required Software
- **.NET 8.0 Runtime** (ASP.NET Core Runtime)
- **SQL Server 2019+** or **Oracle 19c+**
- **Redis 6.0+** (for caching and session management)

#### Optional Components
- **Docker 24.0+** and **Docker Compose 2.0+**
- **Kubernetes 1.25+** (for container orchestration)
- **Nginx 1.20+** or **IIS 10+** (reverse proxy)
- **Prometheus** and **Grafana** (monitoring)
- **Elasticsearch** and **Kibana** (logging)

---

## Environment Configurations

### Development Environment

#### Configuration Settings

**appsettings.Development.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=WorkflowManagementDB_Dev;Trusted_Connection=true;TrustServerCertificate=true;",
    "Redis": "localhost:6379"
  },
  "JWT": {
    "SecretKey": "development-secret-key-min-256-bits",
    "Issuer": "WorkflowManagement-Dev",
    "Audience": "WorkflowManagement-Users-Dev",
    "ExpiryMinutes": 1440
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:3001"]
  },
  "RateLimiting": {
    "Enabled": false
  },
  "Swagger": {
    "Enabled": true
  }
}
```

### Staging Environment

#### Configuration Settings

**appsettings.Staging.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=staging-sql.internal;Database=WorkflowManagementDB_Staging;User Id=app_user;Password=${DB_PASSWORD};TrustServerCertificate=true;",
    "Redis": "staging-redis.internal:6379"
  },
  "JWT": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "WorkflowManagement-Staging",
    "Audience": "WorkflowManagement-Users-Staging",
    "ExpiryMinutes": 120
  },
  "Cors": {
    "AllowedOrigins": ["https://staging.workflowmanagement.com"]
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 200
  },
  "Swagger": {
    "Enabled": true
  },
  "HealthChecks": {
    "Enabled": true
  }
}
```

### Production Environment

#### Configuration Settings

**appsettings.Production.json**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-cluster.internal;Database=WorkflowManagementDB;User Id=app_user;Password=${DB_PASSWORD};TrustServerCertificate=true;Encrypt=true;",
    "Redis": "prod-redis-cluster.internal:6379"
  },
  "JWT": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "WorkflowManagement",
    "Audience": "WorkflowManagement-Users",
    "ExpiryMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["https://app.workflowmanagement.com", "https://workflowmanagement.com"]
  },
  "RateLimiting": {
    "Enabled": true,
    "RequestsPerMinute": 100
  },
  "Swagger": {
    "Enabled": false
  },
  "HealthChecks": {
    "Enabled": true
  },
  "Monitoring": {
    "Enabled": true,
    "PrometheusEndpoint": "/metrics"
  }
}
```

---

## Deployment Methods

## 1. Docker Deployment

### Quick Start with Docker Compose

#### Step 1: Clone Repository
```bash
git clone https://github.com/your-org/workflowmanagement.git
cd workflowmanagement
```

#### Step 2: Configure Environment Variables
Create `.env` file:
```bash
# Database Configuration
DB_PASSWORD=WorkflowManagement@2024!
SA_PASSWORD=WorkflowManagement@2024!

# JWT Configuration
JWT_SECRET_KEY=your-super-secret-jwt-key-min-256-bits-long

# Email Configuration (Optional)
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-app-password

# Build Information
BUILD_DATE=$(date -u +%Y-%m-%dT%H:%M:%SZ)
GIT_COMMIT=$(git rev-parse --short HEAD)
```

#### Step 3: Deploy with Docker Compose
```bash
# Build and start all services
docker-compose up -d

# Check service status
docker-compose ps

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

#### Step 4: Initialize Database
```bash
# Wait for SQL Server to be ready
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'WorkflowManagement@2024!' -Q "SELECT 1"

# Run database initialization
docker-compose exec api dotnet ef database update

# Optional: Seed sample data
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'WorkflowManagement@2024!' -i /docker-entrypoint-initdb.d/seed-data.sql
```

#### Step 5: Verify Deployment
```bash
# Health check
curl http://localhost:8080/health

# API documentation (if enabled)
open http://localhost:8080/swagger
```

### Advanced Docker Configuration

#### Custom Docker Build
```bash
# Build custom image
docker build -t workflowmanagement:latest -f scripts/deployment/Dockerfile .

# Build with build arguments
docker build \
  --build-arg BUILD_VERSION=1.2.0 \
  --build-arg BUILD_DATE=$(date -u +%Y-%m-%dT%H:%M:%SZ) \
  --build-arg GIT_COMMIT=$(git rev-parse --short HEAD) \
  -t workflowmanagement:1.2.0 .

# Push to registry
docker tag workflowmanagement:1.2.0 your-registry.com/workflowmanagement:1.2.0
docker push your-registry.com/workflowmanagement:1.2.0
```

#### Docker Compose Override for Production
Create `docker-compose.prod.yml`:
```yaml
version: '3.8'
services:
  api:
    image: your-registry.com/workflowmanagement:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    deploy:
      replicas: 3
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
    networks:
      - production-network

  sqlserver:
    volumes:
      - /data/sqlserver:/var/opt/mssql
    deploy:
      resources:
        limits:
          cpus: '4'
          memory: 8G
```

Deploy to production:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## 2. Kubernetes Deployment

### Prerequisites

#### Kubernetes Cluster Setup
```bash
# Verify cluster access
kubectl cluster-info

# Create namespace
kubectl create namespace workflowmanagement

# Set default namespace
kubectl config set-context --current --namespace=workflowmanagement
```

#### Required Components
```bash
# Install NGINX Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# Install cert-manager for SSL certificates
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.0/cert-manager.yaml
```

### Deployment Steps

#### Step 1: Configure Secrets
```bash
# Create database password secret
kubectl create secret generic workflowmanagement-secrets \
  --from-literal=SA_PASSWORD='WorkflowManagement@2024!' \
  --from-literal=ConnectionStrings__DefaultConnection='Server=sqlserver-service,1433;Database=WorkflowManagementDB;User Id=sa;Password=WorkflowManagement@2024!;TrustServerCertificate=true;' \
  --from-literal=JWT__SecretKey='your-super-secret-jwt-key-min-256-bits-long' \
  --from-literal=Email__Username='your-email@gmail.com' \
  --from-literal=Email__Password='your-app-password'
```

#### Step 2: Deploy Database
```bash
# Apply SQL Server deployment
kubectl apply -f scripts/deployment/k8s-deployment.yaml

# Wait for SQL Server to be ready
kubectl wait --for=condition=ready pod -l app=sqlserver --timeout=300s

# Check SQL Server status
kubectl get pods -l app=sqlserver
kubectl logs -l app=sqlserver
```

#### Step 3: Deploy Application
```bash
# Deploy API application
kubectl apply -f scripts/deployment/k8s-deployment.yaml

# Check deployment status
kubectl get deployments
kubectl get pods
kubectl get services

# View application logs
kubectl logs -f deployment/workflowmanagement-api
```

#### Step 4: Configure Ingress
Update the ingress section in `k8s-deployment.yaml`:
```yaml
spec:
  tls:
  - hosts:
    - your-domain.com
    secretName: workflowmanagement-tls
  rules:
  - host: your-domain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: workflowmanagement-api-service
            port:
              number: 80
```

Apply the configuration:
```bash
kubectl apply -f scripts/deployment/k8s-deployment.yaml
```

#### Step 5: SSL Certificate Setup
Create ClusterIssuer for Let's Encrypt:
```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: admin@your-domain.com
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
    - http01:
        ingress:
          class: nginx
```

### Monitoring Kubernetes Deployment

#### Health Checks
```bash
# Check pod health
kubectl get pods -l app=workflowmanagement-api

# Check service endpoints
kubectl get endpoints

# Test health endpoint
kubectl port-forward svc/workflowmanagement-api-service 8080:80
curl http://localhost:8080/health
```

#### Scaling
```bash
# Manual scaling
kubectl scale deployment workflowmanagement-api --replicas=5

# Check HPA status
kubectl get hpa

# View resource usage
kubectl top pods
kubectl top nodes
```

#### Troubleshooting
```bash
# View pod details
kubectl describe pod <pod-name>

# Check logs
kubectl logs -f <pod-name>

# Execute into pod
kubectl exec -it <pod-name> -- /bin/bash

# Check service connectivity
kubectl exec -it <pod-name> -- curl sqlserver-service:1433
```

---

## 3. Traditional Server Deployment

### Windows Server (IIS)

#### Prerequisites
- Windows Server 2019 or later
- IIS 10 with ASP.NET Core Module
- .NET 8.0 ASP.NET Core Runtime
- SQL Server 2019 or later

#### Step 1: Prepare Server
```powershell
# Enable IIS and ASP.NET features
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpCompressionDynamic, IIS-HttpCompressionStatic, IIS-HttpErrors, IIS-HttpLogging, IIS-HttpRedirect, IIS-ApplicationDevelopment, IIS-ASPNET45

# Install ASP.NET Core Module
# Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Step 2: Deploy Application
```powershell
# Create application directory
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\WorkflowManagement"

# Copy application files
Copy-Item -Path ".\publish\*" -Destination "C:\inetpub\wwwroot\WorkflowManagement" -Recurse

# Set permissions
icacls "C:\inetpub\wwwroot\WorkflowManagement" /grant "IIS_IUSRS:(OI)(CI)F" /T
```

#### Step 3: Configure IIS
```powershell
# Create application pool
New-WebAppPool -Name "WorkflowManagement" -Force
Set-ItemProperty -Path "IIS:\AppPools\WorkflowManagement" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
Set-ItemProperty -Path "IIS:\AppPools\WorkflowManagement" -Name "managedRuntimeVersion" -Value ""

# Create web application
New-WebApplication -Name "WorkflowManagement" -Site "Default Web Site" -PhysicalPath "C:\inetpub\wwwroot\WorkflowManagement" -ApplicationPool "WorkflowManagement"
```

#### Step 4: Configure SSL
```powershell
# Install SSL certificate (example with self-signed for testing)
New-SelfSignedCertificate -DnsName "workflowmanagement.local" -CertStoreLocation "cert:\LocalMachine\My"

# Add HTTPS binding
New-WebBinding -Name "Default Web Site" -Protocol "https" -Port 443 -SslFlags 1
```

### Linux Server (Nginx + Systemd)

#### Prerequisites
- Ubuntu 22.04 or CentOS 8+
- Nginx 1.20+
- .NET 8.0 Runtime
- PostgreSQL 14+ or SQL Server 2019

#### Step 1: Install Dependencies
```bash
# Ubuntu/Debian
apt update
apt install -y nginx postgresql-14 redis-server

# Install .NET 8.0
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y aspnetcore-runtime-8.0
```

#### Step 2: Deploy Application
```bash
# Create application directory
sudo mkdir -p /var/www/workflowmanagement
sudo chown -R www-data:www-data /var/www/workflowmanagement

# Copy application files
sudo cp -r ./publish/* /var/www/workflowmanagement/

# Set permissions
sudo chmod +x /var/www/workflowmanagement/WorkflowManagement.API
```

#### Step 3: Create Systemd Service
```bash
sudo tee /etc/systemd/system/workflowmanagement.service > /dev/null <<EOF
[Unit]
Description=WorkflowManagement API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/workflowmanagement/WorkflowManagement.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=workflowmanagement-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
WorkingDirectory=/var/www/workflowmanagement

[Install]
WantedBy=multi-user.target
EOF

# Enable and start service
sudo systemctl enable workflowmanagement
sudo systemctl start workflowmanagement
sudo systemctl status workflowmanagement
```

#### Step 4: Configure Nginx
```bash
sudo tee /etc/nginx/sites-available/workflowmanagement > /dev/null <<EOF
server {
    listen 80;
    server_name workflowmanagement.com www.workflowmanagement.com;
    return 301 https://\$host\$request_uri;
}

server {
    listen 443 ssl http2;
    server_name workflowmanagement.com www.workflowmanagement.com;
    
    ssl_certificate /etc/ssl/certs/workflowmanagement.crt;
    ssl_certificate_key /etc/ssl/private/workflowmanagement.key;
    
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers ECDHE-RSA-AES256-GCM-SHA512:DHE-RSA-AES256-GCM-SHA512:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES256-GCM-SHA384;
    ssl_prefer_server_ciphers off;
    
    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
    
    location /health {
        proxy_pass http://localhost:8080/health;
        access_log off;
    }
}
EOF

# Enable site
sudo ln -s /etc/nginx/sites-available/workflowmanagement /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Database Setup

### SQL Server Setup

#### Windows
```sql
-- Create database and user
CREATE DATABASE WorkflowManagementDB;
GO

USE WorkflowManagementDB;
GO

CREATE LOGIN app_user WITH PASSWORD = 'SecurePassword123!';
CREATE USER app_user FOR LOGIN app_user;
ALTER ROLE db_owner ADD MEMBER app_user;
GO

-- Run schema creation script
:r scripts\database\create-database.sql

-- Run seed data script (optional)
:r scripts\database\seed-data.sql
```

#### Linux (SQL Server on Docker)
```bash
# Run SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=WorkflowManagement@2024!" \
   -p 1433:1433 --name sql-server -d mcr.microsoft.com/mssql/server:2022-latest

# Wait for SQL Server to start
sleep 30

# Create database
docker exec -i sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'WorkflowManagement@2024!' <<EOF
CREATE DATABASE WorkflowManagementDB;
GO
USE WorkflowManagementDB;
GO
CREATE LOGIN app_user WITH PASSWORD = 'SecurePassword123!';
CREATE USER app_user FOR LOGIN app_user;
ALTER ROLE db_owner ADD MEMBER app_user;
GO
EOF

# Run database scripts
docker exec -i sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'WorkflowManagement@2024!' -d WorkflowManagementDB -i /scripts/database/create-database.sql
docker exec -i sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'WorkflowManagement@2024!' -d WorkflowManagementDB -i /scripts/database/seed-data.sql
```

### PostgreSQL Setup (Alternative)

#### Installation and Configuration
```bash
# Install PostgreSQL
sudo apt install postgresql-14 postgresql-contrib

# Create database and user
sudo -u postgres psql <<EOF
CREATE DATABASE workflowmanagementdb;
CREATE USER app_user WITH PASSWORD 'SecurePassword123!';
GRANT ALL PRIVILEGES ON DATABASE workflowmanagementdb TO app_user;
\q
EOF

# Update connection string in appsettings.json
"DefaultConnection": "Host=localhost;Database=workflowmanagementdb;Username=app_user;Password=SecurePassword123!"
```

### Redis Setup

#### Linux Installation
```bash
# Install Redis
sudo apt install redis-server

# Configure Redis
sudo nano /etc/redis/redis.conf
# Uncomment and modify:
# bind 127.0.0.1 ::1
# requirepass your-redis-password

# Restart Redis
sudo systemctl restart redis-server
sudo systemctl enable redis-server

# Test Redis connection
redis-cli ping
```

#### Docker Redis
```bash
# Run Redis container
docker run -d --name redis -p 6379:6379 redis:7-alpine redis-server --requirepass "your-redis-password"

# Test connection
docker exec -it redis redis-cli -a "your-redis-password" ping
```

---

## SSL/TLS Configuration

### Let's Encrypt (Free SSL)

#### Using Certbot
```bash
# Install certbot
sudo apt install snapd
sudo snap install core; sudo snap refresh core
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot

# Obtain certificate
sudo certbot --nginx -d workflowmanagement.com -d www.workflowmanagement.com

# Test renewal
sudo certbot renew --dry-run

# Setup automatic renewal
echo "0 12 * * * /usr/bin/certbot renew --quiet" | sudo crontab -
```

#### Manual Certificate Installation
```bash
# Create certificate directory
sudo mkdir -p /etc/ssl/workflowmanagement

# Copy certificate files
sudo cp your-certificate.crt /etc/ssl/workflowmanagement/
sudo cp your-private-key.key /etc/ssl/workflowmanagement/
sudo cp ca-bundle.crt /etc/ssl/workflowmanagement/

# Set permissions
sudo chmod 600 /etc/ssl/workflowmanagement/*
sudo chown root:root /etc/ssl/workflowmanagement/*
```

### Application SSL Configuration

#### Configure HTTPS in appsettings.json
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:8080"
      },
      "Https": {
        "Url": "https://0.0.0.0:8443",
        "Certificate": {
          "Path": "/etc/ssl/workflowmanagement/certificate.pfx",
          "Password": "certificate-password"
        }
      }
    }
  },
  "ForwardedHeaders": {
    "Enabled": true,
    "ForwardedProtoHeaderName": "X-Forwarded-Proto"
  }
}
```

---

## Monitoring and Logging

### Application Performance Monitoring

#### Prometheus Setup
```bash
# Create Prometheus configuration
cat > /etc/prometheus/prometheus.yml <<EOF
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'workflowmanagement-api'
    static_configs:
      - targets: ['localhost:8080']
    metrics_path: '/metrics'
    scrape_interval: 30s
EOF

# Start Prometheus
docker run -d \
  --name prometheus \
  -p 9090:9090 \
  -v /etc/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml \
  prom/prometheus
```

#### Grafana Setup
```bash
# Start Grafana
docker run -d \
  --name grafana \
  -p 3000:3000 \
  -e "GF_SECURITY_ADMIN_PASSWORD=admin" \
  grafana/grafana

# Access Grafana at http://localhost:3000
# Import dashboard ID: 12906 for ASP.NET Core metrics
```

### Centralized Logging

#### ELK Stack Setup
```yaml
# docker-compose.logging.yml
version: '3.8'
services:
  elasticsearch:
    image: elasticsearch:8.11.0
    environment:
      - discovery.type=single-node
      - "ES_JAVA_OPTS=-Xms1g -Xmx1g"
      - xpack.security.enabled=false
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch_data:/usr/share/elasticsearch/data

  kibana:
    image: kibana:8.11.0
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch

  logstash:
    image: logstash:8.11.0
    ports:
      - "5000:5000"
    volumes:
      - ./logstash/pipeline:/usr/share/logstash/pipeline:ro
    depends_on:
      - elasticsearch

volumes:
  elasticsearch_data:
```

#### Serilog Configuration
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Elasticsearch"],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "/app/logs/workflowmanagement-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUris": "http://elasticsearch:9200",
          "indexFormat": "workflowmanagement-logs-{0:yyyy.MM.dd}"
        }
      }
    ]
  }
}
```

---

## Security Hardening

### Application Security

#### Environment Variables Security
```bash
# Use a secrets management system
# Example: HashiCorp Vault, Azure Key Vault, AWS Secrets Manager

# For development, use .env file with restricted permissions
chmod 600 .env

# For production, use environment-specific secret management
export JWT_SECRET_KEY="$(vault kv get -field=jwt_key secret/workflowmanagement)"
export DB_PASSWORD="$(vault kv get -field=db_password secret/workflowmanagement)"
```

#### API Security Headers
```json
{
  "SecurityHeaders": {
    "ContentSecurityPolicy": "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';",
    "XFrameOptions": "DENY",
    "XContentTypeOptions": "nosniff",
    "ReferrerPolicy": "strict-origin-when-cross-origin",
    "PermissionsPolicy": "camera=(), microphone=(), geolocation=()"
  }
}
```

### Infrastructure Security

#### Firewall Configuration
```bash
# Ubuntu UFW
sudo ufw enable
sudo ufw default deny incoming
sudo ufw default allow outgoing
sudo ufw allow ssh
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw allow from 10.0.0.0/8 to any port 8080  # Internal API access only

# CentOS/RHEL firewalld
sudo firewall-cmd --permanent --add-service=ssh
sudo firewall-cmd --permanent --add-service=http
sudo firewall-cmd --permanent --add-service=https
sudo firewall-cmd --permanent --add-port=8080/tcp --source=10.0.0.0/8
sudo firewall-cmd --reload
```

#### Database Security
```sql
-- SQL Server security hardening
-- Disable unnecessary services
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;
EXEC sp_configure 'xp_cmdshell', 0;
RECONFIGURE;

-- Enable encryption
ALTER DATABASE WorkflowManagementDB SET ENCRYPTION ON;

-- Create backup encryption certificate
CREATE CERTIFICATE BackupCertificate
WITH SUBJECT = 'WorkflowManagement Backup Certificate';

-- Backup with encryption
BACKUP DATABASE WorkflowManagementDB 
TO DISK = 'C:\Backups\WorkflowManagementDB.bak'
WITH ENCRYPTION (
    ALGORITHM = AES_256,
    SERVER_CERTIFICATE = BackupCertificate
);
```

---

## Backup and Disaster Recovery

### Database Backup Strategy

#### Automated SQL Server Backup
```sql
-- Create maintenance plan for automated backups
EXEC msdb.dbo.sp_add_job 
    @job_name = 'WorkflowManagement Full Backup',
    @enabled = 1;

EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'WorkflowManagement Full Backup',
    @step_name = 'Full Backup',
    @command = 'BACKUP DATABASE WorkflowManagementDB TO DISK = ''C:\Backups\WorkflowManagementDB_Full_$(ESCAPE_SQUOTE(STRTDT))_$(ESCAPE_SQUOTE(STRTTM)).bak'' WITH COMPRESSION, CHECKSUM, INIT';

EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'Daily at 2 AM',
    @freq_type = 4,
    @freq_interval = 1,
    @active_start_time = 20000;

EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'WorkflowManagement Full Backup',
    @schedule_name = 'Daily at 2 AM';
```

#### Backup Verification Script
```bash
#!/bin/bash
# backup-verify.sh

BACKUP_DIR="/data/backups"
DATE=$(date +%Y%m%d)
RETENTION_DAYS=30

# Verify last backup
if [ -f "$BACKUP_DIR/WorkflowManagementDB_$DATE.bak" ]; then
    echo "Backup found for $DATE"
    # Test restore to verify backup integrity
    docker exec sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" \
        -Q "RESTORE VERIFYONLY FROM DISK='$BACKUP_DIR/WorkflowManagementDB_$DATE.bak'"
else
    echo "ERROR: Backup not found for $DATE"
    exit 1
fi

# Cleanup old backups
find $BACKUP_DIR -name "*.bak" -mtime +$RETENTION_DAYS -delete
```

### Application Backup

#### Configuration Backup
```bash
#!/bin/bash
# backup-config.sh

BACKUP_DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/data/config-backups"

# Create backup directory
mkdir -p $BACKUP_DIR

# Backup configuration files
tar -czf "$BACKUP_DIR/config_$BACKUP_DATE.tar.gz" \
    /etc/nginx/sites-enabled/workflowmanagement \
    /etc/systemd/system/workflowmanagement.service \
    /var/www/workflowmanagement/appsettings.Production.json \
    /etc/ssl/workflowmanagement/

# Backup to cloud storage (example: AWS S3)
aws s3 cp "$BACKUP_DIR/config_$BACKUP_DATE.tar.gz" \
    s3://workflowmanagement-backups/config/

echo "Configuration backup completed: config_$BACKUP_DATE.tar.gz"
```

### Disaster Recovery Plan

#### Recovery Time Objectives (RTO)
- **Database Recovery**: 30 minutes
- **Application Recovery**: 15 minutes
- **Full System Recovery**: 1 hour

#### Recovery Point Objectives (RPO)
- **Database**: 15 minutes (transaction log backups)
- **Configuration**: 24 hours (daily backups)
- **Application Code**: Immediate (source control)

#### Recovery Procedures

**Database Recovery**
```bash
# Stop application
sudo systemctl stop workflowmanagement

# Restore database from backup
docker exec -i sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" <<EOF
RESTORE DATABASE WorkflowManagementDB FROM DISK='/backups/WorkflowManagementDB_latest.bak'
WITH REPLACE, NORECOVERY;

RESTORE LOG WorkflowManagementDB FROM DISK='/backups/WorkflowManagementDB_log_latest.trn'
WITH RECOVERY;
GO
EOF

# Verify database integrity
docker exec -i sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" \
    -Q "DBCC CHECKDB('WorkflowManagementDB') WITH NO_INFOMSGS;"

# Start application
sudo systemctl start workflowmanagement
```

**Application Recovery**
```bash
# Pull latest code from repository
git pull origin main

# Build application
dotnet publish -c Production -o /var/www/workflowmanagement-new

# Stop application
sudo systemctl stop workflowmanagement

# Backup current version
mv /var/www/workflowmanagement /var/www/workflowmanagement-backup

# Deploy new version
mv /var/www/workflowmanagement-new /var/www/workflowmanagement

# Start application
sudo systemctl start workflowmanagement

# Verify deployment
curl http://localhost:8080/health
```

---

## Performance Optimization

### Application Performance

#### Connection Pooling
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WorkflowManagementDB;User Id=app_user;Password=SecurePassword123!;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30;"
  }
}
```

#### Caching Configuration
```json
{
  "Redis": {
    "Configuration": "localhost:6379",
    "InstanceName": "WorkflowManagement",
    "DefaultExpiration": "01:00:00",
    "SlidingExpiration": "00:15:00"
  },
  "MemoryCache": {
    "SizeLimit": 1000,
    "CompactionPercentage": 0.1
  }
}
```

### Database Performance

#### Index Optimization
```sql
-- Monitor index usage
SELECT 
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE s.database_id = DB_ID('WorkflowManagementDB')
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;

-- Rebuild fragmented indexes
ALTER INDEX ALL ON WorkflowExecutions REBUILD WITH (ONLINE = ON);
```

#### Query Performance
```sql
-- Enable Query Store
ALTER DATABASE WorkflowManagementDB SET QUERY_STORE = ON;
ALTER DATABASE WorkflowManagementDB SET QUERY_STORE (
    OPERATION_MODE = READ_WRITE,
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60
);
```

### Web Server Performance

#### Nginx Optimization
```nginx
# /etc/nginx/nginx.conf
worker_processes auto;
worker_connections 1024;

http {
    # Gzip compression
    gzip on;
    gzip_comp_level 6;
    gzip_types text/plain text/css application/javascript application/json;
    
    # Client body buffer
    client_body_buffer_size 128k;
    client_max_body_size 10m;
    
    # Proxy buffering
    proxy_buffering on;
    proxy_buffer_size 128k;
    proxy_buffers 4 256k;
    proxy_busy_buffers_size 256k;
    
    # Keep-alive
    keepalive_timeout 65;
    keepalive_requests 100;
    
    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
    
    server {
        # ... existing configuration ...
        
        location /api/ {
            limit_req zone=api burst=20 nodelay;
            proxy_pass http://localhost:8080;
            # ... proxy headers ...
        }
    }
}
```

---

## Troubleshooting Guide

### Common Issues

#### Application Won't Start
```bash
# Check service status
sudo systemctl status workflowmanagement

# View detailed logs
sudo journalctl -u workflowmanagement -f

# Check port availability
sudo netstat -tlnp | grep 8080

# Verify .NET runtime
dotnet --list-runtimes

# Test configuration
dotnet /var/www/workflowmanagement/WorkflowManagement.API.dll --dry-run
```

#### Database Connection Issues
```bash
# Test SQL Server connection
docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "WorkflowManagement@2024!" -Q "SELECT @@VERSION"

# Check database exists
docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "WorkflowManagement@2024!" -Q "SELECT name FROM sys.databases WHERE name='WorkflowManagementDB'"

# Verify user permissions
docker exec -it sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "WorkflowManagement@2024!" -Q "USE WorkflowManagementDB; SELECT USER_NAME()"
```

#### Performance Issues
```bash
# Monitor system resources
htop
iotop
free -h
df -h

# Check application metrics
curl http://localhost:8080/metrics

# Monitor database performance
# Access SQL Server Management Studio or use DMVs
```

### Log Analysis

#### Application Logs
```bash
# View real-time logs
tail -f /var/www/workflowmanagement/logs/workflowmanagement-$(date +%Y%m%d).log

# Search for errors
grep -i error /var/www/workflowmanagement/logs/*.log

# Analyze performance
grep "Request finished" /var/www/workflowmanagement/logs/*.log | awk '{print $NF}' | sort -n
```

#### System Logs
```bash
# System events
sudo journalctl -f

# Nginx logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log

# Database logs
docker logs -f sql-server
```

### Health Checks

#### Automated Health Monitoring
```bash
#!/bin/bash
# health-check.sh

ENDPOINTS=(
    "http://localhost:8080/health"
    "http://localhost:8080/health/ready"
    "http://localhost:8080/health/live"
)

for endpoint in "${ENDPOINTS[@]}"; do
    if curl -f -s "$endpoint" > /dev/null; then
        echo "✓ $endpoint - OK"
    else
        echo "✗ $endpoint - FAILED"
        exit 1
    fi
done

echo "All health checks passed"
```

#### Database Health Check
```sql
-- Check database health
SELECT 
    name,
    state_desc,
    is_read_only,
    recovery_model_desc
FROM sys.databases 
WHERE name = 'WorkflowManagementDB';

-- Check active connections
SELECT 
    COUNT(*) as active_connections,
    program_name,
    host_name
FROM sys.dm_exec_sessions 
WHERE is_user_process = 1
GROUP BY program_name, host_name;
```

---

## Maintenance

### Regular Maintenance Tasks

#### Daily Tasks
```bash
#!/bin/bash
# daily-maintenance.sh

# Check disk space
df -h | awk '$5 > 80 {print "Warning: " $0}'

# Verify backups
./backup-verify.sh

# Check service status
systemctl is-active workflowmanagement nginx redis-server

# Monitor error logs
error_count=$(grep -c "ERROR" /var/www/workflowmanagement/logs/workflowmanagement-$(date +%Y%m%d).log)
if [ "$error_count" -gt 10 ]; then
    echo "High error count: $error_count"
fi
```

#### Weekly Tasks
```bash
#!/bin/bash
# weekly-maintenance.sh

# Update system packages (test environment first)
sudo apt update && sudo apt list --upgradable

# Database maintenance
docker exec -i sql-server /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" <<EOF
USE WorkflowManagementDB;
-- Update statistics
EXEC sp_updatestats;
-- Rebuild indexes
EXEC sp_MSforeachtable 'ALTER INDEX ALL ON ? REBUILD';
GO
EOF

# Log rotation
sudo logrotate -f /etc/logrotate.d/workflowmanagement

# Clean temporary files
sudo find /tmp -type f -atime +7 -delete
```

### Update Procedures

#### Application Updates
```bash
#!/bin/bash
# update-application.sh

VERSION=$1
if [ -z "$VERSION" ]; then
    echo "Usage: $0 <version>"
    exit 1
fi

# Pull new version
git fetch origin
git checkout "v$VERSION"

# Build application
dotnet publish -c Production -o /var/www/workflowmanagement-$VERSION

# Run database migrations
cd /var/www/workflowmanagement-$VERSION
dotnet WorkflowManagement.API.dll --migrate

# Create backup of current version
sudo systemctl stop workflowmanagement
sudo mv /var/www/workflowmanagement /var/www/workflowmanagement-backup-$(date +%Y%m%d)

# Deploy new version
sudo mv /var/www/workflowmanagement-$VERSION /var/www/workflowmanagement
sudo systemctl start workflowmanagement

# Verify deployment
sleep 10
if curl -f http://localhost:8080/health; then
    echo "✓ Update successful"
    sudo rm -rf /var/www/workflowmanagement-backup-*
else
    echo "✗ Update failed, rolling back"
    sudo systemctl stop workflowmanagement
    sudo mv /var/www/workflowmanagement /var/www/workflowmanagement-failed
    sudo mv /var/www/workflowmanagement-backup-$(date +%Y%m%d) /var/www/workflowmanagement
    sudo systemctl start workflowmanagement
    exit 1
fi
```

---

## Support and Documentation

### Getting Help

- **Documentation**: https://docs.workflowmanagement.com
- **GitHub Issues**: https://github.com/your-org/workflowmanagement/issues
- **Support Email**: support@workflowmanagement.com
- **Status Page**: https://status.workflowmanagement.com

### Additional Resources

- **Architecture Guide**: `/docs/Architecture_Guide.md`
- **API Documentation**: `/docs/API_Documentation.md`
- **Database Schema**: `/docs/Database_Schema.md`
- **Security Guide**: `/docs/Security_Guide.md`

---

This deployment guide provides comprehensive instructions for deploying WorkflowManagement in various environments. Always test deployments in a staging environment before applying to production, and maintain regular backups and monitoring to ensure system reliability.