# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy solution and project files for dependency caching
COPY TourManagement.sln ./
COPY src/TourManagement.Domain/TourManagement.Domain.csproj ./src/TourManagement.Domain/
COPY src/TourManagement.Application/TourManagement.Application.csproj ./src/TourManagement.Application/
COPY src/TourManagement.Infrastructure/TourManagement.Infrastructure.csproj ./src/TourManagement.Infrastructure/
COPY src/TourManagement.Web/TourManagement.Web.csproj ./src/TourManagement.Web/

# Restore dependencies
RUN dotnet restore TourManagement.sln

# Copy source code
COPY src/ ./src/

# Build the application
WORKDIR /src/src/TourManagement.Web
RUN dotnet build TourManagement.Web.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish TourManagement.Web.csproj -c Release --no-build -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published application
COPY --from=builder /app/publish .

# Set ownership
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

# Start the application
ENTRYPOINT ["dotnet", "TourManagement.Web.dll"]