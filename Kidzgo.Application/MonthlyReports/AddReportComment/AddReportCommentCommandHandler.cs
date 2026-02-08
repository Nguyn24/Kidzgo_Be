using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.AddReportComment;

/// <summary>
/// UC-182: Staff/Admin comment Monthly Report
/// </summary>
public sealed class AddReportCommentCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<AddReportCommentCommand, AddReportCommentResponse>
{
    public async Task<Result<AddReportCommentResponse>> Handle(
        AddReportCommentCommand command,
        CancellationToken cancellationToken)
    {
        var report = await context.StudentMonthlyReports
            .FirstOrDefaultAsync(r => r.Id == command.ReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure<AddReportCommentResponse>(
                MonthlyReportErrors.NotFound(command.ReportId));
        }

        var commenterId = userContext.UserId;
        var now = DateTime.UtcNow;

        var comment = new ReportComment
        {
            Id = Guid.NewGuid(),
            ReportId = command.ReportId,
            CommenterId = commenterId,
            Content = command.Content,
            CreatedAt = now
        };

        context.ReportComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        return new AddReportCommentResponse
        {
            Id = comment.Id,
            ReportId = comment.ReportId,
            CommenterId = comment.CommenterId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}

