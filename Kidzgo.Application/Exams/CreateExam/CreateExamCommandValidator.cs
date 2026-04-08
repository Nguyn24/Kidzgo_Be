using FluentValidation;

namespace Kidzgo.Application.Exams.CreateExam;

public sealed class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(command => command.ClassId)
            .NotEmpty()
            .WithMessage("Class ID is required");

        RuleFor(command => command.Date)
            .NotEmpty()
            .WithMessage("Date is required")
            .GreaterThanOrEqualTo(VietnamTime.TodayDateOnly())
            .WithMessage("Exam date cannot be in the past");

        RuleFor(command => command.ScheduledStartTime)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("Scheduled start time cannot be in the past")
            .When(command => command.ScheduledStartTime.HasValue);
    }
}

