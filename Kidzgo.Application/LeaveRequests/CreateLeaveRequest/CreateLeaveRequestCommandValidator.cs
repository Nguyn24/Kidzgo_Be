using FluentValidation;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(c => c.StudentProfileId).NotEmpty();
        RuleFor(c => c.ClassId).NotEmpty();
        RuleFor(c => c.SessionDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Session date cannot be in the past");

        RuleFor(c => c.EndDate)
            .GreaterThanOrEqualTo(c => c.SessionDate)
            .WithMessage("End date must be greater than or equal to session date")
            .When(c => c.EndDate.HasValue)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("End date cannot be in the past")
            .When(c => c.EndDate.HasValue);
    }
}

