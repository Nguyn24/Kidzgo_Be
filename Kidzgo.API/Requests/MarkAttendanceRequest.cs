using Kidzgo.Domain.Sessions;

namespace Kidzgo.API.Requests;

public sealed class MarkAttendanceRequest
{
    public Guid StudentProfileId { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public string? Comment { get; set; }
}

