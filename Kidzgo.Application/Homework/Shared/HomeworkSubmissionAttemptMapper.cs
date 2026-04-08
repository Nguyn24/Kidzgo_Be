using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.Homework.Shared;

internal static class HomeworkSubmissionAttemptMapper
{
    public static List<HomeworkSubmissionAttempt> BuildSnapshots(
        HomeworkStudent homeworkStudent,
        IReadOnlyCollection<HomeworkSubmissionAttempt> persistedAttempts)
    {
        if (persistedAttempts.Count > 0)
        {
            return persistedAttempts
                .OrderByDescending(a => a.AttemptNumber)
                .ToList();
        }

        return HasLegacyAttempt(homeworkStudent)
            ? new List<HomeworkSubmissionAttempt> { BuildLegacyAttempt(homeworkStudent) }
            : new List<HomeworkSubmissionAttempt>();
    }

    public static List<HomeworkSubmissionAttemptDto> BuildDtos(
        HomeworkStudent homeworkStudent,
        IReadOnlyCollection<HomeworkSubmissionAttempt> persistedAttempts)
    {
        var attempts = BuildSnapshots(homeworkStudent, persistedAttempts);
        var latestAttemptNumber = attempts.Count == 0
            ? 0
            : attempts.Max(a => a.AttemptNumber);

        return attempts
            .Select(a => ToDto(a, a.AttemptNumber == latestAttemptNumber))
            .ToList();
    }

    public static bool HasLegacyAttempt(HomeworkStudent homeworkStudent)
    {
        return homeworkStudent.SubmittedAt.HasValue ||
               homeworkStudent.GradedAt.HasValue ||
               homeworkStudent.Score.HasValue ||
               !string.IsNullOrWhiteSpace(homeworkStudent.TextAnswer) ||
               !string.IsNullOrWhiteSpace(homeworkStudent.AttachmentUrl) ||
               !string.IsNullOrWhiteSpace(homeworkStudent.TeacherFeedback) ||
               !string.IsNullOrWhiteSpace(homeworkStudent.AiFeedback);
    }

    public static HomeworkSubmissionAttempt BuildLegacyAttempt(
        HomeworkStudent homeworkStudent,
        int attemptNumber = 1,
        Guid? id = null)
    {
        return new HomeworkSubmissionAttempt
        {
            Id = id ?? Guid.Empty,
            HomeworkStudentId = homeworkStudent.Id,
            AttemptNumber = attemptNumber,
            Status = homeworkStudent.Status,
            StartedAt = homeworkStudent.StartedAt,
            SubmittedAt = homeworkStudent.SubmittedAt,
            GradedAt = homeworkStudent.GradedAt,
            Score = homeworkStudent.Score,
            TeacherFeedback = homeworkStudent.TeacherFeedback,
            AiFeedback = homeworkStudent.AiFeedback,
            TextAnswer = homeworkStudent.TextAnswer,
            AttachmentUrl = homeworkStudent.AttachmentUrl,
            CreatedAt = homeworkStudent.SubmittedAt ?? homeworkStudent.GradedAt ?? homeworkStudent.StartedAt ?? DateTime.UtcNow
        };
    }

    public static HomeworkSubmissionAttemptDto ToDto(HomeworkSubmissionAttempt attempt, bool isLatest)
    {
        return new HomeworkSubmissionAttemptDto
        {
            Id = attempt.Id,
            AttemptNumber = attempt.AttemptNumber,
            Status = attempt.Status.ToString(),
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            GradedAt = attempt.GradedAt,
            Score = attempt.Score,
            TeacherFeedback = attempt.TeacherFeedback,
            AiFeedback = attempt.AiFeedback,
            TextAnswer = attempt.TextAnswer,
            AttachmentUrl = attempt.AttachmentUrl,
            IsLatest = isLatest
        };
    }
}
