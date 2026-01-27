using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.GetPlacementTests;

public sealed class GetPlacementTestsQueryHandler(
    IDbContext context
) : IQueryHandler<GetPlacementTestsQuery, GetPlacementTestsResponse>
{
    public async Task<Result<GetPlacementTestsResponse>> Handle(
        GetPlacementTestsQuery query,
        CancellationToken cancellationToken)
    {
        // UC-028: Query Placement Tests with filters
        var placementTestsQuery = context.PlacementTests.AsQueryable();

        if (query.LeadId.HasValue)
        {
            placementTestsQuery = placementTestsQuery.Where(pt => pt.LeadId == query.LeadId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            placementTestsQuery = placementTestsQuery.Where(pt => pt.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.Status.HasValue)
        {
            placementTestsQuery = placementTestsQuery.Where(pt => pt.Status == query.Status.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromDateUtc = query.FromDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(query.FromDate.Value, DateTimeKind.Utc)
                : query.FromDate.Value.ToUniversalTime();
            placementTestsQuery = placementTestsQuery.Where(pt => pt.ScheduledAt >= fromDateUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toDateUtc = query.ToDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(query.ToDate.Value, DateTimeKind.Utc)
                : query.ToDate.Value.ToUniversalTime();
            placementTestsQuery = placementTestsQuery.Where(pt => pt.ScheduledAt <= toDateUtc);
        }

        var totalCount = await placementTestsQuery.CountAsync(cancellationToken);

        var placementTests = await placementTestsQuery
            .Include(pt => pt.Lead)
            .Include(pt => pt.StudentProfile)
            .Include(pt => pt.Class)
            .Include(pt => pt.InvigilatorUser)
            .OrderByDescending(pt => pt.ScheduledAt ?? pt.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(pt => new PlacementTestDto
            {
                Id = pt.Id,
                LeadId = pt.LeadId,
                LeadContactName = pt.Lead != null ? pt.Lead.ContactName : null,
                StudentProfileId = pt.StudentProfileId,
                StudentName = pt.StudentProfile != null ? pt.StudentProfile.DisplayName : null,
                ClassId = pt.ClassId,
                ClassName = pt.Class != null ? pt.Class.Title : null,
                ScheduledAt = pt.ScheduledAt,
                Status = pt.Status.ToString(),
                Room = pt.Room,
                InvigilatorUserId = pt.InvigilatorUserId,
                InvigilatorName = pt.InvigilatorUser != null ? pt.InvigilatorUser.Name : null,
                ResultScore = pt.ResultScore,
                ListeningScore = pt.ListeningScore,
                SpeakingScore = pt.SpeakingScore,
                ReadingScore = pt.ReadingScore,
                WritingScore = pt.WritingScore,
                LevelRecommendation = pt.LevelRecommendation,
                ProgramRecommendation = pt.ProgramRecommendation,
                Notes = pt.Notes,
                AttachmentUrl = pt.AttachmentUrl,
                CreatedAt = pt.CreatedAt,
                UpdatedAt = pt.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetPlacementTestsResponse
        {
            PlacementTests = placementTests,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        });
    }
}

