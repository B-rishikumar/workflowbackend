# =============================================
# Multi-stage Dockerfile for WorkflowManagement API
# =============================================

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/WorkflowManagement.API/WorkflowManagement.API.csproj", "src/WorkflowManagement.API/"]
COPY ["src/WorkflowManagement.Application/WorkflowManagement.Application.csproj", "src/WorkflowManagement.Application/"]
COPY ["src/WorkflowManagement.Core/WorkflowManagement.Core.csproj", "src/WorkflowManagement.Core/"]
COPY ["src/WorkflowManagement.Infrastructure/WorkflowManagement.Infrastructure.csproj", "src/WorkflowManagement.Infrastructure/"]
COPY ["src/WorkflowManagement.Shared/WorkflowManagement.Shared.csproj", "src/WorkflowManagement.Shared/"]
COPY ["Directory.Build.props", "./"]

RUN dotnet restore "src/WorkflowManagement.API/WorkflowManagement.API.csproj"

# Copy source code
COPY . .

# Build and publish
WORKDIR "/src/src/WorkflowManagement.API"
RUN dotnet build "WorkflowManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WorkflowManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create directories for logs and temp files
RUN mkdir -p /app/logs /app/temp && chown -R appuser:appuser /app

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Entry point
ENTRYPOINT ["dotnet", "WorkflowManagement.API.dll"]

# Labels for metadata
LABEL maintainer="WorkflowManagement Team"
LABEL version="1.0.0"
LABEL description="WorkflowManagement API - Enterprise workflow automation platform"
LABEL source="https://github.com/your-org/workflowmanagement"

# Build arguments for versioning
ARG BUILD_VERSION=1.0.0
ARG BUILD_DATE
ARG GIT_COMMIT

LABEL build.version=$BUILD_VERSION
LABEL build.date=$BUILD_DATE
LABEL build.commit=$GIT_COMMIT