using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.CreateExam;

public sealed class CreateExamCommandHandler(
    IDbContext context,
    Kidzgo.Application.Abstraction.Authentication.IUserContext userContext
) : ICommandHandler<CreateExamCommand, CreateExamResponse>
{
    public async Task<Result<CreateExamResponse>> Handle(CreateExamCommand command, CancellationToken cancellationToken)
    {
        // Check if class exists and is active
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId && c.Status == ClassStatus.Active, cancellationToken);

        if (classEntity == null)
        {
            return Result.Failure<CreateExamResponse>(
                ExamErrors.ClassNotFound);
        }

        // Get current user
        var createdBy = userContext.UserId;
        var now = DateTime.UtcNow;

        // Convert ScheduledStartTime to UTC if provided
        DateTime? scheduledStartTimeUtc = null;
        if (command.ScheduledStartTime.HasValue)
        {
            var scheduledTime = command.ScheduledStartTime.Value;
            if (scheduledTime.Kind == DateTimeKind.Local)
            {
                scheduledStartTimeUtc = scheduledTime.ToUniversalTime();
            }
            else if (scheduledTime.Kind == DateTimeKind.Unspecified)
            {
                // Assume it's already in UTC if unspecified
                scheduledStartTimeUtc = DateTime.SpecifyKind(scheduledTime, DateTimeKind.Utc);
            }
            else
            {
                scheduledStartTimeUtc = scheduledTime;
            }
        }

        // Create exam
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            ExamType = command.ExamType,
            Date = command.Date,
            MaxScore = command.MaxScore,
            Description = command.Description,
            ScheduledStartTime = scheduledStartTimeUtc,
            TimeLimitMinutes = command.TimeLimitMinutes,
            AllowLateStart = command.AllowLateStart,
            LateStartToleranceMinutes = command.LateStartToleranceMinutes,
            AutoSubmitOnTimeLimit = command.AutoSubmitOnTimeLimit,
            PreventCopyPaste = command.PreventCopyPaste,
            PreventNavigation = command.PreventNavigation,
            ShowResultsImmediately = command.ShowResultsImmediately,
            CreatedBy = createdBy,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Exams.Add(exam);
        await context.SaveChangesAsync(cancellationToken);

        // Get created by user name
        var createdByUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == createdBy, cancellationToken);

        return new CreateExamResponse
        {
            Id = exam.Id,
            ClassId = exam.ClassId,
            ClassCode = classEntity.Code,
            ClassTitle = classEntity.Title,
            ExamType = exam.ExamType,
            Date = exam.Date,
            MaxScore = exam.MaxScore,
            Description = exam.Description,
            ScheduledStartTime = exam.ScheduledStartTime,
            TimeLimitMinutes = exam.TimeLimitMinutes,
            AllowLateStart = exam.AllowLateStart,
            LateStartToleranceMinutes = exam.LateStartToleranceMinutes,
            AutoSubmitOnTimeLimit = exam.AutoSubmitOnTimeLimit,
            PreventCopyPaste = exam.PreventCopyPaste,
            PreventNavigation = exam.PreventNavigation,
            ShowResultsImmediately = exam.ShowResultsImmediately,
            CreatedBy = exam.CreatedBy,
            CreatedByName = createdByUser?.Name,
            CreatedAt = exam.CreatedAt
        };
    }
}

