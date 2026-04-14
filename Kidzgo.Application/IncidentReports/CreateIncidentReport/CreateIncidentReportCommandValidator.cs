using FluentValidation;

namespace Kidzgo.Application.IncidentReports.CreateIncidentReport;

public sealed class CreateIncidentReportCommandValidator : AbstractValidator<CreateIncidentReportCommand>
{
    public CreateIncidentReportCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty();

        RuleFor(x => x.Category)
            .IsInEnum();

        RuleFor(x => x.Subject)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
