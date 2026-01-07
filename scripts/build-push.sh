#!/bin/bash

set -e

echo "========================================"
echo "Docker Build and Push Script"
echo "========================================"
echo ""

# Project configuration
PROJECT_NAME="SoftwareComp"

# Sanitize image name: lowercase, replace non-alphanumeric with hyphens, trim hyphens
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

echo "Project: $PROJECT_NAME"
echo "Sanitized Image Name: $IMAGE_NAME"
echo ""

# Select registry type
echo "Select Docker Registry:"
echo "1. AWS ECR (Elastic Container Registry)"
echo "2. Docker Hub"
read -p "Enter your choice (1 or 2): " REGISTRY_CHOICE

if [ "$REGISTRY_CHOICE" == "1" ]; then
    echo ""
    echo "=== AWS ECR Configuration ==="
    read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
    read -p "Enter AWS Account ID: " AWS_ACCOUNT_ID
    read -p "Enter ECR Repository Name [$IMAGE_NAME]: " ECR_REPO
    ECR_REPO=${ECR_REPO:-$IMAGE_NAME}
    
    REGISTRY_URL="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
    
    echo ""
    echo "Logging into AWS ECR..."
    aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY_URL"
    
    if [ $? -ne 0 ]; then
        echo "ERROR: ECR login failed. Please check your AWS credentials and region."
        exit 1
    fi
    
    echo "Checking if ECR repository exists..."
    aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" >/dev/null 2>&1 || {
        echo "Repository does not exist. Creating ECR repository: $ECR_REPO"
        aws ecr create-repository --repository-name "$ECR_REPO" --region "$AWS_REGION"
        if [ $? -ne 0 ]; then
            echo "ERROR: Failed to create ECR repository."
            exit 1
        fi
    }
    
    read -p "Enter image tag [latest]: " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="${REGISTRY_URL}/${ECR_REPO}:${IMAGE_TAG}"
    
elif [ "$REGISTRY_CHOICE" == "2" ]; then
    echo ""
    echo "=== Docker Hub Configuration ==="
    read -p "Enter Docker Hub username: " DOCKER_USERNAME
    read -sp "Enter Docker Hub password or access token: " DOCKER_PASSWORD
    echo ""
    
    echo "Logging into Docker Hub..."
    echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
    
    if [ $? -ne 0 ]; then
        echo "ERROR: Docker Hub login failed. Please check your credentials."
        exit 1
    fi
    
    read -p "Enter image tag [latest]: " IMAGE_TAG
    IMAGE_TAG=${IMAGE_TAG:-latest}
    # Sanitize tag
    IMAGE_TAG=$(echo "$IMAGE_TAG" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9.-' '-' | sed 's/^-*//;s/-*$//')
    IMAGE_TAG=${IMAGE_TAG:-latest}
    
    FULL_IMAGE_NAME="${DOCKER_USERNAME}/${IMAGE_NAME}:${IMAGE_TAG}"
    
else
    echo "ERROR: Invalid choice. Please select 1 or 2."
    exit 1
fi

echo ""
echo "========================================"
echo "Building Docker Image"
echo "========================================"
echo "Image: $FULL_IMAGE_NAME"
echo ""

# Build Docker image from repository root
docker build -f Dockerfile -t "$FULL_IMAGE_NAME" .

if [ $? -ne 0 ]; then
    echo "ERROR: Docker build failed."
    exit 1
fi

echo ""
echo "========================================"
echo "Pushing Docker Image"
echo "========================================"
echo ""

docker push "$FULL_IMAGE_NAME"

if [ $? -ne 0 ]; then
    echo "ERROR: Docker push failed."
    exit 1
fi

echo ""
echo "========================================"
echo "SUCCESS!"
echo "========================================"
echo "Image successfully built and pushed:"
echo "$FULL_IMAGE_NAME"
echo ""
echo "You can now use this image URI in your ECS deployment."
echo ""