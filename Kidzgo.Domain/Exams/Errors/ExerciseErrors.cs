using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams.Errors;

public static class ExerciseErrors
{
    public static Error NotFound(Guid? exerciseId) => Error.NotFound(
        "Exercise.NotFound",
        $"Exercise with Id = '{exerciseId}' was not found");

    public static Error QuestionNotFound(Guid? questionId) => Error.NotFound(
        "ExerciseQuestion.NotFound",
        $"Exercise question with Id = '{questionId}' was not found");

    public static readonly Error InvalidExerciseType = Error.Validation(
        "Exercise.InvalidType",
        "Invalid exercise type");

    public static readonly Error InvalidQuestionType = Error.Validation(
        "ExerciseQuestion.InvalidType",
        "Invalid question type");

    public static readonly Error InvalidPoints = Error.Validation(
        "ExerciseQuestion.InvalidPoints",
        "Points must be >= 0");

    public static Error SubmissionNotFound(Guid? submissionId) => Error.NotFound(
        "ExerciseSubmission.NotFound",
        $"Exercise submission with Id = '{submissionId}' was not found");

    public static readonly Error SubmissionUnauthorized = Error.Problem(
        "ExerciseSubmission.Unauthorized",
        "You are not allowed to access this submission");
}


