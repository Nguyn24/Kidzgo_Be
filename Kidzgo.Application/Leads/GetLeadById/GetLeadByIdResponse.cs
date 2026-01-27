namespace Kidzgo.Application.Leads.GetLeadById;

public sealed class GetLeadByIdResponse
{
    public Guid Id { get; init; }
    public string Source { get; init; } = null!;
    public string? Campaign { get; init; }
    public string ContactName { get; init; } = null!;
    public string? Phone { get; init; }
    public string? ZaloId { get; init; }
    public string? Email { get; init; }
    public string? Company { get; init; }
    public string? Subject { get; init; }
    public Guid? BranchPreference { get; init; }
    public string? BranchPreferenceName { get; init; }
    public string? ProgramInterest { get; init; }
    public string? Notes { get; init; }
    public string Status { get; init; } = null!;
    public Guid? OwnerStaffId { get; init; }
    public string? OwnerStaffName { get; init; }
    public DateTime? FirstResponseAt { get; init; }
    public int TouchCount { get; init; }
    public DateTime? NextActionAt { get; init; }
    public Guid? ConvertedStudentProfileId { get; init; }
    public DateTime? ConvertedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

