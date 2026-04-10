namespace Kidzgo.Application.TeachingMaterials.GetLessonMaterialProgress;

public sealed class GetLessonMaterialProgressResponse
{
    public string ProgramName { get; init; } = null!;
    public int UnitNumber { get; init; }
    public int LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public int TotalMaterials { get; init; }
    public IReadOnlyCollection<LessonMaterialStudentProgressDto> Students { get; init; } = [];
}

public sealed class LessonMaterialStudentProgressDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = null!;
    public int MaterialsViewed { get; init; }
    public int MaterialsCompleted { get; init; }
    public decimal OverallProgress { get; init; }
    public int TotalTimeSeconds { get; init; }
}
