using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Exams.GetExamResults;

public sealed class GetExamResultsResponse
{
    public Page<ExamResultDto> ExamResults { get; init; } = null!;
}

