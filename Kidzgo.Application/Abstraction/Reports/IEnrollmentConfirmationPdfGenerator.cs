namespace Kidzgo.Application.Abstraction.Reports;

public interface IEnrollmentConfirmationPdfGenerator
{
    Task<string> GeneratePdfAsync(
        EnrollmentConfirmationPdfDocument document,
        CancellationToken cancellationToken = default);
}

public sealed class EnrollmentConfirmationPdfDocument
{
    public Guid RegistrationId { get; init; }
    public Guid EnrollmentId { get; init; }
    public string StudentName { get; init; } = null!;
    public string? ParentName { get; init; }
    public string? ParentPhoneNumber { get; init; }
    public string BranchName { get; init; } = null!;
    public string? BranchAddress { get; init; }
    public string? BranchPhoneNumber { get; init; }
    public string ProgramName { get; init; } = null!;
    public string ProgramCode { get; init; } = null!;
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public DateOnly EnrollDate { get; init; }
    public DateOnly? FirstStudyDate { get; init; }
    public string? StudyDaySummary { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public int TotalSessions { get; init; }
    public decimal TuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string Currency { get; init; } = null!;
    public string Track { get; init; } = null!;
    public string EntryType { get; init; } = null!;
    public DateTime GeneratedAt { get; init; }
    public string? IssuedByName { get; init; }
}
