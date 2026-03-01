using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.MonthlyReports.AggregateMonthlyReportData;

/// <summary>
/// UC-175: Gom d? li?u cho Monthly Report
/// T?o reports cho t?t c? students trong job
/// Processes data in batches with progress tracking
/// </summary>
public sealed class AggregateMonthlyReportDataCommandHandler(
    IDbContext context,
    IMonthlyReportDataAggregator dataAggregator,
    ILogger<AggregateMonthlyReportDataCommandHandler> logger)
    : ICommandHandler<AggregateMonthlyReportDataCommand, AggregateMonthlyReportDataResponse>
{
    public async Task<Result<AggregateMonthlyReportDataResponse>> Handle(
        AggregateMonthlyReportDataCommand command,
        CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // Get job
        var job = await context.MonthlyReportJobs
            .Include(j => j.Branch)
            .FirstOrDefaultAsync(j => j.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure<AggregateMonthlyReportDataResponse>(
                MonthlyReportErrors.JobNotFound(command.JobId));
        }

        // Update job status to Generating
        job.Status = MonthlyReportJobStatus.Generating;
        job.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        // Get all active enrollments in the branch for the month
        var startDate = new DateTime(job.Year, job.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var enrollments = await context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Where(e => e.Class.BranchId == job.BranchId &&
                       e.Status == EnrollmentStatus.Active)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var reportIds = new List<Guid>();
        int totalCreated = 0;
        int totalUpdated = 0;
        var now = DateTime.UtcNow;

        // Load ALL existing reports and report data ONCE at the beginning
        var studentProfileIds = enrollments.Select(e => e.StudentProfileId).Distinct().ToList();
        
        var existingReports = await context.StudentMonthlyReports
            .Where(r => studentProfileIds.Contains(r.StudentProfileId) &&
                       r.Month == job.Month &&
                       r.Year == job.Year)
            .ToListAsync(cancellationToken);

        var existingReportData = await context.MonthlyReportData
            .Where(rd => studentProfileIds.Contains(rd.StudentProfileId) &&
                        rd.Month == job.Month &&
                        rd.Year == job.Year)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Track reports in memory
        var reportsInMemory = existingReports
            .ToDictionary(r => (r.StudentProfileId, r.Month, r.Year), r => r);
        
        var reportDataInMemory = existingReportData
            .ToDictionary(rd => rd.ReportId, rd => rd);

        logger.LogInformation("Processing {Count} enrollments for JobId: {JobId}", 
            enrollments.Count, command.JobId);

        foreach (var enrollment in enrollments)
        {
            try
            {
                var key = (enrollment.StudentProfileId, job.Month, job.Year);

                // Check if report already exists
                bool isNewReport = !reportsInMemory.TryGetValue(key, out var report);
                
                if (isNewReport)
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
                    reportsInMemory[key] = report;
                    totalCreated++;
                }
                else
                {
                    // Update existing report - attach to context
                    context.StudentMonthlyReports.Attach(report);
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

                // Check if report data exists
                bool hasExistingData = reportDataInMemory.TryGetValue(report.Id, out var reportData);

                if (!hasExistingData)
                {
                    // Create new report data
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
                    reportDataInMemory[report.Id] = reportData;
                }
                else
                {
                    // Update existing report data - attach to context
                    context.MonthlyReportData.Attach(reportData);
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
                if (root.TryGetProperty("topics", out var topics))
                {
                    reportData.TopicsData = topics.GetRawText();
                }

                // Mark session reports as compiled (only for newly created reports)
                if (isNewReport)
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

                // Save every 10 records to avoid large transactions
                if ((totalCreated + totalUpdated) % 10 == 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing enrollment {StudentProfileId} for JobId: {JobId}", 
                    enrollment.StudentProfileId, command.JobId);
                // Continue with next enrollment on error
                continue;
            }
        }

        // Final save
        await context.SaveChangesAsync(cancellationToken);

        // Update job status to Done
        context.MonthlyReportJobs.Attach(job);
        job.Status = MonthlyReportJobStatus.Done;
        job.FinishedAt = now;
        job.UpdatedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        logger.LogInformation("Completed aggregation for JobId: {JobId}. Created: {Created}, Updated: {Updated}, Duration: {Duration}ms",
            command.JobId, totalCreated, totalUpdated, stopwatch.ElapsedMilliseconds);

        return new AggregateMonthlyReportDataResponse
        {
            TotalReportsCreated = totalCreated,
            TotalReportsUpdated = totalUpdated,
            ReportIds = reportIds,
            DurationMs = stopwatch.ElapsedMilliseconds,
            TotalEnrollmentsProcessed = enrollments.Count
        };
    }
}

