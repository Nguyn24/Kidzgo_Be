using FluentValidation;

namespace Kidzgo.Application.LeaveRequests.CreateLeaveRequest;

public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(c => c.StudentProfileId).NotEmpty();
        RuleFor(c => c.ClassId).NotEmpty();
        RuleFor(c => c.SessionDate).NotEmpty();
    }
}

