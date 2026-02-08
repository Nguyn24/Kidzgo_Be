using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MonthlyReports.AggregateMonthlyReportData;

/// <summary>
/// UC-175: Gom dữ liệu cho Monthly Report
/// Tạo reports cho tất cả students trong job
/// </summary>
public sealed class AggregateMonthlyReportDataCommandHandler(
    IDbContext context,
    IMonthlyReportDataAggregator dataAggregator
) : ICommandHandler<AggregateMonthlyReportDataCommand, AggregateMonthlyReportDataResponse>
{
    public async Task<Result<AggregateMonthlyReportDataResponse>> Handle(
        AggregateMonthlyReportDataCommand command,
        CancellationToken cancellationToken)
    {
        // Get job
        var job = await context.MonthlyReportJobs
            .Include(j => j.Branch)
            .FirstOrDefaultAsync(j => j.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure<AggregateMonthlyReportDataResponse>(
                MonthlyReportErrors.JobNotFound(command.JobId));
        }

        // Get all active enrollments in the branch for the month
        var startDate = new DateTime(job.Year, job.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var enrollments = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Where(e => e.Class.BranchId == job.BranchId &&
                       e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        var reportIds = new List<Guid>();
        int totalCreated = 0;
        int totalUpdated = 0;
        var now = DateTime.UtcNow;

        foreach (var enrollment in enrollments)
        {
            // Check if report already exists
            var existingReport = await context.StudentMonthlyReports
                .FirstOrDefaultAsync(
                    r => r.StudentProfileId == enrollment.StudentProfileId &&
                         r.Month == job.Month &&
                         r.Year == job.Year,
                    cancellationToken);

            StudentMonthlyReport report;
            bool isNew = false;

            if (existingReport is null)
            {
                // Create new report
                report = new StudentMonthlyReport
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = enrollment.StudentProfileId,
                    ClassId = enrollment.ClassId,
                    JobId = job.Id,
                    Month = job.Month,
                    Year = job.Year,
                    Status = ReportStatus.Draft,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.StudentMonthlyReports.Add(report);
                isNew = true;
                totalCreated++;
            }
            else
            {
                // Update existing report
                report = existingReport;
                report.UpdatedAt = now;
                totalUpdated++;
            }

            // Aggregate data
            var aggregatedDataJson = await dataAggregator.AggregateDataAsync(
                enrollment.StudentProfileId,
                job.Month,
                job.Year,
                cancellationToken);

            // Parse JSON to extract individual data sections
            using var doc = System.Text.Json.JsonDocument.Parse(aggregatedDataJson);
            var root = doc.RootElement;

            // Create or update MonthlyReportData
            var reportData = await context.MonthlyReportData
                .FirstOrDefaultAsync(rd => rd.ReportId == report.Id, cancellationToken);

            if (reportData is null)
            {
                reportData = new MonthlyReportData
                {
                    Id = Guid.NewGuid(),
                    ReportId = report.Id,
                    StudentProfileId = enrollment.StudentProfileId,
                    Month = job.Month,
                    Year = job.Year,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                context.MonthlyReportData.Add(reportData);
            }
            else
            {
                reportData.UpdatedAt = now;
            }

            // Set individual data fields
            if (root.TryGetProperty("attendance", out var attendance))
            {
                reportData.AttendanceData = attendance.GetRawText();
            }
            if (root.TryGetProperty("homework", out var homework))
            {
                reportData.HomeworkData = homework.GetRawText();
            }
            if (root.TryGetProperty("test", out var test))
            {
                reportData.TestData = test.GetRawText();
            }
            if (root.TryGetProperty("mission", out var mission))
            {
                reportData.MissionData = mission.GetRawText();
            }
            if (root.TryGetProperty("notes", out var notes))
            {
                reportData.NotesData = notes.GetRawText();
            }

            // Mark session reports as compiled
            if (isNew)
            {
                var sessionReports = await context.SessionReports
                    .Where(sr => sr.StudentProfileId == enrollment.StudentProfileId &&
                               sr.ReportDate >= DateOnly.FromDateTime(startDate) &&
                               sr.ReportDate <= DateOnly.FromDateTime(endDate) &&
                               !sr.IsMonthlyCompiled)
                    .ToListAsync(cancellationToken);

                foreach (var sr in sessionReports)
                {
                    sr.IsMonthlyCompiled = true;
                    sr.UpdatedAt = now;
                }
            }

            reportIds.Add(report.Id);
        }

        // Update job status
        job.Status = MonthlyReportJobStatus.Done;
        job.FinishedAt = now;
        job.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new AggregateMonthlyReportDataResponse
        {
            TotalReportsCreated = totalCreated,
            TotalReportsUpdated = totalUpdated,
            ReportIds = reportIds
        };
    }
}

