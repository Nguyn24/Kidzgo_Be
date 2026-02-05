using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Exercises.Submissions.AutoGradeMultipleChoice;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Submissions.SubmitExerciseSubmission;

public sealed class SubmitExerciseSubmissionCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ISender mediator
) : ICommandHandler<SubmitExerciseSubmissionCommand, SubmitExerciseSubmissionResponse>
{
    public async Task<Result<SubmitExerciseSubmissionResponse>> Handle(
        SubmitExerciseSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await context.ExerciseSubmissions
            .Include(s => s.Exercise)
                .ThenInclude(e => e.Questions)
            .FirstOrDefaultAsync(s => s.Id == command.SubmissionId, cancellationToken);

        if (submission is null)
        {
            return Result.Failure<SubmitExerciseSubmissionResponse>(ExerciseErrors.SubmissionNotFound(command.SubmissionId));
        }

        // Students can only submit their own submission
        if (userContext.StudentId.HasValue && submission.StudentProfileId != userContext.StudentId.Value)
        {
            return Result.Failure<SubmitExerciseSubmissionResponse>(ExerciseErrors.SubmissionUnauthorized);
        }

        // Mark submitted
        var now = DateTime.UtcNow;
        submission.SubmittedAt = now;
        submission.UpdatedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        // UC-147: auto-grade multiple choice immediately on submit
        var autoGradeResult = await mediator.Send(
            new AutoGradeMultipleChoiceCommand { SubmissionId = submission.Id },
            cancellationToken);

        if (autoGradeResult.IsFailure)
        {
            // If auto-grade fails, still return submitted result (submission is already persisted)
            return new SubmitExerciseSubmissionResponse
            {
                SubmissionId = submission.Id,
                SubmittedAt = submission.SubmittedAt,
                Score = submission.Score
            };
        }

        return new SubmitExerciseSubmissionResponse
        {
            SubmissionId = submission.Id,
            SubmittedAt = submission.SubmittedAt,
            Score = autoGradeResult.Value.Score
        };
    }
}


