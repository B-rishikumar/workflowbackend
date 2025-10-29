using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WorkflowManagement.Application.DTOs.Common;
using System.Text.Json;

namespace WorkflowManagement.API.Filters
{
    /// <summary>
    /// Validation filter to handle model state validation and return consistent error responses
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly ILogger<ValidationFilter> _logger;

        public ValidationFilter(ILogger<ValidationFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                // Check model state before action execution
                if (!context.ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation failed for action {Action}. Errors: {Errors}",
                        context.ActionDescriptor.DisplayName,
                        JsonSerializer.Serialize(GetValidationErrors(context.ModelState)));

                    var errorResponse = CreateValidationErrorResponse(context.ModelState);
                    context.Result = new BadRequestObjectResult(errorResponse);
                    return;
                }

                // Validate action parameters
                var parameterValidationResult = ValidateActionParameters(context);
                if (!parameterValidationResult.IsValid)
                {
                    _logger.LogWarning("Parameter validation failed for action {Action}. Errors: {Errors}",
                        context.ActionDescriptor.DisplayName,
                        JsonSerializer.Serialize(parameterValidationResult.Errors));

                    var errorResponse = ResponseDto<object>.Failure("Validation failed", parameterValidationResult.Errors);
                    context.Result = new BadRequestObjectResult(errorResponse);
                    return;
                }

                // Execute the action
                var executedContext = await next();

                // Post-action validation (if needed)
                await OnActionExecutedAsync(executedContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in validation filter for action {Action}", 
                    context.ActionDescriptor.DisplayName);

                var errorResponse = ResponseDto<object>.Failure("An error occurred during validation");
                context.Result = new StatusCodeResult(500);
            }
        }

        private static ResponseDto<object> CreateValidationErrorResponse(ModelStateDictionary modelState)
        {
            var errors = GetValidationErrors(modelState);
            var errorMessage = "One or more validation errors occurred";

            return ResponseDto<object>.Failure(errorMessage, errors);
        }

        private static Dictionary<string, List<string>> GetValidationErrors(ModelStateDictionary modelState)
        {
            var errors = new Dictionary<string, List<string>>();

            foreach (var kvp in modelState)
            {
                var key = kvp.Key;
                var modelStateEntry = kvp.Value;

                if (modelStateEntry.Errors.Count > 0)
                {
                    var errorMessages = modelStateEntry.Errors
                        .Select(error => !string.IsNullOrEmpty(error.ErrorMessage) 
                            ? error.ErrorMessage 
                            : error.Exception?.Message ?? "Unknown error")
                        .Where(msg => !string.IsNullOrWhiteSpace(msg))
                        .ToList();

                    if (errorMessages.Any())
                    {
                        errors[key] = errorMessages;
                    }
                }
            }

            return errors;
        }

        private ValidationResult ValidateActionParameters(ActionExecutingContext context)
        {
            var result = new ValidationResult { IsValid = true, Errors = new Dictionary<string, List<string>>() };

            foreach (var parameter in context.ActionArguments)
            {
                var parameterName = parameter.Key;
                var parameterValue = parameter.Value;

                // Validate required parameters
                if (parameterValue == null)
                {
                    var parameterInfo = context.ActionDescriptor.Parameters
                        .FirstOrDefault(p => p.Name == parameterName);

                    if (parameterInfo != null && !IsNullableType(parameterInfo.ParameterType))
                    {
                        AddValidationError(result, parameterName, "Parameter is required");
                    }
                    continue;
                }

                // Validate specific parameter types
                var validationErrors = ValidateParameter(parameterName, parameterValue);
                foreach (var error in validationErrors)
                {
                    AddValidationError(result, error.Key, error.Value);
                }
            }

            return result;
        }

        private static Dictionary<string, string> ValidateParameter(string parameterName, object parameterValue)
        {
            var errors = new Dictionary<string, string>();

            switch (parameterValue)
            {
                case int intValue when intValue < 0 && parameterName.ToLower().Contains("id"):
                    errors[parameterName] = "ID values must be positive";
                    break;

                case string stringValue:
                    ValidateStringParameter(parameterName, stringValue, errors);
                    break;

                case DateTime dateValue when dateValue == default:
                    errors[parameterName] = "Invalid date value";
                    break;

                // Add more type-specific validations as needed
            }

            return errors;
        }

        private static void ValidateStringParameter(string parameterName, string stringValue, Dictionary<string, string> errors)
        {
            var lowerParamName = parameterName.ToLower();

            // Email validation
            if (lowerParamName.Contains("email") && !IsValidEmail(stringValue))
            {
                errors[parameterName] = "Invalid email format";
            }

            // URL validation
            if (lowerParamName.Contains("url") && !IsValidUrl(stringValue))
            {
                errors[parameterName] = "Invalid URL format";
            }

            // JSON validation
            if (lowerParamName.Contains("json") && !IsValidJson(stringValue))
            {
                errors[parameterName] = "Invalid JSON format";
            }

            // Cron expression validation (basic)
            if (lowerParamName.Contains("cron") && !IsValidCronExpression(stringValue))
            {
                errors[parameterName] = "Invalid cron expression format";
            }

            // String length validations
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                if (lowerParamName.Contains("name") || lowerParamName.Contains("title"))
                {
                    errors[parameterName] = "Name/Title cannot be empty";
                }
            }
            else
            {
                if (stringValue.Length > 500 && lowerParamName.Contains("name"))
                {
                    errors[parameterName] = "Name cannot exceed 500 characters";
                }

                if (stringValue.Length > 2000 && lowerParamName.Contains("description"))
                {
                    errors[parameterName] = "Description cannot exceed 2000 characters";
                }
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private static bool IsValidJson(string json)
        {
            try
            {
                JsonDocument.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidCronExpression(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return false;

            var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Basic validation - cron should have 5 or 6 parts
            return parts.Length >= 5 && parts.Length <= 6;
        }

        private static bool IsNullableType(Type type)
        {
            return !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);
        }

        private static void AddValidationError(ValidationResult result, string key, string errorMessage)
        {
            result.IsValid = false;
            
            if (!result.Errors.ContainsKey(key))
            {
                result.Errors[key] = new List<string>();
            }
            
            result.Errors[key].Add(errorMessage);
        }

        private async Task OnActionExecutedAsync(ActionExecutedContext context)
        {
            // Post-action validation logic can be added here
            // For example, validate response data format, etc.
            
            if (context.Result is ObjectResult objectResult)
            {
                // Validate response structure
                await ValidateResponseAsync(objectResult, context);
            }
        }

        private async Task ValidateResponseAsync(ObjectResult objectResult, ActionExecutedContext context)
        {
            try
            {
                // Validate that responses follow the standard ResponseDto format
                if (objectResult.Value != null)
                {
                    var responseType = objectResult.Value.GetType();
                    
                    // Check if response is wrapped in ResponseDto
                    if (!IsResponseDtoType(responseType) && !IsBuiltInType(responseType))
                    {
                        _logger.LogWarning("Action {Action} returned unwrapped response of type {Type}",
                            context.ActionDescriptor.DisplayName, responseType.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating response for action {Action}", 
                    context.ActionDescriptor.DisplayName);
            }
        }

        private static bool IsResponseDtoType(Type type)
        {
            // Check if type is ResponseDto<T> or inherits from it
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return genericTypeDefinition == typeof(ResponseDto<>);
            }

            return type.Name.StartsWith("ResponseDto");
        }

        private static bool IsBuiltInType(Type type)
        {
            // Allow certain built-in types to pass through without wrapping
            return type.IsPrimitive || 
                   type == typeof(string) || 
                   type == typeof(DateTime) || 
                   type == typeof(decimal) || 
                   type == typeof(Guid) ||
                   type.IsEnum;
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public Dictionary<string, List<string>> Errors { get; set; } = new();
        }
    }

    /// <summary>
    /// Attribute to skip validation for specific actions
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SkipValidationAttribute : Attribute
    {
        public string[] SkipTypes { get; }

        public SkipValidationAttribute(params string[] skipTypes)
        {
            SkipTypes = skipTypes ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Attribute to specify custom validation rules for an action
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomValidationAttribute : Attribute
    {
        public Type ValidatorType { get; }
        public string ValidationMethod { get; }

        public CustomValidationAttribute(Type validatorType, string validationMethod = "Validate")
        {
            ValidatorType = validatorType;
            ValidationMethod = validationMethod;
        }
    }
}