using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Programs.Errors;

public static class ProgramErrors
{
    public static Error NotFound(Guid? programId) => Error.NotFound(
        "Program.NotFound",
        $"Program with Id = '{programId}' was not found");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Program.BranchNotFound",
        "Branch not found or inactive");

    public static readonly Error HasActiveClasses = Error.Conflict(
        "Program.HasActiveClasses",
        "Cannot delete program with active classes");
}

