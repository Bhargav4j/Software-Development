# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy solution file
COPY TourManagement.sln ./

# Copy all project files for dependency restoration
COPY src/TourManagement.Web/TourManagement.Web.csproj src/TourManagement.Web/
COPY src/TourManagement.Application/TourManagement.Application.csproj src/TourManagement.Application/
COPY src/TourManagement.Domain/TourManagement.Domain.csproj src/TourManagement.Domain/
COPY src/TourManagement.Infrastructure/TourManagement.Infrastructure.csproj src/TourManagement.Infrastructure/
COPY Tests/TourManagement.Application.Tests.csproj Tests/
COPY Tests/TourManagement.Domain.Tests.csproj Tests/
COPY Tests/TourManagement.Infrastructure.Tests.csproj Tests/

# Restore dependencies
RUN dotnet restore TourManagement.sln

# Copy all source code
COPY . .

# Build the solution
RUN dotnet build TourManagement.sln -c Release --no-restore

# Publish the web application
WORKDIR /src/src/TourManagement.Web
RUN dotnet publish TourManagement.Web.csproj -c Release -o /app/publish --no-restore --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application from builder
COPY --from=builder /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Expose application port
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "TourManagement.Web.dll"]