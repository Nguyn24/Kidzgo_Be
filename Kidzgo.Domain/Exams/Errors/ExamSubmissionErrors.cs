using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams.Errors;

public static class ExamSubmissionErrors
{
    public static Error NotFound(Guid? submissionId) => Error.NotFound(
        "ExamSubmission.NotFound",
        $"Exam Submission with Id = '{submissionId}' was not found");

    public static Error ExamNotFound(Guid? examId) => Error.NotFound(
        "ExamSubmission.ExamNotFound",
        $"Exam with Id = '{examId}' was not found");

    public static Error AlreadyStarted(Guid? examId, Guid? studentProfileId) => Error.Conflict(
        "ExamSubmission.AlreadyStarted",
        $"Student already started this exam. Submission already exists for ExamId = '{examId}' and StudentProfileId = '{studentProfileId}'");

    public static Error NotStarted => Error.Conflict(
        "ExamSubmission.NotStarted",
        "Exam submission has not been started yet");

    public static Error AlreadySubmitted => Error.Conflict(
        "ExamSubmission.AlreadySubmitted",
        "Exam submission has already been submitted");

    public static Error ExamNotStarted => Error.Conflict(
        "ExamSubmission.ExamNotStarted",
        "Exam has not started yet. Please wait for the scheduled start time");

    public static Error ExamExpired => Error.Conflict(
        "ExamSubmission.ExamExpired",
        "Exam time limit has expired");

    public static Error LateStartNotAllowed => Error.Conflict(
        "ExamSubmission.LateStartNotAllowed",
        "Late start is not allowed for this exam");

    public static Error TooLateToStart => Error.Conflict(
        "ExamSubmission.TooLateToStart",
        "Too late to start the exam. Exceeded late start tolerance");

    public static Error QuestionNotFound(Guid? questionId) => Error.NotFound(
        "ExamSubmission.QuestionNotFound",
        $"Exam Question with Id = '{questionId}' was not found");

    public static Error AlreadyGraded => Error.Conflict(
        "ExamSubmission.AlreadyGraded",
        "Exam submission has already been graded");

    public static Error InvalidStatus => Error.Validation(
        "ExamSubmission.InvalidStatus",
        "Can only grade submitted or auto-submitted exams");
}


