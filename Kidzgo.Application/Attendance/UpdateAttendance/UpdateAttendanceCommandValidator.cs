using FluentValidation;

namespace Kidzgo.Application.Attendance.UpdateAttendance;

public class UpdateAttendanceCommandValidator : AbstractValidator<UpdateAttendanceCommand>
{
    public UpdateAttendanceCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.AttendanceStatus).IsInEnum();
    }
}

