using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExamSubmission;

public sealed class GetExamSubmissionQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamSubmissionQuery, GetExamSubmissionResponse>
{
    public async Task<Result<GetExamSubmissionResponse>> Handle(
        GetExamSubmissionQuery query,
        CancellationToken cancellationToken)
    {
        var submission = await context.ExamSubmissions
            .Include(s => s.StudentProfile)
            .Include(s => s.GradedByUser)
            .FirstOrDefaultAsync(s => s.Id == query.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<GetExamSubmissionResponse>(
                ExamSubmissionErrors.NotFound(query.SubmissionId));
        }

        List<SubmissionAnswerDto>? answers = null;

        if (query.IncludeAnswers)
        {
            var answersQuery = context.ExamSubmissionAnswers
                .Where(a => a.SubmissionId == query.SubmissionId)
                .Include(a => a.Question)
                .OrderBy(a => a.Question.OrderIndex);

            answers = await answersQuery
                .Select(a => new SubmissionAnswerDto
                {
                    Id = a.Id,
                    QuestionId = a.QuestionId,
                    QuestionOrderIndex = a.Question.OrderIndex,
                    QuestionText = a.Question.QuestionText,
                    QuestionType = a.Question.QuestionType,
                    QuestionOptions = a.Question.Options,
                    QuestionCorrectAnswer = query.ShowCorrectAnswers ? a.Question.CorrectAnswer : null,
                    QuestionPoints = a.Question.Points,
                    Answer = a.Answer,
                    IsCorrect = a.IsCorrect,
                    PointsAwarded = a.PointsAwarded,
                    TeacherFeedback = a.TeacherFeedback,
                    AnsweredAt = a.AnsweredAt
                })
                .ToListAsync(cancellationToken);
        }

        return new GetExamSubmissionResponse
        {
            Id = submission.Id,
            ExamId = submission.ExamId,
            StudentProfileId = submission.StudentProfileId,
            StudentName = submission.StudentProfile?.DisplayName,
            ActualStartTime = submission.ActualStartTime,
            SubmittedAt = submission.SubmittedAt,
            AutoSubmittedAt = submission.AutoSubmittedAt,
            TimeSpentMinutes = submission.TimeSpentMinutes,
            AutoScore = submission.AutoScore,
            FinalScore = submission.FinalScore,
            GradedBy = submission.GradedBy,
            GradedByName = submission.GradedByUser?.Name,
            GradedAt = submission.GradedAt,
            TeacherComment = submission.TeacherComment,
            Status = submission.Status,
            Answers = answers
        };
    }
}


