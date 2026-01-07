using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Classes.GetStudentClasses;

public sealed class GetStudentClassesResponse
{
    public Page<StudentClassDto> Classes { get; init; } = null!;
}

public sealed class StudentClassDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public Guid? MainTeacherId { get; init; }
    public string? MainTeacherName { get; init; }
    public Guid? AssistantTeacherId { get; init; }
    public string? AssistantTeacherName { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public Domain.Classes.ClassStatus Status { get; init; }
    public int Capacity { get; init; }
    public int CurrentEnrollmentCount { get; init; }
    public string? SchedulePattern { get; init; }
    public DateOnly EnrollDate { get; init; }
    public Domain.Classes.EnrollmentStatus EnrollmentStatus { get; init; }
}

