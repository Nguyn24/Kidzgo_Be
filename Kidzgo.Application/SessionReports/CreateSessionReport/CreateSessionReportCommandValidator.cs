using FluentValidation;

namespace Kidzgo.Application.SessionReports.CreateSessionReport;

public sealed class CreateSessionReportCommandValidator : AbstractValidator<CreateSessionReportCommand>
{
    public CreateSessionReportCommandValidator()
    {
        RuleFor(command => command.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required");

        RuleFor(command => command.StudentProfileId)
            .NotEmpty()
            .WithMessage("Student Profile ID is required");

        RuleFor(command => command.ReportDate)
            .NotEmpty()
            .WithMessage("Report date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Report date cannot be in the future");

        RuleFor(command => command.Feedback)
            .NotEmpty()
            .WithMessage("Feedback is required");
    }
}

