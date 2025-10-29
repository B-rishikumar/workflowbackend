# WorkflowManagement API Documentation

## Overview

The WorkflowManagement API is a comprehensive RESTful web service that provides workflow automation capabilities for enterprise applications. It allows users to create, manage, and execute complex workflows with API endpoint integrations.

## Base URL

```
Development: https://localhost:5001/api
Staging: https://staging-api.workflowmanagement.com/api
Production: https://api.workflowmanagement.com/api
```

## Authentication

The API uses JWT (JSON Web Tokens) for authentication. Include the token in the Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

### Getting an Authentication Token

**POST /auth/login**

```json
{
  "email": "user@example.com",
  "password": "your-password"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "def50200...",
    "expiresAt": "2024-12-31T23:59:59Z",
    "user": {
      "id": 1,
      "firstName": "John",
      "lastName": "Doe",
      "email": "user@example.com",
      "role": "Developer"
    }
  },
  "message": "Login successful"
}
```

## API Endpoints

### Authentication Endpoints

#### POST /auth/login
Authenticate user and get access token.

#### POST /auth/refresh
Refresh an expired access token.

#### POST /auth/logout
Logout and invalidate tokens.

#### POST /auth/register
Register a new user account.

---

### User Management

#### GET /users
Get all users with pagination and filtering.

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `searchTerm` (string): Search users by name or email
- `role` (UserRole): Filter by user role
- `isActive` (bool): Filter by active status

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@example.com",
        "role": "Developer",
        "isActive": true,
        "createdAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 50,
    "page": 1,
    "pageSize": 10
  }
}
```

#### GET /users/{id}
Get user by ID.

#### POST /users
Create a new user.

**Request Body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@example.com",
  "role": "Developer",
  "department": "Engineering",
  "jobTitle": "Senior Developer"
}
```

#### PUT /users/{id}
Update user information.

#### DELETE /users/{id}
Delete user (soft delete).

---

### Workspace Management

#### GET /workspaces
Get all workspaces.

#### GET /workspaces/{id}
Get workspace by ID.

#### POST /workspaces
Create a new workspace.

**Request Body:**
```json
{
  "name": "My Workspace",
  "description": "Workspace for development projects"
}
```

#### PUT /workspaces/{id}
Update workspace.

#### DELETE /workspaces/{id}
Delete workspace.

#### POST /workspaces/{workspaceId}/users/{userId}
Add user to workspace.

#### DELETE /workspaces/{workspaceId}/users/{userId}
Remove user from workspace.

---

### Project Management

#### GET /projects
Get all projects with filtering.

**Query Parameters:**
- `workspaceId` (int): Filter by workspace
- `isActive` (bool): Filter by active status

#### GET /projects/{id}
Get project by ID.

#### POST /projects
Create a new project.

**Request Body:**
```json
{
  "name": "E-Commerce Integration",
  "description": "Integration project for e-commerce platform",
  "workspaceId": 1
}
```

#### PUT /projects/{id}
Update project.

#### DELETE /projects/{id}
Delete project.

---

### Environment Management

#### GET /environments
Get all environments.

#### GET /environments/{id}
Get environment by ID.

#### POST /environments
Create a new environment.

**Request Body:**
```json
{
  "name": "Production",
  "description": "Production environment",
  "projectId": 1,
  "configuration": {
    "timeout": 120,
    "retries": 2
  },
  "variables": {
    "API_URL": "https://api.example.com",
    "DEBUG": "false"
  }
}
```

#### PUT /environments/{id}
Update environment.

#### DELETE /environments/{id}
Delete environment.

#### GET /environments/{id}/configuration
Get environment configuration.

#### PUT /environments/{id}/configuration
Update environment configuration.

---

### API Endpoint Management

#### GET /api-endpoints
Get all API endpoints.

#### GET /api-endpoints/{id}
Get API endpoint by ID.

#### POST /api-endpoints
Create a new API endpoint.

**Request Body:**
```json
{
  "name": "Get User Info",
  "description": "Retrieve user information",
  "url": "https://api.example.com/users/{userId}",
  "method": "GET",
  "type": "REST",
  "headers": {
    "Content-Type": "application/json",
    "Accept": "application/json"
  },
  "authenticationMethod": "Bearer Token"
}
```

#### PUT /api-endpoints/{id}
Update API endpoint.

#### DELETE /api-endpoints/{id}
Delete API endpoint.

#### POST /api-endpoints/{id}/test
Test API endpoint connectivity.

#### POST /api-endpoints/import/swagger
Import API endpoints from Swagger specification.

**Request Body:**
```json
{
  "swaggerUrl": "https://api.example.com/swagger.json"
}
```

#### POST /api-endpoints/import/soap
Import API endpoints from SOAP WSDL.

**Request Body:**
```json
{
  "wsdlUrl": "https://api.example.com/service.wsdl"
}
```

---

### Workflow Management

#### GET /workflows
Get all workflows with filtering.

**Query Parameters:**
- `projectId` (int): Filter by project
- `environmentId` (int): Filter by environment
- `status` (WorkflowStatus): Filter by status
- `searchTerm` (string): Search workflows

#### GET /workflows/{id}
Get workflow by ID.

#### POST /workflows
Create a new workflow.

**Request Body:**
```json
{
  "name": "Order Processing",
  "description": "Complete order processing workflow",
  "projectId": 1,
  "environmentId": 1,
  "requiresApproval": true,
  "tags": "orders,ecommerce"
}
```

#### PUT /workflows/{id}
Update workflow.

#### DELETE /workflows/{id}
Delete workflow.

#### POST /workflows/{id}/publish
Publish workflow.

#### POST /workflows/{id}/execute
Execute workflow.

**Request Body:**
```json
{
  "initialInputs": {
    "customerId": 1001,
    "orderAmount": 99.99
  },
  "executionContext": "Manual Execution"
}
```

#### POST /workflows/{id}/test
Test workflow connectivity.

#### GET /workflows/{id}/versions
Get workflow version history.

#### POST /workflows/{id}/clone
Clone workflow.

**Request Body:**
```json
{
  "name": "Order Processing - Copy",
  "description": "Cloned workflow for testing"
}
```

---

### Workflow Execution Management

#### GET /workflow-execution
Get all executions with filtering.

**Query Parameters:**
- `workflowId` (int): Filter by workflow
- `status` (ExecutionStatus): Filter by status
- `startDate` (datetime): Filter by start date
- `endDate` (datetime): Filter by end date

#### GET /workflow-execution/{executionId}
Get execution by ID.

#### POST /workflow-execution/{executionId}/cancel
Cancel running execution.

#### POST /workflow-execution/{executionId}/retry
Retry failed execution.

#### GET /workflow-execution/{executionId}/logs
Get execution logs.

#### GET /workflow-execution/running
Get currently running executions.

#### GET /workflow-execution/recent
Get recent executions.

---

### Scheduling Management

#### POST /scheduling/workflow/{workflowId}
Create workflow schedule.

**Request Body:**
```json
{
  "cronExpression": "0 9 * * MON-FRI",
  "timeZone": "UTC",
  "description": "Run every weekday at 9 AM",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

#### PUT /scheduling/workflow/{workflowId}
Update workflow schedule.

#### GET /scheduling/workflow/{workflowId}
Get workflow schedule.

#### POST /scheduling/workflow/{workflowId}/enable
Enable workflow schedule.

#### POST /scheduling/workflow/{workflowId}/disable
Disable workflow schedule.

#### DELETE /scheduling/workflow/{workflowId}
Delete workflow schedule.

#### GET /scheduling/active
Get all active schedules.

#### GET /scheduling/due
Get schedules due for execution.

#### POST /scheduling/validate-cron
Validate cron expression.

**Request Body:**
```json
{
  "cronExpression": "0 9 * * MON-FRI"
}
```

---

### Approval Management

#### POST /approval/request
Request workflow approval.

**Request Body:**
```json
{
  "workflowId": 1,
  "approvalType": "execution",
  "requestReason": "Need to process urgent orders"
}
```

#### POST /approval/{approvalId}/approve
Approve request.

**Request Body:**
```json
{
  "comments": "Approved for urgent order processing"
}
```

#### POST /approval/{approvalId}/reject
Reject request.

**Request Body:**
```json
{
  "comments": "Rejected due to incomplete configuration"
}
```

#### GET /approval/pending
Get pending approvals for current user.

#### GET /approval/workflow/{workflowId}/history
Get approval history for workflow.

---

### Metrics and Analytics

#### GET /metrics/workflow/{workflowId}
Get workflow metrics.

**Query Parameters:**
- `startDate` (datetime): Start date for metrics
- `endDate` (datetime): End date for metrics

**Response:**
```json
{
  "success": true,
  "data": {
    "workflowId": 1,
    "workflowName": "Order Processing",
    "totalExecutions": 156,
    "successfulExecutions": 147,
    "failedExecutions": 9,
    "successRate": 94.2,
    "averageExecutionTime": "00:00:45",
    "lastExecutionAt": "2024-01-15T10:30:00Z"
  }
}
```

#### GET /metrics/project/{projectId}
Get project metrics.

#### GET /metrics/system
Get system-wide metrics.

#### POST /metrics/workflow/{workflowId}/record
Record custom metric.

**Request Body:**
```json
{
  "metricName": "custom_processing_time",
  "value": 1250.5,
  "metadata": {
    "unit": "milliseconds",
    "category": "performance"
  }
}
```

#### GET /metrics/dashboard
Get dashboard metrics summary.

---

### Logging

#### GET /logs/execution/{executionId}
Get execution logs.

#### GET /logs/workflow/{workflowId}
Get workflow logs with pagination.

#### GET /logs/system
Get system logs (admin only).

#### POST /logs/search
Search logs with advanced filters.

**Request Body:**
```json
{
  "searchTerm": "error",
  "logTypes": ["execution", "system"],
  "logLevels": ["Error", "Warning"],
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T23:59:59Z",
  "page": 1,
  "pageSize": 50
}
```

---

## Error Handling

All API responses follow a consistent error format:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": {
    "email": ["Email is required"],
    "password": ["Password must be at least 8 characters"]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### HTTP Status Codes

- **200 OK**: Successful request
- **201 Created**: Resource created successfully
- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict
- **422 Unprocessable Entity**: Validation errors
- **500 Internal Server Error**: Server error

## Rate Limiting

The API implements rate limiting to prevent abuse:

- **Default Limit**: 100 requests per minute per user
- **Admin Users**: 200 requests per minute
- **Rate Limit Headers**:
  - `X-RateLimit-Limit`: Request limit per minute
  - `X-RateLimit-Remaining`: Remaining requests
  - `X-RateLimit-Reset`: Reset time in Unix timestamp

When rate limit is exceeded, the API returns:
```json
{
  "success": false,
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 60
}
```

## Pagination

List endpoints support pagination with the following parameters:

- `page`: Page number (starts from 1)
- `pageSize`: Number of items per page (max 100)

Response includes pagination metadata:
```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 500,
    "page": 1,
    "pageSize": 10,
    "totalPages": 50,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

## Filtering and Sorting

Most list endpoints support filtering and sorting:

**Filtering:**
- Use query parameters to filter results
- Multiple filters can be combined
- Date filters support ISO 8601 format

**Sorting:**
- `sortBy`: Field name to sort by
- `sortDescending`: Boolean for sort direction (default: true)

Example:
```
GET /workflows?projectId=1&status=Published&sortBy=CreatedAt&sortDescending=false
```

## Webhooks

The API supports webhooks for real-time notifications:

### Webhook Events

- `workflow.execution.started`
- `workflow.execution.completed`
- `workflow.execution.failed`
- `workflow.published`
- `approval.requested`
- `approval.approved`
- `approval.rejected`

### Webhook Payload

```json
{
  "event": "workflow.execution.completed",
  "timestamp": "2024-01-15T10:30:00Z",
  "data": {
    "executionId": "exec_123456",
    "workflowId": 1,
    "workflowName": "Order Processing",
    "status": "Completed",
    "executionTime": "00:00:45"
  }
}
```

## SDK and Client Libraries

Official SDKs are available for:

- **.NET**: NuGet package `WorkflowManagement.Client`
- **JavaScript/TypeScript**: npm package `@workflowmanagement/client`
- **Python**: pip package `workflowmanagement-client`
- **Java**: Maven package `com.workflowmanagement.client`

## API Versioning

The API uses URL versioning:

- **Current Version**: v1
- **Base URL Pattern**: `/api/v{version}/`
- **Version Header**: `X-API-Version: 1`

## Testing

### Swagger/OpenAPI

Interactive API documentation is available at:
- Development: `https://localhost:5001/swagger`
- Production: `https://api.workflowmanagement.com/swagger`

### Postman Collection

Import the Postman collection for easy testing:
```
https://api.workflowmanagement.com/postman/collection.json
```

## Support

For API support and questions:

- **Documentation**: https://docs.workflowmanagement.com
- **Support Email**: api-support@workflowmanagement.com
- **GitHub Issues**: https://github.com/workflowmanagement/api/issues
- **Status Page**: https://status.workflowmanagement.com

## Changelog

### v1.2.0 (2024-01-15)
- Added bulk operations for approvals
- Enhanced metrics API with custom metrics
- Improved error handling and validation

### v1.1.0 (2024-01-01)
- Added webhook support
- Implemented advanced logging endpoints
- Added workflow cloning functionality

### v1.0.0 (2023-12-01)
- Initial API release
- Core workflow management features
- Authentication and authorization
- Basic metrics and logging