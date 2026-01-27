using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.GetPlacementTestById;

public sealed class GetPlacementTestByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetPlacementTestByIdQuery, GetPlacementTestByIdResponse>
{
    public async Task<Result<GetPlacementTestByIdResponse>> Handle(
        GetPlacementTestByIdQuery query,
        CancellationToken cancellationToken)
    {
        // UC-028: Get Placement Test by ID
        var placementTest = await context.PlacementTests
            .Include(pt => pt.Lead)
            .Include(pt => pt.StudentProfile)
            .Include(pt => pt.Class)
            .Include(pt => pt.InvigilatorUser)
            .FirstOrDefaultAsync(pt => pt.Id == query.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<GetPlacementTestByIdResponse>(
                PlacementTestErrors.NotFound(query.PlacementTestId));
        }

        return new GetPlacementTestByIdResponse
        {
            Id = placementTest.Id,
            LeadId = placementTest.LeadId,
            LeadContactName = placementTest.Lead?.ContactName,
            StudentProfileId = placementTest.StudentProfileId,
            StudentName = placementTest.StudentProfile?.DisplayName,
            ClassId = placementTest.ClassId,
            ClassName = placementTest.Class?.Title,
            ScheduledAt = placementTest.ScheduledAt,
            Status = placementTest.Status.ToString(),
            Room = placementTest.Room,
            InvigilatorUserId = placementTest.InvigilatorUserId,
            InvigilatorName = placementTest.InvigilatorUser?.Name,
            ResultScore = placementTest.ResultScore,
            ListeningScore = placementTest.ListeningScore,
            SpeakingScore = placementTest.SpeakingScore,
            ReadingScore = placementTest.ReadingScore,
            WritingScore = placementTest.WritingScore,
            LevelRecommendation = placementTest.LevelRecommendation,
            ProgramRecommendation = placementTest.ProgramRecommendation,
            Notes = placementTest.Notes,
            AttachmentUrl = placementTest.AttachmentUrl,
            CreatedAt = placementTest.CreatedAt,
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

