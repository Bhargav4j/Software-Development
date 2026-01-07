#!/bin/bash

set -e
set -o pipefail

echo "========================================"
echo "AWS ECS Fargate Deployment Script"
echo "========================================"
echo ""

# Project configuration
PROJECT_NAME="softwarecomp"
TASK_FAMILY="${PROJECT_NAME}-task"
SERVICE_NAME="${PROJECT_NAME}-service"

# Prompt for AWS configuration
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS cluster name (e.g., my-ecs-cluster): " CLUSTER_NAME
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_IDS
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP
read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/softwarecomp:latest): " IMAGE_URI

# Split subnets into array
IFS=',' read -ra SUBNETS <<< "$SUBNET_IDS"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

echo ""
echo "Getting AWS Account ID..."
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "Account ID: $ACCOUNT_ID"

echo ""
echo "Checking ECS cluster..."
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo "Cluster does not exist. Creating cluster: $CLUSTER_NAME"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}

# Handle load balancer configuration
echo ""
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

LOAD_BALANCER_ARN=""
TARGET_GROUP_ARN=""

if [[ "$NEED_LB" =~ ^[Yy]$ ]]; then
    echo ""
    echo "Creating Application Load Balancer..."
    
    # Create ALB
    ALB_NAME="${PROJECT_NAME}-alb"
    ALB_OUTPUT=$(aws elbv2 create-load-balancer \
        --name "$ALB_NAME" \
        --subnets "$SUBNET_1" "$SUBNET_2" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --output json)
    
    LOAD_BALANCER_ARN=$(echo "$ALB_OUTPUT" | jq -r '.LoadBalancers[0].LoadBalancerArn')
    LOAD_BALANCER_DNS=$(echo "$ALB_OUTPUT" | jq -r '.LoadBalancers[0].DNSName')
    
    echo "Load Balancer created: $LOAD_BALANCER_ARN"
    echo "Load Balancer DNS: $LOAD_BALANCER_DNS"
    
    # Create Target Group with ip target type (required for Fargate awsvpc)
    echo "Creating Target Group..."
    TG_NAME="${PROJECT_NAME}-tg"
    TG_OUTPUT=$(aws elbv2 create-target-group \
        --name "$TG_NAME" \
        --protocol HTTP \
        --port 8080 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-enabled \
        --health-check-path "/health" \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --output json)
    
    TARGET_GROUP_ARN=$(echo "$TG_OUTPUT" | jq -r '.TargetGroups[0].TargetGroupArn')
    echo "Target Group created: $TARGET_GROUP_ARN"
    
    # Create listener
    echo "Creating Load Balancer Listener..."
    aws elbv2 create-listener \
        --load-balancer-arn "$LOAD_BALANCER_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" >/dev/null
    
    echo "Listener created successfully"
else
    echo "Skipping load balancer creation."
    # Remove loadBalancers section from service definition
    jq 'del(.loadBalancers, .healthCheckGracePeriodSeconds)' ecs/service-definition.json > ecs/service-definition.tmp.json
    mv ecs/service-definition.tmp.json ecs/service-definition.json
fi

echo ""
echo "Creating CloudWatch log group..."
aws logs create-log-group --log-group-name "/ecs/${PROJECT_NAME}" --region "$AWS_REGION" 2>/dev/null || echo "Log group already exists"

echo ""
echo "Updating task definition with configuration..."

# Replace placeholders in task definition
sed -i "s|{{IMAGE_URI}}|${IMAGE_URI}|g" ecs/task-definition.json
sed -i "s|{{AWS_REGION}}|${AWS_REGION}|g" ecs/task-definition.json
sed -i "s|{{ACCOUNT_ID}}|${ACCOUNT_ID}|g" ecs/task-definition.json

echo "Registering ECS task definition..."
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://ecs/task-definition.json \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

echo "Task definition registered: $TASK_DEF_ARN"

echo ""
echo "Updating service definition with configuration..."

# Replace placeholders in service definition
sed -i "s|{{CLUSTER_NAME}}|${CLUSTER_NAME}|g" ecs/service-definition.json
sed -i "s|{{SUBNET_1}}|${SUBNET_1}|g" ecs/service-definition.json
sed -i "s|{{SUBNET_2}}|${SUBNET_2}|g" ecs/service-definition.json
sed -i "s|{{SECURITY_GROUP}}|${SECURITY_GROUP}|g" ecs/service-definition.json

if [ -n "$TARGET_GROUP_ARN" ]; then
    sed -i "s|{{TARGET_GROUP_ARN}}|${TARGET_GROUP_ARN}|g" ecs/service-definition.json
fi

# Check if service exists
echo ""
echo "Checking if service exists..."
SERVICE_EXISTS=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].serviceName' \
    --output text 2>/dev/null)

if [ "$SERVICE_EXISTS" != "None" ] && [ "$SERVICE_EXISTS" != "" ] && [ "$SERVICE_EXISTS" == "$SERVICE_NAME" ]; then
    echo "Service exists. Updating service..."
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --force-new-deployment \
        --region "$AWS_REGION" >/dev/null
    echo "Service updated successfully"
else
    echo "Service does not exist. Creating new service..."
    aws ecs create-service \
        --cli-input-json file://ecs/service-definition.json \
        --region "$AWS_REGION" >/dev/null
    echo "Service created successfully"
fi

echo ""
echo "Waiting for service to stabilize..."
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

echo ""
echo "========================================"
echo "DEPLOYMENT SUCCESSFUL!"
echo "========================================"
echo "Cluster: $CLUSTER_NAME"
echo "Service: $SERVICE_NAME"
echo "Task Definition: $TASK_DEF_ARN"
echo "Region: $AWS_REGION"

if [ -n "$LOAD_BALANCER_DNS" ]; then
    echo "Load Balancer DNS: http://$LOAD_BALANCER_DNS"
fi

echo "CloudWatch Logs: /ecs/${PROJECT_NAME}"
echo ""
echo "Service Details:"
aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].{Status:status,Running:runningCount,Desired:desiredCount,Pending:pendingCount}' \
    --output table

echo ""
echo "Deployment complete!"
echo ""