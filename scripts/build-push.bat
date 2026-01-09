@echo off
setlocal enabledelayedexpansion

echo === Docker Build and Push Script ===
echo.

REM Project configuration
set PROJECT_NAME=tourmanagement-web
set DOCKERFILE=Dockerfile

REM Sanitize project name for Docker tag
set IMAGE_NAME=%PROJECT_NAME%
for %%a in (A B C D E F G H I J K L M N O P Q R S T U V W X Y Z) do set IMAGE_NAME=!IMAGE_NAME:%%a=%%a!
set IMAGE_NAME=%IMAGE_NAME: =-%
set IMAGE_NAME=%IMAGE_NAME:_=-%

echo Select Registry:
echo 1. AWS ECR (Elastic Container Registry)
echo 2. Docker Hub
set /p REGISTRY_CHOICE="Enter choice (1 or 2): "

if "!REGISTRY_CHOICE!"=="1" (
    echo Selected: AWS ECR
    
    set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
    set /p AWS_ACCOUNT_ID="Enter AWS Account ID: "
    set /p ECR_REPO="Enter ECR Repository Name (default: %IMAGE_NAME%): "
    if "!ECR_REPO!"=="" set ECR_REPO=%IMAGE_NAME%
    
    set /p IMAGE_TAG_INPUT="Enter image tag (default: latest): "
    if "!IMAGE_TAG_INPUT!"=="" (
        set IMAGE_TAG=latest
    ) else (
        set IMAGE_TAG=!IMAGE_TAG_INPUT!
    )
    
    set REGISTRY_URL=!AWS_ACCOUNT_ID!.dkr.ecr.!AWS_REGION!.amazonaws.com
    set FULL_IMAGE_NAME=!REGISTRY_URL!/!ECR_REPO!:!IMAGE_TAG!
    
    echo Authenticating with AWS ECR...
    for /f "tokens=*" %%i in ('aws ecr get-login-password --region !AWS_REGION!') do set ECR_PASSWORD=%%i
    echo !ECR_PASSWORD! | docker login --username AWS --password-stdin !REGISTRY_URL!
    
    if !ERRORLEVEL! neq 0 (
        echo ECR authentication failed
        exit /b 1
    )
    
    echo Checking if ECR repository exists...
    aws ecr describe-repositories --repository-names !ECR_REPO! --region !AWS_REGION! >nul 2>&1
    if !ERRORLEVEL! neq 0 (
        echo Creating ECR repository: !ECR_REPO!
        aws ecr create-repository --repository-name !ECR_REPO! --region !AWS_REGION!
    )
    
) else if "!REGISTRY_CHOICE!"=="2" (
    echo Selected: Docker Hub
    
    set /p DOCKER_USERNAME="Enter Docker Hub username: "
    set /p DOCKER_PASSWORD="Enter Docker Hub password or access token: "
    
    set /p IMAGE_TAG_INPUT="Enter image tag (default: latest): "
    if "!IMAGE_TAG_INPUT!"=="" (
        set IMAGE_TAG=latest
    ) else (
        set IMAGE_TAG=!IMAGE_TAG_INPUT!
    )
    
    set FULL_IMAGE_NAME=!DOCKER_USERNAME!/%IMAGE_NAME%:!IMAGE_TAG!
    
    echo Authenticating with Docker Hub...
    echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
    
    if !ERRORLEVEL! neq 0 (
        echo Docker Hub authentication failed
        exit /b 1
    )
) else (
    echo Invalid choice
    exit /b 1
)

echo.
echo Building Docker image: !FULL_IMAGE_NAME!
echo Build context: %cd%
echo.

docker build -f %DOCKERFILE% -t !FULL_IMAGE_NAME! .

if !ERRORLEVEL! neq 0 (
    echo Docker build failed
    exit /b 1
)

echo.
echo Pushing image to registry: !FULL_IMAGE_NAME!
docker push !FULL_IMAGE_NAME!

if !ERRORLEVEL! neq 0 (
    echo Docker push failed
    exit /b 1
)

echo.
echo === Build and Push Completed Successfully ===
echo Image: !FULL_IMAGE_NAME!
echo.

endlocal