using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Missions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.MarkHomeworkLateOrMissing;

public sealed class MarkHomeworkLateOrMissingCommandHandler(
    IDbContext context,
    IGamificationService gamificationService
) : ICommandHandler<MarkHomeworkLateOrMissingCommand, MarkHomeworkLateOrMissingResponse>
{
    public async Task<Result<MarkHomeworkLateOrMissingResponse>> Handle(
        MarkHomeworkLateOrMissingCommand command,
        CancellationToken cancellationToken)
    {
        // Validate status is LATE or MISSING
        if (command.Status != HomeworkStatus.Late && command.Status != HomeworkStatus.Missing)
        {
            return Result.Failure<MarkHomeworkLateOrMissingResponse>(
                HomeworkErrors.SubmissionInvalidStatus);
        }

        var homeworkStudent = await context.HomeworkStudents
            .FirstOrDefaultAsync(hs => hs.Id == command.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<MarkHomeworkLateOrMissingResponse>(
                HomeworkErrors.SubmissionNotFound(command.HomeworkStudentId));
        }

        // Only allow marking as LATE or MISSING if current status is ASSIGNED or LATE (can change LATE to MISSING)
        if (homeworkStudent.Status != HomeworkStatus.Assigned && 
            !(homeworkStudent.Status == HomeworkStatus.Late && command.Status == HomeworkStatus.Missing))
        {
            return Result.Failure<MarkHomeworkLateOrMissingResponse>(
                HomeworkErrors.SubmissionInvalidStatusTransition(
                    homeworkStudent.Status.ToString(), 
                    command.Status.ToString()));
        }

        homeworkStudent.Status = command.Status;

        await context.SaveChangesAsync(cancellationToken);

        await HomeworkMissionProgressTracker.TrackAsync(
            context,
            gamificationService,
            homeworkStudent.StudentProfileId,
            VietnamTime.UtcNow(),
            cancellationToken);

        return new MarkHomeworkLateOrMissingResponse
        {
            Id = homeworkStudent.Id,
            Status = homeworkStudent.Status.ToString()
        };
    }
}

