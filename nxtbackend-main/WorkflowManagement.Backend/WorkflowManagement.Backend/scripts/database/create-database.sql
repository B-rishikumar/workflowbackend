-- =============================================
-- WorkflowManagement Database Creation Script
-- =============================================

-- Create database (uncomment if needed)
-- CREATE DATABASE WorkflowManagementDB;
-- GO

USE WorkflowManagementDB;
GO

-- =============================================
-- Create Tables
-- =============================================

-- Users table
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Role INT NOT NULL DEFAULT 3, -- UserRole enum: Admin=0, ProjectManager=1, Developer=2, Viewer=3
    Department NVARCHAR(100) NULL,
    JobTitle NVARCHAR(100) NULL,
    Manager NVARCHAR(255) NULL,
    TimeZone NVARCHAR(50) NOT NULL DEFAULT 'UTC',
    ProfilePicture NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2(7) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    DeletedAt DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

-- Roles table
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL
);

-- Permissions table
CREATE TABLE Permissions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) UNIQUE NOT NULL,
    Description NVARCHAR(500) NULL,
    Category NVARCHAR(50) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE()
);

-- Workspaces table
CREATE TABLE Workspaces (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    DeletedAt DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- Projects table
CREATE TABLE Projects (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    WorkspaceId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    DeletedAt DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- Environments table
CREATE TABLE Environments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ProjectId INT NOT NULL,
    Configuration NVARCHAR(MAX) NULL, -- JSON configuration
    Variables NVARCHAR(MAX) NULL, -- JSON key-value pairs
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    DeletedAt DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- ApiEndpoints table
CREATE TABLE ApiEndpoints (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Url NVARCHAR(2000) NOT NULL,
    Method NVARCHAR(10) NOT NULL, -- GET, POST, PUT, DELETE, etc.
    Type INT NOT NULL DEFAULT 0, -- ApiEndpointType enum: REST=0, SOAP=1
    Headers NVARCHAR(MAX) NULL, -- JSON headers
    AuthenticationMethod NVARCHAR(100) NULL,
    Documentation NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    DeletedAt DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- ApiParameters table
CREATE TABLE ApiParameters (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ApiEndpointId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Type INT NOT NULL DEFAULT 0, -- ParameterType enum
    IsRequired BIT NOT NULL DEFAULT 0,
    DefaultValue NVARCHAR(500) NULL,
    ValidationRules NVARCHAR(1000) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    FOREIGN KEY (ApiEndpointId) REFERENCES ApiEndpoints(Id) ON DELETE CASCADE
);

-- Workflows table
CREATE TABLE Workflows (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ProjectId INT NOT NULL,
    EnvironmentId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- WorkflowStatus enum: Draft=0, Published=1, Inactive=2
    Version INT NOT NULL DEFAULT 1,
    RequiresApproval BIT NOT NULL DEFAULT 0,
    Tags NVARCHAR(500) NULL,
    Configuration NVARCHAR(MAX) NULL, -- JSON configuration
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    DeletedAt DATETIME2(7) NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
    FOREIGN KEY (EnvironmentId) REFERENCES Environments(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- WorkflowVersions table
CREATE TABLE WorkflowVersions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    VersionNumber INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Configuration NVARCHAR(MAX) NULL, -- JSON configuration
    CreatedByUserId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

-- WorkflowSteps table
CREATE TABLE WorkflowSteps (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    [Order] INT NOT NULL,
    ApiEndpointId INT NOT NULL,
    InputMapping NVARCHAR(MAX) NULL, -- JSON mapping
    OutputMapping NVARCHAR(MAX) NULL, -- JSON mapping
    ErrorHandling NVARCHAR(MAX) NULL, -- JSON error handling config
    IsOptional BIT NOT NULL DEFAULT 0,
    TimeoutSeconds INT NOT NULL DEFAULT 30,
    RetryCount INT NOT NULL DEFAULT 0,
    RetryDelaySeconds INT NOT NULL DEFAULT 1,
    Condition NVARCHAR(1000) NULL, -- Conditional execution logic
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE,
    FOREIGN KEY (ApiEndpointId) REFERENCES ApiEndpoints(Id)
);

-- WorkflowSchedules table
CREATE TABLE WorkflowSchedules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL UNIQUE,
    CronExpression NVARCHAR(100) NOT NULL,
    TimeZone NVARCHAR(50) NOT NULL DEFAULT 'UTC',
    StartDate DATETIME2(7) NULL,
    EndDate DATETIME2(7) NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    NextRunTime DATETIME2(7) NULL,
    LastRunTime DATETIME2(7) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2(7) NULL,
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE
);

-- WorkflowExecutions table
CREATE TABLE WorkflowExecutions (
    Id NVARCHAR(50) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    Status INT NOT NULL DEFAULT 0, -- ExecutionStatus enum: Running=0, Completed=1, Failed=2, Cancelled=3
    ExecutedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2(7) NULL,
    InputParameters NVARCHAR(MAX) NULL, -- JSON input parameters
    OutputParameters NVARCHAR(MAX) NULL, -- JSON output parameters
    ErrorMessage NVARCHAR(MAX) NULL,
    ExecutionContext NVARCHAR(500) NULL,
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id)
);

-- WorkflowApprovals table
CREATE TABLE WorkflowApprovals (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    RequestedByUserId INT NOT NULL,
    ApproverUserId INT NULL,
    Status INT NOT NULL DEFAULT 0, -- ApprovalStatus enum: Pending=0, Approved=1, Rejected=2, Cancelled=3
    ApprovalType NVARCHAR(50) NOT NULL, -- execution, publish, update, etc.
    RequestReason NVARCHAR(1000) NULL,
    ApprovalComments NVARCHAR(1000) NULL,
    RequestedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    ApprovedAt DATETIME2(7) NULL,
    Metadata NVARCHAR(MAX) NULL, -- JSON metadata
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id),
    FOREIGN KEY (RequestedByUserId) REFERENCES Users(Id),
    FOREIGN KEY (ApproverUserId) REFERENCES Users(Id)
);

-- ExecutionLogs table
CREATE TABLE ExecutionLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ExecutionId NVARCHAR(50) NOT NULL,
    StepId INT NULL,
    LogLevel NVARCHAR(20) NOT NULL, -- Info, Warning, Error, Debug
    Message NVARCHAR(MAX) NOT NULL,
    Details NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (ExecutionId) REFERENCES WorkflowExecutions(Id),
    FOREIGN KEY (StepId) REFERENCES WorkflowSteps(Id)
);

-- WorkflowMetrics table
CREATE TABLE WorkflowMetrics (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    MetricName NVARCHAR(100) NOT NULL,
    MetricValue FLOAT NOT NULL,
    Metadata NVARCHAR(MAX) NULL, -- JSON metadata
    RecordedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id)
);

-- UserWorkspaces table (Many-to-many relationship)
CREATE TABLE UserWorkspaces (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    WorkspaceId INT NOT NULL,
    Role INT NOT NULL DEFAULT 3, -- UserRole in workspace
    JoinedAt DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (WorkspaceId) REFERENCES Workspaces(Id),
    UNIQUE(UserId, WorkspaceId)
);

-- =============================================
-- Create Indexes for Performance
-- =============================================

-- User indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Role ON Users(Role);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);

-- Workspace indexes
CREATE INDEX IX_Workspaces_CreatedByUserId ON Workspaces(CreatedByUserId);
CREATE INDEX IX_Workspaces_IsActive ON Workspaces(IsActive);

-- Project indexes
CREATE INDEX IX_Projects_WorkspaceId ON Projects(WorkspaceId);
CREATE INDEX IX_Projects_IsActive ON Projects(IsActive);
CREATE INDEX IX_Projects_CreatedByUserId ON Projects(CreatedByUserId);

-- Environment indexes
CREATE INDEX IX_Environments_ProjectId ON Environments(ProjectId);
CREATE INDEX IX_Environments_IsActive ON Environments(IsActive);

-- ApiEndpoint indexes
CREATE INDEX IX_ApiEndpoints_Type ON ApiEndpoints(Type);
CREATE INDEX IX_ApiEndpoints_IsActive ON ApiEndpoints(IsActive);
CREATE INDEX IX_ApiEndpoints_CreatedByUserId ON ApiEndpoints(CreatedByUserId);

-- Workflow indexes
CREATE INDEX IX_Workflows_ProjectId ON Workflows(ProjectId);
CREATE INDEX IX_Workflows_EnvironmentId ON Workflows(EnvironmentId);
CREATE INDEX IX_Workflows_Status ON Workflows(Status);
CREATE INDEX IX_Workflows_CreatedByUserId ON Workflows(CreatedByUserId);
CREATE INDEX IX_Workflows_CreatedAt ON Workflows(CreatedAt);

-- WorkflowStep indexes
CREATE INDEX IX_WorkflowSteps_WorkflowId ON WorkflowSteps(WorkflowId);
CREATE INDEX IX_WorkflowSteps_ApiEndpointId ON WorkflowSteps(ApiEndpointId);
CREATE INDEX IX_WorkflowSteps_Order ON WorkflowSteps([Order]);

-- WorkflowExecution indexes
CREATE INDEX IX_WorkflowExecutions_WorkflowId ON WorkflowExecutions(WorkflowId);
CREATE INDEX IX_WorkflowExecutions_Status ON WorkflowExecutions(Status);
CREATE INDEX IX_WorkflowExecutions_ExecutedAt ON WorkflowExecutions(ExecutedAt);
CREATE INDEX IX_WorkflowExecutions_CompletedAt ON WorkflowExecutions(CompletedAt);

-- WorkflowSchedule indexes
CREATE INDEX IX_WorkflowSchedules_IsActive ON WorkflowSchedules(IsActive);
CREATE INDEX IX_WorkflowSchedules_NextRunTime ON WorkflowSchedules(NextRunTime);

-- WorkflowApproval indexes
CREATE INDEX IX_WorkflowApprovals_WorkflowId ON WorkflowApprovals(WorkflowId);
CREATE INDEX IX_WorkflowApprovals_Status ON WorkflowApprovals(Status);
CREATE INDEX IX_WorkflowApprovals_RequestedByUserId ON WorkflowApprovals(RequestedByUserId);
CREATE INDEX IX_WorkflowApprovals_ApproverUserId ON WorkflowApprovals(ApproverUserId);

-- ExecutionLog indexes
CREATE INDEX IX_ExecutionLogs_ExecutionId ON ExecutionLogs(ExecutionId);
CREATE INDEX IX_ExecutionLogs_LogLevel ON ExecutionLogs(LogLevel);
CREATE INDEX IX_ExecutionLogs_CreatedAt ON ExecutionLogs(CreatedAt);

-- WorkflowMetrics indexes
CREATE INDEX IX_WorkflowMetrics_WorkflowId ON WorkflowMetrics(WorkflowId);
CREATE INDEX IX_WorkflowMetrics_MetricName ON WorkflowMetrics(MetricName);
CREATE INDEX IX_WorkflowMetrics_RecordedAt ON WorkflowMetrics(RecordedAt);

-- UserWorkspace indexes
CREATE INDEX IX_UserWorkspaces_UserId ON UserWorkspaces(UserId);
CREATE INDEX IX_UserWorkspaces_WorkspaceId ON UserWorkspaces(WorkspaceId);
CREATE INDEX IX_UserWorkspaces_IsActive ON UserWorkspaces(IsActive);

-- =============================================
-- Create Views for Common Queries
-- =============================================

-- Active workflows with project and environment info
CREATE VIEW vw_ActiveWorkflows AS
SELECT 
    w.Id,
    w.Name,
    w.Description,
    w.Status,
    w.Version,
    w.RequiresApproval,
    w.Tags,
    p.Name AS ProjectName,
    e.Name AS EnvironmentName,
    ws.Name AS WorkspaceName,
    u.FirstName + ' ' + u.LastName AS CreatedBy,
    w.CreatedAt,
    w.UpdatedAt
FROM Workflows w
INNER JOIN Projects p ON w.ProjectId = p.Id
INNER JOIN Environments e ON w.EnvironmentId = e.Id
INNER JOIN Workspaces ws ON p.WorkspaceId = ws.Id
INNER JOIN Users u ON w.CreatedByUserId = u.Id
WHERE w.IsDeleted = 0 AND p.IsDeleted = 0 AND e.IsDeleted = 0 AND ws.IsDeleted = 0;

-- Workflow execution summary
CREATE VIEW vw_WorkflowExecutionSummary AS
SELECT 
    w.Id AS WorkflowId,
    w.Name AS WorkflowName,
    COUNT(we.Id) AS TotalExecutions,
    SUM(CASE WHEN we.Status = 1 THEN 1 ELSE 0 END) AS SuccessfulExecutions,
    SUM(CASE WHEN we.Status = 2 THEN 1 ELSE 0 END) AS FailedExecutions,
    SUM(CASE WHEN we.Status = 0 THEN 1 ELSE 0 END) AS RunningExecutions,
    CAST(SUM(CASE WHEN we.Status = 1 THEN 1 ELSE 0 END) AS FLOAT) / NULLIF(COUNT(we.Id), 0) * 100 AS SuccessRate,
    MAX(we.ExecutedAt) AS LastExecutionAt,
    AVG(CASE WHEN we.CompletedAt IS NOT NULL THEN DATEDIFF(SECOND, we.ExecutedAt, we.CompletedAt) ELSE NULL END) AS AvgExecutionTimeSeconds
FROM Workflows w
LEFT JOIN WorkflowExecutions we ON w.Id = we.WorkflowId
WHERE w.IsDeleted = 0
GROUP BY w.Id, w.Name;

GO

PRINT 'Database tables, indexes, and views created successfully!';