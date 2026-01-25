using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams.Errors;

public static class ExamQuestionErrors
{
    public static Error NotFound(Guid? questionId) => Error.NotFound(
        "ExamQuestion.NotFound",
        $"Exam Question with Id = '{questionId}' was not found");

    public static Error ExamNotFound(Guid? examId) => Error.NotFound(
        "ExamQuestion.ExamNotFound",
        $"Exam with Id = '{examId}' was not found");

    public static readonly Error InvalidQuestionType = Error.Validation(
        "ExamQuestion.InvalidQuestionType",
        "Invalid question type. Must be MultipleChoice or TextInput");

    public static readonly Error InvalidOptions = Error.Validation(
        "ExamQuestion.InvalidOptions",
        "Options must be a valid JSON array for MultipleChoice questions");

    public static readonly Error MissingCorrectAnswer = Error.Validation(
        "ExamQuestion.MissingCorrectAnswer",
        "Correct answer is required");

    public static readonly Error HasSubmissions = Error.Conflict(
        "ExamQuestion.HasSubmissions",
        "Cannot delete question that has submission answers");
}


