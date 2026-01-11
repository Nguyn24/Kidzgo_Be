using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.StartExamSubmission;

public sealed class StartExamSubmissionCommandHandler(
    IDbContext context
) : ICommandHandler<StartExamSubmissionCommand, StartExamSubmissionResponse>
{
    public async Task<Result<StartExamSubmissionResponse>> Handle(
        StartExamSubmissionCommand command,
        CancellationToken cancellationToken)
    {
        // Check if exam exists
        var exam = await context.Exams
            .FirstOrDefaultAsync(e => e.Id == command.ExamId, cancellationToken);

        if (exam is null)
        {
            return Result.Failure<StartExamSubmissionResponse>(
                ExamSubmissionErrors.ExamNotFound(command.ExamId));
        }

        // Check if submission already exists
        var existingSubmission = await context.ExamSubmissions
            .FirstOrDefaultAsync(s => s.ExamId == command.ExamId && 
                                      s.StudentProfileId == command.StudentProfileId, 
                                      cancellationToken);

        if (existingSubmission != null)
        {
            return Result.Failure<StartExamSubmissionResponse>(
                ExamSubmissionErrors.AlreadyStarted(command.ExamId, command.StudentProfileId));
        }

        var now = DateTime.UtcNow;

        // Check scheduled start time
        if (exam.ScheduledStartTime.HasValue)
        {
            var scheduledTime = exam.ScheduledStartTime.Value;
            var timeDifference = (now - scheduledTime).TotalMinutes;

            // Check if exam has started
            if (now < scheduledTime)
            {
                return Result.Failure<StartExamSubmissionResponse>(
                    ExamSubmissionErrors.ExamNotStarted);
            }

            // Check late start tolerance
            if (!exam.AllowLateStart && timeDifference > 0)
            {
                return Result.Failure<StartExamSubmissionResponse>(
                    ExamSubmissionErrors.LateStartNotAllowed);
            }

            if (exam.AllowLateStart && exam.LateStartToleranceMinutes.HasValue)
            {
                if (timeDifference > exam.LateStartToleranceMinutes.Value)
                {
                    return Result.Failure<StartExamSubmissionResponse>(
                        ExamSubmissionErrors.TooLateToStart);
                }
            }
        }

        var submission = new ExamSubmission
        {
            Id = Guid.NewGuid(),
            ExamId = command.ExamId,
            StudentProfileId = command.StudentProfileId,
            ActualStartTime = now,
            Status = ExamSubmissionStatus.InProgress
        };

        context.ExamSubmissions.Add(submission);
        await context.SaveChangesAsync(cancellationToken);

        return new StartExamSubmissionResponse
        {
            Id = submission.Id,
            ExamId = submission.ExamId,
            StudentProfileId = submission.StudentProfileId,
            ActualStartTime = submission.ActualStartTime,
            Status = submission.Status,
            ScheduledStartTime = exam.ScheduledStartTime,
            TimeLimitMinutes = exam.TimeLimitMinutes
        };
    }
}


