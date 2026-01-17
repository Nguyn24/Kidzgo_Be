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
}

