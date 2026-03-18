using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.GetRegistrations;

public sealed class GetRegistrationsQuery : IQuery<GetRegistrationsResponse>
{
    public Guid? StudentProfileId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public string? Status { get; init; }
    public Guid? ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
