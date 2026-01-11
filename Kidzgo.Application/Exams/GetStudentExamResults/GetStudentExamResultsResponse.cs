using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Exams.GetStudentExamResults;

public sealed class GetStudentExamResultsResponse
{
    public Page<StudentExamResultDto> ExamResults { get; init; } = null!;
}

