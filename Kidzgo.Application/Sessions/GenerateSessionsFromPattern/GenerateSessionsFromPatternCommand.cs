using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GenerateSessionsFromPattern;

public sealed class GenerateSessionsFromPatternCommand : ICommand<GenerateSessionsFromPatternResponse>
{
    public Guid ClassId { get; init; }
    public Guid RoomId { get; init; }
  
    /// Nếu true, chỉ generate các sessions từ hiện tại trở đi.
    /// Nếu false, generate tất cả từ StartDate của Class.
    
    public bool OnlyFutureSessions { get; init; } = true;
}


