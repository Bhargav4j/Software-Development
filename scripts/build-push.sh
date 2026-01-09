#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== Docker Build and Push Script ===${NC}"
echo ""

# Project configuration
PROJECT_NAME="tourmanagement-web"
DOCKERFILE="Dockerfile"

# Sanitize project name for Docker tag
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo -e "${YELLOW}Select Registry:${NC}"
echo "1. AWS ECR (Elastic Container Registry)"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" == "1" ]; then
    echo -e "${GREEN}Selected: AWS ECR${NC}"
    
    read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    read -p "Enter ECR Repository Name (default: $IMAGE_NAME): " ECR_REPO
    ECR_REPO=${ECR_REPO:-$IMAGE_NAME}
    
    read -p "Enter image tag (default: latest): " IMAGE_TAG_INPUT
    IMAGE_TAG=$(echo "${IMAGE_TAG_INPUT:-latest}" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}:${IMAGE_TAG}"
    
    echo -e "${YELLOW}Authenticating with AWS ECR...${NC}"
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}ECR authentication failed${NC}"
        exit 1
    fi
    
    echo -e "${YELLOW}Checking if ECR repository exists...${NC}"
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || {
        echo -e "${YELLOW}Creating ECR repository: $ECR_REPO${NC}"
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
    }
    
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    echo -e "${GREEN}Selected: Docker Hub${NC}"
    
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub password or access token: " DOCKER_PASSWORD
    echo ""
    
    read -p "Enter image tag (default: latest): " IMAGE_TAG_INPUT
    IMAGE_TAG=$(echo "${IMAGE_TAG_INPUT:-latest}" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}:${IMAGE_TAG}"
    
    echo -e "${YELLOW}Authenticating with Docker Hub...${NC}"
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}Docker Hub authentication failed${NC}"
        exit 1
    fi
else
    echo -e "${RED}Invalid choice${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Building Docker image: $FULL_IMAGE_NAME${NC}"
echo -e "${YELLOW}Build context: $(pwd)${NC}"
echo ""

docker build -f "$DOCKERFILE" -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker build failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Pushing image to registry: $FULL_IMAGE_NAME${NC}"
docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo -e "${RED}Docker push failed${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}=== Build and Push Completed Successfully ===${NC}"
echo -e "${GREEN}Image: $FULL_IMAGE_NAME${NC}"
echo ""