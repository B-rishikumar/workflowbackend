// CreateApiEndpointValidator.cs
using FluentValidation;
using WorkflowManagement.Application.DTOs.ApiEndpoint;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Validators;

public class CreateApiEndpointValidator : AbstractValidator<CreateApiEndpointDto>
{
    private readonly IApiEndpointRepository _apiEndpointRepository;

    public CreateApiEndpointValidator(IApiEndpointRepository apiEndpointRepository)
    {
        _apiEndpointRepository = apiEndpointRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("API endpoint name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required")
            .Must(BeValidUrl).WithMessage("Invalid URL format");

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0).WithMessage("Timeout must be greater than 0")
            .LessThanOrEqualTo(3600).WithMessage("Timeout cannot exceed 3600 seconds (1 hour)");

        RuleFor(x => x.RequestContentType)
            .Must(BeValidContentType).WithMessage("Invalid content type")
            .When(x => !string.IsNullOrEmpty(x.RequestContentType));

        RuleFor(x => x)
            .MustAsync(BeUniqueEndpoint).WithMessage("An endpoint with this URL and method already exists");

        RuleForEach(x => x.Parameters)
            .SetValidator(new CreateApiParameterValidator());
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private bool BeValidContentType(string contentType)
    {
        var validContentTypes = new[]
        {
            "application/json",
            "application/xml",
            "text/xml",
            "application/x-www-form-urlencoded",
            "multipart/form-data",
            "text/plain"
        };

        return validContentTypes.Contains(contentType.ToLowerInvariant());
    }

    private async Task<bool> BeUniqueEndpoint(CreateApiEndpointDto dto, CancellationToken cancellationToken)
    {
        return !await _apiEndpointRepository.UrlExistsAsync(dto.Url, dto.Method, cancellationToken: cancellationToken);
    }
}
