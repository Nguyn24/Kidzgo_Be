using FluentValidation;

namespace Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;

public sealed class CreatePauseEnrollmentRequestCommandValidator : AbstractValidator<CreatePauseEnrollmentRequestCommand>
{
    public CreatePauseEnrollmentRequestCommandValidator()
    {
        RuleFor(c => c.StudentProfileId).NotEmpty();
        RuleFor(c => c.PauseFrom)
            .NotEmpty()
            .GreaterThanOrEqualTo(VietnamTime.TodayDateOnly())
            .WithMessage("Pause from date cannot be in the past");

        RuleFor(c => c.PauseTo)
            .NotEmpty()
            .GreaterThanOrEqualTo(c => c.PauseFrom)
            .WithMessage("Pause to date must be greater than or equal to pause from date");
    }
}
