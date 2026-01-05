using FluentValidation;

namespace Kidzgo.Application.Programs.CreateProgram;

public sealed class CreateProgramCommandValidator : AbstractValidator<CreateProgramCommand>
{
    public CreateProgramCommandValidator()
    {
        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Program name is required")
            .MaximumLength(255).WithMessage("Program name must not exceed 255 characters");

        RuleFor(command => command.Level)
            .MaximumLength(100).WithMessage("Level must not exceed 100 characters")
            .When(command => !string.IsNullOrEmpty(command.Level));

        RuleFor(command => command.TotalSessions)
            .GreaterThan(0).WithMessage("Total sessions must be greater than 0");

        RuleFor(command => command.DefaultTuitionAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Default tuition amount must be greater than or equal to 0");

        RuleFor(command => command.UnitPriceSession)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price per session must be greater than or equal to 0");
    }
}

