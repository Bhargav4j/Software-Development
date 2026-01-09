@echo off
setlocal enabledelayedexpansion

echo === AWS ECS Fargate Deployment Script ===
echo.

REM Prompt for deployment configuration
set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS cluster name (e.g., tourmanagement-cluster): "
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNET_IDS="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "
set /p IMAGE_URI="Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/tourmanagement-web:latest): "
set /p DATABASE_CONNECTION_STRING="Enter Database Connection String: "
set /p REDIS_CONNECTION_STRING="Enter Redis Connection String (optional, press Enter to skip): "

REM Split subnet IDs
for /f "tokens=1,2 delims=," %%a in ("!SUBNET_IDS!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

echo.
echo Getting AWS Account ID...
for /f "tokens=*" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo Account ID: !ACCOUNT_ID!

echo.
echo Checking if ECS cluster exists...
aws ecs describe-clusters --clusters !CLUSTER_NAME! --region !AWS_REGION! >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Creating ECS cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name !CLUSTER_NAME! --region !AWS_REGION!
)

echo.
set /p NEED_LB="Do you need a load balancer for this service? (y/n): "

if /i "!NEED_LB!"=="y" (
    echo Creating Application Load Balancer and Target Group...
    
    REM Create ALB
    for /f "tokens=*" %%i in ('aws elbv2 create-load-balancer --name tourmanagement-alb --subnets !SUBNET_1! !SUBNET_2! --security-groups !SECURITY_GROUP! --scheme internet-facing --type application --ip-address-type ipv4 --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set ALB_ARN=%%i
    
    if "!ALB_ARN!"=="" (
        for /f "tokens=*" %%i in ('aws elbv2 describe-load-balancers --names tourmanagement-alb --region !AWS_REGION! --query "LoadBalancers[0].LoadBalancerArn" --output text') do set ALB_ARN=%%i
    )
    
    echo ALB ARN: !ALB_ARN!
    
    REM Create Target Group with target-type ip
    for /f "tokens=*" %%i in ('aws elbv2 create-target-group --name tourmanagement-tg --protocol HTTP --port 8080 --vpc-id !VPC_ID! --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path /health --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    
    if "!TARGET_GROUP_ARN!"=="" (
        for /f "tokens=*" %%i in ('aws elbv2 describe-target-groups --names tourmanagement-tg --region !AWS_REGION! --query "TargetGroups[0].TargetGroupArn" --output text') do set TARGET_GROUP_ARN=%%i
    )
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    
    REM Create listener
    aws elbv2 create-listener --load-balancer-arn !ALB_ARN! --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=!TARGET_GROUP_ARN! --region !AWS_REGION! >nul 2>&1
    
    REM Get ALB DNS name
    for /f "tokens=*" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns !ALB_ARN! --region !AWS_REGION! --query "LoadBalancers[0].DNSName" --output text') do set ALB_DNS=%%i
    
    echo Load Balancer DNS: !ALB_DNS!
) else (
    echo Skipping load balancer creation
    set TARGET_GROUP_ARN=
)

echo.
echo Preparing ECS task definition...

set TASK_DEF_FILE=ecs\task-definition.json
copy !TASK_DEF_FILE! !TASK_DEF_FILE!.tmp >nul

powershell -Command "(Get-Content !TASK_DEF_FILE!.tmp) -replace '{{IMAGE_URI}}', '!IMAGE_URI!' | Set-Content !TASK_DEF_FILE!.tmp"
powershell -Command "(Get-Content !TASK_DEF_FILE!.tmp) -replace '{{AWS_REGION}}', '!AWS_REGION!' | Set-Content !TASK_DEF_FILE!.tmp"
powershell -Command "(Get-Content !TASK_DEF_FILE!.tmp) -replace '{{ACCOUNT_ID}}', '!ACCOUNT_ID!' | Set-Content !TASK_DEF_FILE!.tmp"
powershell -Command "(Get-Content !TASK_DEF_FILE!.tmp) -replace '{{DATABASE_CONNECTION_STRING}}', '!DATABASE_CONNECTION_STRING!' | Set-Content !TASK_DEF_FILE!.tmp"
powershell -Command "(Get-Content !TASK_DEF_FILE!.tmp) -replace '{{REDIS_CONNECTION_STRING}}', '!REDIS_CONNECTION_STRING!' | Set-Content !TASK_DEF_FILE!.tmp"

echo Registering task definition...
for /f "tokens=*" %%i in ('aws ecs register-task-definition --cli-input-json file://!TASK_DEF_FILE!.tmp --region !AWS_REGION! --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

echo Task Definition ARN: !TASK_DEF_ARN!

del !TASK_DEF_FILE!.tmp

echo.
echo Preparing ECS service definition...

set SERVICE_DEF_FILE=ecs\service-definition.json
copy !SERVICE_DEF_FILE! !SERVICE_DEF_FILE!.tmp >nul

powershell -Command "(Get-Content !SERVICE_DEF_FILE!.tmp) -replace '{{CLUSTER_NAME}}', '!CLUSTER_NAME!' | Set-Content !SERVICE_DEF_FILE!.tmp"
powershell -Command "(Get-Content !SERVICE_DEF_FILE!.tmp) -replace '{{SUBNET_1}}', '!SUBNET_1!' | Set-Content !SERVICE_DEF_FILE!.tmp"
powershell -Command "(Get-Content !SERVICE_DEF_FILE!.tmp) -replace '{{SUBNET_2}}', '!SUBNET_2!' | Set-Content !SERVICE_DEF_FILE!.tmp"
powershell -Command "(Get-Content !SERVICE_DEF_FILE!.tmp) -replace '{{SECURITY_GROUP}}', '!SECURITY_GROUP!' | Set-Content !SERVICE_DEF_FILE!.tmp"

if "!TARGET_GROUP_ARN!"=="" (
    powershell -Command "$content = Get-Content !SERVICE_DEF_FILE!.tmp -Raw; $content = $content -replace '(?s)\"loadBalancers\"[^]]*],', ''; $content = $content -replace '\"healthCheckGracePeriodSeconds\"[^,]*,', ''; $content | Set-Content !SERVICE_DEF_FILE!.tmp"
) else (
    powershell -Command "(Get-Content !SERVICE_DEF_FILE!.tmp) -replace '{{TARGET_GROUP_ARN}}', '!TARGET_GROUP_ARN!' | Set-Content !SERVICE_DEF_FILE!.tmp"
)

set SERVICE_NAME=tourmanagement-web-service

echo.
echo Checking if service exists...
for /f "tokens=*" %%i in ('aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION! --query "services[0].serviceName" --output text 2^>nul') do set EXISTING_SERVICE=%%i

if "!EXISTING_SERVICE!"=="None" (
    echo Creating new ECS service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_FILE!.tmp --region !AWS_REGION!
) else if "!EXISTING_SERVICE!"=="" (
    echo Creating new ECS service...
    aws ecs create-service --cli-input-json file://!SERVICE_DEF_FILE!.tmp --region !AWS_REGION!
) else (
    echo Updating existing ECS service...
    aws ecs update-service --cluster !CLUSTER_NAME! --service !SERVICE_NAME! --task-definition !TASK_DEF_ARN! --region !AWS_REGION!
)

del !SERVICE_DEF_FILE!.tmp

echo.
echo Waiting for service to stabilize...
aws ecs wait services-stable --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!

echo.
echo === Deployment Completed Successfully ===
echo Cluster: !CLUSTER_NAME!
echo Service: !SERVICE_NAME!
echo Task Definition: !TASK_DEF_ARN!

if not "!ALB_DNS!"=="" (
    echo Application URL: http://!ALB_DNS!
)

echo CloudWatch Logs: /ecs/tourmanagement-web
echo.
echo View service status:
echo aws ecs describe-services --cluster !CLUSTER_NAME! --services !SERVICE_NAME! --region !AWS_REGION!
echo.

endlocal