using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Programs.Errors;

public static class ProgramLeavePolicyErrors
{
    public static readonly Error InvalidMaxLeavesPerMonth = Error.Validation(
        "ProgramLeavePolicy.InvalidMaxLeavesPerMonth",
        "Max leaves per month must be greater than 0");
}
