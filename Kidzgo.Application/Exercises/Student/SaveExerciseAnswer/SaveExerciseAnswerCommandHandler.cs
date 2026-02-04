using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Student.SaveExerciseAnswer;

public sealed class SaveExerciseAnswerCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SaveExerciseAnswerCommand>
{
    public async Task<Result> Handle(SaveExerciseAnswerCommand command, CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;
        if (!studentId.HasValue)
        {
            return Result.Failure(ProfileErrors.StudentNotFound);
        }

        var submission = await context.ExerciseSubmissions
            .Include(s => s.Exercise)
                .ThenInclude(e => e.Questions)
            .Include(s => s.SubmissionAnswers)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure(ExerciseErrors.SubmissionNotFound(command.SubmissionId));
        }

        if (submission.StudentProfileId != studentId.Value)
        {
            return Result.Failure(ExerciseErrors.SubmissionUnauthorized);
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
                Answer = command.Answer,
                IsCorrect = false,
                PointsAwarded = null,
                TeacherFeedback = null
            };
            context.ExerciseSubmissionAnswers.Add(answer);
        }
        else
        {
            answer.Answer = command.Answer;
            // reset auto-grade fields until submit/auto-grade runs again
            answer.IsCorrect = false;
            answer.PointsAwarded = null;
        }

        submission.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}


