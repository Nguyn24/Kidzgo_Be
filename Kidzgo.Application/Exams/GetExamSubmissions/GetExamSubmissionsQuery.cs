using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.GetExamSubmissions;

public sealed class GetExamSubmissionsQuery : IQuery<GetExamSubmissionsResponse>, IPageableQuery
{
    public Guid ExamId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public ExamSubmissionStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}


