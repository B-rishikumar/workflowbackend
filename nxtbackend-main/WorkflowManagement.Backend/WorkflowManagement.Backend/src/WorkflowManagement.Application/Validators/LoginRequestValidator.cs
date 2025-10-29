// LoginRequestValidator.cs
using FluentValidation;
using WorkflowManagement.Application.DTOs.Auth;

namespace WorkflowManagement.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters");
    }
}