using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.DeleteLeadChild;

public sealed class DeleteLeadChildCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteLeadChildCommand, DeleteLeadChildResponse>
{
    public async Task<Result<DeleteLeadChildResponse>> Handle(
        DeleteLeadChildCommand command,
        CancellationToken cancellationToken)
    {
        // Validate Lead exists
        var leadExists = await context.Leads
            .AnyAsync(l => l.Id == command.LeadId, cancellationToken);

        if (!leadExists)
        {
            return Result.Failure<DeleteLeadChildResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        // Get LeadChild with related PlacementTests
        var leadChild = await context.LeadChildren
            .Include(lc => lc.PlacementTests)
            .FirstOrDefaultAsync(lc => lc.Id == command.ChildId && lc.LeadId == command.LeadId, cancellationToken);

        if (leadChild is null)
        {
            return Result.Failure<DeleteLeadChildResponse>(
                Domain.Common.Error.NotFound("LeadChild", $"LeadChild with Id {command.ChildId} not found"));
        }

        // Check if child has placement tests
        if (leadChild.PlacementTests.Any())
        {
            return Result.Failure<DeleteLeadChildResponse>(
                Domain.Common.Error.Validation("ChildId", "Cannot delete child with existing placement tests"));
        }

        // Check if child is enrolled
        if (leadChild.Status == LeadChildStatus.Enrolled)
        {
            return Result.Failure<DeleteLeadChildResponse>(
                Domain.Common.Error.Validation("ChildId", "Cannot delete enrolled child"));
        }

        // Delete LeadChild
        context.LeadChildren.Remove(leadChild);

        // Create activity for Lead
        var now = VietnamTime.UtcNow();
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = command.LeadId,
            ActivityType = ActivityType.Note,
            Content = $"Child '{leadChild.ChildName}' removed from lead",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteLeadChildResponse
        {
            Id = leadChild.Id,
            LeadId = leadChild.LeadId,
            Success = true
        });
    }
}

