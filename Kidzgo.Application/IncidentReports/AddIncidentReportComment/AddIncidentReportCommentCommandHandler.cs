using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.IncidentReports.AddIncidentReportComment;

public sealed class AddIncidentReportCommentCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<AddIncidentReportCommentCommand, IncidentReportCommentDto>
{
    public async Task<Result<IncidentReportCommentDto>> Handle(AddIncidentReportCommentCommand command, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<IncidentReportCommentDto>(TicketErrors.UserNotFound);
        }

        var ticket = await context.Tickets
            .FirstOrDefaultAsync(t => t.Id == command.TicketId && t.IsIncidentReport, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<IncidentReportCommentDto>(IncidentReportErrors.NotFound(command.TicketId));
        }

        if (!IncidentReportAccessPolicy.CanComment(currentUser, ticket))
        {
            return Result.Failure<IncidentReportCommentDto>(IncidentReportErrors.Unauthorized);
        }

        var now = VietnamTime.UtcNow();
        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            CommenterUserId = currentUser.Id,
            Message = command.Message,
            AttachmentUrl = command.AttachmentUrl,
            IncidentCommentType = command.CommentType,
            CreatedAt = now
        };

        context.TicketComments.Add(comment);
        ticket.UpdatedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        var created = await context.TicketComments
            .Include(c => c.CommenterUser)
            .FirstAsync(c => c.Id == comment.Id, cancellationToken);

        return new IncidentReportCommentDto
        {
            Id = created.Id,
            CommenterUserId = created.CommenterUserId,
            CommenterUserName = created.CommenterUser?.Name ?? string.Empty,
            Message = created.Message,
            AttachmentUrl = created.AttachmentUrl,
            CommentType = created.IncidentCommentType?.ToString(),
            CreatedAt = created.CreatedAt
        };
    }
}
