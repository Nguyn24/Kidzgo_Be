using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Student.StartExerciseSubmission;

public sealed class StartExerciseSubmissionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<StartExerciseSubmissionCommand, StartExerciseSubmissionResponse>
{
    public async Task<Result<StartExerciseSubmissionResponse>> Handle(StartExerciseSubmissionCommand command, CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;
        if (!studentId.HasValue)
        {
            return Result.Failure<StartExerciseSubmissionResponse>(ProfileErrors.StudentNotFound);
        }

        var exercise = await context.Exercises
            .FirstOrDefaultAsync(e => e.Id == command.ExerciseId && !e.IsDeleted && e.IsActive, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure<StartExerciseSubmissionResponse>(ExerciseErrors.NotFound(command.ExerciseId));
        }

        var existing = await context.ExerciseSubmissions
            .FirstOrDefaultAsync(s => s.ExerciseId == command.ExerciseId && s.StudentProfileId == studentId.Value, cancellationToken);

        if (existing is not null)
        {
            return new StartExerciseSubmissionResponse
            {
                SubmissionId = existing.Id,
                ExerciseId = existing.ExerciseId
            };
        }

        var now = DateTime.UtcNow;
        var submission = new ExerciseSubmission
        {
            Id = Guid.NewGuid(),
            ExerciseId = command.ExerciseId,
            StudentProfileId = studentId.Value,
            Answers = null,
            Score = null,
            SubmittedAt = now, // will be updated on actual submit; keep non-null due to schema
            GradedAt = null,
            GradedBy = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ExerciseSubmissions.Add(submission);
        await context.SaveChangesAsync(cancellationToken);

        return new StartExerciseSubmissionResponse
        {
            SubmissionId = submission.Id,
            ExerciseId = submission.ExerciseId
        };
    }
}


