using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Submissions.AutoGradeMultipleChoice;

public sealed class AutoGradeMultipleChoiceCommandHandler(
    IDbContext context
) : ICommandHandler<AutoGradeMultipleChoiceCommand, AutoGradeMultipleChoiceResponse>
{
    public async Task<Result<AutoGradeMultipleChoiceResponse>> Handle(
        AutoGradeMultipleChoiceCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await context.ExerciseSubmissions
            .Include(s => s.Exercise)
                .ThenInclude(e => e.Questions)
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<AutoGradeMultipleChoiceResponse>(ExerciseErrors.SubmissionNotFound(command.SubmissionId));
        }

        if (submission.Exercise.IsDeleted)
        {
            return Result.Failure<AutoGradeMultipleChoiceResponse>(ExerciseErrors.NotFound(submission.ExerciseId));
        }

        var now = DateTime.UtcNow;
        var autoGradedCount = 0;

        foreach (var q in submission.Exercise.Questions.Where(q => q.QuestionType == QuestionType.MultipleChoice))
        {
            var answer = submission.SubmissionAnswers.FirstOrDefault(a => a.QuestionId == q.Id);
            if (answer is null)
            {
                // no answer recorded => skip
                continue;
            }

            // Compare raw string answer with correct answer (both stored as string)
            var isCorrect = !string.IsNullOrWhiteSpace(q.CorrectAnswer) &&
                            string.Equals(answer.Answer?.Trim(), q.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

            answer.IsCorrect = isCorrect;
            answer.PointsAwarded = isCorrect ? q.Points : 0;
            autoGradedCount++;
        }

        // Recalculate total score
        submission.Score = submission.SubmissionAnswers
            .Where(a => a.PointsAwarded.HasValue)
            .Sum(a => a.PointsAwarded!.Value);

        submission.GradedAt = now;
        submission.GradedBy = null; // system auto-grade
        submission.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new AutoGradeMultipleChoiceResponse
        {
            SubmissionId = submission.Id,
            Score = submission.Score,
            AutoGradedAnswerCount = autoGradedCount
        };
    }
}


