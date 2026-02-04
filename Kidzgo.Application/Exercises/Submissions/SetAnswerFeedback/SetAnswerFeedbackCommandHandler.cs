using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Submissions.SetAnswerFeedback;

public sealed class SetAnswerFeedbackCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SetAnswerFeedbackCommand>
{
    public async Task<Result> Handle(SetAnswerFeedbackCommand command, CancellationToken cancellationToken)
    {
        var submission = await context.ExerciseSubmissions
            .Include(s => s.Exercise)
                .ThenInclude(e => e.Questions)
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure(ExerciseErrors.SubmissionNotFound(command.SubmissionId));
        }

        if (submission.Exercise.IsDeleted)
        {
            return Result.Failure(ExerciseErrors.NotFound(submission.ExerciseId));
        }

        var question = submission.Exercise.Questions.FirstOrDefault(q => q.Id == command.QuestionId);
        if (question is null)
        {
            return Result.Failure(ExerciseErrors.QuestionNotFound(command.QuestionId));
        }

        var answer = submission.SubmissionAnswers.FirstOrDefault(a => a.QuestionId == command.QuestionId);
        if (answer is null)
        {
            answer = new ExerciseSubmissionAnswer
            {
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                QuestionId = command.QuestionId,
                Answer = string.Empty,
                IsCorrect = false,
                PointsAwarded = null,
                TeacherFeedback = command.TeacherFeedback
            };
            context.ExerciseSubmissionAnswers.Add(answer);
        }
        else
        {
            answer.TeacherFeedback = command.TeacherFeedback;
        }

        var now = DateTime.UtcNow;
        submission.GradedAt ??= now;
        submission.GradedBy ??= userContext.UserId;
        submission.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}


