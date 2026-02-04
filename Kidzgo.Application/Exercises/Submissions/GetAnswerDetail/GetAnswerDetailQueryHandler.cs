using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Submissions.GetAnswerDetail;

public sealed class GetAnswerDetailQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetAnswerDetailQuery, GetAnswerDetailResponse>
{
    public async Task<Result<GetAnswerDetailResponse>> Handle(GetAnswerDetailQuery query, CancellationToken cancellationToken)
    {
        var submission = await context.ExerciseSubmissions
            .Include(s => s.Exercise)
                .ThenInclude(e => e.Questions)
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.Id == query.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<GetAnswerDetailResponse>(ExerciseErrors.SubmissionNotFound(query.SubmissionId));
        }

        // Student can only view their own submission
        if (userContext.StudentId.HasValue && submission.StudentProfileId != userContext.StudentId.Value)
        {
            return Result.Failure<GetAnswerDetailResponse>(ExerciseErrors.SubmissionUnauthorized);
        }

        var question = submission.Exercise.Questions.FirstOrDefault(q => q.Id == query.QuestionId);
        if (question is null)
        {
            return Result.Failure<GetAnswerDetailResponse>(ExerciseErrors.QuestionNotFound(query.QuestionId));
        }

        var answer = submission.SubmissionAnswers.FirstOrDefault(a => a.QuestionId == query.QuestionId);
        if (answer is null)
        {
            // Return empty answer view instead of 404 to keep UX simple
            return new GetAnswerDetailResponse
            {
                SubmissionId = submission.Id,
                ExerciseId = submission.ExerciseId,
                StudentProfileId = submission.StudentProfileId,
                QuestionId = question.Id,
                OrderIndex = question.OrderIndex,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType.ToString(),
                Options = question.Options,
                CorrectAnswer = question.CorrectAnswer,
                Points = question.Points,
                Answer = string.Empty,
                IsCorrect = false,
                PointsAwarded = null,
                TeacherFeedback = null
            };
        }

        return new GetAnswerDetailResponse
        {
            SubmissionId = submission.Id,
            ExerciseId = submission.ExerciseId,
            StudentProfileId = submission.StudentProfileId,
            QuestionId = question.Id,
            OrderIndex = question.OrderIndex,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType.ToString(),
            Options = question.Options,
            CorrectAnswer = question.CorrectAnswer,
            Points = question.Points,
            Answer = answer.Answer,
            IsCorrect = answer.IsCorrect,
            PointsAwarded = answer.PointsAwarded,
            TeacherFeedback = answer.TeacherFeedback
        };
    }
}


