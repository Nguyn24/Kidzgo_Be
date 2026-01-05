using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Enrollments.GetStudentEnrollmentHistory;

public sealed class GetStudentEnrollmentHistoryQuery : IQuery<GetStudentEnrollmentHistoryResponse>, IPageableQuery
{
    public Guid StudentProfileId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

