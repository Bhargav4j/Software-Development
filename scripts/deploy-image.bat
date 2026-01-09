@echo off
setlocal enabledelayedexpansion

echo ======================================
echo   AWS ECS Fargate Deployment Script
echo ======================================
echo.

set PROJECT_NAME=tourmanagement
set TASK_FAMILY=%PROJECT_NAME%-task
set SERVICE_NAME=%PROJECT_NAME%-service

echo === AWS Configuration ===
set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
set AWS_DEFAULT_REGION=%AWS_REGION%

echo.
set /p CLUSTER_NAME="Enter ECS Cluster Name (e.g., my-ecs-cluster): "

echo.
echo === Network Configuration ===
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNET_IDS="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "

for /f "tokens=1,2 delims=," %%a in ("!SUBNET_IDS!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

echo.
set /p IMAGE_URI="Enter Docker Image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/tourmanagement:latest): "

echo.
echo Getting AWS Account ID...
for /f "delims=" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo Account ID: !ACCOUNT_ID!

echo.
echo Checking if ECS cluster exists...
aws ecs describe-clusters --clusters "!CLUSTER_NAME!" --region "!AWS_REGION!" >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Cluster does not exist. Creating ECS cluster...
    aws ecs create-cluster --cluster-name "!CLUSTER_NAME!" --region "!AWS_REGION!"
    echo ECS cluster created successfully
)

echo.
set /p USE_LB="Do you need a load balancer for this service? (y/n): "

if /i "!USE_LB!"=="y" (
    echo.
    echo === Creating Application Load Balancer ===
    
    set LB_NAME=!PROJECT_NAME!-alb
    set TG_NAME=!PROJECT_NAME!-tg
    
    echo Creating Application Load Balancer...
    for /f "delims=" %%i in ('aws elbv2 create-load-balancer --name "!LB_NAME!" --subnets "!SUBNET_1!" "!SUBNET_2!" --security-groups "!SECURITY_GROUP!" --scheme internet-facing --type application --ip-address-type ipv4 --region "!AWS_REGION!" --query "LoadBalancers[0].LoadBalancerArn" --output text 2^>nul') do set LB_ARN=%%i
    if "!LB_ARN!"=="" for /f "delims=" %%i in ('aws elbv2 describe-load-balancers --names "!LB_NAME!" --query "LoadBalancers[0].LoadBalancerArn" --output text') do set LB_ARN=%%i
    
    echo Load Balancer ARN: !LB_ARN!
    
    echo Creating Target Group...
    for /f "delims=" %%i in ('aws elbv2 create-target-group --name "!TG_NAME!" --protocol HTTP --port 8080 --vpc-id "!VPC_ID!" --target-type ip --health-check-enabled --health-check-protocol HTTP --health-check-path "/health" --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region "!AWS_REGION!" --query "TargetGroups[0].TargetGroupArn" --output text 2^>nul') do set TARGET_GROUP_ARN=%%i
    if "!TARGET_GROUP_ARN!"=="" for /f "delims=" %%i in ('aws elbv2 describe-target-groups --names "!TG_NAME!" --query "TargetGroups[0].TargetGroupArn" --output text') do set TARGET_GROUP_ARN=%%i
    
    echo Target Group ARN: !TARGET_GROUP_ARN!
    
    echo Creating Listener...
    aws elbv2 create-listener --load-balancer-arn "!LB_ARN!" --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn="!TARGET_GROUP_ARN!" --region "!AWS_REGION!" >nul 2>&1
    
    for /f "delims=" %%i in ('aws elbv2 describe-load-balancers --load-balancer-arns "!LB_ARN!" --query "LoadBalancers[0].DNSName" --output text') do set LB_DNS=%%i
    echo Load Balancer DNS: !LB_DNS!
) else (
    echo Skipping load balancer configuration
    set TARGET_GROUP_ARN=
)

echo.
echo === Database Configuration ===
set /p DB_HOST="Enter Database Host: "
set /p DB_PORT="Enter Database Port (default 5432): "
if "!DB_PORT!"=="" set DB_PORT=5432
set /p DB_NAME="Enter Database Name: "
set /p DB_USER="Enter Database User: "
set /p DB_PASSWORD="Enter Database Password: "

echo.
echo Preparing ECS task definition...
copy ecs\task-definition.json ecs\task-definition-temp.json >nul

powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{IMAGE_URI}}','!IMAGE_URI!' | Set-Content ecs\task-definition-temp.json"
powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{AWS_REGION}}','!AWS_REGION!' | Set-Content ecs\task-definition-temp.json"
powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{ACCOUNT_ID}}','!ACCOUNT_ID!' | Set-Content ecs\task-definition-temp.json"
powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{DB_HOST}}','!DB_HOST!' | Set-Content ecs\task-definition-temp.json"
powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{DB_PORT}}','!DB_PORT!' | Set-Content ecs\task-definition-temp.json"
powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{DB_NAME}}','!DB_NAME!' | Set-Content ecs\task-definition-temp.json"
powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{DB_USER}}','!DB_USER!' | Set-Content ecs\task-definition-temp.json"
powershell -Command "(Get-Content ecs\task-definition-temp.json) -replace '{{DB_PASSWORD}}','!DB_PASSWORD!' | Set-Content ecs\task-definition-temp.json"

echo Creating CloudWatch log group...
aws logs create-log-group --log-group-name "/ecs/!PROJECT_NAME!" --region "!AWS_REGION!" 2>nul

echo.
echo Registering ECS task definition...
for /f "delims=" %%i in ('aws ecs register-task-definition --cli-input-json file://ecs/task-definition-temp.json --region "!AWS_REGION!" --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

echo Task Definition ARN: !TASK_DEF_ARN!

del ecs\task-definition-temp.json

echo.
echo Preparing ECS service definition...
copy ecs\service-definition.json ecs\service-definition-temp.json >nul

powershell -Command "(Get-Content ecs\service-definition-temp.json) -replace '{{CLUSTER_NAME}}','!CLUSTER_NAME!' | Set-Content ecs\service-definition-temp.json"
powershell -Command "(Get-Content ecs\service-definition-temp.json) -replace '{{SUBNET_1}}','!SUBNET_1!' | Set-Content ecs\service-definition-temp.json"
powershell -Command "(Get-Content ecs\service-definition-temp.json) -replace '{{SUBNET_2}}','!SUBNET_2!' | Set-Content ecs\service-definition-temp.json"
powershell -Command "(Get-Content ecs\service-definition-temp.json) -replace '{{SECURITY_GROUP}}','!SECURITY_GROUP!' | Set-Content ecs\service-definition-temp.json"

if /i "!USE_LB!"=="y" (
    powershell -Command "(Get-Content ecs\service-definition-temp.json) -replace '{{TARGET_GROUP_ARN}}','!TARGET_GROUP_ARN!' | Set-Content ecs\service-definition-temp.json"
) else (
    powershell -Command "$json = Get-Content ecs\service-definition-temp.json | ConvertFrom-Json; $json.PSObject.Properties.Remove('loadBalancers'); $json.PSObject.Properties.Remove('healthCheckGracePeriodSeconds'); $json | ConvertTo-Json -Depth 10 | Set-Content ecs\service-definition-temp.json"
)

echo.
echo Checking if service exists...
for /f "delims=" %%i in ('aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[?status==`ACTIVE`].serviceName" --output text') do set SERVICE_EXISTS=%%i

if "!SERVICE_EXISTS!"=="" (
    echo Service does not exist. Creating ECS service...
    aws ecs create-service --cli-input-json file://ecs/service-definition-temp.json --region "!AWS_REGION!"
) else (
    echo Service exists. Updating ECS service...
    aws ecs update-service --cluster "!CLUSTER_NAME!" --service "!SERVICE_NAME!" --task-definition "!TASK_DEF_ARN!" --region "!AWS_REGION!"
)

del ecs\service-definition-temp.json

echo.
echo Waiting for service to become stable...
aws ecs wait services-stable --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!"

echo.
echo ======================================
echo   Deployment Complete
echo ======================================
echo.
echo Cluster: !CLUSTER_NAME!
echo Service: !SERVICE_NAME!
echo Task Definition: !TASK_DEF_ARN!

if /i "!USE_LB!"=="y" (
    echo Load Balancer DNS: !LB_DNS!
    echo Application URL: http://!LB_DNS!
)

echo CloudWatch Logs: /ecs/!PROJECT_NAME!
echo.
echo Service Status:
aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[0].{Status:status,DesiredCount:desiredCount,RunningCount:runningCount}" --output table

echo.
echo Deployment completed successfully!

endlocal
