using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.UpdateExam;

public sealed class UpdateExamCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateExamCommand, UpdateExamResponse>
{
    public async Task<Result<UpdateExamResponse>> Handle(UpdateExamCommand command, CancellationToken cancellationToken)
    {
        var exam = await context.Exams
            .Include(e => e.Class)
            .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken);

        if (exam == null)
        {
            return Result.Failure<UpdateExamResponse>(
                ExamErrors.NotFound(command.Id));
        }

        // Update fields if provided
        if (command.ExamType.HasValue)
        {
            exam.ExamType = command.ExamType.Value;
        }

        if (command.Date.HasValue)
        {
            exam.Date = command.Date.Value;
        }

        if (command.MaxScore.HasValue)
        {
            exam.MaxScore = command.MaxScore;
        }

        if (command.Description != null)
        {
            exam.Description = command.Description;
        }

        // Update time settings
        if (command.ScheduledStartTime.HasValue)
        {
            var scheduledTime = command.ScheduledStartTime.Value;
            DateTime scheduledTimeUtc;
            
            if (scheduledTime.Kind == DateTimeKind.Local)
            {
                scheduledTimeUtc = scheduledTime.ToUniversalTime();
            }
            else if (scheduledTime.Kind == DateTimeKind.Unspecified)
            {
                // Assume it's already in UTC if unspecified
                scheduledTimeUtc = DateTime.SpecifyKind(scheduledTime, DateTimeKind.Utc);
            }
            else
            {
                scheduledTimeUtc = scheduledTime;
            }
            
            exam.ScheduledStartTime = scheduledTimeUtc;
        }

        if (command.TimeLimitMinutes.HasValue)
        {
            exam.TimeLimitMinutes = command.TimeLimitMinutes;
        }

        if (command.AllowLateStart.HasValue)
        {
            exam.AllowLateStart = command.AllowLateStart.Value;
        }

        if (command.LateStartToleranceMinutes.HasValue)
        {
            exam.LateStartToleranceMinutes = command.LateStartToleranceMinutes;
        }

        // Update exam settings
        if (command.AutoSubmitOnTimeLimit.HasValue)
        {
            exam.AutoSubmitOnTimeLimit = command.AutoSubmitOnTimeLimit.Value;
        }

        if (command.PreventCopyPaste.HasValue)
        {
            exam.PreventCopyPaste = command.PreventCopyPaste.Value;
        }

        if (command.PreventNavigation.HasValue)
        {
            exam.PreventNavigation = command.PreventNavigation.Value;
        }

        if (command.ShowResultsImmediately.HasValue)
        {
            exam.ShowResultsImmediately = command.ShowResultsImmediately.Value;
        }

        exam.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateExamResponse
        {
            Id = exam.Id,
            ClassId = exam.ClassId,
            ClassCode = exam.Class.Code,
            ClassTitle = exam.Class.Title,
            ExamType = exam.ExamType.ToString(),
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
            UpdatedAt = exam.UpdatedAt
        };
    }
}

