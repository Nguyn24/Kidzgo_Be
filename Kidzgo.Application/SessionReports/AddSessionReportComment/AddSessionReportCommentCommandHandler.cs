using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.AddSessionReportComment;

public sealed class AddSessionReportCommentCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<AddSessionReportCommentCommand, AddSessionReportCommentResponse>
{
    public async Task<Result<AddSessionReportCommentResponse>> Handle(
        AddSessionReportCommentCommand command,
        CancellationToken cancellationToken)
    {
        // Verify session report exists
        var sessionReport = await context.SessionReports
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionReportId, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<AddSessionReportCommentResponse>(
                SessionReportErrors.NotFound(command.SessionReportId));
        }

        var now = DateTime.UtcNow;

        var comment = new ReportComment
        {
            Id = Guid.NewGuid(),
            SessionReportId = command.SessionReportId,
            CommenterId = userContext.UserId,
            Content = command.Content,
            CreatedAt = now
        };

        context.ReportComments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);

        // Get commenter name
        var commenter = await context.Users.FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        return new AddSessionReportCommentResponse
        {
            Id = comment.Id,
            SessionReportId = comment.SessionReportId,
            CommenterId = comment.CommenterId,
            CommenterName = commenter?.Name ?? "Unknown",
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }
}
