# Cookbook API AWS Maintenance Guide

## Initial Setup

### Workstation Configuration
```powershell
# Create SSH directory if it doesn't exist
mkdir ~/.ssh -Force

# Add SSH config entry
@"
Host cookbook-api
    HostName <EC2_ELASTIC_IP_DNS>
    User ec2-user
    IdentityFile ~/.ssh/aws-ec2-zarichney-api.pem
"@ | Add-Content ~/.ssh/config

# Move key to SSH directory and set permissions
Copy-Item aws-ec2-zarichney-api.pem ~/.ssh/
icacls ~/.ssh/aws-ec2-zarichney-api.pem /inheritance:r /grant:r "$($env:USERNAME):(R)"
```

### AWS Session Variables
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

## Infrastructure Management

### EC2 Instance Management
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

### Service Control
```bash
# SSH into instance
ssh zarichney-api

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

# Process monitoring
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

### Backup and Restore
```powershell
# Backup data
$timestamp = Get-Date -Format "yyyy-MM-dd-HHmm"
mkdir -Force "./backup-${timestamp}"
scp -r zarichney-api:/var/lib/cookbook-api/data/* "./backup-${timestamp}/"

# Restore data
scp -r backup-latest/* zarichney-api:/var/lib/cookbook-api/data/
```

### Configuration Management
```powershell
# View secrets
aws ssm get-parameter --name "/cookbook-api/api-key" --with-decryption

# Update secrets
aws ssm put-parameter `
    --name "/cookbook-api/api-key" `
    --value "new-value" `
    --type SecureString `
    --overwrite

# View JWT configuration
aws ssm get-parameter --name "/cookbook-api/jwt-secret-key" --with-decryption

# Update JWT secret key
aws ssm put-parameter `
    --name "/cookbook-api/jwt-secret-key" `
    --value "your_new_strong_secret_key_at_least_32_characters_long" `
    --type SecureString `
    --overwrite

# View database connection string
aws ssm get-parameter --name "/cookbook-api/identity-connection" --with-decryption

# Update database connection string
aws ssm put-parameter `
    --name "/cookbook-api/identity-connection" `
    --value "Host=localhost;Database=zarichney_identity;Username=zarichney_user;Password=new_password" `
    --type SecureString `
    --overwrite
```

### Refresh Token Management
```powershell
# Connect to PostgreSQL on the EC2 instance
ssh zarichney-api "sudo -i -u postgres psql -d zarichney_identity"

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

1. **Service Won't Start**
```bash
# Check logs
sudo journalctl -u cookbook-api -n 50

# Verify permissions
ls -la /opt/cookbook-api/
ls -la /var/lib/cookbook-api/data/
```

2. **Authentication Issues**
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

2. **Playwright Resource Exhaustion**
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

3. **CloudFront 504 Errors**
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

### Important Paths
- Application: `/opt/cookbook-api/`
- Data: `/var/lib/cookbook-api/data/`
- Logs: `/var/log/cookbook-api/`
- Service File: `/etc/systemd/system/cookbook-api.service`
- Cleanup Scripts: `/opt/cookbook-api/cleanup-playwright.sh`, `/opt/cookbook-api/monitor.sh`
- CloudWatch Logs: `/aws/cookbook-api`
- Database Migration Scripts: `/opt/cookbook-api/Server/Auth/Migrations/`
- Refresh Token Cleanup Service: Included in main application service

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