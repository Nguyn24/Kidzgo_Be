using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class LessonPlanTemplateErrors
{
    public static Error NotFound(Guid? templateId) => Error.NotFound(
        "LessonPlanTemplate.NotFound",
        $"Lesson plan template with Id = '{templateId}' was not found");

    public static Error ProgramNotFound(Guid? programId) => Error.NotFound(
        "LessonPlanTemplate.ProgramNotFound",
        $"Program with Id = '{programId}' was not found or inactive");

    public static readonly Error ProgramRequired = Error.Validation(
        "LessonPlanTemplate.ProgramRequired",
        "ProgramId is required");

    public static readonly Error SessionIndexRequired = Error.Validation(
        "LessonPlanTemplate.SessionIndexRequired",
        "SessionIndex is required and must be greater than 0");

    public static Error DuplicateSessionIndex(Guid programId, int sessionIndex) => Error.Conflict(
        "LessonPlanTemplate.DuplicateSessionIndex",
        $"Template with SessionIndex {sessionIndex} already exists for Program {programId}");

    public static readonly Error HasActiveLessonPlans = Error.Conflict(
        "LessonPlanTemplate.HasActiveLessonPlans",
        "Cannot delete template that has active lesson plans");

    public static Error UnsupportedImportFileType(string extension) => Error.Validation(
        "LessonPlanTemplate.UnsupportedImportFileType",
        $"Unsupported syllabus import file type '{extension}'. Only .csv, .xlsx, and .xls are supported");

    public static readonly Error ImportFileRequiresProgramId = Error.Validation(
        "LessonPlanTemplate.ImportFileRequiresProgramId",
        "ProgramId is required when importing a CSV syllabus file");

    public static Error InvalidImportFile(string message) => Error.Validation(
        "LessonPlanTemplate.InvalidImportFile",
        message);

    public static Error ProgramMappingNotFound(string sheetName) => Error.NotFound(
        "LessonPlanTemplate.ProgramMappingNotFound",
        $"Could not map syllabus sheet '{sheetName}' to an active program");

    public static readonly Error Unauthorized = Error.Validation(
        "LessonPlanTemplate.Unauthorized",
        "You do not have permission to modify this lesson plan template");
}
