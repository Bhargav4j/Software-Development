#!/bin/bash
set -e
set -o pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== AWS ECS Fargate Deployment Script ===${NC}"
echo ""

# Prompt for deployment configuration
read -p "Enter AWS region (e.g., us-east-1): " AWS_REGION
read -p "Enter ECS cluster name (e.g., tourmanagement-cluster): " CLUSTER_NAME
read -p "Enter VPC ID (e.g., vpc-0abc123def456): " VPC_ID
read -p "Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): " SUBNET_IDS
read -p "Enter Security Group ID (e.g., sg-0abc123def): " SECURITY_GROUP
read -p "Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/tourmanagement-web:latest): " IMAGE_URI
read -p "Enter Database Connection String: " DATABASE_CONNECTION_STRING
read -p "Enter Redis Connection String (optional, press Enter to skip): " REDIS_CONNECTION_STRING

# Split subnet IDs
IFS=',' read -ra SUBNETS <<< "$SUBNET_IDS"
SUBNET_1="${SUBNETS[0]}"
SUBNET_2="${SUBNETS[1]:-$SUBNET_1}"

echo ""
echo -e "${YELLOW}Getting AWS Account ID...${NC}"
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo -e "${GREEN}Account ID: $ACCOUNT_ID${NC}"

echo ""
echo -e "${YELLOW}Checking if ECS cluster exists...${NC}"
aws ecs describe-clusters --clusters "$CLUSTER_NAME" --region "$AWS_REGION" >/dev/null 2>&1 || {
    echo -e "${YELLOW}Creating ECS cluster: $CLUSTER_NAME${NC}"
    aws ecs create-cluster --cluster-name "$CLUSTER_NAME" --region "$AWS_REGION"
}

echo ""
read -p "Do you need a load balancer for this service? (y/n): " NEED_LB

if [[ "$NEED_LB" =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}Creating Application Load Balancer and Target Group...${NC}"
    
    # Create ALB
    ALB_ARN=$(aws elbv2 create-load-balancer \
        --name tourmanagement-alb \
        --subnets "$SUBNET_1" "$SUBNET_2" \
        --security-groups "$SECURITY_GROUP" \
        --scheme internet-facing \
        --type application \
        --ip-address-type ipv4 \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text 2>/dev/null || aws elbv2 describe-load-balancers \
        --names tourmanagement-alb \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].LoadBalancerArn' \
        --output text)
    
    echo -e "${GREEN}ALB ARN: $ALB_ARN${NC}"
    
    # Create Target Group with target-type ip (required for Fargate)
    TARGET_GROUP_ARN=$(aws elbv2 create-target-group \
        --name tourmanagement-tg \
        --protocol HTTP \
        --port 8080 \
        --vpc-id "$VPC_ID" \
        --target-type ip \
        --health-check-enabled \
        --health-check-protocol HTTP \
        --health-check-path /health \
        --health-check-interval-seconds 30 \
        --health-check-timeout-seconds 5 \
        --healthy-threshold-count 2 \
        --unhealthy-threshold-count 3 \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text 2>/dev/null || aws elbv2 describe-target-groups \
        --names tourmanagement-tg \
        --region "$AWS_REGION" \
        --query 'TargetGroups[0].TargetGroupArn' \
        --output text)
    
    echo -e "${GREEN}Target Group ARN: $TARGET_GROUP_ARN${NC}"
    
    # Create listener
    aws elbv2 create-listener \
        --load-balancer-arn "$ALB_ARN" \
        --protocol HTTP \
        --port 80 \
        --default-actions Type=forward,TargetGroupArn="$TARGET_GROUP_ARN" \
        --region "$AWS_REGION" >/dev/null 2>&1 || echo "Listener already exists"
    
    # Get ALB DNS name
    ALB_DNS=$(aws elbv2 describe-load-balancers \
        --load-balancer-arns "$ALB_ARN" \
        --region "$AWS_REGION" \
        --query 'LoadBalancers[0].DNSName' \
        --output text)
    
    echo -e "${GREEN}Load Balancer DNS: $ALB_DNS${NC}"
else
    echo -e "${YELLOW}Skipping load balancer creation${NC}"
    TARGET_GROUP_ARN=""
fi

echo ""
echo -e "${YELLOW}Preparing ECS task definition...${NC}"

# Update task definition with environment variables
TASK_DEF_FILE="ecs/task-definition.json"
cp "$TASK_DEF_FILE" "${TASK_DEF_FILE}.tmp"

sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{AWS_REGION}}|$AWS_REGION|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{ACCOUNT_ID}}|$ACCOUNT_ID|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{DATABASE_CONNECTION_STRING}}|$DATABASE_CONNECTION_STRING|g" "${TASK_DEF_FILE}.tmp"
sed -i "s|{{REDIS_CONNECTION_STRING}}|$REDIS_CONNECTION_STRING|g" "${TASK_DEF_FILE}.tmp"

echo -e "${YELLOW}Registering task definition...${NC}"
TASK_DEF_ARN=$(aws ecs register-task-definition \
    --cli-input-json file://"${TASK_DEF_FILE}.tmp" \
    --region "$AWS_REGION" \
    --query 'taskDefinition.taskDefinitionArn' \
    --output text)

echo -e "${GREEN}Task Definition ARN: $TASK_DEF_ARN${NC}"

rm "${TASK_DEF_FILE}.tmp"

echo ""
echo -e "${YELLOW}Preparing ECS service definition...${NC}"

SERVICE_DEF_FILE="ecs/service-definition.json"
cp "$SERVICE_DEF_FILE" "${SERVICE_DEF_FILE}.tmp"

sed -i "s|{{CLUSTER_NAME}}|$CLUSTER_NAME|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SUBNET_1}}|$SUBNET_1|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SUBNET_2}}|$SUBNET_2|g" "${SERVICE_DEF_FILE}.tmp"
sed -i "s|{{SECURITY_GROUP}}|$SECURITY_GROUP|g" "${SERVICE_DEF_FILE}.tmp"

if [[ -z "$TARGET_GROUP_ARN" ]]; then
    # Remove loadBalancers section if no LB
    sed -i '/"loadBalancers"/,/],/d' "${SERVICE_DEF_FILE}.tmp"
    sed -i '/"healthCheckGracePeriodSeconds"/d' "${SERVICE_DEF_FILE}.tmp"
else
    sed -i "s|{{TARGET_GROUP_ARN}}|$TARGET_GROUP_ARN|g" "${SERVICE_DEF_FILE}.tmp"
fi

SERVICE_NAME="tourmanagement-web-service"

echo ""
echo -e "${YELLOW}Checking if service exists...${NC}"
EXISTING_SERVICE=$(aws ecs describe-services \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION" \
    --query 'services[0].serviceName' \
    --output text 2>/dev/null || echo "None")

if [ "$EXISTING_SERVICE" == "None" ] || [ "$EXISTING_SERVICE" == "" ]; then
    echo -e "${YELLOW}Creating new ECS service...${NC}"
    aws ecs create-service \
        --cli-input-json file://"${SERVICE_DEF_FILE}.tmp" \
        --region "$AWS_REGION"
else
    echo -e "${YELLOW}Updating existing ECS service...${NC}"
    aws ecs update-service \
        --cluster "$CLUSTER_NAME" \
        --service "$SERVICE_NAME" \
        --task-definition "$TASK_DEF_ARN" \
        --region "$AWS_REGION"
fi

rm "${SERVICE_DEF_FILE}.tmp"

echo ""
echo -e "${YELLOW}Waiting for service to stabilize...${NC}"
aws ecs wait services-stable \
    --cluster "$CLUSTER_NAME" \
    --services "$SERVICE_NAME" \
    --region "$AWS_REGION"

echo ""
echo -e "${GREEN}=== Deployment Completed Successfully ===${NC}"
echo -e "${GREEN}Cluster: $CLUSTER_NAME${NC}"
echo -e "${GREEN}Service: $SERVICE_NAME${NC}"
echo -e "${GREEN}Task Definition: $TASK_DEF_ARN${NC}"

if [[ -n "$ALB_DNS" ]]; then
    echo -e "${GREEN}Application URL: http://$ALB_DNS${NC}"
fi

echo -e "${GREEN}CloudWatch Logs: /ecs/tourmanagement-web${NC}"
echo ""
echo -e "${YELLOW}View service status:${NC}"
echo "aws ecs describe-services --cluster $CLUSTER_NAME --services $SERVICE_NAME --region $AWS_REGION"
echo ""