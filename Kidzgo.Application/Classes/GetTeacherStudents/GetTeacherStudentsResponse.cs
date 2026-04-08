using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Classes.GetTeacherStudents;

public sealed class GetTeacherStudentsResponse
{
    public Page<TeacherStudentDto> Students { get; init; } = null!;
}

public sealed class TeacherStudentDto
{
    public Guid StudentProfileId { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
}
