using WorkflowManagement.Core.Entities;
//using WorkflowManagement.Application.DTOs.Common;

namespace WorkflowManagement.Core.Interfaces.Services
{
    /// <summary>
    /// Interface for handling HTTP requests to external API endpoints
    /// Supports both REST and SOAP operations during workflow execution
    /// </summary>
    public interface IRequestHandler
    {
        /// <summary>
        /// Execute a REST API request
        /// </summary>
        /// <param name="endpoint">The API endpoint configuration</param>
        /// <param name="inputParameters">Dynamic input parameters from previous workflow steps</param>
        /// <param name="headers">Additional headers for the request</param>
        /// <param name="cancellationToken">Cancellation token for request timeout</param>
        /// <returns>Response containing the API result</returns>
        Task<RequestExecutionResult> ExecuteRestRequestAsync(
            ApiEndpoint endpoint, 
            Dictionary<string, object> inputParameters,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a SOAP API request
        /// </summary>
        /// <param name="endpoint">The API endpoint configuration</param>
        /// <param name="inputParameters">Dynamic input parameters from previous workflow steps</param>
        /// <param name="headers">Additional headers for the request</param>
        /// <param name="cancellationToken">Cancellation token for request timeout</param>
        /// <returns>Response containing the API result</returns>
        Task<RequestExecutionResult> ExecuteSoapRequestAsync(
            ApiEndpoint endpoint,
            Dictionary<string, object> inputParameters,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validate request parameters against endpoint configuration
        /// </summary>
        /// <param name="endpoint">The API endpoint configuration</param>
        /// <param name="inputParameters">Parameters to validate</param>
        /// <returns>Validation result with any errors</returns>
        Task<ValidationResult> ValidateRequestParametersAsync(
            ApiEndpoint endpoint,
            Dictionary<string, object> inputParameters);

        /// <summary>
        /// Transform input parameters based on endpoint parameter mapping
        /// </summary>
        /// <param name="endpoint">The API endpoint configuration</param>
        /// <param name="inputParameters">Raw input parameters</param>
        /// <returns>Transformed parameters ready for API call</returns>
        Task<Dictionary<string, object>> TransformInputParametersAsync(
            ApiEndpoint endpoint,
            Dictionary<string, object> inputParameters);

        /// <summary>
        /// Extract and transform output parameters from API response
        /// </summary>
        /// <param name="endpoint">The API endpoint configuration</param>
        /// <param name="response">Raw API response</param>
        /// <returns>Extracted output parameters for next workflow step</returns>
        Task<Dictionary<string, object>> ExtractOutputParametersAsync(
            ApiEndpoint endpoint,
            RequestExecutionResult response);

        /// <summary>
        /// Test connection to an API endpoint
        /// </summary>
        /// <param name="endpoint">The API endpoint to test</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Connection test result</returns>
        Task<ConnectionTestResult> TestConnectionAsync(
            ApiEndpoint endpoint,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute request with retry policy for failed requests
        /// </summary>
        /// <param name="endpoint">The API endpoint configuration</param>
        /// <param name="inputParameters">Dynamic input parameters</param>
        /// <param name="maxRetries">Maximum number of retry attempts</param>
        /// <param name="retryDelay">Delay between retry attempts</param>
        /// <param name="headers">Additional headers</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response containing the API result</returns>
        Task<RequestExecutionResult> ExecuteWithRetryAsync(
            ApiEndpoint endpoint,
            Dictionary<string, object> inputParameters,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            Dictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute multiple requests in parallel (for batch operations)
        /// </summary>
        /// <param name="requests">Collection of request configurations</param>
        /// <param name="maxConcurrency">Maximum number of concurrent requests</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of execution results</returns>
        Task<IEnumerable<RequestExecutionResult>> ExecuteBatchRequestsAsync(
            IEnumerable<BatchRequestItem> requests,
            int maxConcurrency = 5,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of a request execution
    /// </summary>
    public class RequestExecutionResult
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Content { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public Dictionary<string, object> OutputParameters { get; set; } = new();
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public string? RequestId { get; set; }
    }

    /// <summary>
    /// Result of parameter validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Result of connection test
    /// </summary>
    public class ConnectionTestResult
    {
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public int? StatusCode { get; set; }
        public DateTime TestedAt { get; set; } = DateTime.UtcNow;
        public Dictionary<string, string> AdditionalInfo { get; set; } = new();
    }

    /// <summary>
    /// Batch request item for parallel execution
    /// </summary>
    public class BatchRequestItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ApiEndpoint Endpoint { get; set; } = null!;
        public Dictionary<string, object> InputParameters { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
        public int Priority { get; set; } = 0;
    }
}