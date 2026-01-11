namespace Kidzgo.Application.Exams.CreateExamResultsBulk;

public sealed class CreateExamResultsBulkResponse
{
    public int CreatedCount { get; init; }
    public int SkippedCount { get; init; }
    public List<ExamResultResponse> CreatedResults { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}

public sealed class ExamResultResponse
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public decimal? Score { get; init; }
}

