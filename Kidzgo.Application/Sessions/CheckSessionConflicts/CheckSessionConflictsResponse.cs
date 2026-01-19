// using Kidzgo.Application.Services; // TODO: Uncomment after Services namespace is fixed

namespace Kidzgo.Application.Sessions.CheckSessionConflicts;

public sealed class CheckSessionConflictsResponse
{
    public bool HasConflicts { get; init; }
    public List<ConflictDto> Conflicts { get; init; } = new();
    public ConflictSuggestionsDto? Suggestions { get; init; }
}

public sealed class ConflictDto
{
    public string Type { get; init; } = null!; // "Room", "Teacher", "Assistant"
    public Guid SessionId { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateTime ConflictDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
}

public sealed class ConflictSuggestionsDto
{
    public List<SuggestedRoomDto> SuggestedRooms { get; init; } = new();
    public List<DateTime> AlternativeSlots { get; init; } = new();
}

public sealed class SuggestedRoomDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public int Capacity { get; init; }
}