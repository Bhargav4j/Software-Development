#!/bin/bash
set -e

echo "======================================"
echo "  Docker Build & Push Script"
echo "======================================"
echo ""

# Project name
PROJECT_NAME="TourManagement"

# Sanitize project name for Docker tag
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "Project: $PROJECT_NAME"
echo "Image base name: $IMAGE_NAME"
echo ""

# Prompt for registry type
echo "Select container registry:"
echo "1. AWS ECR (Elastic Container Registry)"
echo "2. Docker Hub"
read -p "Enter choice (1 or 2): " REGISTRY_CHOICE
echo ""

if [ "$REGISTRY_CHOICE" == "1" ]; then
    # AWS ECR
    echo "=== AWS ECR Configuration ==="
    read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    read -p "Enter ECR Repository Name (e.g., tourmanagement): " ECR_REPO
    
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    FULL_REPO_NAME="${REGISTRY_URL}/${ECR_REPO}"
    
    echo ""
    echo "Authenticating with AWS ECR..."
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: ECR authentication failed"
        exit 1
    fi
    
    echo "Checking if ECR repository exists..."
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || {
        echo "Repository does not exist. Creating ECR repository..."
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
        echo "ECR repository created successfully"
    }
    
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    # Docker Hub
    echo "=== Docker Hub Configuration ==="
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub password or access token: " DOCKER_PASSWORD
    echo ""
    
    FULL_REPO_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}"
    
    echo ""
    echo "Authenticating with Docker Hub..."
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Docker Hub authentication failed"
        exit 1
    fi
else
    echo "ERROR: Invalid choice. Please select 1 or 2."
    exit 1
fi

echo ""
read -p "Enter image tag (default: latest): " IMAGE_TAG
IMAGE_TAG=${IMAGE_TAG:-latest}

# Sanitize tag
IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
IMAGE_TAG=${IMAGE_TAG:-latest}

FULL_IMAGE_NAME="${FULL_REPO_NAME}:${IMAGE_TAG}"

echo ""
echo "======================================"
echo "Building Docker image..."
echo "Image: $FULL_IMAGE_NAME"
echo "======================================"
echo ""

docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo "ERROR: Docker build failed"
    exit 1
fi

echo ""
echo "======================================"
echo "Pushing Docker image..."
echo "======================================"
echo ""

docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo "ERROR: Docker push failed"
    exit 1
fi

echo ""
echo "======================================"
echo "SUCCESS: Image pushed successfully"
echo "Image: $FULL_IMAGE_NAME"
echo "======================================"
