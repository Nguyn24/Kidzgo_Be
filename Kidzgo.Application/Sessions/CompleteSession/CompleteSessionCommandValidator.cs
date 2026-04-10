using FluentValidation;

namespace Kidzgo.Application.Sessions.CompleteSession;

public sealed class CompleteSessionCommandValidator : AbstractValidator<CompleteSessionCommand>
{
    public CompleteSessionCommandValidator()
    {
        RuleFor(command => command.SessionId)
            .NotEmpty()
            .WithMessage("Session ID is required");

        RuleFor(command => command.ActualDatetime)
            .LessThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("ActualDatetime cannot be in the future")
            .When(command => command.ActualDatetime.HasValue);
    }
}

