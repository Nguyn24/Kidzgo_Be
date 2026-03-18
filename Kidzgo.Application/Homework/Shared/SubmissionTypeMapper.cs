using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.Shared;

internal static class SubmissionTypeMapper
{
    public static string ToApiString(SubmissionType submissionType)
    {
        return submissionType == SubmissionType.Quiz
            ? "MULTIPLE_CHOICE"
            : submissionType.ToString();
    }
}
