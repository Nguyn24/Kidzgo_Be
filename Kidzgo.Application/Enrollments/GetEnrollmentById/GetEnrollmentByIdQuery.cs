using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.GetEnrollmentById;

public sealed class GetEnrollmentByIdQuery : IQuery<GetEnrollmentByIdResponse>
{
    public Guid Id { get; init; }
}

