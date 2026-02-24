using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.CreateHomeworkAssignment;

public sealed class CreateHomeworkAssignmentCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateHomeworkAssignmentCommand, CreateHomeworkAssignmentResponse>
{
    public async Task<Result<CreateHomeworkAssignmentResponse>> Handle(
        CreateHomeworkAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidTitle);
        }

        // Validate MaxScore
        if (command.MaxScore.HasValue && command.MaxScore.Value <= 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidMaxScore);
        }

        // Validate RewardStars
        if (command.RewardStars.HasValue && command.RewardStars.Value < 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidRewardStars);
        }

        // Validate class exists
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.ClassNotFound);
        }

        // Validate session if provided
        if (command.SessionId.HasValue)
        {
            var session = await context.Sessions
                .FirstOrDefaultAsync(s => s.Id == command.SessionId.Value, cancellationToken);

            if (session is null)
            {
                return Result.Failure<CreateHomeworkAssignmentResponse>(
                    HomeworkErrors.SessionNotFound(command.SessionId));
            }

            // Validate session belongs to the class
            if (session.ClassId != command.ClassId)
            {
                return Result.Failure<CreateHomeworkAssignmentResponse>(
                    HomeworkErrors.SessionNotFound(command.SessionId));
            }
        }

        // Validate mission if provided
        if (command.MissionId.HasValue)
        {
            var mission = await context.Missions
                .FirstOrDefaultAsync(m => m.Id == command.MissionId.Value, cancellationToken);

            if (mission is null)
            {
                return Result.Failure<CreateHomeworkAssignmentResponse>(
                    HomeworkErrors.MissionNotFound(command.MissionId));
            }
        }

        // Validate due date
        if (command.DueAt.HasValue && command.DueAt.Value <= DateTime.UtcNow)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidDueDate);
        }

        // Convert DueAt to UTC if provided (PostgreSQL requires UTC for timestamp with time zone)
        var dueAtUtc = command.DueAt.HasValue
            ? DateTime.SpecifyKind(command.DueAt.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        // Get current user ID from context
        var currentUserId = userContext.UserId;

        var now = DateTime.UtcNow;
        var homework = new HomeworkAssignment
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            SessionId = command.SessionId,
            Title = command.Title,
            Description = command.Description,
            DueAt = dueAtUtc,
            Book = command.Book,
            Pages = command.Pages,
            Skills = command.Skills,
            SubmissionType = command.SubmissionType,
            MaxScore = command.MaxScore ?? 10,
            RewardStars = command.RewardStars,
            MissionId = command.MissionId,
            Instructions = command.Instructions,
            ExpectedAnswer = command.ExpectedAnswer,
            Rubric = command.Rubric,
            CreatedBy = currentUserId,
            CreatedAt = now
        };

        context.HomeworkAssignments.Add(homework);

        // Auto-assign to all active students in the class
        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.ClassId == command.ClassId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.ClassHasNoActiveStudents(command.ClassId));
        }

        // If session is specified, only assign to students in that session
        var studentIdsToAssign = activeEnrollments;
        if (command.SessionId.HasValue)
        {
            var sessionAttendances = await context.Attendances
                .Where(a => a.SessionId == command.SessionId.Value)
                .Select(a => a.StudentProfileId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Only assign to students who were in the session
            studentIdsToAssign = activeEnrollments
                .Where(id => sessionAttendances.Contains(id))
                .ToList();
        }

        var homeworkStudents = studentIdsToAssign.Select(studentId => new HomeworkStudent
        {
            Id = Guid.NewGuid(),
            AssignmentId = homework.Id,
            StudentProfileId = studentId,
            Status = HomeworkStatus.Assigned
        }).ToList();

        context.HomeworkStudents.AddRange(homeworkStudents);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateHomeworkAssignmentResponse
        {
            Id = homework.Id,
            ClassId = homework.ClassId,
            SessionId = homework.SessionId,
            Title = homework.Title,
            Description = homework.Description,
            DueAt = homework.DueAt,
            Book = homework.Book,
            Pages = homework.Pages,
            Skills = homework.Skills,
            SubmissionType = homework.SubmissionType.ToString(),
            MaxScore = homework.MaxScore,
            RewardStars = homework.RewardStars,
            MissionId = homework.MissionId,
            Instructions = homework.Instructions,
            ExpectedAnswer = homework.ExpectedAnswer,
            Rubric = homework.Rubric,
            AttachmentUrl = homework.AttachmentUrl,
            CreatedAt = homework.CreatedAt,
            AssignedStudentsCount = homeworkStudents.Count
        };
    }
}

