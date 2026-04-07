using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetStudentHomeworkHistory;

public sealed class GetStudentHomeworkHistoryQueryHandler(
    IDbContext context
) : IQueryHandler<GetStudentHomeworkHistoryQuery, GetStudentHomeworkHistoryResponse>
{
    public async Task<Result<GetStudentHomeworkHistoryResponse>> Handle(
        GetStudentHomeworkHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var homeworkQuery = context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .Where(hs => hs.StudentProfileId == query.StudentProfileId)
            .AsQueryable();

        if (query.ClassId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Assignment.ClassId == query.ClassId.Value);
        }

        var totalCount = await homeworkQuery.CountAsync(cancellationToken);

        var homeworkRows = await homeworkQuery
            .OrderByDescending(hs => hs.Assignment.CreatedAt)
            .ThenByDescending(hs => hs.SubmittedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(hs => new
            {
                hs.Id,
                hs.AssignmentId,
                AssignmentTitle = hs.Assignment.Title,
                hs.Assignment.ClassId,
                ClassCode = hs.Assignment.Class.Code,
                ClassTitle = hs.Assignment.Class.Title,
                hs.Assignment.DueAt,
                hs.Assignment.MaxScore,
                hs.Assignment.MaxAttempts,
                hs.Status,
                hs.StartedAt,
                hs.SubmittedAt,
                hs.GradedAt,
                hs.Score,
                hs.TeacherFeedback,
                hs.AiFeedback,
                hs.TextAnswer,
                hs.AttachmentUrl
            })
            .ToListAsync(cancellationToken);

        var homeworkStudentIds = homeworkRows.Select(x => x.Id).ToList();
        var attemptsLookup = await context.HomeworkSubmissionAttempts
            .Where(a => homeworkStudentIds.Contains(a.HomeworkStudentId))
            .OrderByDescending(a => a.AttemptNumber)
            .Select(a => new
            {
                a.Id,
                a.HomeworkStudentId,
                a.AttemptNumber,
                a.Status,
                a.StartedAt,
                a.SubmittedAt,
                a.GradedAt,
                a.Score,
                a.TeacherFeedback,
                a.AiFeedback,
                a.TextAnswer,
                a.AttachmentUrl,
                a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var attemptsByHomeworkStudentId = attemptsLookup
            .GroupBy(a => a.HomeworkStudentId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(a => new HomeworkSubmissionAttempt
                {
                    Id = a.Id,
                    HomeworkStudentId = a.HomeworkStudentId,
                    AttemptNumber = a.AttemptNumber,
                    Status = a.Status,
                    StartedAt = a.StartedAt,
                    SubmittedAt = a.SubmittedAt,
                    GradedAt = a.GradedAt,
                    Score = a.Score,
                    TeacherFeedback = a.TeacherFeedback,
                    AiFeedback = a.AiFeedback,
                    TextAnswer = a.TextAnswer,
                    AttachmentUrl = a.AttachmentUrl,
                    CreatedAt = a.CreatedAt
                }).ToList());

        var homeworks = homeworkRows.Select(row =>
        {
            var snapshot = new HomeworkStudent
            {
                Id = row.Id,
                AssignmentId = row.AssignmentId,
                Status = row.Status,
                StartedAt = row.StartedAt,
                SubmittedAt = row.SubmittedAt,
                GradedAt = row.GradedAt,
                Score = row.Score,
                TeacherFeedback = row.TeacherFeedback,
                AiFeedback = row.AiFeedback,
                TextAnswer = row.TextAnswer,
                AttachmentUrl = row.AttachmentUrl
            };

            var attempts = attemptsByHomeworkStudentId.TryGetValue(row.Id, out var persistedAttempts)
                ? HomeworkSubmissionAttemptMapper.BuildDtos(snapshot, persistedAttempts)
                : HomeworkSubmissionAttemptMapper.BuildDtos(snapshot, Array.Empty<HomeworkSubmissionAttempt>());

            return new StudentHomeworkHistoryDto
            {
                Id = row.Id,
                AssignmentId = row.AssignmentId,
                AssignmentTitle = row.AssignmentTitle,
                ClassId = row.ClassId,
                ClassCode = row.ClassCode,
                ClassTitle = row.ClassTitle,
                DueAt = row.DueAt,
                Status = row.Status.ToString(),
                SubmittedAt = row.SubmittedAt,
                GradedAt = row.GradedAt,
                Score = row.Score,
                MaxScore = row.MaxScore,
                TeacherFeedback = row.TeacherFeedback,
                AllowResubmit = row.MaxAttempts > 1,
                MaxAttempts = row.MaxAttempts,
                AttemptCount = attempts.Count,
                Attempts = attempts,
                IsLate = row.Status == HomeworkStatus.Late
            };
        }).ToList();

        var page = new Page<StudentHomeworkHistoryDto>(
            homeworks,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetStudentHomeworkHistoryResponse
        {
            Homeworks = page
        };
    }
}
