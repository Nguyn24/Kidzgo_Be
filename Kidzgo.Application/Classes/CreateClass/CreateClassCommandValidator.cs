using FluentValidation;

namespace Kidzgo.Application.Classes.CreateClass;

public sealed class CreateClassCommandValidator : AbstractValidator<CreateClassCommand>
{
    public CreateClassCommandValidator()
    {
        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(command => command.ProgramId)
            .NotEmpty().WithMessage("Program ID is required");

        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("Class code is required")
            .MaximumLength(50).WithMessage("Class code must not exceed 50 characters");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Class title is required")
            .MaximumLength(255).WithMessage("Class title must not exceed 255 characters");

        RuleFor(command => command.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Start date cannot be in the past");

        RuleFor(command => command.EndDate)
            .NotNull().WithMessage("End date is required when schedule pattern is provided")
            .When(command => !string.IsNullOrWhiteSpace(command.SchedulePattern))
            .GreaterThanOrEqualTo(command => command.StartDate)
            .WithMessage("End date must be greater than or equal to start date")
            .When(command => command.EndDate.HasValue)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("End date cannot be in the past")
            .When(command => command.EndDate.HasValue);

        RuleFor(command => command.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");
    }
}

