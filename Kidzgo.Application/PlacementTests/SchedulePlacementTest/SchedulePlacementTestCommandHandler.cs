using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.SchedulePlacementTest;

public sealed class SchedulePlacementTestCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SchedulePlacementTestCommand, SchedulePlacementTestResponse>
{
    public async Task<Result<SchedulePlacementTestResponse>> Handle(
        SchedulePlacementTestCommand command,
        CancellationToken cancellationToken)
    {
        Lead? lead = null;
        LeadChild? leadChild = null;

        // UC-027: Priority 1 - If LeadChildId is provided, use it
        if (command.LeadChildId.HasValue)
        {
            leadChild = await context.LeadChildren
                .Include(lc => lc.Lead)
                .FirstOrDefaultAsync(lc => lc.Id == command.LeadChildId.Value, cancellationToken);

            if (leadChild is null)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    Domain.Common.Error.NotFound("LeadChild", $"LeadChild with Id {command.LeadChildId.Value} not found"));
            }

            lead = leadChild.Lead;

            // Validate LeadId matches if provided
            if (command.LeadId.HasValue && command.LeadId.Value != lead.Id)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    Domain.Common.Error.Validation("LeadId", "LeadId does not match LeadChild's LeadId"));
            }
        }
        // Priority 2 - If only LeadId is provided, use default LeadChild or create one
        else if (command.LeadId.HasValue)
        {
            lead = await context.Leads
                .Include(l => l.LeadChildren)
                .FirstOrDefaultAsync(l => l.Id == command.LeadId.Value, cancellationToken);

            if (lead is null)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    PlacementTestErrors.LeadNotFound(command.LeadId.Value));
            }

            // Get or create default LeadChild
            leadChild = lead.LeadChildren.FirstOrDefault();
            if (leadChild is null)
            {
                // Create default LeadChild for backward compatibility
                var defaultNow = VietnamTime.UtcNow();
                leadChild = new LeadChild
                {
                    Id = Guid.NewGuid(),
                    LeadId = lead.Id,
                    ChildName = lead.ContactName ?? "Child",
                    Dob = null,
                    ProgramInterest = null,
                    Status = LeadChildStatus.New,
                    CreatedAt = defaultNow,
                    UpdatedAt = defaultNow
                };
                context.LeadChildren.Add(leadChild);
            }
        }
        else
        {
            return Result.Failure<SchedulePlacementTestResponse>(
                Domain.Common.Error.Validation("LeadId", "Either LeadId or LeadChildId must be provided"));
        }

        // Validate Invigilator if provided
        if (command.InvigilatorUserId.HasValue)
        {
            var invigilator = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.InvigilatorUserId.Value, cancellationToken);

            if (invigilator is null)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    PlacementTestErrors.InvigilatorNotFound(command.InvigilatorUserId));
            }
        }

        DateTime scheduledAtUtc = VietnamTime.NormalizeToUtc(command.ScheduledAt);
        var now = VietnamTime.UtcNow();
        var placementTest = new PlacementTest
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            LeadChildId = leadChild.Id,
            ScheduledAt = scheduledAtUtc,
            Status = PlacementTestStatus.Scheduled,
            Room = command.Room?.Trim(),
            InvigilatorUserId = command.InvigilatorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.PlacementTests.Add(placementTest);

        // Update LeadChild status to BookedTest
        if (leadChild.Status != LeadChildStatus.BookedTest)
        {
            leadChild.Status = LeadChildStatus.BookedTest;
            leadChild.UpdatedAt = now;
        }

        // Update Lead status to BOOKED_TEST (keep backward compatibility)
        if (lead.Status != LeadStatus.BookedTest)
        {
            lead.Status = LeadStatus.BookedTest;
            lead.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SchedulePlacementTestResponse
        {
            Id = placementTest.Id,
            LeadId = placementTest.LeadId,
            LeadChildId = placementTest.LeadChildId,
            ScheduledAt = placementTest.ScheduledAt,
            Status = placementTest.Status.ToString(),
            Room = placementTest.Room,
            InvigilatorUserId = placementTest.InvigilatorUserId,
            CreatedAt = placementTest.CreatedAt
        };
    }
}

