using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.GetStudentExamResults;

public sealed class GetStudentExamResultsQuery : IQuery<GetStudentExamResultsResponse>, IPageableQuery
{
    public Guid StudentProfileId { get; init; }
    public ExamType? ExamType { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

