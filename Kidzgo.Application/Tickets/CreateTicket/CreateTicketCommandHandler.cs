using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.CreateTicket;

public sealed class CreateTicketCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateTicketCommand, CreateTicketResponse>
{
    public async Task<Result<CreateTicketResponse>> Handle(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var openedByUserId = userContext.UserId;

        // Check if user exists
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == openedByUserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<CreateTicketResponse>(TicketErrors.UserNotFound);
        }

        // Check if branch exists
        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<CreateTicketResponse>(TicketErrors.BranchNotFound);
        }

        // Check if class exists (if provided)
        if (command.ClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.ClassNotFound);
            }
        }

        // Check if profile exists (if provided)
        if (command.OpenedByProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.OpenedByProfileId.Value && p.UserId == openedByUserId, cancellationToken);

            if (profile is null)
            {
                return Result.Failure<CreateTicketResponse>(TicketErrors.ProfileNotFound);
            }
        }

        var now = DateTime.UtcNow;
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            OpenedByUserId = openedByUserId,
            OpenedByProfileId = command.OpenedByProfileId,
            BranchId = command.BranchId,
            ClassId = command.ClassId,
            Category = command.Category,
            Subject = command.Subject,
            Message = command.Message,
            Status = TicketStatus.Open,
            AssignedToUserId = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(cancellationToken);

        // Query ticket with navigation properties for response
        var ticketWithNav = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);

        return new CreateTicketResponse
        {
            Id = ticketWithNav!.Id,
            OpenedByUserId = ticketWithNav.OpenedByUserId,
            OpenedByUserName = ticketWithNav.OpenedByUser.Name,
            OpenedByProfileId = ticketWithNav.OpenedByProfileId,
            OpenedByProfileName = ticketWithNav.OpenedByProfile?.DisplayName,
            BranchId = ticketWithNav.BranchId,
            BranchName = ticketWithNav.Branch.Name,
            ClassId = ticketWithNav.ClassId,
            ClassCode = ticketWithNav.Class?.Code,
            ClassTitle = ticketWithNav.Class?.Title,
            Category = ticketWithNav.Category.ToString(),
            Subject = ticketWithNav.Subject,
            Message = ticketWithNav.Message,
            Status = ticketWithNav.Status.ToString(),
            AssignedToUserId = ticketWithNav.AssignedToUserId,
            CreatedAt = ticketWithNav.CreatedAt,
            UpdatedAt = ticketWithNav.UpdatedAt
        };
    }
}