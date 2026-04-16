using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Registrations;

public class EnrollmentConfirmationPdf : Entity
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public Guid EnrollmentId { get; set; }
    public string Track { get; set; } = null!;
    public EnrollmentConfirmationPdfFormType FormType { get; set; }
    public string PdfUrl { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public Guid? GeneratedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SnapshotJson { get; set; }

    public Registration Registration { get; set; } = null!;
    public ClassEnrollment Enrollment { get; set; } = null!;
}
