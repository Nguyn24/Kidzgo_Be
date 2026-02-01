using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetStudentLevel;

public sealed class GetStudentLevelQuery : IQuery<GetStudentLevelResponse>
{
    public Guid StudentProfileId { get; init; }
}

