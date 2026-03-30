using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Users;
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
            .Include(pt => pt.LeadChild)
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
        var isChildBasedFlow = placementTest.LeadChildId.HasValue && placementTest.LeadChild is not null;

        // Multi-children flow: Lead can be Enrolled because another child already enrolled.
        // Only block if we're in legacy flow (no LeadChild).
        if (!isChildBasedFlow && lead.Status == LeadStatus.Enrolled)
        {
            return Result.Failure<ConvertLeadToEnrolledResponse>(
                PlacementTestErrors.LeadAlreadyEnrolled);
        }

        // Validate StudentProfile if provided
        Profile? studentProfile = null;
        if (command.StudentProfileId.HasValue)
        {
            studentProfile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value, cancellationToken);

            if (studentProfile is null || studentProfile.ProfileType != ProfileType.Student)
            {
                return Result.Failure<ConvertLeadToEnrolledResponse>(
                    PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
            }

            // Check if this StudentProfile is already assigned to another LeadChild
            var existingAssignment = await context.LeadChildren
                .FirstOrDefaultAsync(lc => lc.ConvertedStudentProfileId == command.StudentProfileId.Value, cancellationToken);

            if (existingAssignment is not null)
            {
                // If it's assigned to a different LeadChild, return error
                if (!isChildBasedFlow || existingAssignment.Id != placementTest.LeadChildId)
                {
                    return Result.Failure<ConvertLeadToEnrolledResponse>(
                        PlacementTestErrors.StudentProfileAlreadyAssigned(command.StudentProfileId.Value, existingAssignment.Id));
                }
            }

            // Link student profile to placement test
            placementTest.StudentProfileId = command.StudentProfileId.Value;
        }

        var now = DateTime.UtcNow;

        // Update LeadChild if LeadChildId exists
        if (isChildBasedFlow)
        {
            var leadChild = placementTest.LeadChild;

            // Check if child is already enrolled
            if (leadChild.Status == LeadChildStatus.Enrolled)
            {
                return Result.Failure<ConvertLeadToEnrolledResponse>(
                    Domain.Common.Error.Validation("LeadChild", "Child is already enrolled"));
            }

            // Link student profile to LeadChild
            if (command.StudentProfileId.HasValue)
            {
                leadChild.ConvertedStudentProfileId = command.StudentProfileId.Value;
            }

            // Copy LeadChild data into the linked StudentProfile
            if (studentProfile is not null)
            {
                // Only fill if the profile fields are empty (preserve existing data)
                if (string.IsNullOrWhiteSpace(studentProfile.Name))
                    studentProfile.Name = leadChild.ChildName;

                if (!studentProfile.DateOfBirth.HasValue && leadChild.Dob.HasValue)
                    studentProfile.DateOfBirth = leadChild.Dob;

                if (!studentProfile.Gender.HasValue)
                    studentProfile.Gender = leadChild.Gender;
            }

            // Update LeadChild status to Enrolled
            leadChild.Status = LeadChildStatus.Enrolled;
            leadChild.UpdatedAt = now;

            // Create activity for LeadChild
            var childActivityContent = command.StudentProfileId.HasValue
                ? $"Child '{leadChild.ChildName}' converted to ENROLLED (via enrollment API)"
                : $"Child '{leadChild.ChildName}' converted to ENROLLED after Placement Test completion";

            context.LeadActivities.Add(new LeadActivity
            {
                Id = Guid.NewGuid(),
                LeadId = leadChild.LeadId,
                ActivityType = ActivityType.Note,
                Content = childActivityContent,
                CreatedAt = now
            });

            // Update Lead status: if at least 1 child is Enrolled, Lead.Status = Enrolled
            var hasEnrolledChild = await context.LeadChildren
                .AnyAsync(lc => lc.LeadId == lead.Id && lc.Status == LeadChildStatus.Enrolled, cancellationToken);

            if (hasEnrolledChild && lead.Status != LeadStatus.Enrolled)
            {
                lead.Status = LeadStatus.Enrolled;
                lead.UpdatedAt = now;
            }
        }
        else
        {
            // Backward compatibility: Update Lead directly if no LeadChild
            // Update Lead status to ENROLLED
            lead.Status = LeadStatus.Enrolled;
            lead.UpdatedAt = now;

            // Create activity for Lead
            context.LeadActivities.Add(new LeadActivity
            {
                Id = Guid.NewGuid(),
                LeadId = lead.Id,
                ActivityType = ActivityType.Note,
                Content = "Lead converted to ENROLLED after Placement Test completion",
                CreatedAt = now
            });
        }

        // Update Placement Test status to Completed if not already
        if (placementTest.Status != PlacementTestStatus.Completed)
        {
            placementTest.Status = PlacementTestStatus.Completed;
            placementTest.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ConvertLeadToEnrolledResponse
        {
            LeadId = lead.Id,
            LeadStatus = lead.Status.ToString(),
            PlacementTestId = placementTest.Id,
            PlacementTestStatus = placementTest.Status.ToString(),
            StudentProfileId = placementTest.StudentProfileId,
            LeadChildId = placementTest.LeadChildId
        };
    }
}

