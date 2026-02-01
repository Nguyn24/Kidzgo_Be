using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Classes.GetTeacherClassStudents;

public sealed class GetTeacherClassStudentsResponse
{
    public Page<TeacherClassStudentDto> Students { get; init; } = null!;
}

public sealed class TeacherClassStudentDto
{
    public Guid EnrollmentId { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public DateOnly EnrollDate { get; init; }
    public string Status { get; init; } = null!;
}


