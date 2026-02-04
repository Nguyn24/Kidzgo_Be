using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Student.GetMyExerciseResult;

public sealed class GetMyExerciseResultQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMyExerciseResultQuery, GetMyExerciseResultResponse>
{
    public async Task<Result<GetMyExerciseResultResponse>> Handle(GetMyExerciseResultQuery query, CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;
        if (!studentId.HasValue)
        {
            return Result.Failure<GetMyExerciseResultResponse>(ExerciseErrors.SubmissionUnauthorized);
        }

        var submission = await context.ExerciseSubmissions
            .Include(s => s.Exercise)
                .ThenInclude(e => e.Questions)
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.Id == query.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<GetMyExerciseResultResponse>(ExerciseErrors.SubmissionNotFound(query.SubmissionId));
        }

        if (submission.StudentProfileId != studentId.Value)
        {
            return Result.Failure<GetMyExerciseResultResponse>(ExerciseErrors.SubmissionUnauthorized);
        }

        return new GetMyExerciseResultResponse
        {
            SubmissionId = submission.Id,
            ExerciseId = submission.ExerciseId,
            ExerciseTitle = submission.Exercise.Title,
            Score = submission.Score,
            SubmittedAt = submission.SubmittedAt,
            GradedAt = submission.GradedAt,
            Answers = submission.Exercise.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q =>
                {
                    var a = submission.SubmissionAnswers.FirstOrDefault(x => x.QuestionId == q.Id);
                    return new MyExerciseAnswerResultItem
                    {
                        QuestionId = q.Id,
                        OrderIndex = q.OrderIndex,
                        QuestionText = q.QuestionText,
                        QuestionType = q.QuestionType.ToString(),
                        Points = q.Points,
                        Answer = a?.Answer ?? string.Empty,
                        IsCorrect = a?.IsCorrect ?? false,
                        PointsAwarded = a?.PointsAwarded,
                        TeacherFeedback = a?.TeacherFeedback
                    };
                })
                .ToList()
        };
    }
}


