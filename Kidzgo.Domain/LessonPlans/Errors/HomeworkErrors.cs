using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class HomeworkErrors
{
    public static Error NotFound(Guid? homeworkId) => Error.NotFound(
        "Homework.NotFound",
        $"Homework assignment with Id = '{homeworkId}' was not found");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Homework.ClassNotFound",
        "Class not found or inactive");

    public static Error SessionNotFound(Guid? sessionId) => Error.NotFound(
        "Homework.SessionNotFound",
        $"Session with Id = '{sessionId}' was not found or does not belong to the class");

    public static Error MissionNotFound(Guid? missionId) => Error.NotFound(
        "Homework.MissionNotFound",
        $"Mission with Id = '{missionId}' was not found or inactive");

    public static readonly Error InvalidDueDate = Error.Validation(
        "Homework.InvalidDueDate",
        "Due date must be in the future");

    public static readonly Error CannotUpdate = Error.Validation(
        "Homework.CannotUpdate",
        "Cannot update homework assignment that has submitted or graded submissions");

    public static Error Unauthorized(Guid? homeworkId) => Error.Validation(
        "Homework.Unauthorized",
        $"You do not have permission to access homework assignment with Id = '{homeworkId}'");

    public static Error ClassHasNoActiveStudents(Guid? classId) => Error.Validation(
        "Homework.ClassHasNoActiveStudents",
        $"Class with Id = '{classId}' has no active enrolled students");

    // Homework Submission errors
    public static Error SubmissionNotFound(Guid? homeworkStudentId) => Error.NotFound(
        "HomeworkSubmission.NotFound",
        $"Homework submission with Id = '{homeworkStudentId}' was not found");

    public static readonly Error SubmissionInvalidScore = Error.Validation(
        "HomeworkSubmission.InvalidScore",
        "Score cannot be negative");

    public static Error SubmissionScoreExceedsMax(decimal maxScore) => Error.Validation(
        "HomeworkSubmission.ScoreExceedsMax",
        $"Score cannot exceed maximum score of {maxScore}");

    public static readonly Error SubmissionInvalidStatus = Error.Validation(
        "HomeworkSubmission.InvalidStatus",
        "Status must be either LATE or MISSING");

    public static Error SubmissionInvalidStatusTransition(string currentStatus, string targetStatus) => Error.Validation(
        "HomeworkSubmission.InvalidStatusTransition",
        $"Cannot change status from {currentStatus} to {targetStatus}");

    public static readonly Error SubmissionUnauthorized = Error.Validation(
        "HomeworkSubmission.Unauthorized",
        "You do not have permission to access this homework submission");

    public static readonly Error SubmissionAlreadySubmitted = Error.Validation(
        "HomeworkSubmission.AlreadySubmitted",
        "This homework has already been submitted");

    public static Error SubmissionInvalidData(string submissionType) => Error.Validation(
        "HomeworkSubmission.InvalidData",
        $"Submission data is required for {submissionType} submission type");

    public static readonly Error InvalidTitle = Error.Validation(
        "Homework.InvalidTitle",
        "Title cannot be empty or whitespace");

    public static readonly Error InvalidMaxScore = Error.Validation(
        "Homework.InvalidMaxScore",
        "MaxScore must be greater than 0");

    public static readonly Error InvalidRewardStars = Error.Validation(
        "Homework.InvalidRewardStars",
        "RewardStars must be greater than or equal to 0");

    public static readonly Error SubmissionCannotSubmitMissing = Error.Validation(
        "HomeworkSubmission.CannotSubmitMissing",
        "Cannot submit homework with MISSING status");

    public static readonly Error SubmissionNotSubmitted = Error.Validation(
        "HomeworkSubmission.NotSubmitted",
        "Can only grade homework that has been submitted");

    // Input validation errors
    public static readonly Error InvalidSubmissionType = Error.Validation(
        "Homework.InvalidSubmissionType",
        "Invalid submission type");

    public static readonly Error InvalidStatusForMarking = Error.Validation(
        "Homework.InvalidStatusForMarking",
        "Status must be either 'LATE' or 'MISSING'");
}


