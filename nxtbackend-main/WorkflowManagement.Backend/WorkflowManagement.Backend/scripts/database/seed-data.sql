-- =============================================
-- WorkflowManagement Database Seed Data Script
-- =============================================

USE WorkflowManagementDB;
GO

-- =============================================
-- Seed Roles
-- =============================================

INSERT INTO Roles (Name, Description) VALUES 
('Admin', 'Full system administrator with all permissions'),
('ProjectManager', 'Project manager with project-level permissions'),
('Developer', 'Developer with workflow creation and management permissions'),
('Viewer', 'Read-only access to workflows and executions');

-- =============================================
-- Seed Permissions
-- =============================================

INSERT INTO Permissions (Name, Description, Category) VALUES 
('users.create', 'Create new users', 'User Management'),
('users.read', 'View user information', 'User Management'),
('users.update', 'Update user information', 'User Management'),
('users.delete', 'Delete users', 'User Management'),

('workspaces.create', 'Create new workspaces', 'Workspace Management'),
('workspaces.read', 'View workspace information', 'Workspace Management'),
('workspaces.update', 'Update workspace information', 'Workspace Management'),
('workspaces.delete', 'Delete workspaces', 'Workspace Management'),

('projects.create', 'Create new projects', 'Project Management'),
('projects.read', 'View project information', 'Project Management'),
('projects.update', 'Update project information', 'Project Management'),
('projects.delete', 'Delete projects', 'Project Management'),

('environments.create', 'Create new environments', 'Environment Management'),
('environments.read', 'View environment information', 'Environment Management'),
('environments.update', 'Update environment information', 'Environment Management'),
('environments.delete', 'Delete environments', 'Environment Management'),

('workflows.create', 'Create new workflows', 'Workflow Management'),
('workflows.read', 'View workflow information', 'Workflow Management'),
('workflows.update', 'Update workflow information', 'Workflow Management'),
('workflows.delete', 'Delete workflows', 'Workflow Management'),
('workflows.execute', 'Execute workflows', 'Workflow Management'),
('workflows.publish', 'Publish workflows', 'Workflow Management'),

('api-endpoints.create', 'Create new API endpoints', 'API Management'),
('api-endpoints.read', 'View API endpoint information', 'API Management'),
('api-endpoints.update', 'Update API endpoint information', 'API Management'),
('api-endpoints.delete', 'Delete API endpoints', 'API Management'),

('approvals.create', 'Create approval requests', 'Approval Management'),
('approvals.approve', 'Approve workflow requests', 'Approval Management'),
('approvals.reject', 'Reject workflow requests', 'Approval Management'),

('schedules.create', 'Create workflow schedules', 'Schedule Management'),
('schedules.update', 'Update workflow schedules', 'Schedule Management'),
('schedules.delete', 'Delete workflow schedules', 'Schedule Management'),

('metrics.view', 'View workflow metrics and analytics', 'Analytics'),
('logs.view', 'View execution logs', 'Logging'),
('system.admin', 'System administration functions', 'System');

-- =============================================
-- Seed Default Admin User
-- =============================================
-- Password: Admin@123 (hashed using BCrypt)
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, Department, JobTitle, IsActive) VALUES 
('System', 'Administrator', 'admin@workflowmanagement.com', '$2a$11$8K1p/a0dRt1z5pxZU5gULumHAGRaZb5z8sK7nL9vOoNwJfP6bX7we', 0, 'IT', 'System Administrator', 1);

-- =============================================
-- Seed Sample Users
-- =============================================

INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, Department, JobTitle, IsActive) VALUES 
('John', 'Smith', 'john.smith@workflowmanagement.com', '$2a$11$8K1p/a0dRt1z5pxZU5gULumHAGRaZb5z8sK7nL9vOoNwJfP6bX7we', 1, 'IT', 'Project Manager', 1),
('Sarah', 'Johnson', 'sarah.johnson@workflowmanagement.com', '$2a$11$8K1p/a0dRt1z5pxZU5gULumHAGRaZb5z8sK7nL9vOoNwJfP6bX7we', 2, 'Development', 'Senior Developer', 1),
('Mike', 'Davis', 'mike.davis@workflowmanagement.com', '$2a$11$8K1p/a0dRt1z5pxZU5gULumHAGRaZb5z8sK7nL9vOoNwJfP6bX7we', 2, 'Development', 'Full Stack Developer', 1),
('Emily', 'Wilson', 'emily.wilson@workflowmanagement.com', '$2a$11$8K1p/a0dRt1z5pxZU5gULumHAGRaZb5z8sK7nL9vOoNwJfP6bX7we', 3, 'QA', 'Quality Analyst', 1);

-- =============================================
-- Seed Sample Workspaces
-- =============================================

INSERT INTO Workspaces (Name, Description, CreatedByUserId) VALUES 
('Default Workspace', 'Default workspace for getting started', 1),
('E-Commerce Platform', 'Workspace for e-commerce integration workflows', 1),
('Customer Service', 'Workspace for customer service automation workflows', 2);

-- =============================================
-- Seed User-Workspace Relationships
-- =============================================

INSERT INTO UserWorkspaces (UserId, WorkspaceId, Role) VALUES 
(1, 1, 0), -- Admin in Default Workspace
(1, 2, 0), -- Admin in E-Commerce Platform
(1, 3, 0), -- Admin in Customer Service
(2, 1, 1), -- John Smith as Project Manager in Default Workspace
(2, 2, 1), -- John Smith as Project Manager in E-Commerce Platform
(3, 1, 2), -- Sarah Johnson as Developer in Default Workspace
(3, 2, 2), -- Sarah Johnson as Developer in E-Commerce Platform
(4, 1, 2), -- Mike Davis as Developer in Default Workspace
(5, 1, 3), -- Emily Wilson as Viewer in Default Workspace
(5, 3, 2); -- Emily Wilson as Developer in Customer Service

-- =============================================
-- Seed Sample Projects
-- =============================================

INSERT INTO Projects (Name, Description, WorkspaceId, CreatedByUserId) VALUES 
('Sample Integration Project', 'Sample project to demonstrate workflow capabilities', 1, 1),
('Order Processing System', 'Automated order processing workflows', 2, 2),
('Customer Support Automation', 'Automated customer support workflows', 3, 2),
('Payment Gateway Integration', 'Integration workflows for payment processing', 2, 1);

-- =============================================
-- Seed Sample Environments
-- =============================================

INSERT INTO Environments (Name, Description, ProjectId, Configuration, Variables, CreatedByUserId) VALUES 
('Development', 'Development environment for testing', 1, '{"timeout": 30, "retries": 3}', '{"API_URL": "https://dev-api.example.com", "DEBUG": "true"}', 1),
('Staging', 'Staging environment for pre-production testing', 1, '{"timeout": 60, "retries": 2}', '{"API_URL": "https://staging-api.example.com", "DEBUG": "false"}', 1),
('Production', 'Production environment', 1, '{"timeout": 120, "retries": 1}', '{"API_URL": "https://api.example.com", "DEBUG": "false"}', 1),

('Dev Environment', 'Development environment for order processing', 2, '{"timeout": 45, "retries": 3}', '{"ORDER_API_URL": "https://dev-orders.example.com", "PAYMENT_API_URL": "https://dev-payments.example.com"}', 2),
('Production Environment', 'Production environment for order processing', 2, '{"timeout": 90, "retries": 2}', '{"ORDER_API_URL": "https://orders.example.com", "PAYMENT_API_URL": "https://payments.example.com"}', 2),

('Support Dev', 'Development environment for support workflows', 3, '{"timeout": 30, "retries": 2}', '{"SUPPORT_API_URL": "https://dev-support.example.com", "EMAIL_SERVICE_URL": "https://dev-email.example.com"}', 2),
('Support Prod', 'Production environment for support workflows', 3, '{"timeout": 60, "retries": 1}', '{"SUPPORT_API_URL": "https://support.example.com", "EMAIL_SERVICE_URL": "https://email.example.com"}', 2);

-- =============================================
-- Seed Sample API Endpoints
-- =============================================

INSERT INTO ApiEndpoints (Name, Description, Url, Method, Type, Headers, AuthenticationMethod, Documentation, CreatedByUserId) VALUES 
('Get User Info', 'Retrieve user information from user service', 'https://api.example.com/users/{userId}', 'GET', 0, '{"Content-Type": "application/json", "Accept": "application/json"}', 'Bearer Token', 'Gets user information by user ID', 1),
('Create Order', 'Create a new order in the order management system', 'https://api.example.com/orders', 'POST', 0, '{"Content-Type": "application/json", "Accept": "application/json"}', 'API Key', 'Creates a new order with the provided details', 1),
('Update Order Status', 'Update the status of an existing order', 'https://api.example.com/orders/{orderId}/status', 'PUT', 0, '{"Content-Type": "application/json"}', 'API Key', 'Updates order status to specified value', 1),
('Send Email Notification', 'Send email notification using email service', 'https://api.example.com/notifications/email', 'POST', 0, '{"Content-Type": "application/json"}', 'Bearer Token', 'Sends email notification to specified recipient', 1),
('Process Payment', 'Process payment through payment gateway', 'https://api.example.com/payments/process', 'POST', 0, '{"Content-Type": "application/json", "Authorization": "Bearer {token}"}', 'Bearer Token', 'Processes payment using provided payment details', 2),
('Get Weather Data', 'Get weather information from weather service', 'https://api.weather.com/v1/current', 'GET', 0, '{"Accept": "application/json"}', 'API Key', 'Retrieves current weather data for specified location', 3);

-- =============================================
-- Seed API Parameters
-- =============================================

INSERT INTO ApiParameters (ApiEndpointId, Name, Description, Type, IsRequired, DefaultValue, ValidationRules) VALUES 
-- Get User Info parameters
(1, 'userId', 'User ID to retrieve information for', 1, 1, NULL, '{"type": "integer", "minimum": 1}'),

-- Create Order parameters
(2, 'customerId', 'Customer ID placing the order', 1, 1, NULL, '{"type": "integer", "minimum": 1}'),
(2, 'items', 'Array of order items', 5, 1, NULL, '{"type": "array", "minItems": 1}'),
(2, 'totalAmount', 'Total order amount', 3, 1, NULL, '{"type": "number", "minimum": 0.01}'),
(2, 'currency', 'Order currency code', 0, 0, 'USD', '{"type": "string", "length": 3}'),

-- Update Order Status parameters
(3, 'orderId', 'Order ID to update', 1, 1, NULL, '{"type": "integer", "minimum": 1}'),
(3, 'status', 'New order status', 0, 1, NULL, '{"type": "string", "enum": ["pending", "confirmed", "shipped", "delivered", "cancelled"]}'),

-- Send Email parameters
(4, 'to', 'Recipient email address', 0, 1, NULL, '{"type": "email"}'),
(4, 'subject', 'Email subject', 0, 1, NULL, '{"type": "string", "maxLength": 200}'),
(4, 'body', 'Email body content', 0, 1, NULL, '{"type": "string", "maxLength": 5000}'),

-- Process Payment parameters
(5, 'amount', 'Payment amount', 3, 1, NULL, '{"type": "number", "minimum": 0.01}'),
(5, 'currency', 'Payment currency', 0, 0, 'USD', '{"type": "string", "length": 3}'),
(5, 'paymentMethod', 'Payment method details', 5, 1, NULL, '{"type": "object"}'),

-- Weather API parameters
(6, 'location', 'Location for weather data', 0, 1, NULL, '{"type": "string", "minLength": 2}'),
(6, 'units', 'Temperature units', 0, 0, 'metric', '{"type": "string", "enum": ["metric", "imperial"]}');

-- =============================================
-- Seed Sample Workflows
-- =============================================

INSERT INTO Workflows (Name, Description, ProjectId, EnvironmentId, Status, RequiresApproval, Tags, CreatedByUserId) VALUES 
('Sample User Workflow', 'Sample workflow demonstrating user data processing', 1, 1, 0, 0, 'sample,demo,user', 1),
('Order Processing Workflow', 'Complete order processing from creation to notification', 2, 4, 1, 1, 'orders,ecommerce,automation', 2),
('Customer Support Ticket', 'Automated customer support ticket processing', 3, 6, 1, 0, 'support,automation,customer', 2),
('Payment Processing Flow', 'Secure payment processing workflow', 4, 4, 0, 1, 'payment,security,finance', 1);

-- =============================================
-- Seed Workflow Steps
-- =============================================

-- Sample User Workflow steps
INSERT INTO WorkflowSteps (WorkflowId, Name, Description, [Order], ApiEndpointId, InputMapping, OutputMapping, IsOptional, TimeoutSeconds, RetryCount) VALUES 
(1, 'Get User Information', 'Retrieve user details from user service', 1, 1, '{"userId": "{input.userId}"}', '{"user": "{response.data}"}', 0, 30, 2),
(1, 'Send Welcome Email', 'Send welcome email to user', 2, 4, '{"to": "{user.email}", "subject": "Welcome!", "body": "Welcome {user.name}!"}', '{"emailSent": true}', 1, 45, 1);

-- Order Processing Workflow steps
INSERT INTO WorkflowSteps (WorkflowId, Name, Description, [Order], ApiEndpointId, InputMapping, OutputMapping, IsOptional, TimeoutSeconds, RetryCount) VALUES 
(2, 'Create Order', 'Create new order in system', 1, 2, '{"customerId": "{input.customerId}", "items": "{input.items}", "totalAmount": "{input.totalAmount}"}', '{"orderId": "{response.orderId}", "orderNumber": "{response.orderNumber}"}', 0, 60, 2),
(2, 'Process Payment', 'Process payment for the order', 2, 5, '{"amount": "{input.totalAmount}", "paymentMethod": "{input.paymentMethod}"}', '{"transactionId": "{response.transactionId}", "status": "{response.status}"}', 0, 120, 1),
(2, 'Update Order Status', 'Update order status after payment', 3, 3, '{"orderId": "{orderId}", "status": "confirmed"}', '{"updated": true}', 0, 30, 2),
(2, 'Send Order Confirmation', 'Send order confirmation email', 4, 4, '{"to": "{input.customerEmail}", "subject": "Order Confirmation", "body": "Your order {orderNumber} has been confirmed"}', '{"confirmationSent": true}', 0, 45, 1);

-- =============================================
-- Seed Sample Schedules
-- =============================================

INSERT INTO WorkflowSchedules (WorkflowId, CronExpression, TimeZone, Description, IsActive, NextRunTime) VALUES 
(2, '0 9 * * MON-FRI', 'UTC', 'Process orders every weekday at 9 AM UTC', 1, DATEADD(DAY, 1, CAST(CAST(GETUTCDATE() AS DATE) AS DATETIME2) + CAST('09:00:00' AS TIME))),
(3, '*/30 * * * *', 'UTC', 'Check for new support tickets every 30 minutes', 1, DATEADD(MINUTE, 30, GETUTCDATE()));

-- =============================================
-- Seed Sample Workflow Versions
-- =============================================

INSERT INTO WorkflowVersions (WorkflowId, VersionNumber, Name, Description, Configuration, CreatedByUserId) VALUES 
(1, 1, 'Sample User Workflow v1.0', 'Initial version of sample user workflow', '{"version": "1.0", "created": "2024-01-01"}', 1),
(2, 1, 'Order Processing Workflow v1.0', 'Initial version of order processing workflow', '{"version": "1.0", "created": "2024-01-01", "paymentGateway": "stripe"}', 2),
(2, 2, 'Order Processing Workflow v1.1', 'Updated version with improved error handling', '{"version": "1.1", "created": "2024-01-15", "paymentGateway": "stripe", "errorHandling": "enhanced"}', 2);

-- =============================================
-- Seed Sample Metrics
-- =============================================

INSERT INTO WorkflowMetrics (WorkflowId, MetricName, MetricValue, Metadata) VALUES 
(1, 'execution_count', 25, '{"period": "last_30_days"}'),
(1, 'success_rate', 96.0, '{"period": "last_30_days"}'),
(1, 'avg_execution_time', 1245, '{"unit": "milliseconds", "period": "last_30_days"}'),

(2, 'execution_count', 156, '{"period": "last_30_days"}'),
(2, 'success_rate', 94.2, '{"period": "last_30_days"}'),
(2, 'avg_execution_time', 2847, '{"unit": "milliseconds", "period": "last_30_days"}'),
(2, 'total_orders_processed', 1247, '{"period": "last_30_days"}'),
(2, 'total_revenue_processed', 89456.78, '{"currency": "USD", "period": "last_30_days"}'),

(3, 'execution_count', 78, '{"period": "last_30_days"}'),
(3, 'success_rate', 98.7, '{"period": "last_30_days"}'),
(3, 'avg_execution_time', 856, '{"unit": "milliseconds", "period": "last_30_days"}'),
(3, 'tickets_processed', 145, '{"period": "last_30_days"}');

GO

-- =============================================
-- Create Sample Executions (for demonstration)
-- =============================================

DECLARE @ExecutionId1 NVARCHAR(50) = NEWID();
DECLARE @ExecutionId2 NVARCHAR(50) = NEWID();
DECLARE @ExecutionId3 NVARCHAR(50) = NEWID();

INSERT INTO WorkflowExecutions (Id, WorkflowId, Status, ExecutedAt, CompletedAt, InputParameters, OutputParameters, ExecutionContext) VALUES 
(@ExecutionId1, 1, 1, DATEADD(HOUR, -2, GETUTCDATE()), DATEADD(HOUR, -2, DATEADD(SECOND, 12, GETUTCDATE())), '{"userId": 1001}', '{"user": {"id": 1001, "name": "Test User", "email": "test@example.com"}, "emailSent": true}', 'Manual Execution'),
(@ExecutionId2, 2, 1, DATEADD(HOUR, -1, GETUTCDATE()), DATEADD(HOUR, -1, DATEADD(SECOND, 45, GETUTCDATE())), '{"customerId": 2001, "items": [{"id": 1, "quantity": 2}], "totalAmount": 99.99}', '{"orderId": 5001, "transactionId": "tx_12345", "confirmationSent": true}', 'Scheduled Execution'),
(@ExecutionId3, 1, 2, DATEADD(MINUTE, -30, GETUTCDATE()), DATEADD(MINUTE, -30, DATEADD(SECOND, 8, GETUTCDATE())), '{"userId": 1002}', NULL, 'Manual Execution');

-- =============================================
-- Create Sample Execution Logs
-- =============================================

INSERT INTO ExecutionLogs (ExecutionId, StepId, LogLevel, Message, Details) VALUES 
(@ExecutionId1, NULL, 'Info', 'Workflow execution started', 'Starting execution of Sample User Workflow'),
(@ExecutionId1, 1, 'Info', 'Executing step: Get User Information', 'Making API call to retrieve user data'),
(@ExecutionId1, 1, 'Info', 'Step completed successfully', 'User data retrieved successfully'),
(@ExecutionId1, 2, 'Info', 'Executing step: Send Welcome Email', 'Sending welcome email to user'),
(@ExecutionId1, 2, 'Info', 'Step completed successfully', 'Welcome email sent successfully'),
(@ExecutionId1, NULL, 'Info', 'Workflow execution completed', 'All steps completed successfully'),

(@ExecutionId2, NULL, 'Info', 'Workflow execution started', 'Starting execution of Order Processing Workflow'),
(@ExecutionId2, 3, 'Info', 'Executing step: Create Order', 'Creating new order in system'),
(@ExecutionId2, 3, 'Info', 'Step completed successfully', 'Order created with ID: 5001'),
(@ExecutionId2, 4, 'Info', 'Executing step: Process Payment', 'Processing payment through gateway'),
(@ExecutionId2, 4, 'Info', 'Step completed successfully', 'Payment processed successfully'),
(@ExecutionId2, NULL, 'Info', 'Workflow execution completed', 'Order processing completed successfully'),

(@ExecutionId3, NULL, 'Info', 'Workflow execution started', 'Starting execution of Sample User Workflow'),
(@ExecutionId3, 1, 'Error', 'Step failed: Get User Information', 'API call failed: User not found'),
(@ExecutionId3, NULL, 'Error', 'Workflow execution failed', 'Required step failed, stopping execution');

GO

PRINT 'Sample data seeded successfully!';
PRINT 'Default admin user created with email: admin@workflowmanagement.com and password: Admin@123';
PRINT 'Sample users, workspaces, projects, and workflows have been created for demonstration.';