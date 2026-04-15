using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.RetakePlacementTest;

public sealed class RetakePlacementTestCommand : ICommand<RetakePlacementTestResponse>
{
    /// <summary>
    /// Id cua PlacementTest goc (lan test dau tien)
    /// </summary>
    public Guid OriginalPlacementTestId { get; init; }

    /// <summary>
    /// Id cua StudentProfile - bat buoc phai co vi can ket noi voi Registration hien tai
    /// </summary>
    public Guid StudentProfileId { get; init; }

    /// <summary>
    /// Id cua Program moi muon dang ky (sau khi test lai)
    /// </summary>
    public Guid NewProgramId { get; init; }

    /// <summary>
    /// Id cua TuitionPlan moi muon dang ky
    /// </summary>
    public Guid NewTuitionPlanId { get; init; }

    /// <summary>
    /// Chi nhanh dang ky
    /// </summary>
    public Guid BranchId { get; init; }

    /// <summary>
    /// Thoi gian thi lai
    /// </summary>
    public DateTime? ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }

    /// <summary>
    /// Phong thi
    /// </summary>
    public Guid? RoomId { get; init; }
    public string? Room { get; init; }

    /// <summary>
    /// Nguoi coi thi
    /// </summary>
    public Guid? InvigilatorUserId { get; init; }

    /// <summary>
    /// Ghi chu
    /// </summary>
    public string? Note { get; init; }
}
