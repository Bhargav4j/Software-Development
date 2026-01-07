# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

WORKDIR /src

# Copy project files for dependency restoration
COPY *.csproj ./
COPY *.sln* ./

# Restore NuGet packages
RUN dotnet restore

# Copy remaining source code
COPY . .

# Build the application
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-build

# Runtime stage - using explicit base image from parameter
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

WORKDIR /app

# Install dependencies for time zone and culture support
RUN apk add --no-cache icu-libs tzdata

# Set environment variables for .NET runtime
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8 \
    TZ=UTC \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080

# Create non-root user for security
RUN addgroup -g 1001 appuser && \
    adduser -D -u 1001 -G appuser appuser && \
    chown -R appuser:appuser /app

# Copy published application from builder
COPY --from=builder --chown=appuser:appuser /app/publish .

# Switch to non-root user
USER appuser

# Expose application port
EXPOSE 8080

# Configure entry point
ENTRYPOINT ["dotnet", "SoftwareComp.dll"]