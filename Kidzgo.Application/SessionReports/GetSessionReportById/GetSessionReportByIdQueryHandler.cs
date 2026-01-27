using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.GetSessionReportById;

public sealed class GetSessionReportByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionReportByIdQuery, GetSessionReportByIdResponse>
{
    public async Task<Result<GetSessionReportByIdResponse>> Handle(
        GetSessionReportByIdQuery query,
        CancellationToken cancellationToken)
    {
        var sessionReport = await context.SessionReports
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
                    .ThenInclude(c => c.Branch)
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .FirstOrDefaultAsync(sr => sr.Id == query.Id, cancellationToken);

        if (sessionReport is null)
        {
            return Result.Failure<GetSessionReportByIdResponse>(SessionReportErrors.NotFound(query.Id));
        }

        return new GetSessionReportByIdResponse
        {
            Id = sessionReport.Id,
            SessionId = sessionReport.SessionId,
            SessionDate = sessionReport.Session.PlannedDatetime,
            SessionStatus = sessionReport.Session.Status.ToString(),
            ClassId = sessionReport.Session.ClassId,
            ClassCode = sessionReport.Session.Class.Code,
            ClassTitle = sessionReport.Session.Class.Title,
            BranchId = sessionReport.Session.BranchId,
            BranchName = sessionReport.Session.Class.Branch.Name,
            StudentProfileId = sessionReport.StudentProfileId,
            StudentName = sessionReport.StudentProfile.DisplayName,
            TeacherUserId = sessionReport.TeacherUserId,
            TeacherName = sessionReport.TeacherUser.Name,
            ReportDate = sessionReport.ReportDate,
            Feedback = sessionReport.Feedback,
            AiGeneratedSummary = sessionReport.AiGeneratedSummary,
            IsMonthlyCompiled = sessionReport.IsMonthlyCompiled,
            CreatedAt = sessionReport.CreatedAt,
            UpdatedAt = sessionReport.UpdatedAt
        };
    }
}

