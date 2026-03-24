using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.UpdateHomeworkAssignment;

public sealed class UpdateHomeworkAssignmentCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateHomeworkAssignmentCommand, UpdateHomeworkAssignmentResponse>
{
    public async Task<Result<UpdateHomeworkAssignmentResponse>> Handle(
        UpdateHomeworkAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await context.HomeworkAssignments
            .Include(h => h.HomeworkStudents)
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken);

        if (homework is null)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.NotFound(command.Id));
        }

        // Check if there are any submitted or graded submissions
        bool hasSubmittedOrGraded = homework.HomeworkStudents
            .Any(hs => hs.Status == HomeworkStatus.Submitted || hs.Status == HomeworkStatus.Graded);

        if (hasSubmittedOrGraded)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.CannotUpdate);
        }

        // Validate mission if provided
        if (command.MissionId.HasValue)
        {
            var mission = await context.Missions
                .FirstOrDefaultAsync(m => m.Id == command.MissionId.Value, cancellationToken);

            if (mission is null)
            {
                return Result.Failure<UpdateHomeworkAssignmentResponse>(
                    HomeworkErrors.MissionNotFound(command.MissionId));
            }
        }

        // Validate due date
        if (command.DueAt.HasValue && command.DueAt.Value <= DateTime.UtcNow)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidDueDate);
        }

        // Convert DueAt to UTC if provided (PostgreSQL requires UTC for timestamp with time zone)
        var dueAtUtc = command.DueAt.HasValue
            ? DateTime.SpecifyKind(command.DueAt.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        // Validate Title if provided
        if (command.Title != null && string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidTitle);
        }

        // Validate MaxScore if provided
        if (command.MaxScore.HasValue && command.MaxScore.Value <= 0)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidMaxScore);
        }

        // Validate RewardStars if provided
        if (command.RewardStars.HasValue && command.RewardStars.Value < 0)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidRewardStars);
        }

        if (command.TimeLimitMinutes.HasValue && command.TimeLimitMinutes.Value <= 0)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidTimeLimitMinutes);
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(command.Title))
        {
            homework.Title = command.Title;
        }

        if (command.Description != null)
        {
            homework.Description = command.Description;
        }

        if (command.DueAt.HasValue)
        {
            homework.DueAt = dueAtUtc;
        }

        if (command.Book != null)
        {
            homework.Book = command.Book;
        }

        if (command.Pages != null)
        {
            homework.Pages = command.Pages;
        }

        if (command.Skills != null)
        {
            homework.Skills = command.Skills;
        }

        if (command.SubmissionType.HasValue)
        {
            homework.SubmissionType = command.SubmissionType.Value;
        }

        if (command.MaxScore.HasValue)
        {
            homework.MaxScore = command.MaxScore.Value;
        }

        if (command.RewardStars.HasValue)
        {
            homework.RewardStars = command.RewardStars;
        }

        if (command.TimeLimitMinutes.HasValue)
        {
            homework.TimeLimitMinutes = command.TimeLimitMinutes;
        }

        if (command.AllowResubmit.HasValue)
        {
            homework.AllowResubmit = command.AllowResubmit.Value;
        }

        if (command.MissionId.HasValue)
        {
            homework.MissionId = command.MissionId;
        }

        if (command.Instructions != null)
        {
            homework.Instructions = command.Instructions;
        }

        if (command.ExpectedAnswer != null)
        {
            homework.ExpectedAnswer = command.ExpectedAnswer;
        }

        if (command.Rubric != null)
        {
            homework.Rubric = command.Rubric;
        }

        // Update LATE status for submissions if due date changed
        if (command.DueAt.HasValue && command.DueAt.Value < homework.DueAt)
        {
            var now = DateTime.UtcNow;
            var lateSubmissions = homework.HomeworkStudents
                .Where(hs => hs.Status == HomeworkStatus.Assigned && 
                            homework.DueAt.HasValue && 
                            now > homework.DueAt.Value)
                .ToList();

            foreach (var submission in lateSubmissions)
            {
                submission.Status = HomeworkStatus.Late;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateHomeworkAssignmentResponse
        {
            Id = homework.Id,
            Title = homework.Title,
            DueAt = homework.DueAt
        };
    }
}

