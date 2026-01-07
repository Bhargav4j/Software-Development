@echo off
setlocal enabledelayedexpansion

echo ========================================
echo AWS ECS Fargate Deployment Script
echo ========================================
echo.

REM Project configuration
set PROJECT_NAME=softwarecomp
set TASK_FAMILY=!PROJECT_NAME!-task
set SERVICE_NAME=!PROJECT_NAME!-service

REM Prompt for AWS configuration
set /p AWS_REGION="Enter AWS region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter ECS cluster name (e.g., my-ecs-cluster): "
set /p VPC_ID="Enter VPC ID (e.g., vpc-0abc123def456): "
set /p SUBNET_IDS="Enter Subnet IDs comma-separated (e.g., subnet-0abc123,subnet-0def456): "
set /p SECURITY_GROUP="Enter Security Group ID (e.g., sg-0abc123def): "
set /p IMAGE_URI="Enter Docker image URI (e.g., 123456789.dkr.ecr.us-east-1.amazonaws.com/softwarecomp:latest): "

REM Parse subnets
for /f "tokens=1,2 delims=," %%a in ("!SUBNET_IDS!") do (
    set SUBNET_1=%%a
    set SUBNET_2=%%b
)
if "!SUBNET_2!"=="" set SUBNET_2=!SUBNET_1!

echo.
echo Getting AWS Account ID...
for /f "delims=" %%i in ('aws sts get-caller-identity --query Account --output text') do set ACCOUNT_ID=%%i
echo Account ID: !ACCOUNT_ID!

echo.
echo Checking ECS cluster...
aws ecs describe-clusters --clusters "!CLUSTER_NAME!" --region "!AWS_REGION!" >nul 2>&1
if !ERRORLEVEL! neq 0 (
    echo Cluster does not exist. Creating cluster: !CLUSTER_NAME!
    aws ecs create-cluster --cluster-name "!CLUSTER_NAME!" --region "!AWS_REGION!"
)

REM Handle load balancer configuration
echo.
set /p NEED_LB="Do you need a load balancer for this service? (y/n): "

set LOAD_BALANCER_ARN=
set TARGET_GROUP_ARN=
set LOAD_BALANCER_DNS=

if /i "!NEED_LB!"=="y" (
    echo.
    echo Creating Application Load Balancer...
    
    set ALB_NAME=!PROJECT_NAME!-alb
    for /f "delims=" %%i in ('aws elbv2 create-load-balancer --name "!ALB_NAME!" --subnets "!SUBNET_1!" "!SUBNET_2!" --security-groups "!SECURITY_GROUP!" --scheme internet-facing --type application --ip-address-type ipv4 --region "!AWS_REGION!" --output json') do set ALB_OUTPUT=%%i
    
    for /f "delims=" %%i in ('echo !ALB_OUTPUT! ^| jq -r ".LoadBalancers[0].LoadBalancerArn"') do set LOAD_BALANCER_ARN=%%i
    for /f "delims=" %%i in ('echo !ALB_OUTPUT! ^| jq -r ".LoadBalancers[0].DNSName"') do set LOAD_BALANCER_DNS=%%i
    
    echo Load Balancer created: !LOAD_BALANCER_ARN!
    echo Load Balancer DNS: !LOAD_BALANCER_DNS!
    
    echo Creating Target Group...
    set TG_NAME=!PROJECT_NAME!-tg
    for /f "delims=" %%i in ('aws elbv2 create-target-group --name "!TG_NAME!" --protocol HTTP --port 8080 --vpc-id "!VPC_ID!" --target-type ip --health-check-enabled --health-check-path "/health" --health-check-interval-seconds 30 --health-check-timeout-seconds 5 --healthy-threshold-count 2 --unhealthy-threshold-count 3 --region "!AWS_REGION!" --output json') do set TG_OUTPUT=%%i
    
    for /f "delims=" %%i in ('echo !TG_OUTPUT! ^| jq -r ".TargetGroups[0].TargetGroupArn"') do set TARGET_GROUP_ARN=%%i
    echo Target Group created: !TARGET_GROUP_ARN!
    
    echo Creating Load Balancer Listener...
    aws elbv2 create-listener --load-balancer-arn "!LOAD_BALANCER_ARN!" --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn="!TARGET_GROUP_ARN!" --region "!AWS_REGION!" >nul
    echo Listener created successfully
) else (
    echo Skipping load balancer creation.
    REM Remove loadBalancers section using PowerShell
    powershell -Command "(Get-Content ecs\service-definition.json | ConvertFrom-Json | Select-Object * -ExcludeProperty loadBalancers,healthCheckGracePeriodSeconds | ConvertTo-Json -Depth 10) | Set-Content ecs\service-definition.tmp.json"
    move /y ecs\service-definition.tmp.json ecs\service-definition.json >nul
)

echo.
echo Creating CloudWatch log group...
aws logs create-log-group --log-group-name "/ecs/!PROJECT_NAME!" --region "!AWS_REGION!" 2>nul
if !ERRORLEVEL! neq 0 echo Log group already exists

echo.
echo Updating task definition with configuration...

REM Replace placeholders in task definition using PowerShell
powershell -Command "(Get-Content ecs\task-definition.json) -replace '{{IMAGE_URI}}','!IMAGE_URI!' -replace '{{AWS_REGION}}','!AWS_REGION!' -replace '{{ACCOUNT_ID}}','!ACCOUNT_ID!' | Set-Content ecs\task-definition.json"

echo Registering ECS task definition...
for /f "delims=" %%i in ('aws ecs register-task-definition --cli-input-json file://ecs/task-definition.json --region "!AWS_REGION!" --query "taskDefinition.taskDefinitionArn" --output text') do set TASK_DEF_ARN=%%i

echo Task definition registered: !TASK_DEF_ARN!

echo.
echo Updating service definition with configuration...

REM Replace placeholders in service definition
powershell -Command "(Get-Content ecs\service-definition.json) -replace '{{CLUSTER_NAME}}','!CLUSTER_NAME!' -replace '{{SUBNET_1}}','!SUBNET_1!' -replace '{{SUBNET_2}}','!SUBNET_2!' -replace '{{SECURITY_GROUP}}','!SECURITY_GROUP!' -replace '{{TARGET_GROUP_ARN}}','!TARGET_GROUP_ARN!' | Set-Content ecs\service-definition.json"

echo.
echo Checking if service exists...
for /f "delims=" %%i in ('aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[0].serviceName" --output text 2^>nul') do set SERVICE_EXISTS=%%i

if "!SERVICE_EXISTS!"=="!SERVICE_NAME!" (
    echo Service exists. Updating service...
    aws ecs update-service --cluster "!CLUSTER_NAME!" --service "!SERVICE_NAME!" --task-definition "!TASK_DEF_ARN!" --force-new-deployment --region "!AWS_REGION!" >nul
    echo Service updated successfully
) else (
    echo Service does not exist. Creating new service...
    aws ecs create-service --cli-input-json file://ecs/service-definition.json --region "!AWS_REGION!" >nul
    echo Service created successfully
)

echo.
echo Waiting for service to stabilize...
aws ecs wait services-stable --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!"

echo.
echo ========================================
echo DEPLOYMENT SUCCESSFUL!
echo ========================================
echo Cluster: !CLUSTER_NAME!
echo Service: !SERVICE_NAME!
echo Task Definition: !TASK_DEF_ARN!
echo Region: !AWS_REGION!

if not "!LOAD_BALANCER_DNS!"=="" (
    echo Load Balancer DNS: http://!LOAD_BALANCER_DNS!
)

echo CloudWatch Logs: /ecs/!PROJECT_NAME!
echo.
echo Service Details:
aws ecs describe-services --cluster "!CLUSTER_NAME!" --services "!SERVICE_NAME!" --region "!AWS_REGION!" --query "services[0].{Status:status,Running:runningCount,Desired:desiredCount,Pending:pendingCount}" --output table

echo.
echo Deployment complete!
echo.

endlocal