namespace Kidzgo.Application.Gamification.ExportDeliveredRewardRedemptions;

public sealed class ExportDeliveredRewardRedemptionsResponse
{
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public byte[] Content { get; init; } = [];
    public int RowCount { get; init; }
}
