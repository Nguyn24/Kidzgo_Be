using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Submissions.GradeTextInput;

public sealed class GradeTextInputCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<GradeTextInputCommand, GradeTextInputResponse>
{
    public async Task<Result<GradeTextInputResponse>> Handle(
        GradeTextInputCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await context.ExerciseSubmissions
            .Include(s => s.Exercise)
                .ThenInclude(e => e.Questions)
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<GradeTextInputResponse>(ExerciseErrors.SubmissionNotFound(command.SubmissionId));
        }

        if (submission.Exercise.IsDeleted)
        {
            return Result.Failure<GradeTextInputResponse>(ExerciseErrors.NotFound(submission.ExerciseId));
        }

        var now = DateTime.UtcNow;
        var gradedCount = 0;

        foreach (var item in command.AnswerGrades)
        {
            if (item.PointsAwarded < 0)
            {
                return Result.Failure<GradeTextInputResponse>(ExerciseErrors.InvalidPoints);
            }

            var q = submission.Exercise.Questions.FirstOrDefault(q => q.Id == item.QuestionId);
            if (q is null)
            {
                return Result.Failure<GradeTextInputResponse>(ExerciseErrors.QuestionNotFound(item.QuestionId));
            }

            // Only allow grading text input here
            if (q.QuestionType != QuestionType.TextInput)
            {
                continue;
            }

            var answer = submission.SubmissionAnswers.FirstOrDefault(a => a.QuestionId == item.QuestionId);
            if (answer is null)
            {
                // Create an empty answer row if missing (keeps data consistent for later review)
                answer = new ExerciseSubmissionAnswer
                {
                    Id = Guid.NewGuid(),
                    SubmissionId = submission.Id,
                    QuestionId = item.QuestionId,
                    Answer = string.Empty,
                    IsCorrect = false,
                    PointsAwarded = null,
                    TeacherFeedback = null
                };
                context.ExerciseSubmissionAnswers.Add(answer);
                submission.SubmissionAnswers.Add(answer);
            }

            answer.PointsAwarded = item.PointsAwarded;
            answer.TeacherFeedback = item.TeacherFeedback;
            // For writing/text input, IsCorrect is optional; keep existing value.
            gradedCount++;
        }

        submission.Score = submission.SubmissionAnswers
            .Where(a => a.PointsAwarded.HasValue)
            .Sum(a => a.PointsAwarded!.Value);

        submission.GradedAt = now;
        submission.GradedBy = userContext.UserId;
        submission.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new GradeTextInputResponse
        {
            SubmissionId = submission.Id,
            Score = submission.Score,
            GradedAnswerCount = gradedCount
        };
    }
}


