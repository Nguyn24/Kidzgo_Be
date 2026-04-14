using FluentValidation;

namespace Kidzgo.Application.IncidentReports.UpdateIncidentReportStatus;

public sealed class UpdateIncidentReportStatusCommandValidator : AbstractValidator<UpdateIncidentReportStatusCommand>
{
    public UpdateIncidentReportStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
