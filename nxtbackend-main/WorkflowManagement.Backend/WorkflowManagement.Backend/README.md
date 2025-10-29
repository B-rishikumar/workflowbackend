# Workflow Management Backend API

Enterprise-ready .NET 8 backend API for workflow management system that enables users to create, manage, and execute workflows with REST/SOAP API endpoints.

## 🚀 Features

### Core Functionality
- **User Management**: Complete user authentication and authorization with JWT tokens
- **Workspace Organization**: Hierarchical structure (Workspace → Project → Environment → Workflow)
- **Workflow Designer**: Create and manage complex workflows with multiple API endpoints
- **API Endpoint Support**: REST, SOAP, GraphQL, and Webhook integrations
- **Swagger/WSDL Import**: Automatically import API definitions
- **Workflow Execution**: Run workflows manually, on schedule, or via triggers
- **Approval System**: Workflow approval workflow for publishing and modifications
- **Version Control**: Track workflow versions and changes
- **Scheduling**: Cron-based workflow scheduling with Quartz.NET
- **Metrics & Analytics**: Comprehensive execution metrics and reporting
- **Detailed Logging**: Complete audit trail and execution logs

### Technical Features
- **Clean Architecture**: Domain-driven design with clear separation of concerns
- **Entity Framework Core 8**: Support for SQL Server and Oracle databases
- **Redis Caching**: Distributed caching for improved performance
- **Background Jobs**: Quartz.NET for scheduled tasks and background processing
- **Health Checks**: Comprehensive health monitoring
- **Rate Limiting**: API rate limiting and throttling
- **Swagger Documentation**: Complete API documentation
- **Docker Support**: Containerized deployment
- **Logging**: Structured logging with Serilog

## 🏗️ Architecture

```
├── WorkflowManagement.API/          # Web API Layer
├── WorkflowManagement.Application/  # Application Services Layer
├── WorkflowManagement.Core/         # Domain Layer (Entities, Interfaces)
├── WorkflowManagement.Infrastructure/ # Infrastructure Layer (Data, External Services)
├── WorkflowManagement.Shared/       # Shared Utilities and Constants
└── tests/                          # Test Projects
```

## 🛠️ Technology Stack

- **.NET 8**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core 8**: ORM for database operations
- **SQL Server/Oracle**: Primary database options
- **Redis**: Distributed caching
- **JWT**: Authentication and authorization
- **AutoMapper**: Object mapping
- **FluentValidation**: Input validation
- **MediatR**: CQRS pattern implementation
- **Quartz.NET**: Background job scheduling
- **Serilog**: Structured logging
- **Swagger/OpenAPI**: API documentation
- **Docker**: Containerization
- **Health Checks**: Application monitoring

## 🚦 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server or Oracle Database
- Redis (optional, will fall back to in-memory cache)
- Docker (for containerized deployment)

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd WorkflowManagement.Backend
   ```

2. **Update connection strings**
   Edit `src/WorkflowManagement.API/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=WorkflowManagementDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true",
       "Redis": "localhost:6379"
     }
   }
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Run database migrations**
   ```bash
   cd src/WorkflowManagement.API
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the API**
   - API: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`
   - Health Checks: `https://localhost:5001/health-ui`

### Docker Deployment

1. **Using Docker Compose (Recommended)**
   ```bash
   docker-compose up -d
   ```

2. **Build and run manually**
   ```bash
   docker build -t workflow-management-api .
   docker run -p 8080:8080 workflow-management-api
   ```

## 📊 Database Schema

### Core Entities

- **Users**: System users with roles and permissions
- **Workspaces**: Top-level organizational units
- **Projects**: Project containers within workspaces
- **Environments**: Environment-specific configurations (Dev, Test, Prod)
- **Workflows**: Workflow definitions with steps and configuration
- **ApiEndpoints**: REST/SOAP/GraphQL endpoint definitions
- **WorkflowExecutions**: Execution instances with status and logs
- **WorkflowApprovals**: Approval requests and responses
- **WorkflowSchedules**: Scheduled execution configurations
- **WorkflowMetrics**: Performance and usage analytics

### Supported Database Providers

- **SQL Server** (default)
- **Oracle** (configure via `DatabaseProvider` setting)

## 🔐 Security

- **JWT Authentication**: Secure token-based authentication
- **Role-based Authorization**: User roles (SuperAdmin, Admin, Manager, Developer, Viewer)
- **API Rate Limiting**: Prevent abuse with configurable limits
- **CORS**: Configurable cross-origin resource sharing
- **HTTPS**: Force HTTPS in production
- **Password Hashing**: BCrypt for secure password storage

## 📈 Monitoring & Observability

### Health Checks
- Database connectivity
- Redis availability
- Application status
- Custom health check endpoints

### Logging
- Structured logging with Serilog
- Request/response logging
- Performance metrics
- Error tracking
- Configurable log levels

### Metrics
- Workflow execution statistics
- API performance metrics
- User activity tracking
- System resource monitoring

## 🔧 Configuration

### Key Configuration Sections

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "SQL Server connection string",
    "Redis": "Redis connection string"
  },
  "JwtSettings": {
    "SecretKey": "JWT signing key",
    "Issuer": "Token issuer",
    "Audience": "Token audience",
    "ExpiryMinutes": 60
  },
  "SmtpSettings": {
    "Host": "SMTP server",
    "Port": 587,
    "Username": "SMTP username",
    "Password": "SMTP password"
  }
}
```

## 🧪 Testing

Run all tests:
```bash
dotnet test
```

Run specific test project:
```bash
dotnet test tests/WorkflowManagement.UnitTests
dotnet test tests/WorkflowManagement.IntegrationTests
```

## 📚 API Documentation

### Authentication Endpoints
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh token
- `POST /api/v1/auth/logout` - User logout
- `GET /api/v1/auth/me` - Current user info

### Workflow Management
- `GET /api/v1/workflows` - List workflows
- `POST /api/v1/workflows` - Create workflow
- `GET /api/v1/workflows/{id}` - Get workflow details
- `PUT /api/v1/workflows/{id}` - Update workflow
- `DELETE /api/v1/workflows/{id}` - Delete workflow
- `POST /api/v1/workflows/{id}/execute` - Execute workflow
- `POST /api/v1/workflows/{id}/publish` - Publish workflow

### API Endpoint Management
- `GET /api/v1/apiendpoints` - List API endpoints
- `POST /api/v1/apiendpoints` - Create API endpoint
- `POST /api/v1/apiendpoints/import/swagger` - Import from Swagger
- `POST /api/v1/apiendpoints/import/soap` - Import from WSDL

Complete API documentation is available at `/swagger` when running the application.

## 🚀 Deployment

### Environment Variables (Production)

```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Production database connection"
ConnectionStrings__Redis="Production Redis connection"
JwtSettings__SecretKey="Production JWT secret"
SmtpSettings__Host="Production SMTP server"
```

### Docker Production Deployment

1. **Build production image**
   ```bash
   docker build -f scripts/deployment/Dockerfile -t workflow-management:latest .
   ```

2. **Deploy with environment-specific configuration**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is license