// CreateApiParameterValidator.cs
using FluentValidation;
using WorkflowManagement.Application.DTOs.ApiEndpoint;

namespace WorkflowManagement.Application.Validators;

public class CreateApiParameterValidator : AbstractValidator<CreateApiParameterDto>
{
    public CreateApiParameterValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Parameter name is required")
            .MaximumLength(100).WithMessage("Parameter name cannot exceed 100 characters")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Parameter name can only contain letters, numbers, hyphens and underscores");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Location)
            .Must(BeValidLocation).WithMessage("Invalid parameter location")
            .When(x => !string.IsNullOrEmpty(x.Location));

        RuleFor(x => x.DefaultValue)
            .MaximumLength(1000).WithMessage("Default value cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.DefaultValue));
    }

    private bool BeValidLocation(string location)
    {
        var validLocations = new[] { "query", "header", "path", "body", "form" };
        return validLocations.Contains(location.ToLowerInvariant());
    }
}