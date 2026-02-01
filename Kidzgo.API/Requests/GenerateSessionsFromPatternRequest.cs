namespace Kidzgo.API.Requests;

public sealed class GenerateSessionsFromPatternRequest
{
    public Guid ClassId { get; set; }

    /// Room ID để gán cho các sessions được tạo. Nếu null, sessions sẽ không có room.
    public Guid? RoomId { get; set; }

    /// Nếu true, chỉ generate các sessions từ hiện tại trở đi.
    /// Nếu false, generate tất cả từ StartDate của Class.
    public bool OnlyFutureSessions { get; set; } = true;
}


