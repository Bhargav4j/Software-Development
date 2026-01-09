# TourManagement.Web - AWS ECS Fargate Deployment Guide

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Project Overview](#project-overview)
3. [Local Development Setup](#local-development-setup)
4. [Docker Deployment](#docker-deployment)
5. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
6. [ECS Fargate Setup](#ecs-fargate-setup)
7. [ECS Task Definition Explained](#ecs-task-definition-explained)
8. [ECS Service Configuration](#ecs-service-configuration)
9. [ECS Fargate Deployment Walkthrough](#ecs-fargate-deployment-walkthrough)
10. [ECS-Specific Troubleshooting](#ecs-specific-troubleshooting)
11. [ECS Fargate Scaling and Management](#ecs-fargate-scaling-and-management)
12. [Configuration Management](#configuration-management)
13. [Security Considerations](#security-considerations)
14. [Monitoring and Logging](#monitoring-and-logging)

---

## Prerequisites

### Required Software
- **.NET 8.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **AWS CLI v2** - [Installation Guide](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- **Git** - Version control

### AWS Account Requirements
- Active AWS account with appropriate permissions
- IAM user with ECS, ECR, EC2, and IAM permissions
- Configured AWS CLI with valid credentials

### Verify Prerequisites

```bash
# Check .NET version
dotnet --version
# Expected: 8.0.x

# Check Docker
docker --version
# Expected: Docker version 20.x or higher

# Check AWS CLI
aws --version
# Expected: aws-cli/2.x

# Verify AWS credentials
aws sts get-caller-identity
```

---

## Project Overview

**TourManagement.Web** is an ASP.NET Core 8.0 Razor Pages application with the following characteristics:

### Technology Stack
- **Framework**: ASP.NET Core 8.0
- **Project Type**: Web Application (Razor Pages)
- **Database**: PostgreSQL with Entity Framework Core
- **Caching**: Redis (optional)
- **Logging**: Serilog with Console and File sinks
- **Authentication**: ASP.NET Core Identity

### Key Dependencies
- Microsoft.EntityFrameworkCore.Design 8.0.0
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.0
- Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore 8.0.0
- Microsoft.Extensions.Caching.StackExchangeRedis 8.0.0
- Serilog.AspNetCore 8.0.0
- BCrypt.Net-Next 4.0.3

### Application Configuration
- **Port**: 8080 (HTTP)
- **Health Endpoints**: 
  - `/health` - Database connectivity check
  - `/ready` - Readiness probe
- **Environment**: Production (configurable)

---

## Local Development Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd <repository-directory>
```

### 2. Configure Database Connection

Update `src/TourManagement.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tourmanagementdb;Username=postgres;Password=postgres"
  }
}
```

### 3. Run Database Migrations

```bash
cd src/TourManagement.Web
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run --project src/TourManagement.Web/TourManagement.Web.csproj
```

The application will start on `http://localhost:5000` or `https://localhost:5001`.

### 5. Verify Application

Open browser and navigate to:
- Application: `http://localhost:5000`
- Health Check: `http://localhost:5000/health`

---

## Docker Deployment

### Build Docker Image

```bash
# From repository root
docker build -f Dockerfile -t tourmanagement-web:latest .
```

### Run with Docker Compose

```bash
# Set environment variables
export DATABASE_CONNECTION_STRING="Host=your-db-host;Port=5432;Database=tourmanagementdb;Username=postgres;Password=your-password"
export REDIS_CONNECTION_STRING="your-redis-host:6379"

# Start the application
docker-compose up -d

# View logs
docker-compose logs -f tourmanagement-web

# Stop the application
docker-compose down
```

### Access the Application

- Application: `http://localhost:8080`
- Health Check: `http://localhost:8080/health`

---

## AWS ECS Fargate Prerequisites

### 1. AWS Account Setup

Ensure your AWS account has the following:
- VPC with at least 2 subnets in different availability zones
- Security groups configured for ECS tasks
- Internet Gateway attached to VPC (for public IP assignment)

### 2. Required IAM Roles

#### ECS Task Execution Role

This role allows ECS to pull images from ECR and send logs to CloudWatch.

```bash
aws iam create-role --role-name ecsTaskExecutionRole --assume-role-policy-document '{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Principal": {"Service": "ecs-tasks.amazonaws.com"},
    "Action": "sts:AssumeRole"
  }]
}'

aws iam attach-role-policy --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

#### ECS Task Role (Optional)

For tasks that need to access other AWS services:

```bash
aws iam create-role --role-name ecsTaskRole --assume-role-policy-document '{
  "Version": "2012-10-17",
  "Statement": [{
    "Effect": "Allow",
    "Principal": {"Service": "ecs-tasks.amazonaws.com"},
    "Action": "sts:AssumeRole"
  }]
}'

# Attach policies as needed (e.g., S3, DynamoDB access)
```

### 3. Network Configuration

#### Create or Identify VPC

```bash
# List existing VPCs
aws ec2 describe-vpcs --query 'Vpcs[*].[VpcId,CidrBlock,Tags[?Key==`Name`].Value]' --output table

# Or create a new VPC
aws ec2 create-vpc --cidr-block 10.0.0.0/16
```

#### Create Subnets

```bash
# Create subnet 1
aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.1.0/24 --availability-zone us-east-1a

# Create subnet 2
aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.2.0/24 --availability-zone us-east-1b
```

#### Configure Security Group

```bash
# Create security group
aws ec2 create-security-group --group-name ecs-tourmanagement-sg \
  --description "Security group for TourManagement ECS tasks" \
  --vpc-id vpc-xxxxx

# Allow inbound HTTP traffic on port 8080
aws ec2 authorize-security-group-ingress --group-id sg-xxxxx \
  --protocol tcp --port 8080 --cidr 0.0.0.0/0

# Allow outbound traffic (required for database, Redis connections)
aws ec2 authorize-security-group-egress --group-id sg-xxxxx \
  --protocol -1 --cidr 0.0.0.0/0
```

### 4. CloudWatch Log Group

```bash
# Create log group for ECS tasks
aws logs create-log-group --log-group-name /ecs/tourmanagement-web

# Set retention period (optional)
aws logs put-retention-policy --log-group-name /ecs/tourmanagement-web --retention-in-days 7
```

---

## ECS Fargate Setup

### 1. Build and Push Docker Image

Use the provided build-push script:

#### Linux/macOS

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
```

#### Windows

```cmd
scripts\build-push.bat
```

**Follow the prompts:**
1. Select registry (AWS ECR recommended)
2. Enter AWS region
3. Enter AWS Account ID
4. Enter ECR repository name (default: tourmanagement-web)
5. Enter image tag (default: latest)

The script will:
- Authenticate with the selected registry
- Create ECR repository if it doesn't exist (for ECR)
- Build the Docker image
- Push the image to the registry

### 2. Verify Image in ECR

```bash
aws ecr describe-images --repository-name tourmanagement-web --region us-east-1
```

---

## ECS Task Definition Explained

### Fargate Requirements

The `ecs/task-definition.json` includes critical Fargate-specific configurations:

#### Launch Type and Network Mode

```json
{
  "requiresCompatibilities": ["FARGATE"],
  "networkMode": "awsvpc"
}
```

- **FARGATE**: Specifies serverless compute for containers
- **awsvpc**: Required network mode for Fargate (each task gets its own ENI)

#### CPU and Memory

**Valid Fargate CPU/Memory Combinations:**

| CPU (vCPU) | Memory (MB) |
|------------|-------------|
| 256 (.25)  | 512, 1024, 2048 |
| 512 (.5)   | 1024, 2048, 3072, 4096 |
| 1024 (1)   | 2048, 3072, 4096, 5120, 6144, 7168, 8192 |
| 2048 (2)   | 4096-16384 (increments of 1024) |
| 4096 (4)   | 8192-30720 (increments of 1024) |

**Default configuration:**

```json
{
  "cpu": "512",
  "memory": "1024"
}
```

#### Execution Role

```json
{
  "executionRoleArn": "arn:aws:iam::{{ACCOUNT_ID}}:role/ecsTaskExecutionRole"
}
```

Allows ECS to:
- Pull images from ECR
- Send logs to CloudWatch
- Retrieve secrets from Secrets Manager

#### Container Definition

```json
{
  "containerDefinitions": [
    {
      "name": "tourmanagement-web",
      "image": "{{IMAGE_URI}}",
      "essential": true,
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [...],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/tourmanagement-web",
          "awslogs-region": "{{AWS_REGION}}",
          "awslogs-stream-prefix": "ecs",
          "awslogs-create-group": "true"
        }
      }
    }
  ]
}
```

**Note**: No `hostPort` is specified for Fargate (not needed with awsvpc mode).

---

## ECS Service Configuration

### Service Definition Overview

The `ecs/service-definition.json` configures how ECS manages your tasks:

#### Network Configuration

```json
{
  "networkConfiguration": {
    "awsvpcConfiguration": {
      "subnets": ["{{SUBNET_1}}", "{{SUBNET_2}}"],
      "securityGroups": ["{{SECURITY_GROUP}}"],
      "assignPublicIp": "ENABLED"
    }
  }
}
```

- **subnets**: At least 2 subnets in different AZs for high availability
- **securityGroups**: Controls inbound/outbound traffic
- **assignPublicIp**: `ENABLED` for tasks in public subnets

#### Load Balancer Integration

```json
{
  "loadBalancers": [
    {
      "targetGroupArn": "{{TARGET_GROUP_ARN}}",
      "containerName": "tourmanagement-web",
      "containerPort": 8080
    }
  ],
  "healthCheckGracePeriodSeconds": 300
}
```

- **targetGroupArn**: Application Load Balancer target group
- **healthCheckGracePeriodSeconds**: Time before health checks start (allows for startup)

#### Deployment Configuration

```json
{
  "deploymentConfiguration": {
    "maximumPercent": 200,
    "minimumHealthyPercent": 50,
    "deploymentCircuitBreaker": {
      "enable": true,
      "rollback": true
    }
  }
}
```

- **maximumPercent**: Max tasks during deployment (200% = rolling update)
- **minimumHealthyPercent**: Min healthy tasks during deployment
- **deploymentCircuitBreaker**: Auto-rollback on failed deployments

#### Service Tags

**CRITICAL**: Use `tags` parameter (NOT `serviceTags`):

```json
{
  "tags": [
    {"key": "Environment", "value": "production"},
    {"key": "Application", "value": "tourmanagement-web"},
    {"key": "ManagedBy", "value": "ECS"}
  ]
}
```

---

## ECS Fargate Deployment Walkthrough

### Step-by-Step Deployment

#### Step 1: Prepare Configuration

1. **Gather Required Information:**
   - AWS Region (e.g., `us-east-1`)
   - ECS Cluster Name (e.g., `tourmanagement-cluster`)
   - VPC ID (e.g., `vpc-0abc123def456`)
   - Subnet IDs (e.g., `subnet-0abc123,subnet-0def456`)
   - Security Group ID (e.g., `sg-0abc123def`)
   - Docker Image URI (from ECR after build-push)
   - Database Connection String
   - Redis Connection String (optional)

2. **Verify Network Configuration:**

```bash
# Check subnets
aws ec2 describe-subnets --subnet-ids subnet-0abc123 subnet-0def456

# Check security group
aws ec2 describe-security-groups --group-ids sg-0abc123def
```

#### Step 2: Run Deployment Script

##### Linux/macOS

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

##### Windows

```cmd
scripts\deploy-image.bat
```

#### Step 3: Follow Deployment Prompts

The script will prompt for:

1. **AWS Region**: Enter your target region
2. **ECS Cluster Name**: Cluster will be created if it doesn't exist
3. **VPC ID**: Your VPC identifier
4. **Subnet IDs**: Comma-separated subnet IDs
5. **Security Group ID**: Security group for tasks
6. **Docker Image URI**: Full ECR image URI with tag
7. **Database Connection String**: PostgreSQL connection details
8. **Redis Connection String**: Optional Redis endpoint
9. **Load Balancer**: Whether to create an Application Load Balancer

#### Step 4: Monitor Deployment

The script will:
1. Get AWS Account ID
2. Check/create ECS cluster
3. Create Application Load Balancer and Target Group (if requested)
4. Register task definition with your configuration
5. Create or update ECS service
6. Wait for service to stabilize
7. Display deployment results

#### Step 5: Verify Deployment

```bash
# Check service status
aws ecs describe-services --cluster tourmanagement-cluster \
  --services tourmanagement-web-service --region us-east-1

# List running tasks
aws ecs list-tasks --cluster tourmanagement-cluster \
  --service-name tourmanagement-web-service --region us-east-1

# View task details
aws ecs describe-tasks --cluster tourmanagement-cluster \
  --tasks <task-id> --region us-east-1
```

#### Step 6: Access the Application

If load balancer was created:

```bash
# Get load balancer DNS
aws elbv2 describe-load-balancers --names tourmanagement-alb \
  --query 'LoadBalancers[0].DNSName' --output text
```

Access application at: `http://<alb-dns-name>`

Health check: `http://<alb-dns-name>/health`

---

## ECS-Specific Troubleshooting

### Task Failures

#### Task Fails to Start

**Symptoms**: Tasks enter STOPPED state immediately

**Solutions**:

1. **Check Task Logs:**

```bash
aws logs tail /ecs/tourmanagement-web --follow
```

2. **Verify Task Definition:**

```bash
aws ecs describe-task-definition --task-definition tourmanagement-web-task
```

3. **Check Stopped Task Reason:**

```bash
aws ecs describe-tasks --cluster tourmanagement-cluster \
  --tasks <task-id> --query 'tasks[0].stoppedReason'
```

#### Common Task Stop Reasons

| Reason | Solution |
|--------|----------|
| Essential container in task exited | Check application logs for errors |
| Task failed ELB health checks | Verify health endpoint and grace period |
| OutOfMemory | Increase memory in task definition |
| CannotPullContainerError | Verify ECR permissions and image URI |

### Network Issues

#### Task Cannot Connect to Database

**Check**:
1. Security group allows outbound traffic
2. Database security group allows inbound from ECS security group
3. Connection string is correct
4. Database is accessible from VPC

```bash
# Test from ECS task
aws ecs execute-command --cluster tourmanagement-cluster \
  --task <task-id> --container tourmanagement-web \
  --interactive --command "/bin/sh"

# Inside container
telnet your-db-host 5432
```

#### Task Cannot Access Internet

**Check**:
1. Subnet has route to Internet Gateway (for public subnets)
2. Or use NAT Gateway (for private subnets)
3. assignPublicIp is set correctly

```bash
# Check route table
aws ec2 describe-route-tables --filters "Name=association.subnet-id,Values=<subnet-id>"
```

### CPU/Memory Errors

#### Invalid CPU or Memory Value

**Error**: `ClientException: Invalid CPU or memory value specified`

**Solution**: Use valid Fargate CPU/memory combinations (see table above)

**Example Fix**:

```json
{
  "cpu": "512",
  "memory": "1024"
}
```

### Health Check Failures

#### Service Fails Health Checks

**Symptoms**: Tasks start but are marked unhealthy and replaced

**Solutions**:

1. **Verify Health Endpoint:**

```bash
curl http://<task-public-ip>:8080/health
```

2. **Increase Health Check Grace Period:**

```bash
aws ecs update-service --cluster tourmanagement-cluster \
  --service tourmanagement-web-service \
  --health-check-grace-period-seconds 300
```

3. **Check Application Startup Time:**
   - .NET applications may take 30-60 seconds to start
   - Increase grace period if needed

### Deployment Failures

#### Service Update Fails

**Symptoms**: New tasks start but are immediately stopped

**Solutions**:

1. **Check Deployment Events:**

```bash
aws ecs describe-services --cluster tourmanagement-cluster \
  --services tourmanagement-web-service \
  --query 'services[0].events[0:10]'
```

2. **Enable Deployment Circuit Breaker:**

Already enabled in service definition for auto-rollback

3. **Manual Rollback:**

```bash
aws ecs update-service --cluster tourmanagement-cluster \
  --service tourmanagement-web-service \
  --task-definition tourmanagement-web-task:<previous-revision>
```

---

## ECS Fargate Scaling and Management

### Manual Scaling

#### Update Desired Task Count

```bash
aws ecs update-service --cluster tourmanagement-cluster \
  --service tourmanagement-web-service \
  --desired-count 4
```

### Auto Scaling

#### Configure Service Auto Scaling

1. **Register Scalable Target:**

```bash
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/tourmanagement-cluster/tourmanagement-web-service \
  --min-capacity 2 \
  --max-capacity 10
```

2. **Create Target Tracking Scaling Policy (CPU):**

```bash
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/tourmanagement-cluster/tourmanagement-web-service \
  --policy-name cpu-target-tracking \
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

3. **Create Target Tracking Scaling Policy (Memory):**

```bash
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --scalable-dimension ecs:service:DesiredCount \
  --resource-id service/tourmanagement-cluster/tourmanagement-web-service \
  --policy-name memory-target-tracking \
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

#### Using AWS CodeDeploy

1. **Create CodeDeploy Application:**

```bash
aws deploy create-application \
  --application-name tourmanagement-ecs-app \
  --compute-platform ECS
```

2. **Create Deployment Group:**

```bash
aws deploy create-deployment-group \
  --application-name tourmanagement-ecs-app \
  --deployment-group-name tourmanagement-dg \
  --deployment-config-name CodeDeployDefault.ECSAllAtOnce \
  --service-role-arn arn:aws:iam::<account-id>:role/CodeDeployServiceRole \
  --ecs-services clusterName=tourmanagement-cluster,serviceName=tourmanagement-web-service \
  --load-balancer-info targetGroupPairInfoList=[{targetGroups=[{name=tourmanagement-tg-blue},{name=tourmanagement-tg-green}],prodTrafficRoute={listenerArns=[arn:aws:elasticloadbalancing:...]}}]
```

3. **Trigger Deployment:**

```bash
aws deploy create-deployment \
  --application-name tourmanagement-ecs-app \
  --deployment-group-name tourmanagement-dg \
  --revision revisionType=AppSpecContent,appSpecContent={content='{"version":0.0,"Resources":[{"TargetService":{"Type":"AWS::ECS::Service","Properties":{"TaskDefinition":"<new-task-def-arn>","LoadBalancerInfo":{"ContainerName":"tourmanagement-web","ContainerPort":8080}}}}]}'}
```

### Service Updates

#### Update Task Definition

```bash
# Register new task definition revision
aws ecs register-task-definition --cli-input-json file://ecs/task-definition.json

# Update service with new revision
aws ecs update-service --cluster tourmanagement-cluster \
  --service tourmanagement-web-service \
  --task-definition tourmanagement-web-task:<new-revision>
```

#### Update Environment Variables

```bash
# Modify task definition environment variables
# Then register new revision and update service
aws ecs update-service --cluster tourmanagement-cluster \
  --service tourmanagement-web-service \
  --force-new-deployment
```

---

## Configuration Management

### Environment Variables

#### In Task Definition

Environment variables are defined in `ecs/task-definition.json`:

```json
{
  "environment": [
    {"name": "ASPNETCORE_ENVIRONMENT", "value": "Production"},
    {"name": "ASPNETCORE_URLS", "value": "http://+:8080"},
    {"name": "ConnectionStrings__DefaultConnection", "value": "..."}
  ]
}
```

#### Using AWS Secrets Manager (Recommended)

For sensitive data:

1. **Store Secret:**

```bash
aws secretsmanager create-secret \
  --name tourmanagement/db-connection \
  --secret-string '{"ConnectionString":"Host=...;Password=secret"}'
```

2. **Reference in Task Definition:**

```json
{
  "secrets": [
    {
      "name": "ConnectionStrings__DefaultConnection",
      "valueFrom": "arn:aws:secretsmanager:us-east-1:<account-id>:secret:tourmanagement/db-connection"
    }
  ]
}
```

3. **Update Execution Role:**

```bash
aws iam put-role-policy --role-name ecsTaskExecutionRole \
  --policy-name SecretsManagerAccess \
  --policy-document '{
    "Version": "2012-10-17",
    "Statement": [{
      "Effect": "Allow",
      "Action": ["secretsmanager:GetSecretValue"],
      "Resource": "arn:aws:secretsmanager:*:*:secret:tourmanagement/*"
    }]
  }'
```

### Application Settings

#### appsettings.json Override

For .NET applications, environment variables override `appsettings.json` using the double underscore (`__`) syntax:

- `ConnectionStrings__DefaultConnection` → `ConnectionStrings:DefaultConnection`
- `Serilog__MinimumLevel__Default` → `Serilog:MinimumLevel:Default`

#### Configuration Providers

ASP.NET Core loads configuration in this order:
1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables
4. Command-line arguments

---

## Security Considerations

### Container Security

#### Non-Root User

The Dockerfile creates a non-root user:

```dockerfile
RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser
```

#### Image Scanning

Enable ECR image scanning:

```bash
aws ecr put-image-scanning-configuration \
  --repository-name tourmanagement-web \
  --image-scanning-configuration scanOnPush=true
```

### Network Security

#### Security Groups

**Best Practices**:
- Only allow necessary inbound ports (8080 for ALB → tasks)
- Use security group references instead of CIDR blocks
- Separate security groups for ALB, ECS tasks, and database

**Example:**

```bash
# Allow ALB to communicate with ECS tasks
aws ec2 authorize-security-group-ingress \
  --group-id sg-ecs-tasks \
  --protocol tcp --port 8080 \
  --source-group sg-alb

# Allow ECS tasks to communicate with database
aws ec2 authorize-security-group-ingress \
  --group-id sg-database \
  --protocol tcp --port 5432 \
  --source-group sg-ecs-tasks
```

### IAM Policies

#### Principle of Least Privilege

Grant only necessary permissions:

**Task Execution Role**: ECR pull, CloudWatch logs

**Task Role**: Application-specific AWS service access

### Secrets Management

**Never** hardcode sensitive data:
- Use AWS Secrets Manager or Systems Manager Parameter Store
- Reference secrets in task definition
- Rotate credentials regularly

---

## Monitoring and Logging

### CloudWatch Logs

#### View Logs

```bash
# Tail logs
aws logs tail /ecs/tourmanagement-web --follow

# Filter logs
aws logs filter-log-events --log-group-name /ecs/tourmanagement-web \
  --filter-pattern "ERROR"

# Get logs for specific time range
aws logs filter-log-events --log-group-name /ecs/tourmanagement-web \
  --start-time $(date -d '1 hour ago' +%s)000 \
  --end-time $(date +%s)000
```

#### CloudWatch Insights

Query logs with CloudWatch Insights:

```
fields @timestamp, @message
| filter @message like /ERROR/
| sort @timestamp desc
| limit 100
```

### Metrics and Alarms

#### Key ECS Metrics

- `CPUUtilization`
- `MemoryUtilization`
- `DesiredTaskCount`
- `RunningTaskCount`
- `PendingTaskCount`

#### Create CloudWatch Alarm

```bash
aws cloudwatch put-metric-alarm \
  --alarm-name tourmanagement-high-cpu \
  --alarm-description "Alert when CPU exceeds 80%" \
  --metric-name CPUUtilization \
  --namespace AWS/ECS \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --dimensions Name=ServiceName,Value=tourmanagement-web-service Name=ClusterName,Value=tourmanagement-cluster
```

### Application Performance Monitoring

#### Integrate with Application Insights (Optional)

For .NET applications, add Application Insights:

1. **Add NuGet Package:**

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

2. **Configure in Program.cs:**

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

3. **Set Instrumentation Key:**

```json
{
  "environment": [
    {"name": "APPLICATIONINSIGHTS_CONNECTION_STRING", "value": "InstrumentationKey=..."}
  ]
}
```

---

## Additional Resources

### AWS Documentation
- [Amazon ECS Developer Guide](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/)
- [AWS Fargate User Guide](https://docs.aws.amazon.com/AmazonECS/latest/userguide/)
- [Amazon ECR User Guide](https://docs.aws.amazon.com/AmazonECR/latest/userguide/)

### .NET on AWS
- [.NET on AWS](https://aws.amazon.com/developer/language/net/)
- [Deploying ASP.NET Core to ECS](https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/deployment-ecs.html)

### Best Practices
- [ECS Best Practices Guide](https://docs.aws.amazon.com/AmazonECS/latest/bestpracticesguide/intro.html)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/aspnet/core/performance/performance-best-practices)

---

## Support and Troubleshooting

For issues or questions:

1. Check CloudWatch logs for application errors
2. Review ECS service events for deployment issues
3. Verify network configuration and security groups
4. Consult AWS documentation for platform-specific issues
5. Review .NET application logs for runtime errors

---

**Last Updated**: 2026-01-09
**Version**: 1.0.0