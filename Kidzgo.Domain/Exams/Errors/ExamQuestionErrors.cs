using Kidzgo.Domain.Common;

using Kidzgo.Domain.Homework;

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

    public static readonly Error InvalidMatrixDistribution = Error.Validation(
        "ExamQuestion.InvalidMatrixDistribution",
        "Question distribution must have at least one level with count > 0");

    public static Error MatrixTotalMismatch(int expectedTotal, int distributedTotal) => Error.Validation(
        "ExamQuestion.MatrixTotalMismatch",
        $"TotalQuestions = {expectedTotal} does not match distributed total = {distributedTotal}");

    public static readonly Error UnsupportedQuestionBankType = Error.Validation(
        "ExamQuestion.UnsupportedQuestionBankType",
        "Question bank matrix currently supports only MultipleChoice and Text question types");

    public static readonly Error CannotRegenerateWhenSubmissionsExist = Error.Conflict(
        "ExamQuestion.CannotRegenerateWhenSubmissionsExist",
        "Cannot regenerate exam questions because this exam already has submissions");

    public static Error InsufficientQuestionsInBank(QuestionLevel level, int required, int available) => Error.Validation(
        "ExamQuestion.InsufficientQuestionsInBank",
        $"Not enough question bank items for level '{level}'. Required {required}, available {available}");
}


