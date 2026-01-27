using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.ConvertLeadToEnrolled;

public sealed class ConvertLeadToEnrolledCommandHandler(
    IDbContext context
) : ICommandHandler<ConvertLeadToEnrolledCommand, ConvertLeadToEnrolledResponse>
{
    public async Task<Result<ConvertLeadToEnrolledResponse>> Handle(
        ConvertLeadToEnrolledCommand command,
        CancellationToken cancellationToken)
    {
        // UC-038: Convert Lead to ENROLLED after Placement Test
        var placementTest = await context.PlacementTests
            .Include(pt => pt.Lead)
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<ConvertLeadToEnrolledResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        if (placementTest.Lead is null)
        {
            return Result.Failure<ConvertLeadToEnrolledResponse>(
                PlacementTestErrors.LeadNotFound(placementTest.LeadId));
        }

        var lead = placementTest.Lead;

        // Check if lead is already enrolled
        if (lead.Status == LeadStatus.Enrolled)
        {
            return Result.Failure<ConvertLeadToEnrolledResponse>(
                PlacementTestErrors.LeadAlreadyEnrolled);
        }

        // Validate StudentProfile if provided
        if (command.StudentProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value, cancellationToken);

            if (profile is null || profile.ProfileType != Kidzgo.Domain.Users.ProfileType.Student)
            {
                return Result.Failure<ConvertLeadToEnrolledResponse>(
                    PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
            }

            // Link student profile to placement test
            placementTest.StudentProfileId = command.StudentProfileId.Value;
            
            // Link student profile to lead
            lead.ConvertedStudentProfileId = command.StudentProfileId.Value;
        }

        var now = DateTime.UtcNow;

        // Update Lead status to ENROLLED
        lead.Status = LeadStatus.Enrolled;
        lead.ConvertedAt = now;
        lead.UpdatedAt = now;

        // Update Placement Test status to Completed if not already
        if (placementTest.Status != PlacementTestStatus.Completed)
        {
            placementTest.Status = PlacementTestStatus.Completed;
            placementTest.UpdatedAt = now;
        }

        // Create activity for Lead
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = ActivityType.Note,
            Content = "Lead converted to ENROLLED after Placement Test completion",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return new ConvertLeadToEnrolledResponse
        {
            LeadId = lead.Id,
            LeadStatus = lead.Status.ToString(),
            PlacementTestId = placementTest.Id,
            PlacementTestStatus = placementTest.Status.ToString(),
            StudentProfileId = lead.ConvertedStudentProfileId,
            ConvertedAt = lead.ConvertedAt
        };
    }
}

