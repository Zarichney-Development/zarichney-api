# Cookbook API AWS Maintenance Guide

This guide covers maintenance tasks for the Zarichney API application hosted on AWS EC2, including infrastructure, application management, data, and monitoring.

## Quick Reference

### Common Commands
```bash
# SSH to EC2 instance
ssh cookbook-api
# alias
ssh Zarichney.Server

# Check service status
ssh cookbook-api "sudo systemctl status cookbook-api"

# View logs
ssh cookbook-api "sudo journalctl -u cookbook-api -f"

# Restart service
ssh cookbook-api "sudo systemctl restart cookbook-api"

# Check AWS identity
aws sts get-caller-identity

# Get instance info
aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0].[InstanceId,PublicDnsName,State.Name]" --output table
```

### Key Information
- **EC2 Instance ID:** `i-0b4bbb68afeded2e3`
- **EC2 DNS:** `ec2-123.us-east-2.compute.amazonaws.com`
- **Region:** `us-east-2`

## Initial Setup

### Prerequisites
- AWS CLI v2 installed
- SSH client installed
- AWS access credentials (IAM user with appropriate permissions)
- SSH private key file (`.pem`) for EC2 access

### Workstation Configuration

#### Windows Setup
```powershell
# Create SSH directory if it doesn't exist
mkdir ~/.ssh -Force

# Add SSH config entry
@"
Host cookbook-api
    HostName <EC2_ELASTIC_IP_DNS>
    User ec2-user
    IdentityFile ~/.ssh/ssh-ec2.pem
"@ | Add-Content ~/.ssh/config

```

#### Linux/Unix Setup
```bash
# Install AWS CLI v2
curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
unzip awscliv2.zip
./aws/install --install-dir ~/.local/aws-cli --bin-dir ~/.local/bin
echo 'export PATH="$HOME/.local/bin:$PATH"' >> ~/.bashrc && source ~/.bashrc
rm -rf awscliv2.zip aws/

# Create SSH directory with proper permissions
mkdir -p ~/.ssh
chmod 700 ~/.ssh

# Create SSH config file

chmod 600 ~/.ssh/config

# Copy SSH key and set permissions
cp /path/to/your-key.pem ~/.ssh/ssh-ec2.pem
chmod 400 ~/.ssh/ssh-ec2.pem

# Configure AWS credentials
mkdir -p ~/.aws
cat > ~/.aws/config << EOF
[default]
region = us-east-2
output = json
EOF

cat > ~/.aws/credentials << EOF
[default]
aws_access_key_id = YOUR_ACCESS_KEY_ID
aws_secret_access_key = YOUR_SECRET_ACCESS_KEY
EOF

chmod 600 ~/.aws/credentials

# Add EC2 host to known hosts
ssh-keyscan -H ec2-123.us-east-2.compute.amazonaws.com >> ~/.ssh/known_hosts

# Test connectivity
aws sts get-caller-identity
ssh cookbook-api "echo 'SSH connection successful!'"
```

### AWS Configuration Overview
* **Region:** us-east-2
* **EC2 Instance:** 
  * Instance ID: `i-0b4bbb68afeded2e3`
  * Instance Type: `t3.small` (2 vCPU, 2GB RAM)
  * Public DNS: `ec2-123.us-east-2.compute.amazonaws.com`
  * Tag: `Name=cookbook-api`
  * IAM Role: `cookbook-api-role`
* **Security Group:** Attached to EC2 instance (example ID `sg-00b18fae24f53e666`). Ensure ports 22 (SSH, restricted IPs recommended) and 443 (HTTPS) are open inbound.
* **CloudFront:** Used for caching and potentially routing traffic to the EC2 instance (example distribution ID needed).
* **Secrets Manager:** Used to store sensitive data like the database password (Secret ID: `cookbook-factory-secrets`, Key: `DbPassword`).
* **SSM Parameter Store:** May be used for other configuration (e.g., API keys, JWT secrets). Parameters prefixed with `/cookbook-api/`.
* **PostgreSQL:** Running on the EC2 instance (or potentially RDS - adjust connection details if using RDS). Database `zarichney_identity`, user `zarichney_user`.

### AWS Session Variables

#### PowerShell
```powershell
# Core variables
$region = "us-east-2"
$sgId = "sg-00b18fae24f53e666"
$cfDistId = (aws cloudfront list-distributions --query "DistributionList.Items[?Aliases.Items[?contains(@, 'zarichney.com')]].Id" --output text)
$instanceId = (aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0].InstanceId" --output text)
$ec2Host = (aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0].PublicDnsName" --output text)

# Echo values for verification
Write-Host "Region: ${region}"
Write-Host "Security Group: ${sgId}"
Write-Host "CloudFront Distribution: ${cfDistId}"
Write-Host "Instance ID: ${instanceId}"
Write-Host "EC2 Host: ${ec2Host}"
```

#### Bash/Linux
```bash
# Core variables
export AWS_REGION="us-east-2"
export SG_ID=""
export CF_DIST_ID=$(aws cloudfront list-distributions --query "DistributionList.Items[?Aliases.Items[?contains(@, 'zarichney.com')]].Id" --output text)
export INSTANCE_ID=""
export EC2_HOST=""

# Echo values for verification
echo "Region: ${AWS_REGION}"
echo "Security Group: ${SG_ID}"
echo "CloudFront Distribution: ${CF_DIST_ID}"
echo "Instance ID: ${INSTANCE_ID}"
echo "EC2 Host: ${EC2_HOST}"

# Alternative: Dynamic lookup (if instance DNS changes)
export INSTANCE_ID=$(aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0].InstanceId" --output text)
export EC2_HOST=$(aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0].PublicDnsName" --output text)
```

## Infrastructure Management

### EC2 Instance Management

#### PowerShell
```powershell
# Get instance details
aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0]"

# Stop instance
aws ec2 stop-instances --instance-ids $instanceId

# Start instance
aws ec2 start-instances --instance-ids $instanceId

# Check instance status
aws ec2 describe-instance-status --instance-id $instanceId
```

#### Bash/Linux
```bash
# Get instance details
aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0]"

# Stop instance
aws ec2 stop-instances --instance-ids ${INSTANCE_ID}

# Start instance
aws ec2 start-instances --instance-ids ${INSTANCE_ID}

# Check instance status
aws ec2 describe-instance-status --instance-id ${INSTANCE_ID}

# Wait for instance to be running
aws ec2 wait instance-running --instance-ids ${INSTANCE_ID}

# Get updated DNS after restart
export EC2_HOST=$(aws ec2 describe-instances --instance-ids ${INSTANCE_ID} --query "Reservations[0].Instances[0].PublicDnsName" --output text)
echo "New EC2 Host: ${EC2_HOST}"
```

### Security Group Management
```powershell
# List security group rules
aws ec2 describe-security-groups --group-ids $sgId

# Add inbound rule
aws ec2 authorize-security-group-ingress `
    --group-id $sgId `
    --protocol tcp `
    --port 443 `
    --cidr 0.0.0.0/0

# Update SSH access
aws ec2 update-security-group-rule-descriptions-ingress `
    --group-id $sgId `
    --ip-permissions "IpProtocol=tcp,FromPort=22,ToPort=22,IpRanges=[{CidrIp=0.0.0.0/0}]"
```

### CloudFront Management
```powershell
# Create cache invalidation
aws cloudfront create-invalidation --distribution-id $cfDistId --paths "/api/factory/*"

# Get distribution config
aws cloudfront get-distribution-config --id $cfDistId

# Update origin domain after instance restart
$distributionConfig = aws cloudfront get-distribution-config --id $cfDistId | ConvertFrom-Json
$etag = $distributionConfig.ETag

$distributionConfig.DistributionConfig.Origins.Items | 
    Where-Object { $_.Id -eq "cookbook-api" } | 
    ForEach-Object {
        $_.DomainName = $ec2Host
    }

$distributionConfig.DistributionConfig | 
    ConvertTo-Json -Depth 10 | 
    Set-Content -Path .\cloudfront-config.json

aws cloudfront update-distribution --id $cfDistId --distribution-config file://cloudfront-config.json --if-match $etag
```

## Application Management

### Deployment Process (via GitHub Actions)
* Code is checked out.
* EF Core idempotent migration script (`ApplyAllMigrations.sql`) is generated.
* Application is published (includes migration script and `.sh` runner).
* Files are copied to `/opt/cookbook-api/` on EC2 via `scp`.
* SSH command executes on EC2:
    * Retrieves DB password from **AWS Secrets Manager**.
    * Runs `/opt/cookbook-api//Services/Auth/Migrations/ApplyMigrations.sh` (which executes the SQL script via `psql`).
    * Restarts the `cookbook-api` service (`sudo systemctl restart cookbook-api`).
* CloudFront cache is invalidated.

### Service Control
```bash
# SSH into instance
ssh Zarichney.Server

# View real-time logs
sudo journalctl -u cookbook-api -f

# Restart service
sudo systemctl restart cookbook-api

# Check service status
sudo systemctl status cookbook-api
```

### Resource Monitoring
```bash
# System resources
free -h
top -b -n 1
df -h

### Process monitoring
ps aux | grep -E 'chrome|node|dotnet'

# Memory usage by component
ps aux | grep -E 'chrome|node|dotnet' | awk '{sum+=\$4} END {print \"Total Memory %: \" sum}'
```

### Playwright Management

#### Automated Cleanup Scripts
Located at `/opt/cookbook-api/cleanup-playwright.sh`:
```bash
#!/bin/bash
# Kill Playwright processes
pkill -f "chrome.*--headless"
pkill -f "node.*run-driver"

# Clean up temporary files
find /tmp -name '.org.chromium.*' -type d -exec rm -rf {} +
rm -rf /dev/shm/* 2>/dev/null

# Reset system resources
echo 3 > /proc/sys/vm/drop_caches 2>/dev/null || true
```

Located at `/opt/cookbook-api/monitor.sh`:
```bash
#!/bin/bash
# Check memory and restart if needed
MEM_USED=$(free | grep Mem | awk '{print $3/$2 * 100}')
if (( $(echo "$MEM_USED > 85" | bc -l) )); then
    /opt/cookbook-api/cleanup-playwright.sh
    systemctl restart cookbook-api
fi

# Clean up zombie processes
ZOMBIES=$(ps aux | awk '{if ($8=="Z") print $2}')
if [ ! -z "$ZOMBIES" ]; then
    kill -9 $ZOMBIES 2>/dev/null
fi
```

#### Manual Cleanup Commands
```bash
# Kill all Playwright processes
pkill -f "chrome.*--headless"
pkill -f "node.*run-driver"

# Clean up filesystem
find /tmp -name '.org.chromium.*' -type d -exec rm -rf {} +

# Reset system resources
echo 3 > /proc/sys/vm/drop_caches
```

## Data Management

### Backup and Restore (Database)
Refer to the `PostgreSQL Database Maintenance Guide` for `pg_dump` and `pg_restore` commands. Ensure backups are stored securely (e.g., S3).

### Backup and Restore (Application Data)
This applies if your application stores other data directly on the EC2 filesystem (e.g., under `/var/lib/cookbook-api/data/`).

#### PowerShell
```powershell
# Run from a machine with SSH access
# Backup data directory
$timestamp = Get-Date -Format "yyyy-MM-dd-HHmm"
mkdir -Force "./app-data-backup-${timestamp}"
scp -r cookbook-api:/var/lib/cookbook-api/data/* "./app-data-backup-${timestamp}/"

# Restore data directory (use with caution!)
# scp -r ./path/to/app-data-backup/* cookbook-api:/var/lib/cookbook-api/data/
```

#### Bash/Linux
```bash
# Backup data directory
timestamp=$(date +"%Y-%m-%d-%H%M")
mkdir -p "./app-data-backup-${timestamp}"
scp -r cookbook-api:/var/lib/cookbook-api/data/* "./app-data-backup-${timestamp}/"

# Create compressed backup
tar -czf "app-data-backup-${timestamp}.tar.gz" "./app-data-backup-${timestamp}/"
rm -rf "./app-data-backup-${timestamp}/"

# Restore data directory (use with caution!)
# tar -xzf app-data-backup-YYYY-MM-DD-HHMM.tar.gz
# scp -r ./app-data-backup-YYYY-MM-DD-HHMM/* cookbook-api:/var/lib/cookbook-api/data/
```

### Configuration Management (Secrets & Parameters)

**Database Password (AWS Secrets Manager):**
```bash
# View DB Password (Value is JSON string, needs parsing)
aws secretsmanager get-secret-value --secret-id cookbook-factory-secrets --region us-east-2

# To get JUST the password using AWS CLI v2 and jq:
aws secretsmanager get-secret-value --secret-id cookbook-factory-secrets --region us-east-2 --query SecretString --output text | jq -r .DbPassword

# Update DB Password in Secrets Manager (update the secret value via Console or CLI update-secret)
# Example: aws secretsmanager update-secret --secret-id cookbook-factory-secrets --secret-string "{\"DbPassword\":\"new_secure_password\",\"OtherKey\":\"value\"}"
```

**Other Parameters (AWS Systems Manager Parameter Store):**
*(Examples assume parameters are still in SSM - adjust if moved to Secrets Manager)*
```bash
# View non-sensitive parameter (replace name)
aws ssm get-parameter --name "/cookbook-api/some-parameter" --region us-east-2

# View sensitive parameter (replace name)
aws ssm get-parameter --name "/cookbook-api/jwt-secret-key" --with-decryption --region us-east-2

# Update sensitive parameter (replace name and value)
aws ssm put-parameter \
    --name "/cookbook-api/jwt-secret-key" \
    --value "new_very_strong_secret_key_32_chars_plus" \
    --type SecureString \
    --overwrite \
    --region us-east-2
```

### Refresh Token Management (Direct SQL)
```powershell
# Connect to PostgreSQL on the EC2 instance
ssh Zarichney.Server "sudo -i -u postgres psql -d zarichney_identity"

# View token statistics
\x on
SELECT 
  COUNT(*) as total_tokens,
  COUNT(*) FILTER (WHERE "ExpiresAt" < NOW()) as expired_tokens,
  COUNT(*) FILTER (WHERE "IsUsed" = true) as used_tokens,
  COUNT(*) FILTER (WHERE "IsRevoked" = true) as revoked_tokens,
  COUNT(*) FILTER (WHERE "ExpiresAt" >= NOW() AND "IsUsed" = false AND "IsRevoked" = false) as active_tokens
FROM "RefreshTokens";
\x off

# Clean up expired, used or revoked tokens
DELETE FROM "RefreshTokens"
WHERE "ExpiresAt" < NOW() OR "IsUsed" = true OR "IsRevoked" = true;

# Find users with suspicious token activity (many tokens in short time)
SELECT u."Email", COUNT(r."Id") as token_count, MIN(r."CreatedAt") as first_token, MAX(r."CreatedAt") as last_token
FROM "RefreshTokens" r
JOIN "AspNetUsers" u ON r."UserId" = u."Id"
WHERE r."CreatedAt" > NOW() - INTERVAL '24 HOURS'
GROUP BY u."Email"
HAVING COUNT(r."Id") > 10;

# Exit PostgreSQL
\q
```

## Health Checks and Monitoring

### API Health Checks
```powershell
# Public health check
curl "https://zarichney.com/api/factory/health"

# Secure health check
$apiKey = (aws ssm get-parameter --name "/cookbook-api/api-key" --with-decryption --query "Parameter.Value" --output text)
curl -H "X-Api-Key: ${apiKey}" "https://zarichney.com/api/factory/health/secure"

# Test Authentication Flow
$testUserEmail = "testuser@example.com"
$testUserPassword = "TestPassword123!"

# Create test user if needed (only run once)
$createUserPayload = @{
    "email" = $testUserEmail
    "password" = $testUserPassword
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://zarichney.com/api/auth/register" -Method POST -ContentType "application/json" -Body $createUserPayload

# Test login and refresh token flow
$loginPayload = @{
    "email" = $testUserEmail
    "password" = $testUserPassword
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "https://zarichney.com/api/auth/login" -Method POST -ContentType "application/json" -Body $loginPayload

# Store tokens
$accessToken = $loginResponse.token
$refreshToken = $loginResponse.refreshToken

# Test protected endpoint with access token
Invoke-RestMethod -Uri "https://zarichney.com/api/health/secure" -Method GET -Headers @{
    "Authorization" = "Bearer $accessToken"
}

# Test refresh token endpoint
$refreshPayload = @{
    "refreshToken" = $refreshToken
} | ConvertTo-Json

$refreshResponse = Invoke-RestMethod -Uri "https://zarichney.com/api/auth/refresh" -Method POST -ContentType "application/json" -Body $refreshPayload

# Verify new tokens were returned
if ($refreshResponse.token -and $refreshResponse.refreshToken) {
    Write-Host "Refresh token flow successful" -ForegroundColor Green
} else {
    Write-Host "Refresh token flow failed" -ForegroundColor Red
}
```

### System Health
```bash
# Service health
sudo systemctl status cookbook-api

# Resource usage
free -h
top -b -n 1
df -h

# Process health
ps aux | grep -E 'chrome|node|dotnet'
```

### CloudWatch Metrics
```powershell
# Check CPU utilization
aws cloudwatch get-metric-statistics `
    --namespace AWS/EC2 `
    --metric-name CPUUtilization `
    --dimensions Name=InstanceId,Value=$instanceId `
    --start-time (Get-Date).AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ss") `
    --end-time (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss") `
    --period 300 `
    --statistics Average
```

## Troubleshooting Guide

### Common Issues and Solutions

1.  **Deployment Fails during Migration Step:**
    * **Access Denied (Secrets Manager):** Check EC2 instance role (`cookbook-api-role`) IAM policy. Needs `secretsmanager:GetSecretValue` permission for the `cookbook-factory-secrets` ARN. [cite: image_a458a3.png]
    * **Ident/Password Authentication Failed (psql):** Check PostgreSQL `pg_hba.conf` on EC2. Ensure connections from `localhost` (127.0.0.1) for `zarichney_user` use `scram-sha-256` or `md5`, not `ident` or `peer`. Reload PostgreSQL service after changes.
    * **Permission Denied for Schema Public (psql):** Connect to `zarichney_identity` DB as superuser (e.g., `postgres`) and grant privileges: `GRANT USAGE, CREATE ON SCHEMA public TO zarichney_user;`
    * **Migration SQL Script Not Found:** Verify the GitHub Action generated `ApplyAllMigrations.sql` and the `scp` command copied it to the expected location (e.g., `/opt/cookbook-api/migrations/`). Check paths in `ApplyMigrations.sh`.
    * **SQL Error during Script Execution:** Examine the `psql` output in the pipeline logs. The error likely indicates an issue in the generated SQL or an unexpected database state.

2.  **Service Won't Start (Post-Deployment):**
    * Check service status and logs: `sudo systemctl status cookbook-api`, `sudo journalctl -u cookbook-api -n 100 -f`.
    * Verify permissions on `/opt/cookbook-api/` (`sudo chown -R ec2-user:ec2-user /opt/cookbook-api/`).
    * Check `appsettings.Production.json` for syntax errors.
    * Ensure connection strings/secrets are correctly configured and accessible (test `aws secretsmanager get-secret-value` manually from EC2).

```bash
# Check logs
sudo journalctl -u cookbook-api -n 50

# Verify permissions
ls -la /opt/cookbook-api/
ls -la /var/lib/cookbook-api/data/
```

3.  **Authentication Issues (Runtime):**
    * Check application logs filtered for auth keywords (`sudo journalctl -u cookbook-api | grep -i "auth\|token\|login\|jwt\|identity"`).
    * Verify database connectivity from the application.
    * Check JWT settings in configuration (SSM/Secrets Manager) match application expectations.
    * Check clock skew between EC2 instance and token issuer if validating lifetime.
```bash
# Check authentication-related logs
sudo journalctl -u cookbook-api | grep -i "auth\|token\|login\|jwt"

# Check PostgreSQL connection
sudo systemctl status postgresql

# Verify database connection by running test query
sudo -i -u postgres psql -d zarichney_identity -c "SELECT COUNT(*) FROM \"AspNetUsers\""

# Check refresh token table
sudo -i -u postgres psql -d zarichney_identity -c "SELECT COUNT(*) FROM \"RefreshTokens\""

# Ensure JWT settings are properly configured
cat /opt/cookbook-api/appsettings.Production.json | grep -A 10 "JwtSettings"
```

4.  **Playwright Resource Exhaustion:**
```bash
# Check for hung processes
ps aux | grep -E 'chrome|node'

# Kill stuck processes
pkill -f "chrome.*--headless"
pkill -f "node.*run-driver"

# Clean up and restart
sudo /opt/cookbook-api/cleanup-playwright.sh
sudo systemctl restart cookbook-api
```

5.  **CloudFront Errors (5xx):**
    * Check application logs on EC2 for errors corresponding to the request time.
    * Verify the application service `cookbook-api` is running (`sudo systemctl status cookbook-api`).
    * Ensure the EC2 instance's security group allows traffic from CloudFront IPs on port 443 (or 80 if using HTTP).
    * Verify CloudFront Origin settings point to the correct EC2 DNS/IP and port. Check health checks if configured.
```powershell
# Verify EC2 DNS matches CloudFront origin
$distributionConfig = aws cloudfront get-distribution-config --id $cfDistId | ConvertFrom-Json
$currentOrigin = $distributionConfig.DistributionConfig.Origins.Items | 
    Where-Object { $_.Id -eq "cookbook-api" } | 
    Select-Object -ExpandProperty DomainName

Write-Host "Current Origin: $currentOrigin"
Write-Host "EC2 DNS: $ec2Host"

# Update if different
if ($currentOrigin -ne $ec2Host) {
    # Update CloudFront origin (see CloudFront Management section)
}
```

4. **System Resource Issues**
```bash
# Check memory
free -h

# Check process count
ps aux | wc -l

# Monitor CPU
top -b -n 1

# Clean up resources
sudo /opt/cookbook-api/cleanup-playwright.sh
```

### Important Paths (on EC2)
* Application Root: `/opt/cookbook-api/`
* Published App Files: `/opt/cookbook-api/*` (DLLs, configs, etc.)
* Migration Runner Script: `/opt/cookbook-api//Services/Auth/Migrations/ApplyMigrations.sh`
* Generated Migration SQL: `/opt/cookbook-api/migrations/ApplyAllMigrations.sql` (Check `ApplyMigrations.sh` for exact path used)
* Other App Data (if any): `/var/lib/cookbook-api/data/`
* Service Logs: `sudo journalctl -u cookbook-api`
* Service File: `/etc/systemd/system/cookbook-api.service`
* Cleanup Scripts: `/opt/cookbook-api/cleanup-playwright.sh`, `/opt/cookbook-api/monitor.sh`

### Service Configuration
The service configuration at `/etc/systemd/system/cookbook-api.service`:
```ini
[Unit]
Description=Zarichney API Service
After=network.target

[Service]
Type=simple
User=ec2-user
WorkingDirectory=/opt/cookbook-api
ExecStart=/opt/cookbook-api/start-cookbook.sh
Restart=on-failure
RestartSec=5s
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=APP_DATA_PATH=/var/lib/cookbook-api/data/

# Adjusted resource limits for t3.small
CPUQuota=90%
MemoryHigh=1.5G
MemoryMax=1.8G
TasksMax=200
LimitNPROC=200
LimitNOFILE=8192

# Run cleanup script as root on stop
ExecStopPost=/bin/sh -c 'sudo /opt/cookbook-api/cleanup-playwright.sh'

[Install]
WantedBy=multi-user.target
```


## **Managing Systemd Service**

### **Service File Configuration**

Ensure `/etc/systemd/system/cookbook-api.service` is properly configured.

- **Set `MemoryLimit` to prevent the application from consuming all available memory.

### **Reload and Restart Service**

- **Ensure Execute Permissions is set:**

  ```bash
  chmod +x /etc/systemd/system/cookbook-api.service
  ```

- **Reload systemd daemon:**

  ```bash
  sudo systemctl daemon-reload
  ```

- **Restart the service:**

  ```bash
  sudo systemctl restart cookbook-api.service
  ```

- **Check service status:**

  ```bash
  sudo systemctl status cookbook-api.service
  ```

---

## **Scripts**

### **start-server.sh**

- **Purpose:** Starts the `cookbook-api` application.

- **Ensure it has execute permissions:**

  ```bash
  chmod +x /opt/cookbook-api/start-server.sh
  ```

### **cleanup-playwright.sh**

- **Purpose:** Cleans up Playwright processes and temporary files.

- **Modifications:**

    - **Remove or comment out commands requiring root permissions, such as writing to `/proc/sys/vm/drop_caches`.
    - **Handle `find` command permission errors by excluding protected directories or suppressing errors:

      ```bash
      find /tmp -name "playwright*" 2>/dev/null -exec rm -rf {} +
      ```

- **Ensure it has execute permissions:**

  ```bash
  chmod +x /opt/cookbook-api/cleanup-playwright.sh
  ```

---