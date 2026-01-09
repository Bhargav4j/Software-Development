# TourManagement Application - AWS ECS Fargate Deployment Guide

This guide provides comprehensive instructions for deploying the TourManagement ASP.NET Core 8.0 application to AWS ECS Fargate.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development Setup](#local-development-setup)
3. [Docker Deployment](#docker-deployment)
4. [AWS ECS Fargate Prerequisites](#aws-ecs-fargate-prerequisites)
5. [ECS Fargate Setup](#ecs-fargate-setup)
6. [ECS Task Definition Explained](#ecs-task-definition-explained)
7. [ECS Service Configuration](#ecs-service-configuration)
8. [ECS Fargate Deployment Walkthrough](#ecs-fargate-deployment-walkthrough)
9. [Configuration Management](#configuration-management)
10. [ECS-Specific Troubleshooting](#ecs-specific-troubleshooting)
11. [ECS Fargate Scaling and Management](#ecs-fargate-scaling-and-management)
12. [Security Considerations](#security-considerations)
13. [Monitoring and Logging](#monitoring-and-logging)

---

## Prerequisites

### Required Software

- **.NET 8.0 SDK** - For local development and building
- **Docker Desktop** - For containerization (version 20.10 or later)
- **AWS CLI** - For AWS ECS deployment (version 2.x)
- **Git** - For version control
- **PostgreSQL 12+** - Database (managed via AWS RDS recommended)

### Development Tools (Optional)

- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **AWS Toolkit for Visual Studio/VS Code**
- **Docker Extension for VS Code**

### AWS Account Requirements

- Active AWS account with appropriate permissions
- IAM user with ECS, ECR, EC2, VPC, and CloudWatch permissions
- AWS CLI configured with access credentials

---

## Local Development Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TourManagement
```

### 2. Configure Database Connection

Create an `appsettings.Development.json` file in `src/TourManagement.Web/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=tourmanagement;Username=postgres;Password=yourpassword;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=20;Connection Lifetime=0;Command Timeout=30;Timeout=30"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  },
  "AllowedHosts": "*"
}
```

### 3. Restore Dependencies

```bash
dotnet restore TourManagement.sln
```

### 4. Apply Database Migrations

```bash
cd src/TourManagement.Web
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run --project src/TourManagement.Web/TourManagement.Web.csproj
```

The application will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### 6. Verify Health Endpoint

```bash
curl http://localhost:5000/health
```

Expected response: `Healthy`

---

## Docker Deployment

### Building the Docker Image

#### Using Docker CLI

```bash
docker build -t tourmanagement:latest -f Dockerfile .
```

#### Using Docker Compose

```bash
docker-compose build
```

### Running the Container Locally

#### Using Docker CLI

```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e DB_HOST=host.docker.internal \
  -e DB_PORT=5432 \
  -e DB_NAME=tourmanagement \
  -e DB_USER=postgres \
  -e DB_PASSWORD=yourpassword \
  --name tourmanagement-app \
  tourmanagement:latest
```

#### Using Docker Compose

Create a `.env` file with your database configuration:

```env
DB_HOST=host.docker.internal
DB_PORT=5432
DB_NAME=tourmanagement
DB_USER=postgres
DB_PASSWORD=yourpassword
```

Run:

```bash
docker-compose up -d
```

### Verify Docker Container

```bash
# Check container status
docker ps

# View logs
docker logs tourmanagement-app

# Test health endpoint
curl http://localhost:8080/health
```

---

## AWS ECS Fargate Prerequisites

### 1. AWS CLI Configuration

Configure AWS CLI with your credentials:

```bash
aws configure
```

Provide:
- AWS Access Key ID
- AWS Secret Access Key
- Default region (e.g., `us-east-1`)
- Default output format (e.g., `json`)

### 2. Required IAM Roles

#### ECS Task Execution Role

Create a role named `ecsTaskExecutionRole` with the following managed policy:
- `AmazonECSTaskExecutionRolePolicy`

This role allows ECS to:
- Pull container images from ECR
- Write logs to CloudWatch

```bash
aws iam create-role --role-name ecsTaskExecutionRole \
  --assume-role-policy-document file://ecs-task-execution-trust-policy.json

aws iam attach-role-policy --role-name ecsTaskExecutionRole \
  --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
```

**Trust Policy (`ecs-task-execution-trust-policy.json`):**

```json
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
```

#### ECS Task Role (Optional)

Create a role named `ecsTaskRole` for application-level permissions (e.g., accessing S3, DynamoDB).

### 3. VPC and Networking Setup

#### Create or Identify VPC

Ensure you have:
- A VPC with at least 2 subnets in different availability zones
- An Internet Gateway attached to the VPC (for public IP assignment)
- Route tables configured for internet access

#### Create Security Group

Create a security group that allows:
- **Inbound**: Port 8080 (application) from your load balancer security group
- **Inbound**: Port 80/443 (load balancer) from 0.0.0.0/0
- **Outbound**: All traffic

```bash
aws ec2 create-security-group \
  --group-name tourmanagement-ecs-sg \
  --description "Security group for TourManagement ECS tasks" \
  --vpc-id vpc-xxxxxxxx

aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxxxxx \
  --protocol tcp \
  --port 8080 \
  --cidr 0.0.0.0/0
```

### 4. Amazon ECR Repository

Create an ECR repository to store your Docker images:

```bash
aws ecr create-repository --repository-name tourmanagement --region us-east-1
```

Note the repository URI (e.g., `123456789012.dkr.ecr.us-east-1.amazonaws.com/tourmanagement`).

### 5. PostgreSQL Database (AWS RDS)

Create an RDS PostgreSQL instance:

```bash
aws rds create-db-instance \
  --db-instance-identifier tourmanagement-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --engine-version 14.7 \
  --master-username postgres \
  --master-user-password YourSecurePassword123 \
  --allocated-storage 20 \
  --vpc-security-group-ids sg-xxxxxxxx \
  --db-subnet-group-name your-db-subnet-group \
  --publicly-accessible
```

Note the endpoint address for later configuration.

---

## ECS Fargate Setup

### 1. CloudWatch Log Group

Create a CloudWatch log group for application logs:

```bash
aws logs create-log-group --log-group-name /ecs/tourmanagement --region us-east-1
```

### 2. ECS Cluster

Create an ECS cluster:

```bash
aws ecs create-cluster --cluster-name tourmanagement-cluster --region us-east-1
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
- **awsvpc**: Each task gets its own elastic network interface (ENI)

#### CPU and Memory

```json
"cpu": "512",
"memory": "1024"
```

Valid Fargate CPU/Memory combinations:
- CPU: 256 (.25 vCPU) → Memory: 512, 1024, 2048 MB
- CPU: 512 (.5 vCPU) → Memory: 1024-4096 MB
- CPU: 1024 (1 vCPU) → Memory: 2048-8192 MB

#### Execution Role

```json
"executionRoleArn": "arn:aws:iam::{{ACCOUNT_ID}}:role/ecsTaskExecutionRole"
```

Allows ECS to pull images and write logs.

#### Container Definition

```json
"containerDefinitions": [
  {
    "name": "tourmanagement",
    "image": "{{IMAGE_URI}}",
    "essential": true,
    "portMappings": [{"containerPort": 8080, "protocol": "tcp"}],
    "environment": [...],
    "logConfiguration": {...}
  }
]
```

- **essential**: If this container fails, the entire task stops
- **portMappings**: Expose container port 8080
- **environment**: Application configuration variables

#### Logging Configuration

```json
"logConfiguration": {
  "logDriver": "awslogs",
  "options": {
    "awslogs-group": "/ecs/tourmanagement",
    "awslogs-region": "us-east-1",
    "awslogs-stream-prefix": "ecs"
  }
}
```

Sends container logs to CloudWatch.

---

## ECS Service Configuration

### Key Components

#### Service Definition

```json
{
  "serviceName": "tourmanagement-service",
  "cluster": "tourmanagement-cluster",
  "taskDefinition": "tourmanagement-task",
  "desiredCount": 2,
  "launchType": "FARGATE"
}
```

- **desiredCount**: Number of tasks to run (2 for high availability)
- **launchType**: FARGATE for serverless deployment

#### Network Configuration

```json
"networkConfiguration": {
  "awsvpcConfiguration": {
    "subnets": ["subnet-xxxxx", "subnet-yyyyy"],
    "securityGroups": ["sg-xxxxx"],
    "assignPublicIp": "ENABLED"
  }
}
```

- **subnets**: Deploy tasks across multiple AZs for availability
- **assignPublicIp**: Required if tasks need internet access without NAT gateway

#### Deployment Configuration

```json
"deploymentConfiguration": {
  "maximumPercent": 200,
  "minimumHealthyPercent": 50
}
```

- **maximumPercent**: Can have up to 2x desired count during deployment
- **minimumHealthyPercent**: Maintain at least 50% of tasks during deployment

#### Load Balancer Integration

```json
"loadBalancers": [
  {
    "targetGroupArn": "arn:aws:elasticloadbalancing:...",
    "containerName": "tourmanagement",
    "containerPort": 8080
  }
]
```

Integrates with Application Load Balancer for traffic distribution.

---

## ECS Fargate Deployment Walkthrough

### Step 1: Build and Push Docker Image

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
1. Select registry type (1 for AWS ECR, 2 for Docker Hub)
2. Provide AWS region and account ID (for ECR)
3. Enter repository name
4. Specify image tag (default: `latest`)

The script will:
- Authenticate with the selected registry
- Create ECR repository if it doesn't exist
- Build the Docker image
- Push to the registry

### Step 2: Deploy to ECS Fargate

#### Linux/macOS

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows

```cmd
scripts\deploy-image.bat
```

**Follow the prompts:**

1. **AWS Configuration:**
   - AWS Region (e.g., `us-east-1`)
   - ECS Cluster Name (e.g., `tourmanagement-cluster`)

2. **Network Configuration:**
   - VPC ID
   - Subnet IDs (comma-separated, at least 2)
   - Security Group ID

3. **Docker Image:**
   - Image URI from ECR (e.g., `123456789012.dkr.ecr.us-east-1.amazonaws.com/tourmanagement:latest`)

4. **Load Balancer:**
   - Choose whether to create a load balancer (y/n)
   - If yes, script will create ALB and target group automatically

5. **Database Configuration:**
   - Database Host (RDS endpoint)
   - Database Port (default: 5432)
   - Database Name
   - Database User
   - Database Password

The script will:
- Create/verify ECS cluster
- Create CloudWatch log group
- Create Application Load Balancer and Target Group (if selected)
- Register ECS task definition
- Create or update ECS service
- Wait for service to become stable
- Display deployment summary and access URLs

### Step 3: Verify Deployment

#### Check Service Status

```bash
aws ecs describe-services \
  --cluster tourmanagement-cluster \
  --services tourmanagement-service \
  --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount}'
```

#### View Running Tasks

```bash
aws ecs list-tasks --cluster tourmanagement-cluster --service-name tourmanagement-service
```

#### Check Task Logs

```bash
aws logs tail /ecs/tourmanagement --follow
```

#### Test Health Endpoint

If using load balancer:

```bash
curl http://<load-balancer-dns>/health
```

Expected response: `Healthy`

---

## Configuration Management

### Environment Variables

The application uses the following environment variables (configured in ECS task definition):

#### ASP.NET Core Configuration

- `ASPNETCORE_ENVIRONMENT`: Set to `Production`
- `ASPNETCORE_URLS`: Application binding URL (`http://+:8080`)
- `DOTNET_RUNNING_IN_CONTAINER`: Set to `true`
- `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT`: Set to `false`

#### Database Configuration

- `DB_HOST`: Database hostname (RDS endpoint)
- `DB_PORT`: Database port (default: 5432)
- `DB_NAME`: Database name
- `DB_USER`: Database username
- `DB_PASSWORD`: Database password (use AWS Secrets Manager for production)
- `DB_MIN_POOL_SIZE`: Minimum connection pool size (default: 5)
- `DB_MAX_POOL_SIZE`: Maximum connection pool size (default: 20)
- `DB_COMMAND_TIMEOUT`: Command timeout in seconds (default: 30)

#### Application Configuration

- `SESSION_TIMEOUT_MINUTES`: Session timeout duration (default: 30)
- `DISABLE_HTTPS_REDIRECT`: Disable HTTPS redirection (default: false)
- `MAX_RETRY_COUNT`: Database connection retry count (default: 5)
- `MAX_RETRY_DELAY_SECONDS`: Max retry delay (default: 30)
- `ALLOWED_HOSTS`: Allowed hosts (default: `*`)

### Using AWS Secrets Manager (Recommended)

For production, store sensitive data in AWS Secrets Manager:

1. **Create Secret:**

```bash
aws secretsmanager create-secret \
  --name tourmanagement/db-password \
  --secret-string "YourSecurePassword123"
```

2. **Update Task Definition:**

Replace environment variable with secrets reference:

```json
"secrets": [
  {
    "name": "DB_PASSWORD",
    "valueFrom": "arn:aws:secretsmanager:us-east-1:123456789012:secret:tourmanagement/db-password"
  }
]
```

3. **Update IAM Task Execution Role:**

Add permissions to read secrets:

```json
{
  "Effect": "Allow",
  "Action": [
    "secretsmanager:GetSecretValue"
  ],
  "Resource": "arn:aws:secretsmanager:us-east-1:123456789012:secret:tourmanagement/*"
}
```

---

## ECS-Specific Troubleshooting

### Common Issues

#### 1. Task Fails to Start

**Symptoms:**
- Service shows 0 running tasks
- Tasks transition to STOPPED state

**Diagnosis:**

```bash
aws ecs describe-tasks \
  --cluster tourmanagement-cluster \
  --tasks <task-arn> \
  --query 'tasks[0].stoppedReason'
```

**Common Causes:**

- **Invalid CPU/Memory combination**: Use valid Fargate combinations
- **Image pull errors**: Check ECR permissions and image URI
- **Missing execution role**: Verify `ecsTaskExecutionRole` exists
- **Network issues**: Check security group rules and subnet configuration

**Solutions:**

```bash
# Check ECS task logs
aws logs tail /ecs/tourmanagement --since 30m

# Verify ECR image exists
aws ecr describe-images --repository-name tourmanagement

# Check security group rules
aws ec2 describe-security-groups --group-ids sg-xxxxx
```

#### 2. Tasks Running but Unhealthy

**Symptoms:**
- Tasks start but load balancer marks them as unhealthy
- Service repeatedly starts new tasks

**Diagnosis:**

```bash
# Check target group health
aws elbv2 describe-target-health --target-group-arn <target-group-arn>
```

**Common Causes:**

- Health check path misconfigured
- Security group blocks traffic from load balancer
- Application not listening on correct port
- Health check timeout too short

**Solutions:**

```bash
# Verify health endpoint
curl http://<task-public-ip>:8080/health

# Check security group allows port 8080 from ALB
aws ec2 authorize-security-group-ingress \
  --group-id sg-xxxxx \
  --protocol tcp \
  --port 8080 \
  --source-group sg-alb-xxxxx

# Update health check settings
aws elbv2 modify-target-group \
  --target-group-arn <arn> \
  --health-check-interval-seconds 30 \
  --health-check-timeout-seconds 10
```

#### 3. Database Connection Failures

**Symptoms:**
- Application logs show database connection errors
- Health checks fail

**Diagnosis:**

```bash
# Check application logs
aws logs tail /ecs/tourmanagement --follow
```

**Common Causes:**

- Incorrect database credentials
- RDS security group doesn't allow ECS tasks
- Database endpoint incorrect

**Solutions:**

```bash
# Test database connectivity from ECS task
aws ecs execute-command \
  --cluster tourmanagement-cluster \
  --task <task-id> \
  --container tourmanagement \
  --interactive \
  --command "/bin/bash"

# Inside container, test connection
telnet <rds-endpoint> 5432

# Update RDS security group
aws ec2 authorize-security-group-ingress \
  --group-id sg-rds-xxxxx \
  --protocol tcp \
  --port 5432 \
  --source-group sg-ecs-xxxxx
```

#### 4. Out of Memory Errors

**Symptoms:**
- Tasks exit with code 137
- Logs show OutOfMemoryException

**Solutions:**

```bash
# Increase task memory in task definition
aws ecs register-task-definition \
  --family tourmanagement-task \
  --memory 2048 \
  --cpu 1024

# Update service
aws ecs update-service \
  --cluster tourmanagement-cluster \
  --service tourmanagement-service \
  --task-definition tourmanagement-task:2
```

#### 5. Service Not Accessible via Load Balancer

**Symptoms:**
- Load balancer returns 503 errors
- Targets show as unhealthy

**Solutions:**

```bash
# Check target group registration
aws elbv2 describe-target-health --target-group-arn <arn>

# Verify load balancer listener
aws elbv2 describe-listeners --load-balancer-arn <arn>

# Check load balancer security group
aws ec2 describe-security-groups --group-ids sg-alb-xxxxx

# Ensure ingress on port 80/443 from 0.0.0.0/0
```

---

## ECS Fargate Scaling and Management

### Service Auto Scaling

#### Setup Target Tracking Scaling

```bash
# Register scalable target
aws application-autoscaling register-scalable-target \
  --service-namespace ecs \
  --resource-id service/tourmanagement-cluster/tourmanagement-service \
  --scalable-dimension ecs:service:DesiredCount \
  --min-capacity 2 \
  --max-capacity 10

# Create scaling policy (CPU-based)
aws application-autoscaling put-scaling-policy \
  --service-namespace ecs \
  --resource-id service/tourmanagement-cluster/tourmanagement-service \
  --scalable-dimension ecs:service:DesiredCount \
  --policy-name cpu-scaling-policy \
  --policy-type TargetTrackingScaling \
  --target-tracking-scaling-policy-configuration file://scaling-policy.json
```

**scaling-policy.json:**

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

### Manual Scaling

```bash
# Scale up to 5 tasks
aws ecs update-service \
  --cluster tourmanagement-cluster \
  --service tourmanagement-service \
  --desired-count 5

# Scale down to 2 tasks
aws ecs update-service \
  --cluster tourmanagement-cluster \
  --service tourmanagement-service \
  --desired-count 2
```

### Blue/Green Deployments

1. **Create New Task Definition Version:**

```bash
aws ecs register-task-definition --cli-input-json file://task-definition-v2.json
```

2. **Update Service:**

```bash
aws ecs update-service \
  --cluster tourmanagement-cluster \
  --service tourmanagement-service \
  --task-definition tourmanagement-task:2
```

3. **Monitor Deployment:**

```bash
aws ecs describe-services \
  --cluster tourmanagement-cluster \
  --services tourmanagement-service \
  --query 'services[0].deployments'
```

### Rolling Back Deployments

```bash
# Revert to previous task definition
aws ecs update-service \
  --cluster tourmanagement-cluster \
  --service tourmanagement-service \
  --task-definition tourmanagement-task:1
```

---

## Security Considerations

### Network Security

- **Use Private Subnets**: Deploy tasks in private subnets with NAT Gateway
- **Security Groups**: Implement least privilege access
- **Network ACLs**: Add additional network-level security

### IAM Security

- **Task Execution Role**: Only permissions needed to run tasks
- **Task Role**: Only permissions needed by application
- **Least Privilege**: Grant minimum required permissions

### Secrets Management

- **AWS Secrets Manager**: Store all sensitive data
- **Rotate Secrets**: Enable automatic rotation
- **Audit Access**: Monitor secret access via CloudTrail

### Container Security

- **Non-root User**: Application runs as non-root user
- **Image Scanning**: Enable ECR image scanning
- **Minimal Base Image**: Use official Microsoft runtime images
- **Regular Updates**: Keep base images and dependencies updated

### Database Security

- **Encryption at Rest**: Enable RDS encryption
- **Encryption in Transit**: Use SSL/TLS connections
- **Private Access**: Place RDS in private subnets
- **IAM Authentication**: Consider using IAM database authentication

---

## Monitoring and Logging

### CloudWatch Logs

#### View Application Logs

```bash
# Tail logs in real-time
aws logs tail /ecs/tourmanagement --follow

# View logs from last hour
aws logs tail /ecs/tourmanagement --since 1h

# Filter logs by keyword
aws logs tail /ecs/tourmanagement --filter-pattern "ERROR"
```

#### Create Log Insights Queries

```sql
-- Top 10 slowest requests
fields @timestamp, @message
| filter @message like /ResponseTime/
| sort @message desc
| limit 10

-- Error rate by minute
fields @timestamp, @message
| filter @message like /ERROR/
| stats count() by bin(5m)
```

### CloudWatch Metrics

#### ECS Service Metrics

- **CPUUtilization**: Average CPU usage across tasks
- **MemoryUtilization**: Average memory usage
- **RunningTaskCount**: Number of tasks in RUNNING state

#### Application Load Balancer Metrics

- **TargetResponseTime**: Response time from targets
- **HealthyHostCount**: Number of healthy targets
- **UnHealthyHostCount**: Number of unhealthy targets
- **RequestCount**: Total requests
- **HTTPCode_Target_4XX_Count**: Client errors
- **HTTPCode_Target_5XX_Count**: Server errors

### CloudWatch Alarms

#### Create CPU Alarm

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
  --dimensions Name=ServiceName,Value=tourmanagement-service Name=ClusterName,Value=tourmanagement-cluster
```

#### Create Error Rate Alarm

```bash
aws cloudwatch put-metric-alarm \
  --alarm-name tourmanagement-high-errors \
  --alarm-description "Alert on high 5xx error rate" \
  --metric-name HTTPCode_Target_5XX_Count \
  --namespace AWS/ApplicationELB \
  --statistic Sum \
  --period 60 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 1 \
  --dimensions Name=LoadBalancer,Value=app/tourmanagement-alb/xxxxx
```

### X-Ray Tracing (Optional)

Enable AWS X-Ray for distributed tracing:

1. **Add X-Ray daemon sidecar to task definition**
2. **Instrument application with AWS X-Ray SDK**
3. **View traces in X-Ray console**

---

## Additional Resources

- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [AWS Fargate Documentation](https://docs.aws.amazon.com/fargate/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)

---

## Support

For issues or questions:
- Check CloudWatch logs: `/ecs/tourmanagement`
- Review ECS service events
- Consult AWS Support if needed

---

**Last Updated:** 2026-01-09
**Application Version:** 1.0.0
**Target Platform:** AWS ECS Fargate
