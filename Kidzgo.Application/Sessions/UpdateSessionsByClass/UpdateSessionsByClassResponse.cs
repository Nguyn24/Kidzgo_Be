namespace Kidzgo.Application.Sessions.UpdateSessionsByClass;

public sealed class UpdateSessionsByClassResponse
{
    public int UpdatedSessionsCount { get; init; }
    public List<Guid> UpdatedSessionIds { get; init; } = new();
    public List<Guid> SkippedSessionIds { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}

