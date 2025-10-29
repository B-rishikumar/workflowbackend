using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowManagement.Core.Interfaces.Services;
using WorkflowManagement.Application.DTOs.ApiEndpoint;
using WorkflowManagement.Application.DTOs.Common;
using WorkflowManagement.Core.Enums;
using WorkflowManagement.API.Filters;

namespace WorkflowManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApiEndpointsController : ControllerBase
    {
        private readonly IApiEndpointService _apiEndpointService;
        private readonly ISwaggerParserService _swaggerParserService;
        private readonly ISoapParserService _soapParserService;
        private readonly ILogger<ApiEndpointsController> _logger;

        public ApiEndpointsController(
            IApiEndpointService apiEndpointService,
            ISwaggerParserService swaggerParserService,
            ISoapParserService soapParserService,
            ILogger<ApiEndpointsController> logger)
        {
            _apiEndpointService = apiEndpointService;
            _swaggerParserService = swaggerParserService;
            _soapParserService = soapParserService;
            _logger = logger;
        }

        /// <summary>
        /// Get all API endpoints with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ResponseDto<PagedResultDto<ApiEndpointDto>>>> GetApiEndpoints(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] ApiEndpointType? type = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true)
        {
            try
            {
                var result = await _apiEndpointService.GetApiEndpointsAsync(page, pageSize, searchTerm, type, isActive, sortBy, sortDescending);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API endpoints");
                return StatusCode(500, ResponseDto<PagedResultDto<ApiEndpointDto>>.Failure("An error occurred while retrieving API endpoints"));
            }
        }

        /// <summary>
        /// Get API endpoint by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<ApiEndpointDto>>> GetApiEndpoint(int id)
        {
            try
            {
                var result = await _apiEndpointService.GetApiEndpointByIdAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API endpoint {EndpointId}", id);
                return StatusCode(500, ResponseDto<ApiEndpointDto>.Failure("An error occurred while retrieving the API endpoint"));
            }
        }

        /// <summary>
        /// Get API endpoint with detailed information
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ResponseDto<ApiEndpointDetailDto>>> GetApiEndpointDetail(int id)
        {
            try
            {
                var result = await _apiEndpointService.GetApiEndpointDetailAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API endpoint detail {EndpointId}", id);
                return StatusCode(500, ResponseDto<ApiEndpointDetailDto>.Failure("An error occurred while retrieving API endpoint details"));
            }
        }

        /// <summary>
        /// Get active API endpoints for dropdown lists
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<ResponseDto<List<ApiEndpointDto>>>> GetActiveApiEndpoints([FromQuery] ApiEndpointType? type = null)
        {
            try
            {
                var result = await _apiEndpointService.GetActiveApiEndpointsAsync(type);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active API endpoints");
                return StatusCode(500, ResponseDto<List<ApiEndpointDto>>.Failure("An error occurred while retrieving active API endpoints"));
            }
        }

        /// <summary>
        /// Create a new API endpoint
        /// </summary>
        [HttpPost]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<ApiEndpointDto>>> CreateApiEndpoint([FromBody] CreateApiEndpointDto createEndpointDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var result = await _apiEndpointService.CreateApiEndpointAsync(createEndpointDto, currentUserId);
                
                if (result.Success)
                    return CreatedAtAction(nameof(GetApiEndpoint), new { id = result.Data?.Id }, result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API endpoint");
                return StatusCode(500, ResponseDto<ApiEndpointDto>.Failure("An error occurred while creating the API endpoint"));
            }
        }

        /// <summary>
        /// Update API endpoint information
        /// </summary>
        [HttpPut("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<ApiEndpointDto>>> UpdateApiEndpoint(int id, [FromBody] UpdateApiEndpointDto updateEndpointDto)
        {
            try
            {
                var result = await _apiEndpointService.UpdateApiEndpointAsync(id, updateEndpointDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API endpoint {EndpointId}", id);
                return StatusCode(500, ResponseDto<ApiEndpointDto>.Failure("An error occurred while updating the API endpoint"));
            }
        }

        /// <summary>
        /// Delete API endpoint (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager)]
        public async Task<ActionResult<ResponseDto<bool>>> DeleteApiEndpoint(int id)
        {
            try
            {
                var result = await _apiEndpointService.DeleteApiEndpointAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting API endpoint {EndpointId}", id);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while deleting the API endpoint"));
            }
        }

        /// <summary>
        /// Test API endpoint connectivity
        /// </summary>
        [HttpPost("{id}/test")]
        public async Task<ActionResult<ResponseDto<ApiEndpointTestResultDto>>> TestApiEndpoint(int id)
        {
            try
            {
                var result = await _apiEndpointService.TestApiEndpointAsync(id);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing API endpoint {EndpointId}", id);
                return StatusCode(500, ResponseDto<ApiEndpointTestResultDto>.Failure("An error occurred while testing the API endpoint"));
            }
        }

        /// <summary>
        /// Import API endpoints from Swagger specification
        /// </summary>
        [HttpPost("import/swagger")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<List<ApiEndpointDto>>>> ImportFromSwagger([FromBody] SwaggerImportDto swaggerImportDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                List<ApiEndpointDto> endpoints;

                if (!string.IsNullOrEmpty(swaggerImportDto.SwaggerUrl))
                {
                    // Import from URL
                    var parseResult = await _swaggerParserService.ParseFromUrlAsync(swaggerImportDto.SwaggerUrl);
                    if (!parseResult.Success || parseResult.Data == null)
                    {
                        return BadRequest(ResponseDto<List<ApiEndpointDto>>.Failure(parseResult.Message ?? "Failed to parse Swagger from URL"));
                    }
                    endpoints = parseResult.Data;
                }
                else if (!string.IsNullOrEmpty(swaggerImportDto.SwaggerContent))
                {
                    // Import from content
                    var parseResult = await _swaggerParserService.ParseFromContentAsync(swaggerImportDto.SwaggerContent);
                    if (!parseResult.Success || parseResult.Data == null)
                    {
                        return BadRequest(ResponseDto<List<ApiEndpointDto>>.Failure(parseResult.Message ?? "Failed to parse Swagger content"));
                    }
                    endpoints = parseResult.Data;
                }
                else
                {
                    return BadRequest(ResponseDto<List<ApiEndpointDto>>.Failure("Either SwaggerUrl or SwaggerContent must be provided"));
                }

                // Save the imported endpoints
                var saveResult = await _apiEndpointService.ImportApiEndpointsAsync(endpoints, currentUserId);
                
                if (saveResult.Success)
                    return Ok(saveResult);
                
                return BadRequest(saveResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing API endpoints from Swagger");
                return StatusCode(500, ResponseDto<List<ApiEndpointDto>>.Failure("An error occurred while importing from Swagger"));
            }
        }

        /// <summary>
        /// Import API endpoints from SOAP WSDL
        /// </summary>
        [HttpPost("import/soap")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<List<ApiEndpointDto>>>> ImportFromSoap([FromBody] SoapImportDto soapImportDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                List<ApiEndpointDto> endpoints;

                if (!string.IsNullOrEmpty(soapImportDto.WsdlUrl))
                {
                    // Import from URL
                    var parseResult = await _soapParserService.ParseFromUrlAsync(soapImportDto.WsdlUrl);
                    if (!parseResult.Success || parseResult.Data == null)
                    {
                        return BadRequest(ResponseDto<List<ApiEndpointDto>>.Failure(parseResult.Message ?? "Failed to parse WSDL from URL"));
                    }
                    endpoints = parseResult.Data;
                }
                else if (!string.IsNullOrEmpty(soapImportDto.WsdlContent))
                {
                    // Import from content
                    var parseResult = await _soapParserService.ParseFromContentAsync(soapImportDto.WsdlContent);
                    if (!parseResult.Success || parseResult.Data == null)
                    {
                        return BadRequest(ResponseDto<List<ApiEndpointDto>>.Failure(parseResult.Message ?? "Failed to parse WSDL content"));
                    }
                    endpoints = parseResult.Data;
                }
                else
                {
                    return BadRequest(ResponseDto<List<ApiEndpointDto>>.Failure("Either WsdlUrl or WsdlContent must be provided"));
                }

                // Save the imported endpoints
                var saveResult = await _apiEndpointService.ImportApiEndpointsAsync(endpoints, currentUserId);
                
                if (saveResult.Success)
                    return Ok(saveResult);
                
                return BadRequest(saveResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing API endpoints from SOAP");
                return StatusCode(500, ResponseDto<List<ApiEndpointDto>>.Failure("An error occurred while importing from SOAP"));
            }
        }

        /// <summary>
        /// Get API endpoint parameters
        /// </summary>
        [HttpGet("{endpointId}/parameters")]
        public async Task<ActionResult<ResponseDto<List<ApiParameterDto>>>> GetApiEndpointParameters(int endpointId)
        {
            try
            {
                var result = await _apiEndpointService.GetApiEndpointParametersAsync(endpointId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parameters for API endpoint {EndpointId}", endpointId);
                return StatusCode(500, ResponseDto<List<ApiParameterDto>>.Failure("An error occurred while retrieving endpoint parameters"));
            }
        }

        /// <summary>
        /// Add parameter to API endpoint
        /// </summary>
        [HttpPost("{endpointId}/parameters")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<ApiParameterDto>>> AddApiEndpointParameter(int endpointId, [FromBody] CreateApiParameterDto parameterDto)
        {
            try
            {
                var result = await _apiEndpointService.AddApiEndpointParameterAsync(endpointId, parameterDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding parameter to API endpoint {EndpointId}", endpointId);
                return StatusCode(500, ResponseDto<ApiParameterDto>.Failure("An error occurred while adding the parameter"));
            }
        }

        /// <summary>
        /// Update API endpoint parameter
        /// </summary>
        [HttpPut("{endpointId}/parameters/{parameterId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<ApiParameterDto>>> UpdateApiEndpointParameter(
            int endpointId, 
            int parameterId, 
            [FromBody] UpdateApiParameterDto parameterDto)
        {
            try
            {
                var result = await _apiEndpointService.UpdateApiEndpointParameterAsync(endpointId, parameterId, parameterDto);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parameter {ParameterId} for API endpoint {EndpointId}", parameterId, endpointId);
                return StatusCode(500, ResponseDto<ApiParameterDto>.Failure("An error occurred while updating the parameter"));
            }
        }

        /// <summary>
        /// Remove parameter from API endpoint
        /// </summary>
        [HttpDelete("{endpointId}/parameters/{parameterId}")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<bool>>> RemoveApiEndpointParameter(int endpointId, int parameterId)
        {
            try
            {
                var result = await _apiEndpointService.RemoveApiEndpointParameterAsync(endpointId, parameterId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing parameter {ParameterId} from API endpoint {EndpointId}", parameterId, endpointId);
                return StatusCode(500, ResponseDto<bool>.Failure("An error occurred while removing the parameter"));
            }
        }

        /// <summary>
        /// Execute API endpoint directly (for testing purposes)
        /// </summary>
        [HttpPost("{id}/execute")]
        [RequireRole(UserRole.Admin, UserRole.ProjectManager, UserRole.Developer)]
        public async Task<ActionResult<ResponseDto<ApiExecutionResultDto>>> ExecuteApiEndpoint(
            int id,
            [FromBody] ExecuteApiEndpointDto executeDto)
        {
            try
            {
                var result = await _apiEndpointService.ExecuteApiEndpointAsync(id, executeDto.Parameters, executeDto.Headers);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing API endpoint {EndpointId}", id);
                return StatusCode(500, ResponseDto<ApiExecutionResultDto>.Failure("An error occurred while executing the API endpoint"));
            }
        }

        /// <summary>
        /// Get API endpoint usage statistics
        /// </summary>
        [HttpGet("{endpointId}/statistics")]
        public async Task<ActionResult<ResponseDto<ApiEndpointStatisticsDto>>> GetApiEndpointStatistics(
            int endpointId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _apiEndpointService.GetApiEndpointStatisticsAsync(endpointId, startDate, endDate);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for API endpoint {EndpointId}", endpointId);
                return StatusCode(500, ResponseDto<ApiEndpointStatisticsDto>.Failure("An error occurred while retrieving endpoint statistics"));
            }
        }

        #region Helper Methods

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
        }

        #endregion
    }

    /// <summary>
    /// Supporting DTOs
    /// </summary>
    public class ExecuteApiEndpointDto
    {
        public Dictionary<string, object> Parameters { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
    }

    public class CreateApiParameterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ParameterType Type { get; set; }
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? ValidationRules { get; set; }
    }

    public class UpdateApiParameterDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ParameterType? Type { get; set; }
        public bool? IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? ValidationRules { get; set; }
    }

    public class ApiEndpointTestResultDto
    {
        public bool IsSuccessful { get; set; }
        public int? StatusCode { get; set; }
        public string? ResponseContent { get; set; }
        public TimeSpan ResponseTime { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; } = new();
        public DateTime TestedAt { get; set; }
    }

    public class ApiExecutionResultDto
    {
        public bool IsSuccessful { get; set; }
        public int StatusCode { get; set; }
        public string? ResponseContent { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; } = new();
        public TimeSpan ExecutionTime { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ExecutedAt { get; set; }
    }

    public class ApiEndpointStatisticsDto
    {
        public int EndpointId { get; set; }
        public string EndpointName { get; set; } = string.Empty;
        public int TotalCalls { get; set; }
        public int SuccessfulCalls { get; set; }
        public int FailedCalls { get; set; }
        public double SuccessRate { get; set; }
        public TimeSpan? AverageResponseTime { get; set; }
        public DateTime? LastCalledAt { get; set; }
        public Dictionary<int, int> StatusCodeDistribution { get; set; } = new();
        public Dictionary<DateTime, int> DailyCallTrend { get; set; } = new();
        public List<string> MostCommonErrors { get; set; } = new();
    }

    public class ApiEndpointDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public ApiEndpointType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedByUserName { get; set; }
        public List<ApiParameterDto> Parameters { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? AuthenticationMethod { get; set; }
        public string? Documentation { get; set; }
        public ApiEndpointStatisticsDto Statistics { get; set; } = new();
    }

    public class UpdateApiEndpointDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Method { get; set; }
        public bool? IsActive { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public string? AuthenticationMethod { get; set; }
        public string? Documentation { get; set; }
    }
}