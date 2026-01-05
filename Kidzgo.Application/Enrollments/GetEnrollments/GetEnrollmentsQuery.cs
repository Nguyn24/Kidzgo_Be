using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Enrollments.GetEnrollments;

public sealed class GetEnrollmentsQuery : IQuery<GetEnrollmentsResponse>, IPageableQuery
{
    public Guid? ClassId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public EnrollmentStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

