// SwaggerImportValidator.cs
using FluentValidation;
using WorkflowManagement.Application.DTOs.ApiEndpoint;

namespace WorkflowManagement.Application.Validators;

public class SwaggerImportValidator : AbstractValidator<SwaggerImportDto>
{
    public SwaggerImportValidator()
    {
        RuleFor(x => x.SwaggerUrl)
            .NotEmpty().WithMessage("Swagger URL is required")
            .Must(BeValidUrl).WithMessage("Invalid Swagger URL format");

        RuleFor(x => x.BaseUrl)
            .Must(BeValidUrl).WithMessage("Invalid base URL format")
            .When(x => !string.IsNullOrEmpty(x.BaseUrl));
    }

    private bool BeValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
