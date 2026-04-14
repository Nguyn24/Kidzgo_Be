using FluentValidation;

namespace Kidzgo.Application.IncidentReports.AddIncidentReportComment;

public sealed class AddIncidentReportCommentCommandValidator : AbstractValidator<AddIncidentReportCommentCommand>
{
    public AddIncidentReportCommentCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.CommentType).IsInEnum();
    }
}
