# SoftwareComp - AWS ECS Fargate Deployment Guide

This guide provides comprehensive instructions for deploying the SoftwareComp .NET 8 application to AWS ECS Fargate.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development Setup](#local-development-setup)
3. [Docker Deployment](#docker-deployment)
4. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
5. [ECS Fargate Setup](#ecs-fargate-setup)
6. [ECS Task Definition Explained](#ecs-task-definition-explained)
7. [ECS Service Configuration](#ecs-service-configuration)
8. [Deployment Walkthrough](#deployment-walkthrough)
9. [Troubleshooting](#troubleshooting)
10. [Scaling and Management](#scaling-and-management)
11. [Security Considerations](#security-considerations)
12. [Monitoring and Observability](#monitoring-and-observability)

---

## Prerequisites

### Required Software

- **.NET 8 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop**: [Download](https://www.docker.com/products/docker-desktop)
- **AWS CLI v2**: [Installation Guide](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- **jq** (for Linux/macOS): JSON processor for shell scripts
  - Ubuntu/Debian: `sudo apt-get install jq`
  - macOS: `brew install jq`

### AWS Account Requirements

- Active AWS account with appropriate permissions
- IAM user with programmatic access
- AWS CLI configured with credentials

### Required AWS Permissions

Your IAM user/role must have permissions for:
- ECS (create/update clusters, services, task definitions)
- ECR (create repositories, push images)
- IAM (pass role to ECS tasks)
- EC2 (VPC, subnets, security groups)
- Elastic Load Balancing (create/manage ALBs)
- CloudWatch Logs (create log groups)

---

## Local Development Setup

### 1. Clone and Build

```bash
cd /modernize-data/studio-data/TNT1001/APP2784/transformed-code/915/studio-workspace/SoftwareComp
dotnet restore
dotnet build -c Release
```

### 2. Run Locally

```bash
dotnet run --project SoftwareComp.csproj
```

The application will start on `http://localhost:8080`.

### 3. Test Health Endpoint

```bash
curl http://localhost:8080/health
```

---

## Docker Deployment

### 1. Build Docker Image Locally

```bash
docker build -t softwarecomp:latest -f Dockerfile .
```

### 2. Run Container Locally

```bash
docker run -d -p 8080:8080 --name softwarecomp-app softwarecomp:latest
```

### 3. Test Containerized Application

```bash
curl http://localhost:8080/health
```

### 4. View Logs

```bash
docker logs -f softwarecomp-app
```

### 5. Stop and Remove

```bash
docker stop softwarecomp-app
docker rm softwarecomp-app
```

### Using Docker Compose

```bash
# Start application
docker-compose up -d

# View logs
docker-compose logs -f

# Stop application
docker-compose down
```

---

## AWS ECS Fargate Prerequisites

### 1. Configure AWS CLI

```bash
aws configure
```

Provide:
- AWS Access Key ID
- AWS Secret Access Key
- Default region (e.g., `us-east-1`)
- Default output format (e.g., `json`)

### 2. Create VPC and Networking (if not exists)

```bash
# Create VPC
VPC_ID=$(aws ec2 create-vpc --cidr-block 10.0.0.0/16 --query 'Vpc.VpcId' --output text)

# Enable DNS hostnames
aws ec2 modify-vpc-attribute --vpc-id $VPC_ID --enable-dns-hostnames

# Create Internet Gateway
IGW_ID=$(aws ec2 create-internet-gateway --query 'InternetGateway.InternetGatewayId' --output text)
aws ec2 attach-internet-gateway --vpc-id $VPC_ID --internet-gateway-id $IGW_ID

# Create public subnets in different AZs
SUBNET_1=$(aws ec2 create-subnet --vpc-id $VPC_ID --cidr-block 10.0.1.0/24 --availability-zone us-east-1a --query 'Subnet.SubnetId' --output text)
SUBNET_2=$(aws ec2 create-subnet --vpc-id $VPC_ID --cidr-block 10.0.2.0/24 --availability-zone us-east-1b --query 'Subnet.SubnetId' --output text)

# Create route table and associate with subnets
RTB_ID=$(aws ec2 create-route-table --vpc-id $VPC_ID --query 'RouteTable.RouteTableId' --output text)
aws ec2 create-route --route-table-id $RTB_ID --destination-cidr-block 0.0.0.0/0 --gateway-id $IGW_ID
aws ec2 associate-route-table --route-table-id $RTB_ID --subnet-id $SUBNET_1
aws ec2 associate-route-table --route-table-id $RTB_ID --subnet-id $SUBNET_2
```

### 3. Create Security Group

```bash
SG_ID=$(aws ec2 create-security-group \
    --group-name softwarecomp-sg \
    --description "Security group for SoftwareComp ECS tasks" \
    --vpc-id $VPC_ID \
    --query 'GroupId' \
    --output text)

# Allow inbound HTTP traffic
aws ec2 authorize-security-group-ingress \
    --group-id $SG_ID \
    --protocol tcp \
    --port 8080 \
    --cidr 0.0.0.0/0

# Allow inbound traffic from ALB (port 80)
aws ec2 authorize-security-group-ingress \
    --group-id $SG_ID \
    --protocol tcp \
    --port 80 \
    --cidr 0.0.0.0/0
```

### 4. Create IAM Roles

#### ECS Task Execution Role

This role allows ECS to pull images from ECR and write logs to CloudWatch.

```bash
# Create trust policy
cat > ecs-task-execution-trust-policy.json <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ecs-tasks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
EOF

# Create role
aws iam create-role \
    --role-name ecsTaskExecutionRole \
    --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach managed policy
aws iam attach-role-policy \
    --role-name ecsTaskExecutionRole \
    --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

#### ECS Task Role (Optional)

This role grants permissions to your application (e.g., access to S3, DynamoDB).

```bash
# Create task role
aws iam create-role \
    --role-name ecsTaskRole \
    --assume-role-policy-document file://ecs-task-execution-trust-policy.json

# Attach policies as needed (example: S3 read access)
aws iam attach-role-policy \
    --role-name ecsTaskRole \
    --policy-arn arn:aws:iam::aws:policy/AmazonS3ReadOnlyAccess
```

---

## ECS Fargate Setup

### 1. Create ECS Cluster

```bash
aws ecs create-cluster --cluster-name softwarecomp-cluster --region us-east-1
```

### 2. Create CloudWatch Log Group

```bash
aws logs create-log-group --log-group-name /ecs/softwarecomp --region us-east-1
```

---

## ECS Task Definition Explained

### Key Components

#### Launch Type Configuration

```json
"requiresCompatibilities": ["FARGATE"],
"networkMode": "awsvpc"
```

- **FARGATE**: Serverless compute engine for containers
- **awsvpc**: Each task gets its own ENI and private IP

#### CPU and Memory

```json
"cpu": "512",
"memory": "1024"
```

Valid Fargate combinations:
- CPU: 256 (.25 vCPU) → Memory: 512, 1024, 2048 MB
- CPU: 512 (.5 vCPU) → Memory: 1024, 2048, 3072, 4096 MB
- CPU: 1024 (1 vCPU) → Memory: 2048-8192 MB
- CPU: 2048 (2 vCPU) → Memory: 4096-16384 MB
- CPU: 4096 (4 vCPU) → Memory: 8192-30720 MB

#### Execution Role

```json
"executionRoleArn": "arn:aws:iam::{{ACCOUNT_ID}}:role/ecsTaskExecutionRole"
```

Required for ECS to:
- Pull container images from ECR
- Write logs to CloudWatch

#### Container Definition

```json
"containerDefinitions": [
  {
    "name": "softwarecomp",
    "image": "{{IMAGE_URI}}",
    "essential": true,
    "portMappings": [{"containerPort": 8080, "protocol": "tcp"}],
    "environment": [...],
    "logConfiguration": {...},
    "healthCheck": {...}
  }
]
```

- **essential: true**: Task stops if this container stops
- **portMappings**: Only containerPort needed (no hostPort for Fargate)
- **environment**: Application-specific environment variables
- **logConfiguration**: CloudWatch Logs integration
- **healthCheck**: Container-level health monitoring

---

## ECS Service Configuration

### Key Components

#### Launch Type

```json
"launchType": "FARGATE"
```

#### Network Configuration

```json
"networkConfiguration": {
  "awsvpcConfiguration": {
    "subnets": ["subnet-xxx", "subnet-yyy"],
    "securityGroups": ["sg-xxx"],
    "assignPublicIp": "ENABLED"
  }
}
```

- **subnets**: Use public subnets with assignPublicIp=ENABLED or private subnets with NAT Gateway
- **securityGroups**: Control inbound/outbound traffic
- **assignPublicIp**: ENABLED for public subnets without NAT

#### Load Balancer Integration

```json
"loadBalancers": [
  {
    "targetGroupArn": "arn:aws:elasticloadbalancing:...",
    "containerName": "softwarecomp",
    "containerPort": 8080
  }
],
"healthCheckGracePeriodSeconds": 300
```

- **targetGroupArn**: ALB target group (target-type must be `ip`)
- **healthCheckGracePeriodSeconds**: Time before health checks start (allows for app startup)

#### Deployment Configuration

```json
"deploymentConfiguration": {
  "maximumPercent": 200,
  "minimumHealthyPercent": 50,
  "deploymentCircuitBreaker": {
    "enable": true,
    "rollback": true
  }
}
```

- **maximumPercent**: 200 allows rolling deployment (2x desired count during deployment)
- **minimumHealthyPercent**: 50 ensures at least half of tasks remain running
- **deploymentCircuitBreaker**: Auto-rollback on failed deployments

---

## Deployment Walkthrough

### Step 1: Build and Push Docker Image

#### Using Linux/macOS Script

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Using Windows Script

```cmd
scripts\build-push.bat
```

**Script prompts:**
1. Select registry type (1=ECR, 2=Docker Hub)
2. Enter registry details (region, account ID, repository name)
3. Enter image tag (default: latest)

**Example ECR flow:**
```
Select Docker Registry:
1. AWS ECR (Elastic Container Registry)
2. Docker Hub
Enter your choice (1 or 2): 1

Enter AWS Region (e.g., us-east-1): us-east-1
Enter AWS Account ID: 123456789012
Enter ECR Repository Name [softwarecomp]: softwarecomp
Enter image tag [latest]: v1.0.0
```

The script will:
- Sanitize the image name and tag
- Authenticate to the selected registry
- Create ECR repository if it doesn't exist
- Build the Docker image
- Push to the registry

### Step 2: Deploy to ECS Fargate

#### Using Linux/macOS Script

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Using Windows Script

```cmd
scripts\deploy-image.bat
```

**Script prompts:**
1. AWS region
2. ECS cluster name
3. VPC ID
4. Subnet IDs (comma-separated)
5. Security Group ID
6. Docker image URI
7. Load balancer needed? (y/n)

**Example deployment flow:**
```
Enter AWS region (e.g., us-east-1): us-east-1
Enter ECS cluster name (e.g., my-ecs-cluster): softwarecomp-cluster
Enter VPC ID (e.g., vpc-0abc123def456): vpc-0abc123def456
Enter Subnet IDs comma-separated: subnet-0abc123,subnet-0def456
Enter Security Group ID (e.g., sg-0abc123def): sg-0abc123def
Enter Docker image URI: 123456789012.dkr.ecr.us-east-1.amazonaws.com/softwarecomp:v1.0.0
Do you need a load balancer for this service? (y/n): y
```

The script will:
- Check/create ECS cluster
- Create ALB and Target Group (if requested)
- Create CloudWatch log group
- Register task definition
- Create or update ECS service
- Wait for service stability
- Display deployment status

### Step 3: Verify Deployment

#### Check Service Status

```bash
aws ecs describe-services \
    --cluster softwarecomp-cluster \
    --services softwarecomp-service \
    --region us-east-1 \
    --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount}'
```

#### Check Running Tasks

```bash
aws ecs list-tasks \
    --cluster softwarecomp-cluster \
    --service-name softwarecomp-service \
    --region us-east-1
```

#### View CloudWatch Logs

```bash
aws logs tail /ecs/softwarecomp --follow --region us-east-1
```

#### Test Application (with Load Balancer)

```bash
# Get ALB DNS name
ALB_DNS=$(aws elbv2 describe-load-balancers \
    --names softwarecomp-alb \
    --query 'LoadBalancers[0].DNSName' \
    --output text)

# Test health endpoint
curl http://$ALB_DNS/health
```

---

## Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptoms:** Tasks immediately stop after starting

**Causes:**
- Invalid CPU/memory combination
- Missing IAM permissions
- Container image not found
- Application crashes on startup

**Solutions:**
```bash
# Check task stopped reason
aws ecs describe-tasks \
    --cluster softwarecomp-cluster \
    --tasks <task-id> \
    --query 'tasks[0].stoppedReason'

# Check container logs
aws logs tail /ecs/softwarecomp --since 5m
```

#### 2. Cannot Pull Image from ECR

**Symptoms:** `CannotPullContainerError`

**Causes:**
- Missing executionRoleArn
- Insufficient ECR permissions
- Image doesn't exist

**Solutions:**
```bash
# Verify image exists
aws ecr describe-images \
    --repository-name softwarecomp \
    --region us-east-1

# Check task execution role permissions
aws iam get-role --role-name ecsTaskExecutionRole
```

#### 3. Network Connectivity Issues

**Symptoms:** Tasks can't reach external services or internet

**Causes:**
- Security group rules blocking traffic
- No route to internet (private subnet without NAT)
- assignPublicIp disabled in private subnet

**Solutions:**
```bash
# Check security group rules
aws ec2 describe-security-groups --group-ids <sg-id>

# Verify subnet route table
aws ec2 describe-route-tables --filters "Name=association.subnet-id,Values=<subnet-id>"

# Option 1: Use public subnets with assignPublicIp=ENABLED
# Option 2: Use private subnets with NAT Gateway
```

#### 4. Health Check Failures

**Symptoms:** Tasks cycle between starting and stopping

**Causes:**
- Application not listening on configured port
- Health endpoint returns non-200 status
- Health check timeout too short

**Solutions:**
```bash
# Check application logs
aws logs tail /ecs/softwarecomp --follow

# Test health endpoint from within task
aws ecs execute-command \
    --cluster softwarecomp-cluster \
    --task <task-id> \
    --container softwarecomp \
    --interactive \
    --command "/bin/sh"

# Inside container:
wget -O- http://localhost:8080/health
```

#### 5. Service Won't Stabilize

**Symptoms:** Deployment stuck in progress

**Causes:**
- Health check failures
- Load balancer target health checks failing
- Insufficient capacity

**Solutions:**
```bash
# Check service events
aws ecs describe-services \
    --cluster softwarecomp-cluster \
    --services softwarecomp-service \
    --query 'services[0].events[0:10]'

# Check target group health
aws elbv2 describe-target-health \
    --target-group-arn <target-group-arn>
```

---

## Scaling and Management

### Manual Scaling

```bash
aws ecs update-service \
    --cluster softwarecomp-cluster \
    --service softwarecomp-service \
    --desired-count 4
```

### Auto Scaling

#### 1. Register Scalable Target

```bash
aws application-autoscaling register-scalable-target \
    --service-namespace ecs \
    --scalable-dimension ecs:service:DesiredCount \
    --resource-id service/softwarecomp-cluster/softwarecomp-service \
    --min-capacity 2 \
    --max-capacity 10
```

#### 2. Create Target Tracking Scaling Policy (CPU)

```bash
aws application-autoscaling put-scaling-policy \
    --service-namespace ecs \
    --scalable-dimension ecs:service:DesiredCount \
    --resource-id service/softwarecomp-cluster/softwarecomp-service \
    --policy-name cpu-scaling-policy \
    --policy-type TargetTrackingScaling \
    --target-tracking-scaling-policy-configuration file://cpu-scaling-policy.json
```

**cpu-scaling-policy.json:**
```json
{
  "TargetValue": 70.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
  },
  "ScaleInCooldown": 300,
  "ScaleOutCooldown": 60
}
```

#### 3. Create Target Tracking Scaling Policy (Memory)

```bash
aws application-autoscaling put-scaling-policy \
    --service-namespace ecs \
    --scalable-dimension ecs:service:DesiredCount \
    --resource-id service/softwarecomp-cluster/softwarecomp-service \
    --policy-name memory-scaling-policy \
    --policy-type TargetTrackingScaling \
    --target-tracking-scaling-policy-configuration file://memory-scaling-policy.json
```

**memory-scaling-policy.json:**
```json
{
  "TargetValue": 80.0,
  "PredefinedMetricSpecification": {
    "PredefinedMetricType": "ECSServiceAverageMemoryUtilization"
  },
  "ScaleInCooldown": 300,
  "ScaleOutCooldown": 60
}
```

### Blue/Green Deployments

For zero-downtime deployments with AWS CodeDeploy:

1. Create CodeDeploy application and deployment group
2. Configure AppSpec file
3. Integrate with CI/CD pipeline

Refer to [AWS CodeDeploy ECS Documentation](https://docs.aws.amazon.com/codedeploy/latest/userguide/deployment-steps-ecs.html).

---

## Security Considerations

### 1. Use Secrets Manager for Sensitive Data

```bash
# Create secret
aws secretsmanager create-secret \
    --name softwarecomp/db-password \
    --secret-string "your-secure-password"

# Reference in task definition
"secrets": [
  {
    "name": "DB_PASSWORD",
    "valueFrom": "arn:aws:secretsmanager:region:account-id:secret:softwarecomp/db-password"
  }
]
```

### 2. Enable Container Insights

```bash
aws ecs update-cluster-settings \
    --cluster softwarecomp-cluster \
    --settings name=containerInsights,value=enabled
```

### 3. Use Private Subnets

For production:
- Deploy tasks in private subnets
- Use NAT Gateway for outbound internet access
- Route traffic through ALB in public subnets

### 4. Implement Least Privilege IAM Policies

- Restrict task role to only required AWS services
- Use IAM policy conditions (e.g., IP restrictions)
- Regularly audit IAM permissions

### 5. Enable AWS WAF on ALB

Protect against common web exploits:
```bash
aws wafv2 create-web-acl \
    --name softwarecomp-waf \
    --scope REGIONAL \
    --default-action Allow={} \
    --rules file://waf-rules.json
```

---

## Monitoring and Observability

### CloudWatch Metrics

ECS publishes metrics to CloudWatch:
- CPUUtilization
- MemoryUtilization
- TargetResponseTime (ALB)
- RequestCount (ALB)

**Create CloudWatch Dashboard:**
```bash
aws cloudwatch put-dashboard \
    --dashboard-name softwarecomp-dashboard \
    --dashboard-body file://dashboard.json
```

### CloudWatch Alarms

**High CPU Alarm:**
```bash
aws cloudwatch put-metric-alarm \
    --alarm-name softwarecomp-high-cpu \
    --alarm-description "Alert when CPU exceeds 80%" \
    --metric-name CPUUtilization \
    --namespace AWS/ECS \
    --statistic Average \
    --period 300 \
    --threshold 80 \
    --comparison-operator GreaterThanThreshold \
    --evaluation-periods 2 \
    --dimensions Name=ServiceName,Value=softwarecomp-service Name=ClusterName,Value=softwarecomp-cluster
```

### Application Insights (Recommended)

For .NET applications, integrate Azure Application Insights or AWS X-Ray:

**Add to .csproj:**
```xml
<PackageReference Include="Amazon.XRay.Recorder.Handlers.AspNetCore" Version="2.13.0" />
```

**Configure in Program.cs:**
```csharp
using Amazon.XRay.Recorder.Handlers.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAWSXRayTracing();

var app = builder.Build();
app.UseXRay("SoftwareComp");
```

### Structured Logging

Use Serilog for structured logging:

**Add to .csproj:**
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
```

**Configure in Program.cs:**
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## Additional Resources

- [AWS ECS Best Practices](https://docs.aws.amazon.com/AmazonECS/latest/bestpracticesguide/intro.html)
- [AWS Fargate Documentation](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/AWS_Fargate.html)
- [.NET on AWS](https://aws.amazon.com/developer/language/net/)
- [ECS Task Networking](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-networking.html)

---

## Support

For issues or questions:
1. Check CloudWatch Logs: `/ecs/softwarecomp`
2. Review ECS service events
3. Consult AWS Support or documentation

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-07  
**Target Platform:** AWS ECS Fargate  
**Application:** SoftwareComp .NET 8
