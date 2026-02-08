namespace Kidzgo.API.Requests;

public sealed class CreateMonthlyReportJobRequest
{
    public int Month { get; init; }
    public int Year { get; init; }
    public Guid BranchId { get; init; }
}

public sealed class UpdateMonthlyReportDraftRequest
{
    public string DraftContent { get; init; } = null!;
}

public sealed class AddReportCommentRequest
{
    public string Content { get; init; } = null!;
}

