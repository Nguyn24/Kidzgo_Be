using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Domain.Reports;
using Kidzgo.Infrastructure.AI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kidzgo.Infrastructure.AI;

/// <summary>
/// Implementation of IAiReportGenerator that calls A6 API (Python FastAPI)
/// </summary>
public sealed class HttpAiReportGenerator : IAiReportGenerator
{
    private readonly HttpClient _httpClient;
    private readonly IDbContext _context;
    private readonly string _baseUrl;

    public HttpAiReportGenerator(
        HttpClient httpClient,
        IDbContext context,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _context = context;
        _baseUrl = configuration["AiService:BaseUrl"] 
            ?? throw new InvalidOperationException("AiService:BaseUrl not configured");
    }

    public async Task<string> GenerateDraftAsync(
        string dataJson,
        Guid studentProfileId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            throw new ArgumentException("Aggregated data JSON cannot be null or empty", nameof(dataJson));
        }

        // Parse aggregated data JSON (case-insensitive to handle CamelCase from reconstructed data)
        JsonSerializerOptions parseOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        Dictionary<string, JsonElement>? aggregatedData;
        try
        {
            aggregatedData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dataJson, parseOptions);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format in aggregated data: {ex.Message}", nameof(dataJson), ex);
        }

        if (aggregatedData is null)
        {
            throw new ArgumentException("Aggregated data JSON is empty", nameof(dataJson));
        }

        // Extract session feedbacks from notes data (case-insensitive)
        var sessionFeedbacks = new List<A6SessionFeedback>();
        
        // Try both lowercase and CamelCase for "notes" key
        if (!aggregatedData.TryGetValue("notes", out var notesElement) &&
            !aggregatedData.TryGetValue("Notes", out notesElement))
        {
            Console.WriteLine("[DEBUG] No 'notes' or 'Notes' key found in aggregated data");
            // No notes data, return empty
        }
        else
        {

            if (notesElement.TryGetProperty("sessionReports", out var sessionReportsElement) ||
                notesElement.TryGetProperty("SessionReports", out sessionReportsElement))
            {
                foreach (var report in sessionReportsElement.EnumerateArray())
                {
                    string? feedback = null;
                    
                    // Try both lowercase and CamelCase
                    if (report.TryGetProperty("feedback", out var feedbackElement))
                    {
                        feedback = feedbackElement.GetString();
                    }
                    else if (report.TryGetProperty("Feedback", out feedbackElement))
                    {
                        feedback = feedbackElement.GetString();
                    }

                    if (!string.IsNullOrWhiteSpace(feedback))
                    {
                        string? reportDate = null;
                        
                        if (report.TryGetProperty("reportDate", out var dateElement))
                        {
                            reportDate = dateElement.GetString();
                        }
                        else if (report.TryGetProperty("sessionDate", out var sessionDateElement))
                        {
                            reportDate = sessionDateElement.GetString();
                        }
                        else if (report.TryGetProperty("ReportDate", out dateElement))
                        {
                            reportDate = dateElement.GetString();
                        }
                        else if (report.TryGetProperty("SessionDate", out sessionDateElement))
                        {
                            reportDate = sessionDateElement.GetString();
                        }

                        sessionFeedbacks.Add(new A6SessionFeedback
                        {
                            Date = reportDate ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                            Text = feedback
                        });
                        
                    }
                }
            }
        }

        

        // Get student profile to extract student info
        var studentProfile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentProfileId, cancellationToken);

        if (studentProfile is null)
        {
            throw new InvalidOperationException($"Student profile with ID {studentProfileId} not found");
        }

        var studentId = studentProfile.Id.ToString();
        var studentName = studentProfile.DisplayName ?? "Unknown Student";
        
        // Get program from class enrollment (if any)
        var enrollment = await _context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Where(e => e.StudentProfileId == studentProfileId && 
                       e.Status == Domain.Classes.EnrollmentStatus.Active)
            .OrderByDescending(e => e.EnrollDate)
            .FirstOrDefaultAsync(cancellationToken);
        
        var programName = enrollment?.Class?.Program?.Name;

        // Get recent reports (3 months before current month)
        var recentReports = await GetRecentReportsAsync(
            studentProfileId,
            month,
            year,
            cancellationToken);

        // Calculate date range
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Build request
        var request = new A6MonthlyReportRequest
        {
            Student = new A6StudentInfo
            {
                StudentId = studentId,
                Name = studentName,
                Program = programName
            },
            Range = new A6ReportRange
            {
                FromDate = startDate.ToString("yyyy-MM-dd"),
                ToDate = endDate.ToString("yyyy-MM-dd")
            },
            SessionFeedbacks = sessionFeedbacks,
            RecentReports = recentReports,
            Language = "vi"
        };

        

        // Call A6 API
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/a6/generate-monthly-report",
                request,
                jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"A6 API returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<A6MonthlyReportResponse>(
                jsonOptions,
                cancellationToken: cancellationToken);

            if (result is null)
            {
                throw new InvalidOperationException("A6 API returned null response");
            }

            // Serialize the full response to JSON string for storage (keep snake_case for consistency)
            return JsonSerializer.Serialize(result, jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to call A6 API: {ex.Message}. Make sure AI-KidzGo service is running at {_baseUrl}",
                ex);
        }
    }

    private async Task<List<A6RecentMonthlyReport>> GetRecentReportsAsync(
        Guid studentProfileId,
        int currentMonth,
        int currentYear,
        CancellationToken cancellationToken)
    {
        if (studentProfileId == Guid.Empty)
        {
            return new List<A6RecentMonthlyReport>();
        }

        var recentReports = new List<A6RecentMonthlyReport>();

        // Get reports from 3 months before current month
        for (int i = 1; i <= 3; i++)
        {
            var targetMonth = currentMonth - i;
            var targetYear = currentYear;

            if (targetMonth <= 0)
            {
                targetMonth += 12;
                targetYear--;
            }

            var report = await _context.StudentMonthlyReports
                .Where(r => r.StudentProfileId == studentProfileId &&
                           r.Month == targetMonth &&
                           r.Year == targetYear &&
                           r.Status == ReportStatus.Published &&
                           r.FinalContent != null)
                .OrderByDescending(r => r.PublishedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (report?.FinalContent != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(report.FinalContent);
                    var root = doc.RootElement;

                    var recentReport = new A6RecentMonthlyReport
                    {
                        Month = $"{targetYear}-{targetMonth:D2}"
                    };

                    // Case-insensitive property access
                    if (root.TryGetProperty("sections", out var sections) ||
                        root.TryGetProperty("Sections", out sections))
                    {
                        if (sections.TryGetProperty("overview", out var overview) ||
                            sections.TryGetProperty("Overview", out overview))
                        {
                            recentReport.Overview = overview.GetString();
                        }

                        if (sections.TryGetProperty("strengths", out var strengths) ||
                            sections.TryGetProperty("Strengths", out strengths))
                        {
                            recentReport.Strengths = strengths.EnumerateArray()
                                .Select(s => s.GetString() ?? string.Empty)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();
                        }

                        if (sections.TryGetProperty("improvements", out var improvements) ||
                            sections.TryGetProperty("Improvements", out improvements))
                        {
                            recentReport.Improvements = improvements.EnumerateArray()
                                .Select(s => s.GetString() ?? string.Empty)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();
                        }

                        if (sections.TryGetProperty("highlights", out var highlights) ||
                            sections.TryGetProperty("Highlights", out highlights))
                        {
                            recentReport.Highlights = highlights.EnumerateArray()
                                .Select(s => s.GetString() ?? string.Empty)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();
                        }

                        if (sections.TryGetProperty("goalsNextMonth", out var goals) ||
                            sections.TryGetProperty("GoalsNextMonth", out goals))
                        {
                            recentReport.GoalsNextMonth = goals.EnumerateArray()
                                .Select(s => s.GetString() ?? string.Empty)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();
                        }
                    }

                    recentReports.Add(recentReport);
                }
                catch (JsonException)
                {
                    // Skip invalid JSON
                    continue;
                }
            }
        }

        return recentReports;
    }
}
