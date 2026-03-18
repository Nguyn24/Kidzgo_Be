using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Registrations.GetWaitingList;

public sealed class WaitingListItemDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid TuitionPlanId { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public DateTime RegistrationDate { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public int DaysWaiting { get; init; }
}

public sealed class GetWaitingListResponse
{
    public List<WaitingListItemDto> Items { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}
