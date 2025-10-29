// CreateWorkflowValidator.cs
using FluentValidation;
using WorkflowManagement.Application.DTOs.Workflow;
using WorkflowManagement.Core.Interfaces.Repositories;

namespace WorkflowManagement.Application.Validators;

public class CreateWorkflowValidator : AbstractValidator<CreateWorkflowDto>
{
    private readonly IEnvironmentRepository _environmentRepository;

    public CreateWorkflowValidator(IEnvironmentRepository environmentRepository)
    {
        _environmentRepository = environmentRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workflow name is required")
            .MaximumLength(200).WithMessage("Workflow name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.EnvironmentId)
            .NotEmpty().WithMessage("Environment is required")
            .MustAsync(EnvironmentExists).WithMessage("Environment does not exist");

        RuleFor(x => x.TimeoutMinutes)
            .GreaterThan(0).WithMessage("Timeout must be greater than 0")
            .LessThanOrEqualTo(1440).WithMessage("Timeout cannot exceed 1440 minutes (24 hours)");

        RuleFor(x => x.RetryCount)
            .GreaterThanOrEqualTo(0).WithMessage("Retry count cannot be negative")
            .LessThanOrEqualTo(10).WithMessage("Retry count cannot exceed 10");
    }

    private async Task<bool> EnvironmentExists(Guid environmentId, CancellationToken cancellationToken)
    {
        var environment = await _environmentRepository.GetByIdAsync(environmentId, cancellationToken);
        return environment != null && environment.IsActive;
    }
}