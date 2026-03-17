using FluentValidation;

namespace Kidzgo.Application.LeaveRequests.BulkApproveLeaveRequests;

public sealed class BulkApproveLeaveRequestsCommandValidator
    : AbstractValidator<BulkApproveLeaveRequestsCommand>
{
    public BulkApproveLeaveRequestsCommandValidator()
    {
        RuleFor(c => c.Ids)
            .NotEmpty()
            .WithMessage("Ids are required");
    }
}
