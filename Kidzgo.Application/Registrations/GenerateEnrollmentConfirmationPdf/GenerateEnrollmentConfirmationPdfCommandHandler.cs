using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GenerateEnrollmentConfirmationPdf;

public sealed class GenerateEnrollmentConfirmationPdfCommandHandler(
    IDbContext context,
    IEnrollmentConfirmationPdfGenerator pdfGenerator,
    IFileStorageService fileStorage,
    IUserContext userContext,
    ILogger<GenerateEnrollmentConfirmationPdfCommandHandler> logger
) : ICommandHandler<GenerateEnrollmentConfirmationPdfCommand, GenerateEnrollmentConfirmationPdfResponse>
{
    public async Task<Result<GenerateEnrollmentConfirmationPdfResponse>> Handle(
        GenerateEnrollmentConfirmationPdfCommand command,
        CancellationToken cancellationToken)
    {
        var track = RegistrationTrackHelper.NormalizeTrack(command.Track);
        var trackType = RegistrationTrackHelper.ToTrackType(track);

        var registration = await context.Registrations
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(
                RegistrationErrors.NotFound(command.RegistrationId));
        }

        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.TuitionPlan)
            .Where(e => e.RegistrationId == registration.Id &&
                        e.Track == trackType &&
                        e.Status == EnrollmentStatus.Active)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(
                Error.NotFound(
                    "Registration.EnrollmentNotFound",
                    $"No active enrollment was found for registration {registration.Id} and track '{track}'."));
        }

        var firstStudyDate = await GetFirstStudyDateAsync(enrollment.Id, cancellationToken);
        var tuitionPlan = enrollment.TuitionPlan ?? registration.TuitionPlan;

        if (!command.Regenerate && !string.IsNullOrWhiteSpace(enrollment.EnrollmentConfirmationPdfUrl))
        {
            return BuildResponse(
                registration,
                enrollment,
                track,
                GetDownloadUrl(enrollment.EnrollmentConfirmationPdfUrl),
                enrollment.EnrollmentConfirmationPdfGeneratedAt ?? enrollment.UpdatedAt,
                reusedExistingPdf: true,
                firstStudyDate,
                tuitionPlan);
        }

        var parent = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(l => l.StudentProfileId == registration.StudentProfileId)
            .Select(l => new ParentContactDto(
                l.ParentProfile.DisplayName,
                l.ParentProfile.User.PhoneNumber))
            .FirstOrDefaultAsync(cancellationToken);

        var issuedByName = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userContext.UserId)
            .Select(u => u.Name ?? u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        var entryType = trackType == Domain.Registrations.RegistrationTrackType.Secondary
            ? registration.SecondaryEntryType?.ToString()
            : registration.EntryType?.ToString();
        var schedule = RegistrationActualStudyScheduleMapper
            .Map(new[] { enrollment })
            .FirstOrDefault();

        var document = new EnrollmentConfirmationPdfDocument
        {
            RegistrationId = registration.Id,
            EnrollmentId = enrollment.Id,
            StudentName = registration.StudentProfile.DisplayName,
            ParentName = parent?.Name,
            ParentPhoneNumber = parent?.PhoneNumber,
            BranchName = registration.Branch.Name,
            BranchAddress = registration.Branch.Address,
            BranchPhoneNumber = registration.Branch.ContactPhone,
            ProgramName = enrollment.Class.Program.Name,
            ProgramCode = enrollment.Class.Program.Code,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            EnrollDate = enrollment.EnrollDate,
            FirstStudyDate = firstStudyDate,
            StudyDaySummary = schedule?.StudyDaySummary,
            TuitionPlanName = tuitionPlan.Name,
            TotalSessions = tuitionPlan.TotalSessions,
            TuitionAmount = tuitionPlan.TuitionAmount,
            UnitPriceSession = tuitionPlan.UnitPriceSession,
            Currency = tuitionPlan.Currency,
            Track = track,
            EntryType = entryType ?? string.Empty,
            GeneratedAt = now,
            IssuedByName = issuedByName
        };

        try
        {
            var pdfUrl = await pdfGenerator.GeneratePdfAsync(document, cancellationToken);

            enrollment.EnrollmentConfirmationPdfUrl = pdfUrl;
            enrollment.EnrollmentConfirmationPdfGeneratedAt = now;
            enrollment.EnrollmentConfirmationPdfGeneratedBy = userContext.UserId;
            enrollment.UpdatedAt = now;

            await context.SaveChangesAsync(cancellationToken);

            return BuildResponse(
                registration,
                enrollment,
                track,
                GetDownloadUrl(pdfUrl),
                now,
                reusedExistingPdf: false,
                firstStudyDate,
                tuitionPlan);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to generate enrollment confirmation PDF for registration {RegistrationId}, enrollment {EnrollmentId}",
                registration.Id,
                enrollment.Id);

            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(
                Error.Failure(
                    "Registration.EnrollmentConfirmationPdfGenerationFailed",
                    $"Failed to generate enrollment confirmation PDF: {ex.Message}"));
        }
    }

    private async Task<DateOnly?> GetFirstStudyDateAsync(Guid enrollmentId, CancellationToken cancellationToken)
    {
        var firstSessionDateTime = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => a.ClassEnrollmentId == enrollmentId &&
                        a.Status == StudentSessionAssignmentStatus.Assigned)
            .OrderBy(a => a.Session.PlannedDatetime)
            .Select(a => (DateTime?)a.Session.PlannedDatetime)
            .FirstOrDefaultAsync(cancellationToken);

        return firstSessionDateTime.HasValue
            ? VietnamTime.ToVietnamDateOnly(firstSessionDateTime.Value)
            : null;
    }

    private string GetDownloadUrl(string pdfUrl)
    {
        try
        {
            return fileStorage.GetDownloadUrl(pdfUrl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create download URL for enrollment confirmation PDF");
            return pdfUrl;
        }
    }

    private static GenerateEnrollmentConfirmationPdfResponse BuildResponse(
        Domain.Registrations.Registration registration,
        ClassEnrollment enrollment,
        string track,
        string pdfUrl,
        DateTime pdfGeneratedAt,
        bool reusedExistingPdf,
        DateOnly? firstStudyDate,
        Domain.Programs.TuitionPlan tuitionPlan)
    {
        return new GenerateEnrollmentConfirmationPdfResponse
        {
            RegistrationId = registration.Id,
            EnrollmentId = enrollment.Id,
            Track = track,
            PdfUrl = pdfUrl,
            PdfGeneratedAt = pdfGeneratedAt,
            ReusedExistingPdf = reusedExistingPdf,
            EnrollDate = enrollment.EnrollDate,
            FirstStudyDate = firstStudyDate,
            StudentName = registration.StudentProfile.DisplayName,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            ProgramName = enrollment.Class.Program.Name,
            TuitionPlanName = tuitionPlan.Name,
            TuitionAmount = tuitionPlan.TuitionAmount,
            Currency = tuitionPlan.Currency
        };
    }

    private sealed record ParentContactDto(string? Name, string? PhoneNumber);
}
