using FluentValidation;

namespace Kidzgo.Application.Exams.UpdateExam;

public sealed class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
{
    public UpdateExamCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Exam ID is required");

        RuleFor(command => command.Date)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Exam date cannot be in the past")
            .When(command => command.Date.HasValue);

        RuleFor(command => command.ScheduledStartTime)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Scheduled start time cannot be in the past")
            .When(command => command.ScheduledStartTime.HasValue);
    }
}

