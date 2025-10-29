# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/WorkflowManagement.API/WorkflowManagement.API.csproj", "src/WorkflowManagement.API/"]
COPY ["src/WorkflowManagement.Application/WorkflowManagement.Application.csproj", "src/WorkflowManagement.Application/"]
COPY ["src/WorkflowManagement.Core/WorkflowManagement.Core.csproj", "src/WorkflowManagement.Core/"]
COPY ["src/WorkflowManagement.Infrastructure/WorkflowManagement.Infrastructure.csproj", "src/WorkflowManagement.Infrastructure/"]
COPY ["src/WorkflowManagement.Shared/WorkflowManagement.Shared.csproj", "src/WorkflowManagement.Shared/"]
COPY ["Directory.Build.props", "./"]

# Restore dependencies
RUN dotnet restore "src/WorkflowManagement.API/WorkflowManagement.API.csproj"

# Copy source code
COPY . .

# Build
WORKDIR "/src/src/WorkflowManagement.API"
RUN dotnet build "WorkflowManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WorkflowManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WorkflowManagement.API.dll"]

# docker-compose.yml
version: '3.8'

services:
  # SQL Server Database
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: workflow-sqlserver
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourStrong@Password123"
      MSSQL_PID: "Express"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - workflow-network
    restart: unless-stopped

  # Redis Cache
  redis:
    image: redis:7-alpine
    container_name: workflow-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    networks:
      - workflow-network
    restart: unless-stopped
    command: redis-server --appendonly yes

  # Workflow Management API
  workflow-api:
    build:
      context: .
      dockerfile: scripts/deployment/Dockerfile
    container_name: workflow-api
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WorkflowManagementDb;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=true;MultipleActiveResultSets=true
      - ConnectionStrings__Redis=redis:6379
      - JwtSettings__SecretKey=YourProductionSecretKeyThatIsAtLeast32CharactersLong!
      - JwtSettings__Issuer=WorkflowManagement
      - JwtSettings__Audience=WorkflowManagement.Users
    ports:
      - "8080:8080"
    depends_on:
      - sqlserver
      - redis
    networks:
      - workflow-network
    restart: unless-stopped
    volumes:
      - ./logs:/app/logs
      - ./uploads:/app/wwwroot/uploads

  # Seq Logging (Optional)
  seq:
    image: datalust/seq:latest
    container_name: workflow-seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data
    networks:
      - workflow-network
    restart: unless-stopped

  # Nginx Reverse Proxy (Optional)
  nginx:
    image: nginx:alpine
    container_name: workflow-nginx
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - workflow-api
    networks:
      - workflow-network
    restart: unless-stopped

volumes:
  sqlserver_data:
  redis_data:
  seq_data:

networks:
  workflow-network:
    driver: bridge

# docker-compose.override.yml (for development)
version: '3.8'

services:
  workflow-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=WorkflowManagementDb_Dev;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=true;MultipleActiveResultSets=true
    volumes:
      - ./src:/src:cached
      - ./logs:/app/logs
    ports:
      - "5000:8080"
      - "5001:8081"
    command: dotnet watch run --project /src/WorkflowManagement.API/WorkflowManagement.API.csproj

  sqlserver:
    environment:
      SA_PASSWORD: "DevPassword123!"
