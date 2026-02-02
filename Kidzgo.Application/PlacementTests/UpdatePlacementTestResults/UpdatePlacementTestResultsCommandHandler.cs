using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;

public sealed class UpdatePlacementTestResultsCommandHandler(
    IDbContext context
) : ICommandHandler<UpdatePlacementTestResultsCommand, UpdatePlacementTestResultsResponse>
{
    public async Task<Result<UpdatePlacementTestResultsResponse>> Handle(
        UpdatePlacementTestResultsCommand command,
        CancellationToken cancellationToken)
    {
        // UC-032 to UC-036: Update Placement Test Results
        var placementTest = await context.PlacementTests
            .Include(pt => pt.LeadChild)
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<UpdatePlacementTestResultsResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        // Update scores (UC-032, UC-033)
        if (command.ListeningScore.HasValue)
        {
            placementTest.ListeningScore = command.ListeningScore.Value;
        }

        if (command.SpeakingScore.HasValue)
        {
            placementTest.SpeakingScore = command.SpeakingScore.Value;
        }

        if (command.ReadingScore.HasValue)
        {
            placementTest.ReadingScore = command.ReadingScore.Value;
        }

        if (command.WritingScore.HasValue)
        {
            placementTest.WritingScore = command.WritingScore.Value;
        }

        if (command.ResultScore.HasValue)
        {
            placementTest.ResultScore = command.ResultScore.Value;
        }

        // Update recommendations (UC-034, UC-035)
        if (command.LevelRecommendation is not null)
        {
            placementTest.LevelRecommendation = string.IsNullOrWhiteSpace(command.LevelRecommendation)
                ? null
                : command.LevelRecommendation.Trim();
        }

        if (command.ProgramRecommendation is not null)
        {
            placementTest.ProgramRecommendation = string.IsNullOrWhiteSpace(command.ProgramRecommendation)
                ? null
                : command.ProgramRecommendation.Trim();
        }

        // Update attachment URL (UC-036)
        if (command.AttachmentUrl is not null)
        {
            placementTest.AttachmentUrl = string.IsNullOrWhiteSpace(command.AttachmentUrl)
                ? null
                : command.AttachmentUrl.Trim();
        }

        var now = DateTime.UtcNow;

        // Mark as Completed if all scores are entered
        if (placementTest.ListeningScore.HasValue &&
            placementTest.SpeakingScore.HasValue &&
            placementTest.ReadingScore.HasValue &&
            placementTest.WritingScore.HasValue &&
            placementTest.ResultScore.HasValue &&
            placementTest.Status != PlacementTestStatus.Completed)
        {
            placementTest.Status = PlacementTestStatus.Completed;

            // Update LeadChild status if LeadChildId exists
            if (placementTest.LeadChildId.HasValue && placementTest.LeadChild is not null)
            {
                var leadChild = placementTest.LeadChild;
                if (leadChild.Status == LeadChildStatus.BookedTest)
                {
                    leadChild.Status = LeadChildStatus.TestDone;
                    leadChild.UpdatedAt = now;

                    // Create activity for LeadChild
                    context.LeadActivities.Add(new LeadActivity
                    {
                        Id = Guid.NewGuid(),
                        LeadId = leadChild.LeadId,
                        ActivityType = ActivityType.Note,
                        Content = $"Child '{leadChild.ChildName}' placement test completed -> status: TestDone",
                        CreatedAt = now,
                        CreatedBy = null
                    });
                }
            }

            // Auto-transition lead status to TestDone when test is completed (backward compatibility)
            if (placementTest.LeadId.HasValue)
            {
                var lead = await context.Leads
                    .FirstOrDefaultAsync(l => l.Id == placementTest.LeadId.Value, cancellationToken);

                if (lead is not null && lead.Status == LeadStatus.BookedTest)
                {
                    lead.Status = LeadStatus.TestDone;
                    lead.UpdatedAt = now;

                    // Only create activity if LeadChild was not updated (to avoid duplicate)
                    if (!placementTest.LeadChildId.HasValue || placementTest.LeadChild is null)
                    {
                        context.LeadActivities.Add(new LeadActivity
                        {
                            Id = Guid.NewGuid(),
                            LeadId = lead.Id,
                            ActivityType = ActivityType.Note,
                            Content = "Placement test completed -> Lead status: TestDone",
                            CreatedAt = now,
                            CreatedBy = null
                        });
                    }
                }
            }
        }

        placementTest.UpdatedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        return new UpdatePlacementTestResultsResponse
        {
            Id = placementTest.Id,
            ListeningScore = placementTest.ListeningScore,
            SpeakingScore = placementTest.SpeakingScore,
            ReadingScore = placementTest.ReadingScore,
            WritingScore = placementTest.WritingScore,
            ResultScore = placementTest.ResultScore,
            LevelRecommendation = placementTest.LevelRecommendation,
            ProgramRecommendation = placementTest.ProgramRecommendation,
            AttachmentUrl = placementTest.AttachmentUrl,
            Status = placementTest.Status.ToString(),
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

