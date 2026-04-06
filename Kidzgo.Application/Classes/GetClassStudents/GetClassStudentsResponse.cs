using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Classes.GetClassStudents;

public sealed class GetClassStudentsResponse
{
    public Page<ClassStudentDto> Students { get; init; } = null!;
}

public sealed class ClassStudentDto
{
    public Guid StudentProfileId { get; init; }
    public string FullName { get; init; } = null!;
    public string? AvatarUrl { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateOnly EnrollDate { get; init; }
    public string Status { get; init; } = null!;
    public decimal AttendanceRate { get; init; }
    public decimal ProgressPercent { get; init; }
    public int Stars { get; init; }
    public DateTime? LastActiveAt { get; init; }
}
