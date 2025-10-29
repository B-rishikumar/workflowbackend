namespace WorkflowManagement.Shared.Constants;

public static class CacheKeys
{
    public const string UserPrefix = "user:";
    public const string WorkflowPrefix = "workflow:";
    public const string ApiEndpointPrefix = "api-endpoint:";
    public const string WorkflowExecutionPrefix = "execution:";
    public const string MetricsPrefix = "metrics:";

    public static string UserById(Guid userId) => $"{UserPrefix}{userId}";
    public static string UserByEmail(string email) => $"{UserPrefix}email:{email}";
    public static string UserByUsername(string username) => $"{UserPrefix}username:{username}";
    
    public static string WorkflowById(Guid workflowId) => $"{WorkflowPrefix}{workflowId}";
    public static string WorkflowsByEnvironment(Guid environmentId) => $"{WorkflowPrefix}env:{environmentId}";
    
    public static string ApiEndpointById(Guid endpointId) => $"{ApiEndpointPrefix}{endpointId}";
    
    public static string WorkflowExecutionById(Guid executionId) => $"{WorkflowExecutionPrefix}{executionId}";
    
    public static string DailyMetrics(Guid workflowId, DateTime date) => $"{MetricsPrefix}{workflowId}:{date:yyyy-MM-dd}";
}