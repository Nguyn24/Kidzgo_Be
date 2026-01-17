using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.GetBranchById;

public sealed class GetBranchByIdQuery : IQuery<GetBranchByIdResponse>
{
    public Guid Id { get; init; }
}

