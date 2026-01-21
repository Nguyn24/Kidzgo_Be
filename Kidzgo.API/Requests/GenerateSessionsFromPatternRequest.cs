namespace Kidzgo.API.Requests;

public sealed class GenerateSessionsFromPatternRequest
{
    public Guid ClassId { get; set; }

    /// Nếu true, chỉ generate các sessions từ hiện tại trở đi.
    /// Nếu false, generate tất cả từ StartDate của Class.
    public bool OnlyFutureSessions { get; set; } = true;
}


