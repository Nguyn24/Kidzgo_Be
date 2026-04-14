using FluentValidation;

namespace Kidzgo.Application.IncidentReports.AssignIncidentReport;

public sealed class AssignIncidentReportCommandValidator : AbstractValidator<AssignIncidentReportCommand>
{
    public AssignIncidentReportCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AssignedToUserId).NotEmpty();
    }
}
