using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class MakeupCreditErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "MakeupCredit.NotFound",
        $"The makeup credit with Id = '{id}' was not found.");

    public static Error NotAvailable(Guid id) => Error.Validation(
        "MakeupCredit.NotAvailable",
        $"Makeup credit '{id}' is not available for allocation.");

    public static Error Expired(Guid id) => Error.Validation(
        "MakeupCredit.Expired",
        $"Makeup credit '{id}' is expired.");

    public static Error NotBelongToStudent => Error.Validation(
        "MakeupCredit.NotBelongToStudent",
        "This makeup credit does not belong to the specified student.");

    public static Error MustBeWeekend => Error.Validation(
        "MakeupCredit.MustBeWeekend",
        "Makeup session must be on Saturday or Sunday.");

    public static Error MustBeFutureWeek => Error.Validation(
        "MakeupCredit.MustBeFutureWeek",
        "Makeup session must be in the weeks after the missed week.");

    public static Error CannotUsePastDate => Error.Validation(
        "MakeupCredit.CannotUsePastDate",
        "Cannot use makeup credit for past dates.");

    public static Error CannotChangeAllocatedPastSession => Error.Validation(
        "MakeupCredit.CannotChangeAllocatedPastSession",
        "Cannot change makeup session because the allocated session is today or has already passed.");

    public static Error ParentMustProvideStudentProfileId => Error.Validation(
        "MakeupCredit.ParentMustProvideStudentProfileId",
        "Parent must provide StudentProfileId.");

    public static Error StudentNotBelongToParent => Error.Validation(
        "MakeupCredit.StudentNotBelongToParent",
        "Student does not belong to this parent.");

    public static Error SessionNotBelongToClass => Error.Validation(
        "MakeupCredit.SessionNotBelongToClass",
        "Target session does not belong to the specified class.");
}

