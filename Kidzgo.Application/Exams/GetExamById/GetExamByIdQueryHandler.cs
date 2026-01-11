using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExamById;

public sealed class GetExamByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamByIdQuery, GetExamByIdResponse>
{
    public async Task<Result<GetExamByIdResponse>> Handle(GetExamByIdQuery query, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .Include(e => e.Class)
            .Include(e => e.CreatedByUser)
            .FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken);

        if (exam == null)
        {
            return Result.Failure<GetExamByIdResponse>(
                ExamErrors.NotFound(query.Id));
        }

        return new GetExamByIdResponse
        {
            Id = exam.Id,
            ClassId = exam.ClassId,
            ClassCode = exam.Class.Code,
            ClassTitle = exam.Class.Title,
            ExamType = exam.ExamType,
            Date = exam.Date,
            MaxScore = exam.MaxScore,
            Description = exam.Description,
            ScheduledStartTime = exam.ScheduledStartTime,
            TimeLimitMinutes = exam.TimeLimitMinutes,
            AllowLateStart = exam.AllowLateStart,
            LateStartToleranceMinutes = exam.LateStartToleranceMinutes,
            AutoSubmitOnTimeLimit = exam.AutoSubmitOnTimeLimit,
            PreventCopyPaste = exam.PreventCopyPaste,
            PreventNavigation = exam.PreventNavigation,
            ShowResultsImmediately = exam.ShowResultsImmediately,
            CreatedBy = exam.CreatedBy,
            CreatedByName = exam.CreatedByUser != null ? exam.CreatedByUser.Name : null,
            CreatedAt = exam.CreatedAt,
            ResultCount = exam.ExamResults.Count
        };
    }
}

