# Zarichney Full-Stack AWS Infrastructure Guide

This comprehensive guide covers maintenance tasks for the complete Zarichney full-stack application hosted on AWS, including both the Angular frontend and ASP.NET 8 backend, infrastructure management, deployment processes, and monitoring.

## Quick Reference

### Environment Setup

Before running any commands, set up your environment variables. **For security, actual values are stored in `~/system-maintenance/README.md`**:

```bash
# Set these variables before running commands (replace with actual values from system-maintenance docs)
export BACKEND_INSTANCE_ID="<backend-instance-id>"
export BACKEND_EC2_DNS="<backend-ec2-dns>"
export FRONTEND_INSTANCE_ID="<frontend-instance-id>"
export FRONTEND_EC2_DNS="<frontend-ec2-dns>"
export CLOUDFRONT_DIST_ID="<cloudfront-distribution-id>"
export S3_BUCKET="<s3-bucket-name>"
export BACKEND_SG_ID="<backend-security-group-id>"
export FRONTEND_SG_ID="<frontend-security-group-id>"

# Verify setup
echo "Backend Instance: $BACKEND_INSTANCE_ID at $BACKEND_EC2_DNS"
echo "Frontend Instance: $FRONTEND_INSTANCE_ID at $FRONTEND_EC2_DNS"
echo "CloudFront: $CLOUDFRONT_DIST_ID"
```

> **Security Note:** Never commit actual instance IDs, DNS names, or other sensitive AWS resource identifiers to this public repository. Always reference `~/system-maintenance/README.md` for actual values.

### Common Commands

#### Backend (ASP.NET 8 API)
```bash
# SSH to backend instance
ssh cookbook-api
ssh Zarichney.Server  # alias

# Backend service management
ssh cookbook-api "sudo systemctl status cookbook-api"
ssh cookbook-api "sudo systemctl restart cookbook-api"
ssh cookbook-api "sudo journalctl -u cookbook-api -f"

# Get backend instance info
aws ec2 describe-instances --instance-ids $BACKEND_INSTANCE_ID --query "Reservations[0].Instances[0].[InstanceId,PublicDnsName,State.Name]" --output table
```

#### Frontend (Angular SSR)
```bash
# SSH to frontend instance
ssh zarichney-frontend
ssh zarichney-static  # alias

# Frontend service management (PM2)
ssh zarichney-frontend "pm2 status"
ssh zarichney-frontend "pm2 restart server"
ssh zarichney-frontend "pm2 logs server"

# Get frontend instance info
aws ec2 describe-instances --instance-ids $FRONTEND_INSTANCE_ID --query "Reservations[0].Instances[0].[InstanceId,PublicDnsName,State.Name]" --output table
```

#### General AWS Operations
```bash
# Check AWS identity
aws sts get-caller-identity

# S3 static assets management
aws s3 ls s3://$S3_BUCKET
aws s3 sync local-folder/ s3://$S3_BUCKET

# CloudFront cache invalidation
aws cloudfront create-invalidation --distribution-id $CLOUDFRONT_DIST_ID --paths "/*"
aws cloudfront create-invalidation --distribution-id $CLOUDFRONT_DIST_ID --paths "/api/*"
```

### Infrastructure Overview

#### Backend Infrastructure
- **Instance Type:** `t3.small` (2 vCPU, 2GB RAM)
- **Purpose:** ASP.NET 8 API + PostgreSQL database
- **Service Management:** systemd service
- **Security Group:** Allows SSH, HTTPS, and API port access
- **IAM Role:** Configured for Secrets Manager and SSM access

#### Frontend Infrastructure
- **Instance Type:** `t2.micro` (1 vCPU, 1GB RAM)
- **Purpose:** Angular SSR with Node.js
- **Service Management:** PM2 process manager
- **Security Group:** Allows SSH, HTTP, and HTTPS access
- **Static Assets:** Deployed to S3 bucket

#### Shared Infrastructure
- **Region:** `us-east-2` (Ohio)
- **CDN:** CloudFront distribution with dual origins
- **Domain:** Custom domain with SSL certificate
- **Configuration:** AWS Secrets Manager and SSM Parameter Store

> **Note:** For specific instance IDs, DNS names, and other sensitive details, refer to `~/system-maintenance/README.md` on your local system.

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
    HostName <BACKEND_EC2_DNS>
    User ec2-user
    IdentityFile ~/.ssh/<BACKEND_SSH_KEY>

Host zarichney-frontend
    HostName <FRONTEND_EC2_DNS>
    User ec2-user
    IdentityFile ~/.ssh/<FRONTEND_SSH_KEY>
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

# Add both EC2 hosts to known hosts (replace with actual DNS names)
ssh-keyscan -H <BACKEND_EC2_DNS> >> ~/.ssh/known_hosts
ssh-keyscan -H <FRONTEND_EC2_DNS> >> ~/.ssh/known_hosts

# Test connectivity
aws sts get-caller-identity
ssh cookbook-api "echo 'Backend SSH connection successful!'"
ssh zarichney-frontend "echo 'Frontend SSH connection successful!'"

# Note: If frontend SSH fails, see troubleshooting section for key setup
```

### AWS Configuration Overview

#### Backend Infrastructure (ASP.NET 8 API)
* **Region:** us-east-2
* **EC2 Instance:** 
  * Instance Type: `t3.small` (2 vCPU, 2GB RAM)
  * Tag: `Name=cookbook-api`
  * IAM Role: Configured for AWS services access
* **Security Group:** Allows ports 22 (SSH), 443 (HTTPS), 5000 (API)
* **Database:** PostgreSQL running on the same instance

#### Frontend Infrastructure (Angular SSR)
* **EC2 Instance:**
  * Instance Type: `t2.micro` (1 vCPU, 1GB RAM)
  * Tag: `Name=zarichney-static`
  * SSH Key: Same as backend (unified SSH access)
* **Security Group:** Allows ports 22 (SSH), 80 (HTTP), 443 (HTTPS)
* **Static Assets:** S3 bucket for Angular build artifacts
* **Service Management:** PM2 process manager for Node.js SSR application
* **Application Path:** `~/app/` on instance

#### Shared Infrastructure
* **CloudFront Distribution:** Custom domain with SSL
  * Default Origin: S3 bucket for static files
  * `/api/*` Origin: Backend EC2 instance on port 5000
* **Secrets Manager:** Database password and application secrets
* **SSM Parameter Store:** Configuration parameters for application settings
* **SSL Certificate:** ACM certificate for HTTPS

### AWS Session Variables

#### PowerShell
```powershell
# Set these variables with actual values from ~/system-maintenance/README.md
$region = "us-east-2"
$backendInstanceId = "<BACKEND_INSTANCE_ID>"
$backendEc2Host = "<BACKEND_EC2_DNS>"
$backendSgId = "<BACKEND_SG_ID>"
$frontendInstanceId = "<FRONTEND_INSTANCE_ID>"
$frontendEc2Host = "<FRONTEND_EC2_DNS>"
$frontendSgId = "<FRONTEND_SG_ID>"
$cfDistId = "<CLOUDFRONT_DIST_ID>"
$s3Bucket = "<S3_BUCKET>"

# Alternative: Dynamic lookup (requires AWS CLI)
$cfDistId = (aws cloudfront list-distributions --query "DistributionList.Items[?Aliases.Items[?contains(@, 'zarichney.com')]].Id" --output text)
$backendInstanceId = (aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0].InstanceId" --output text)
$frontendInstanceId = (aws ec2 describe-instances --filters "Name=tag:Name,Values=zarichney-static" --query "Reservations[0].Instances[0].InstanceId" --output text)

# Echo values for verification
Write-Host "Region: ${region}"
Write-Host "Backend Instance: ${backendInstanceId} at ${backendEc2Host}"
Write-Host "Frontend Instance: ${frontendInstanceId} at ${frontendEc2Host}"
Write-Host "CloudFront: ${cfDistId}"
Write-Host "S3 Bucket: ${s3Bucket}"
```

#### Bash/Linux
```bash
# Set these variables with actual values from ~/system-maintenance/README.md
export AWS_REGION="us-east-2"
export BACKEND_INSTANCE_ID="<BACKEND_INSTANCE_ID>"
export BACKEND_EC2_DNS="<BACKEND_EC2_DNS>"
export BACKEND_SG_ID="<BACKEND_SG_ID>"
export FRONTEND_INSTANCE_ID="<FRONTEND_INSTANCE_ID>"
export FRONTEND_EC2_DNS="<FRONTEND_EC2_DNS>"
export FRONTEND_SG_ID="<FRONTEND_SG_ID>"
export CLOUDFRONT_DIST_ID="<CLOUDFRONT_DIST_ID>"
export S3_BUCKET="<S3_BUCKET>"

# Alternative: Dynamic lookup (requires AWS CLI)
export CLOUDFRONT_DIST_ID=$(aws cloudfront list-distributions --query "DistributionList.Items[?Aliases.Items[?contains(@, 'zarichney.com')]].Id" --output text)
export BACKEND_INSTANCE_ID=$(aws ec2 describe-instances --filters "Name=tag:Name,Values=cookbook-api" --query "Reservations[0].Instances[0].InstanceId" --output text)
export FRONTEND_INSTANCE_ID=$(aws ec2 describe-instances --filters "Name=tag:Name,Values=zarichney-static" --query "Reservations[0].Instances[0].InstanceId" --output text)

# Echo values for verification
echo "Region: ${AWS_REGION}"
echo "Backend Instance: ${BACKEND_INSTANCE_ID} at ${BACKEND_EC2_DNS}"
echo "Frontend Instance: ${FRONTEND_INSTANCE_ID} at ${FRONTEND_EC2_DNS}"
echo "CloudFront: ${CLOUDFRONT_DIST_ID}"
echo "S3 Bucket: ${S3_BUCKET}"
```

## Infrastructure Management

### EC2 Instance Management

#### PowerShell
```powershell
# Backend instance management
aws ec2 describe-instances --instance-ids $backendInstanceId
aws ec2 stop-instances --instance-ids $backendInstanceId
aws ec2 start-instances --instance-ids $backendInstanceId
aws ec2 describe-instance-status --instance-id $backendInstanceId

# Frontend instance management
aws ec2 describe-instances --instance-ids $frontendInstanceId
aws ec2 stop-instances --instance-ids $frontendInstanceId
aws ec2 start-instances --instance-ids $frontendInstanceId
aws ec2 describe-instance-status --instance-id $frontendInstanceId

# Both instances at once
aws ec2 stop-instances --instance-ids $backendInstanceId $frontendInstanceId
aws ec2 start-instances --instance-ids $backendInstanceId $frontendInstanceId
```

#### Bash/Linux
```bash
# Backend instance management
aws ec2 describe-instances --instance-ids ${BACKEND_INSTANCE_ID}
aws ec2 stop-instances --instance-ids ${BACKEND_INSTANCE_ID}
aws ec2 start-instances --instance-ids ${BACKEND_INSTANCE_ID}
aws ec2 describe-instance-status --instance-id ${BACKEND_INSTANCE_ID}
aws ec2 wait instance-running --instance-ids ${BACKEND_INSTANCE_ID}

# Frontend instance management
aws ec2 describe-instances --instance-ids ${FRONTEND_INSTANCE_ID}
aws ec2 stop-instances --instance-ids ${FRONTEND_INSTANCE_ID}
aws ec2 start-instances --instance-ids ${FRONTEND_INSTANCE_ID}
aws ec2 describe-instance-status --instance-id ${FRONTEND_INSTANCE_ID}
aws ec2 wait instance-running --instance-ids ${FRONTEND_INSTANCE_ID}

# Both instances at once
aws ec2 stop-instances --instance-ids ${BACKEND_INSTANCE_ID} ${FRONTEND_INSTANCE_ID}
aws ec2 start-instances --instance-ids ${BACKEND_INSTANCE_ID} ${FRONTEND_INSTANCE_ID}

# Get updated DNS after restart (if needed)
export BACKEND_EC2_DNS=$(aws ec2 describe-instances --instance-ids ${BACKEND_INSTANCE_ID} --query "Reservations[0].Instances[0].PublicDnsName" --output text)
export FRONTEND_EC2_DNS=$(aws ec2 describe-instances --instance-ids ${FRONTEND_INSTANCE_ID} --query "Reservations[0].Instances[0].PublicDnsName" --output text)
echo "Backend EC2 DNS: ${BACKEND_EC2_DNS}"
echo "Frontend EC2 DNS: ${FRONTEND_EC2_DNS}"
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

## GitHub Actions OIDC Authentication

### Overview
The deployment pipeline uses AWS OIDC (OpenID Connect) authentication for secure, short-lived credential access without storing long-term AWS access keys. This provides enhanced security and eliminates the need to rotate static credentials.

### AWS Infrastructure Components

#### 1. OIDC Identity Provider
- **Provider URL**: `https://token.actions.githubusercontent.com`
- **Audience**: `sts.amazonaws.com`
- **Thumbprints**: GitHub's OIDC certificate thumbprints
- **ARN**: `arn:aws:iam::${AWS_ACCOUNT_ID}:oidc-provider/token.actions.githubusercontent.com`

#### 2. IAM Role Configuration
- **Role Name**: `GitHubActionsDeploymentRole`
- **ARN**: `arn:aws:iam::${AWS_ACCOUNT_ID}:role/GitHubActionsDeploymentRole`
- **Trust Policy**: Allows GitHub Actions from `Zarichney-Development/zarichney-api` repository
- **Subject Restrictions**:
  - If workflow jobs use GitHub Environments (e.g., `environment: production`), the OIDC token `sub` claim is environment-scoped (`repo:<org>/<repo>:environment:<env>`).
  - Allow both branch and environment subjects when using environments:
    - `repo:Zarichney-Development/zarichney-api:ref:refs/heads/main`
    - `repo:Zarichney-Development/zarichney-api:ref:refs/heads/develop`
    - `repo:Zarichney-Development/zarichney-api:environment:production`
    - `repo:Zarichney-Development/zarichney-api:environment:staging`

#### 3. Permission Policies
The role includes permissions for:
- **EC2**: Instance management and status checks
- **S3**: Static asset deployment to frontend bucket
- **CloudFront**: Cache invalidation for deployments
- **Secrets Manager**: Database password retrieval
- **SSM Parameter Store**: Application configuration access

### Repository Secrets Configuration
Required GitHub repository secrets for OIDC authentication:

```bash
# Core OIDC Configuration
AWS_OIDC_ROLE_ARN=arn:aws:iam::${AWS_ACCOUNT_ID}:role/GitHubActionsDeploymentRole

# Infrastructure Endpoints
EC2_HOST_BACKEND=${BACKEND_EC2_DNS}
EC2_HOST_FRONTEND=${FRONTEND_EC2_DNS}
CLOUDFRONT_DISTRIBUTION_ID=${CLOUDFRONT_DIST_ID}
S3_BUCKET=${S3_BUCKET}

# Application Configuration
SECRET_ID=cookbook-factory-secrets
SECRET_DB_PASSWORD_KEY=DatabasePassword

# SSH Access
EC2_SSH_KEY=${EC2_SSH_PRIVATE_KEY_CONTENT}
```

### Deployment Workflow Integration
The GitHub Actions workflow (`.github/workflows/deploy.yml`) uses OIDC authentication:

```yaml
permissions:
  id-token: write
  contents: read
  actions: read

steps:
  - name: Configure AWS credentials
    uses: aws-actions/configure-aws-credentials@v2
    with:
      role-to-assume: ${{ secrets.AWS_OIDC_ROLE_ARN }}
      aws-region: us-east-2
      role-session-name: GitHubActions-Backend-${{ github.run_id }}
```

Example trust policy Condition (merge with your existing policy):

```json
{
  "Condition": {
    "StringEquals": {
      "token.actions.githubusercontent.com:aud": "sts.amazonaws.com"
    },
    "StringLike": {
      "token.actions.githubusercontent.com:sub": [
        "repo:Zarichney-Development/zarichney-api:ref:refs/heads/main",
        "repo:Zarichney-Development/zarichney-api:ref:refs/heads/develop",
        "repo:Zarichney-Development/zarichney-api:environment:production",
        "repo:Zarichney-Development/zarichney-api:environment:staging"
      ]
    }
  }
}
```

### Security Features
- **Short-lived tokens**: Credentials are valid only for the duration of the GitHub Actions run
- **Repository scoping**: Access restricted to specific repository and branches
- **Branch-based conditions**: Only `main` and `develop` branches can assume the role
- **Least privilege**: Role has minimal required permissions for deployment tasks
- **No static credentials**: Eliminates need for long-term AWS access keys

### Troubleshooting OIDC Authentication

#### Common Issues

1. **"Not authorized to perform sts:AssumeRoleWithWebIdentity"**
   - Verify OIDC provider exists in AWS IAM
   - Check role trust policy includes correct repository path
   - Ensure branch name matches trust policy conditions

2. **"No OpenIDConnect provider found"**
   - Verify OIDC provider was created with correct URL and audience
   - Check provider thumbprints are current

3. **Role assumption succeeds but permissions denied**
   - Verify attached policies have required permissions
   - Check resource ARNs in policies match actual infrastructure
   - Ensure policy conditions don't block the action

#### Validation Commands

```bash
# Verify OIDC provider exists
aws iam list-open-id-connect-providers --query 'OpenIDConnectProviderList[?contains(Arn, `token.actions.githubusercontent.com`)]'

# Check role trust policy
aws iam get-role --role-name GitHubActionsDeploymentRole --query 'Role.AssumeRolePolicyDocument'

# List attached policies
aws iam list-attached-role-policies --role-name GitHubActionsDeploymentRole

# Test policy permissions (replace with actual policy ARN)
aws iam simulate-principal-policy \
  --policy-source-arn arn:aws:iam::${AWS_ACCOUNT_ID}:role/GitHubActionsDeploymentRole \
  --action-names s3:PutObject \
  --resource-arns arn:aws:s3:::${S3_BUCKET}/*
```

#### Recovery Procedures

If OIDC authentication fails:

1. **Verify Infrastructure**:
   ```bash
   # Check if OIDC provider exists
   aws iam get-open-id-connect-provider --open-id-connect-provider-arn arn:aws:iam::${AWS_ACCOUNT_ID}:oidc-provider/token.actions.githubusercontent.com
   
   # Check if role exists
   aws iam get-role --role-name GitHubActionsDeploymentRole
   ```

2. **Update Trust Policy** (if needed):
   ```bash
   aws iam update-assume-role-policy --role-name GitHubActionsDeploymentRole --policy-document file://trust-policy.json
   ```

3. **Regenerate OIDC Provider** (if corrupted):
   ```bash
   # Delete existing provider
   aws iam delete-open-id-connect-provider --open-id-connect-provider-arn arn:aws:iam::${AWS_ACCOUNT_ID}:oidc-provider/token.actions.githubusercontent.com
   
   # Recreate provider
   aws iam create-open-id-connect-provider \
     --url https://token.actions.githubusercontent.com \
     --client-id-list sts.amazonaws.com \
     --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aea1 1c58a3a8518e8759bf075b76b750d4f2df264fcd
   ```

### Monitoring and Maintenance

#### CloudTrail Events
Monitor OIDC authentication events:
- `AssumeRoleWithWebIdentity` - Role assumption attempts
- `GetOpenIDConnectProvider` - Provider access attempts
- `GetRole` - Role configuration access

#### Regular Maintenance
- **Thumbprint Updates**: GitHub may update OIDC certificate thumbprints
- **Permission Reviews**: Regularly audit role permissions for least privilege
- **Branch Updates**: Update trust policy when branch strategy changes
- **Security Scanning**: Monitor for unauthorized role assumptions

## Application Management

### Backend Deployment Process (via GitHub Actions)
* Code is checked out.
* EF Core idempotent migration script (`ApplyAllMigrations.sql`) is generated.
* Application is published (includes migration script and `.sh` runner).
* Files are copied to `/opt/cookbook-api/` on EC2 via `scp`.
* SSH command executes on EC2:
    * Retrieves DB password from **AWS Secrets Manager**.
    * Runs `/opt/cookbook-api//Services/Auth/Migrations/ApplyMigrations.sh` (which executes the SQL script via `psql`).
    * Restarts the `cookbook-api` service (`sudo systemctl restart cookbook-api`).
* CloudFront cache is invalidated for `/api/*` paths.

### Frontend Deployment Process (via GitHub Actions)
* Code is checked out and Node.js dependencies are installed.
* Angular application is built for production with SSR enabled.
* **Static Assets Deployment:**
    * Built client files are deployed to S3 bucket `static.zarichney.com`.
    * S3 sync deletes old files and uploads new ones.
* **SSR Server Deployment:**
    * Server-side files are prepared in a deployment folder.
    * Files are copied to `~/app/` on the frontend EC2 instance via `scp`.
    * SSH command executes on frontend EC2:
        * Node.js dependencies are installed with `npm ci --omit=dev`.
        * PM2 restarts the SSR server or starts it if not running.
* CloudFront cache is invalidated for all paths (`/*`).

### Service Control

#### Backend Service Control (ASP.NET 8 API)
```bash
# SSH into backend instance
ssh cookbook-api
ssh Zarichney.Server  # alias

# View real-time logs
sudo journalctl -u cookbook-api -f

# Restart service
sudo systemctl restart cookbook-api

# Check service status
sudo systemctl status cookbook-api

# Check database connection
sudo -i -u postgres psql -d zarichney_identity -c "SELECT COUNT(*) FROM \"AspNetUsers\""
```

#### Frontend Service Control (Angular SSR)
```bash
# SSH into frontend instance
ssh zarichney-frontend
ssh zarichney-static  # alias

# View PM2 process status
pm2 status

# View real-time logs
pm2 logs server

# Restart SSR server
pm2 restart server

# Check PM2 startup configuration
pm2 startup
pm2 save

# Monitor PM2 processes
pm2 monit
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

#### Frontend Health Checks
```bash
# Website accessibility
curl -I "https://yourdomain.com"

# Check if SSR server is responding
ssh zarichney-frontend "curl -I http://localhost:4000"

# Verify S3 static assets
aws s3 ls s3://static.zarichney.com --recursive | head -10

# Test CloudFront distribution
curl -I "https://yourdomain.com/favicon.ico"
```

#### Backend API Health Checks
```powershell
# Public health check
curl "https://yourdomain.com/api/factory/health"

# Secure health check
$apiKey = (aws ssm get-parameter --name "/cookbook-api/api-key" --with-decryption --query "Parameter.Value" --output text)
curl -H "X-Api-Key: ${apiKey}" "https://yourdomain.com/api/factory/health/secure"

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

#### Full-Stack Deployment Issues

1.  **GitHub Actions Pipeline Failures:**
    * **Missing GitHub Secrets:** Ensure all required secrets are configured (refer to `GITHUB_SECRETS_REQUIRED.md` for details):
      - `EC2_HOST_BACKEND` (backend instance DNS)
      - `EC2_HOST_FRONTEND` (frontend instance DNS)
      - `CLOUDFRONT_DISTRIBUTION_ID` (CloudFront distribution ID)
      - `EC2_SSH_KEY` (private key content)
    * **SSH Authentication Failures:** The frontend instance uses different key pair than backend. Verify SSH key is correct and accessible.

#### Backend Deployment Issues

2.  **Backend Deployment Fails during Migration Step:**
    * **Access Denied (Secrets Manager):** Check EC2 instance role (`cookbook-api-role`) IAM policy. Needs `secretsmanager:GetSecretValue` permission for the `cookbook-factory-secrets` ARN.
    * **Ident/Password Authentication Failed (psql):** Check PostgreSQL `pg_hba.conf` on EC2. Ensure connections from `localhost` (127.0.0.1) for `zarichney_user` use `scram-sha-256` or `md5`, not `ident` or `peer`. Reload PostgreSQL service after changes.
    * **Permission Denied for Schema Public (psql):** Connect to `zarichney_identity` DB as superuser (e.g., `postgres`) and grant privileges: `GRANT USAGE, CREATE ON SCHEMA public TO zarichney_user;`
    * **Migration SQL Script Not Found:** Verify the GitHub Action generated `ApplyAllMigrations.sql` and the `scp` command copied it to the expected location (e.g., `/opt/cookbook-api/migrations/`). Check paths in `ApplyMigrations.sh`.
    * **SQL Error during Script Execution:** Examine the `psql` output in the pipeline logs. The error likely indicates an issue in the generated SQL or an unexpected database state.

#### Frontend Deployment Issues

3.  **Frontend SSH Connection Failures:**
    * **Permission Denied:** Add your SSH public key to the instance authorized_keys file
    * **Host Key Verification Failed:** Add frontend host to known_hosts: `ssh-keyscan -H $FRONTEND_EC2_DNS >> ~/.ssh/known_hosts`
    * **Connection Timeout:** Check frontend security group allows SSH (port 22) from your IP.

#### SSH Key Setup for Frontend (if needed)
If SSH to frontend fails, add your public key via AWS Console:

1. **Access EC2 via AWS Console**: Use EC2 Instance Connect or Session Manager
2. **Add Public Key**: Run on the frontend instance:
   ```bash
   # Get your public key first (run locally)
   ssh-keygen -y -f ~/.ssh/your-private-key.pem
   
   # Then on the EC2 instance:
   mkdir -p ~/.ssh && chmod 700 ~/.ssh
   echo "your-public-key-here" >> ~/.ssh/authorized_keys
   chmod 600 ~/.ssh/authorized_keys
   ```
3. **Test Connection**: `ssh zarichney-frontend "echo 'Success'"`

4.  **PM2 Process Management Issues:**
    * **PM2 Not Found:** Install PM2 globally: `npm install -g pm2`
    * **Process Won't Start:** Check Node.js version compatibility and dependencies: `node --version && npm list`
    * **Memory Issues:** Monitor resources: `free -h` and adjust PM2 configuration for t2.micro limits

5.  **S3 Static Asset Deployment Issues:**
    * **Access Denied:** Verify AWS credentials have S3 write permissions for the static assets bucket
    * **Sync Failures:** Check bucket policy and CORS configuration
    * **CDN Cache Issues:** Ensure CloudFront invalidation is working: `aws cloudfront create-invalidation --distribution-id $CLOUDFRONT_DIST_ID --paths "/*"`

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

### Important Paths

#### Backend Instance (cookbook-api)
* Application Root: `/opt/cookbook-api/`
* Published App Files: `/opt/cookbook-api/*` (DLLs, configs, etc.)
* Migration Runner Script: `/opt/cookbook-api//Services/Auth/Migrations/ApplyMigrations.sh`
* Generated Migration SQL: `/opt/cookbook-api/migrations/ApplyAllMigrations.sql`
* Other App Data (if any): `/var/lib/cookbook-api/data/`
* Service Logs: `sudo journalctl -u cookbook-api`
* Service File: `/etc/systemd/system/cookbook-api.service`
* Cleanup Scripts: `/opt/cookbook-api/cleanup-playwright.sh`, `/opt/cookbook-api/monitor.sh`

#### Frontend Instance (zarichney-static)
* Application Root: `~/app/`
* SSR Server Files: `~/app/server/` (Node.js application)
* Static Build Files: Initially deployed to S3, then served via CloudFront
* Node.js Dependencies: `~/app/node_modules/`
* PM2 Configuration: `~/.pm2/`
* PM2 Logs: `~/.pm2/logs/`
* PM2 Process File: `~/.pm2/dump.pm2`

#### Shared Resources
* S3 Bucket: Static assets bucket (see system-maintenance docs for name)
* CloudFront Distribution: (see system-maintenance docs for ID)
* Secrets Manager: Application secrets storage
* Parameter Store: Configuration parameters with `/cookbook-api/*` prefix

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

## **Security Note**

This documentation uses placeholder values for security. For actual instance IDs, DNS names, security group IDs, and other sensitive AWS resource identifiers, refer to:

**`~/system-maintenance/README.md`** (Local system only - never commit to public repository)

The system-maintenance documentation contains:
- Actual AWS resource IDs and DNS names
- GitHub repository secrets configuration
- SSH key troubleshooting information
- Production environment details

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
