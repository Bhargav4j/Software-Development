#!/bin/bash
set -e
set -o pipefail

echo "======================================"
echo "  AWS ECS Fargate Deployment Script"
echo "======================================"
echo ""

# Configuration
PROJECT_NAME="tourmanagement"
TASK_FAMILY="${PROJECT_NAME}-task"
SERVICE_NAME="${PROJECT_NAME}-service"

# Prompt for AWS configuration
echo "=== AWS Configuration ==="
read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
export AWS_DEFAULT_REGION="$AWS_REGION"

echo ""
read -p "Enter ECS Cluster Name (e.g., my-ecs-cluster): " CLUSTER_NAME

echo ""
echo "=== Network Configuration ==="
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_IDS
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP

# Parse subnet IDs
IFS=',' read -ra SUBNETS <<< "$SUBNET_IDS"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

echo ""
read -p "Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/tourmanagement:latest): " IMAGE_URI

echo ""
echo "Getting AWS Account ID..."
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "Account ID: $ACCOUNT_ID"

echo ""
echo "Checking if ECS cluster exists..."
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo "Cluster does not exist. Creating ECS cluster..."
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
    echo "ECS cluster created successfully"
}

echo ""
read -p "Do you need a load balancer for this service? (y/n): " USE_LB

if [[ "$USE_LB" =~ ^[Yy]$ ]]; then
    echo ""
    echo "=== Creating Application Load Balancer ==="
    
    LB_NAME="${PROJECT_NAME}-alb"
    TG_NAME="${PROJECT_NAME}-tg"
    
    echo "Creating Application Load Balancer..."
    LB_ARN=$(aws elbv2 create-load-balancer \
        --name "$LB_NAME" \
        --subnets "$SUBNET_1" "$SUBNET_2" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text 2>/dev/null || aws elbv2 describe-load-balancers --names "$LB_NAME" --query 'LoadBalancers[0].LoadBalancerArn' --output text)
    
    echo "Load Balancer ARN: $LB_ARN"
    
    echo "Creating Target Group..."
    TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
        --name "$TG_NAME" \
        --protocol HTTP \
        --port 8080 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-enabled \
        --health-check-protocol HTTP \
        --health-check-path "/health" \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text 2>/dev/null || aws elbv2 describe-target-groups --names "$TG_NAME" --query 'TargetGroups[0].TargetGroupArn' --output text)
    
    echo "Target Group ARN: $TARGET_GROUP_ARN"
    
    echo "Creating Listener..."
    aws elbv2 create-listener \
        --load-balancer-arn "$LB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" >/dev/null 2>&1 || echo "Listener already exists"
    
    LB_DNS=$(aws elbv2 describe-load-balancers --load-balancer-arns "$LB_ARN" --query 'LoadBalancers[0].DNSName' --output text)
    echo "Load Balancer DNS: $LB_DNS"
else
    echo "Skipping load balancer configuration"
    TARGET_GROUP_ARN=""
fi

echo ""
echo "=== Database Configuration ==="
read -p "Enter Database Host: " DB_HOST
read -p "Enter Database Port (default: 5432): " DB_PORT
DB_PORT=${DB_PORT:-5432}
read -p "Enter Database Name: " DB_NAME
read -p "Enter Database User: " DB_USER
read -sp "Enter Database Password: " DB_PASSWORD
echo ""

echo ""
echo "Preparing ECS task definition..."
cp ecs/task-definition.json ecs/task-definition-temp.json

# Replace placeholders
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" ecs/task-definition-temp.json
sed -i "s|{{AWS_REGION}}|$AWS_REGION|g" ecs/task-definition-temp.json
sed -i "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" ecs/task-definition-temp.json
sed -i "s|{{DB_HOST}}|$DB_HOST|g" ecs/task-definition-temp.json
sed -i "s|{{DB_PORT}}|$DB_PORT|g" ecs/task-definition-temp.json
sed -i "s|{{DB_NAME}}|$DB_NAME|g" ecs/task-definition-temp.json
sed -i "s|{{DB_USER}}|$DB_USER|g" ecs/task-definition-temp.json
sed -i "s|{{DB_PASSWORD}}|$DB_PASSWORD|g" ecs/task-definition-temp.json

echo "Creating CloudWatch log group..."
aws logs create-log-group --log-group-name "/ecs/$PROJECT_NAME" --region "$AWS_REGION" 2>/dev/null || echo "Log group already exists"

echo ""
echo "Registering ECS task definition..."
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://ecs/task-definition-temp.json \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

echo "Task Definition ARN: $TASK_DEF_ARN"

rm -f ecs/task-definition-temp.json

echo ""
echo "Preparing ECS service definition..."
cp ecs/service-definition.json ecs/service-definition-temp.json

sed -i "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" ecs/service-definition-temp.json
sed -i "s|{{SUBNET_1}}|$SUBNET_1|g" ecs/service-definition-temp.json
sed -i "s|{{SUBNET_2}}|$SUBNET_2|g" ecs/service-definition-temp.json
sed -i "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" ecs/service-definition-temp.json

if [[ "$USE_LB" =~ ^[Yy]$ ]]; then
    sed -i "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" ecs/service-definition-temp.json
else
    # Remove loadBalancers section if no LB
    python3 -c "import json; d=json.load(open('ecs/service-definition-temp.json')); d.pop('loadBalancers',None); d.pop('healthCheckGracePeriodSeconds',None); json.dump(d,open('ecs/service-definition-temp.json','w'),indent=2)"
fi

echo ""
echo "Checking if service exists..."
SERVICE_EXISTS=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[?status==`ACTIVE`].serviceName' \
    --output text)

if [ -z "$SERVICE_EXISTS" ] || [ "$SERVICE_EXISTS" == "None" ]; then
    echo "Service does not exist. Creating ECS service..."
    aws ecs create-service \
        --cli-input-json file://ecs/service-definition-temp.json \
        --region "$AWS_REGION"
else
    echo "Service exists. Updating ECS service..."
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --region "$AWS_REGION"
fi

rm -f ecs/service-definition-temp.json

echo ""
echo "Waiting for service to become stable..."
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

echo ""
echo "======================================"
echo "  Deployment Complete"
echo "======================================"
echo ""
echo "Cluster: $CLUSTER_NAME"
echo "Service: $SERVICE_NAME"
echo "Task Definition: $TASK_DEF_ARN"

if [[ "$USE_LB" =~ ^[Yy]$ ]]; then
    echo "Load Balancer DNS: $LB_DNS"
    echo "Application URL: http://$LB_DNS"
fi

echo "CloudWatch Logs: /ecs/$PROJECT_NAME"
echo ""
echo "Service Status:"
aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount}' \
    --output table

echo ""
echo "Deployment completed successfully!"
