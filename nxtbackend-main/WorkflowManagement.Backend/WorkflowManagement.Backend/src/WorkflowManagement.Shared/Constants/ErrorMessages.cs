namespace WorkflowManagement.Shared.Constants;

public static class ErrorMessages
{
    // Authentication & Authorization
    public const string InvalidCredentials = "Invalid username or password.";
    public const string UserNotFound = "User not found.";
    public const string UserAlreadyExists = "User already exists.";
    public const string EmailAlreadyExists = "Email address already exists.";
    public const string UsernameAlreadyExists = "Username already exists.";
    public const string UnauthorizedAccess = "Unauthorized access.";
    public const string TokenExpired = "Token has expired.";
    public const string InvalidToken = "Invalid token.";
    public const string RefreshTokenExpired = "Refresh token has expired.";

    // Workflow Management
    public const string WorkflowNotFound = "Workflow not found.";
    public const string WorkflowAlreadyPublished = "Workflow is already published.";
    public const string WorkflowNotPublished = "Workflow is not published.";
    public const string WorkflowExecutionNotFound = "Workflow execution not found.";
    public const string WorkflowExecutionAlreadyRunning = "Workflow execution is already running.";
    public const string WorkflowValidationFailed = "Workflow validation failed.";
    public const string WorkflowStepNotFound = "Workflow step not found.";

    // API Endpoints
    public const string ApiEndpointNotFound = "API endpoint not found.";
    public const string ApiEndpointAlreadyExists = "API endpoint already exists.";
    public const string ApiEndpointTestFailed = "API endpoint test failed.";
    public const string InvalidSwaggerDefinition = "Invalid Swagger definition.";
    public const string InvalidWsdlDefinition = "Invalid WSDL definition.";

    // General
    public const string ValidationFailed = "Validation failed.";
    public const string InternalServerError = "An internal server error occurred.";
    public const string NotFound = "Resource not found.";
    public const string BadRequest = "Bad request.";
    public const string ConcurrencyConflict = "The resource has been modified by another user.";
}