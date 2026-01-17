using FluentValidation;

namespace Kidzgo.Application.Attendance.MarkAttendance;

public class MarkAttendanceCommandValidator : AbstractValidator<MarkAttendanceCommand>
{
    public MarkAttendanceCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.StudentProfileId).NotEmpty();
        RuleFor(c => c.AttendanceStatus).IsInEnum();
    }
}

