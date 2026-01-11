using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Exams.GetExams;

public sealed class GetExamsResponse
{
    public Page<ExamDto> Exams { get; init; } = null!;
}

