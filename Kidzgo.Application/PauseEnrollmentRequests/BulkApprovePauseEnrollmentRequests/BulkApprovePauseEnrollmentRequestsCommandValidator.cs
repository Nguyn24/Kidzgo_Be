using FluentValidation;

namespace Kidzgo.Application.PauseEnrollmentRequests.BulkApprovePauseEnrollmentRequests;

public sealed class BulkApprovePauseEnrollmentRequestsCommandValidator
    : AbstractValidator<BulkApprovePauseEnrollmentRequestsCommand>
{
    public BulkApprovePauseEnrollmentRequestsCommandValidator()
    {
        RuleFor(c => c.Ids)
            .NotEmpty()
            .WithMessage("Ids are required");
    }
}
