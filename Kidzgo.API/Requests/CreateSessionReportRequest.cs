namespace Kidzgo.API.Requests;

public sealed class CreateSessionReportRequest
{
    public Guid SessionId { get; set; }
    public Guid StudentProfileId { get; set; }
    public DateOnly ReportDate { get; set; }
    public string Feedback { get; set; } = null!;
}

