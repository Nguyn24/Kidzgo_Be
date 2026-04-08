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
        Guid? classId,
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
        }
        else
        {
            if (notesElement.TryGetProperty("sessionReports", out var sessionReportsElement) ||
                notesElement.TryGetProperty("SessionReports", out sessionReportsElement))
            {
                foreach (var report in sessionReportsElement.EnumerateArray())
                {
                    string? feedback = null;
                    
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
                            Date = reportDate ?? VietnamTime.TodayDateOnly().ToString("yyyy-MM-dd"),
                            Text = feedback
                        });
                    }
                }
            }
        }

        // Extract attendance data
        A6AttendanceData? attendanceData = null;
        if (aggregatedData.TryGetValue("attendance", out var attElement) ||
            aggregatedData.TryGetValue("Attendance", out attElement))
        {
            attendanceData = new A6AttendanceData
            {
                Total = attElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Present = attElement.TryGetProperty("present", out var p) ? p.GetInt32() : 0,
                Absent = attElement.TryGetProperty("absent", out var a) ? a.GetInt32() : 0,
                Makeup = attElement.TryGetProperty("makeup", out var m) ? m.GetInt32() : 0,
                NotMarked = attElement.TryGetProperty("notMarked", out var nm) ? nm.GetInt32() : 0,
                Percentage = attElement.TryGetProperty("percentage", out var pct) ? pct.GetSingle() : 0
            };
        }

        // Extract homework data
        A6HomeworkData? homeworkData = null;
        if (aggregatedData.TryGetValue("homework", out var hwElement) ||
            aggregatedData.TryGetValue("Homework", out hwElement))
        {
            homeworkData = new A6HomeworkData
            {
                Total = hwElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Completed = hwElement.TryGetProperty("completed", out var c) ? c.GetInt32() : 0,
                Submitted = hwElement.TryGetProperty("submitted", out var s) ? s.GetInt32() : 0,
                Pending = hwElement.TryGetProperty("pending", out var p) ? p.GetInt32() : 0,
                Late = hwElement.TryGetProperty("late", out var l) ? l.GetInt32() : 0,
                Missing = hwElement.TryGetProperty("missing", out var m) ? m.GetInt32() : 0,
                Average = hwElement.TryGetProperty("average", out var avg) ? avg.GetSingle() : 0,
                CompletionRate = hwElement.TryGetProperty("completionRate", out var cr) ? cr.GetSingle() : 0,
                Topics = ReadStringList(hwElement, "topics", "Topics"),
                Skills = ReadStringList(hwElement, "skills", "Skills"),
                GrammarTags = ReadStringList(hwElement, "grammarTags", "GrammarTags"),
                VocabularyTags = ReadStringList(hwElement, "vocabularyTags", "VocabularyTags"),
                SpeakingAssignments = hwElement.TryGetProperty("speakingAssignments", out var sa) ? sa.GetInt32() : 0,
                AiSupportedAssignments = hwElement.TryGetProperty("aiSupportedAssignments", out var asa) ? asa.GetInt32() : 0
            };
        }

        // Extract test data
        A6TestData? testData = null;
        if (aggregatedData.TryGetValue("test", out var testElement) ||
            aggregatedData.TryGetValue("Test", out testElement))
        {
            testData = new A6TestData
            {
                Total = testElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Tests = new List<A6TestResult>()
            };

            if (testElement.TryGetProperty("tests", out var testsElement) ||
                testElement.TryGetProperty("Tests", out testsElement))
            {
                foreach (var test in testsElement.EnumerateArray())
                {
                    testData.Tests.Add(new A6TestResult
                    {
                        ExamId = test.TryGetProperty("examId", out var eid) ? eid.GetString() ?? "" : "",
                        Type = test.TryGetProperty("type", out var typ) ? typ.GetString() ?? "" : "",
                        Score = test.TryGetProperty("score", out var sc) ? sc.GetSingle() : 0,
                        MaxScore = test.TryGetProperty("maxScore", out var ms) ? ms.GetSingle() : 0,
                        Date = test.TryGetProperty("date", out var dt) ? dt.GetString() ?? "" : "",
                        Comment = test.TryGetProperty("comment", out var cm) ? cm.GetString() : null
                    });
                }
            }
        }

        // Extract mission data
        A6MissionData? missionData = null;
        if (aggregatedData.TryGetValue("mission", out var missionElement) ||
            aggregatedData.TryGetValue("Mission", out missionElement))
        {
            missionData = new A6MissionData
            {
                Completed = missionElement.TryGetProperty("completed", out var c) ? c.GetInt32() : 0,
                Total = missionElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                InProgress = missionElement.TryGetProperty("inProgress", out var ip) ? ip.GetInt32() : 0,
                Stars = missionElement.TryGetProperty("stars", out var s) ? s.GetInt32() : 0,
                Xp = missionElement.TryGetProperty("xp", out var x) ? x.GetInt32() : 0,
                CurrentLevel = missionElement.TryGetProperty("currentLevel", out var cl) ? cl.GetString() ?? "0" : "0",
                CurrentXp = missionElement.TryGetProperty("currentXp", out var cxp) ? cxp.GetInt32() : 0
            };
        }

        // Extract topics data
        A6TopicsData? topicsData = null;
        if (aggregatedData.TryGetValue("topics", out var topicsElement) ||
            aggregatedData.TryGetValue("Topics", out topicsElement))
        {
            topicsData = new A6TopicsData
            {
                Total = topicsElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Topics = new List<string>(),
                LessonContents = new List<string>()
            };

            if (topicsElement.TryGetProperty("topics", out var topicsList) ||
                topicsElement.TryGetProperty("Topics", out topicsList))
            {
                topicsData.Topics = topicsList.EnumerateArray()
                    .Select(t => t.GetString() ?? "")
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
            }

            if (topicsElement.TryGetProperty("lessonContents", out var contentsList) ||
                topicsElement.TryGetProperty("LessonContents", out contentsList))
            {
                topicsData.LessonContents = contentsList.EnumerateArray()
                    .Select(c => c.GetString() ?? "")
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList();
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
        
        Domain.Classes.Class? reportClass = null;

        if (classId.HasValue)
        {
            reportClass = await _context.Classes
                .Include(c => c.Program)
                .FirstOrDefaultAsync(c => c.Id == classId.Value, cancellationToken);
        }

        if (reportClass is null)
        {
            reportClass = await _context.ClassEnrollments
                .Include(e => e.Class)
                    .ThenInclude(c => c.Program)
                .Where(e => e.StudentProfileId == studentProfileId &&
                           e.Status == Domain.Classes.EnrollmentStatus.Active)
                .OrderByDescending(e => e.EnrollDate)
                .Select(e => e.Class)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var programId = reportClass?.ProgramId;
        var programName = reportClass?.Program?.Name;
        var className = reportClass?.Title;

        // Get recent reports (3 months before current month)
        var recentReports = await GetRecentReportsAsync(
            studentProfileId,
            programId,
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
                Program = programName,
                ClassName = className
            },
            Range = new A6ReportRange
            {
                FromDate = startDate.ToString("yyyy-MM-dd"),
                ToDate = endDate.ToString("yyyy-MM-dd")
            },
            Attendance = attendanceData,
            Homework = homeworkData,
            Test = testData,
            Mission = missionData,
            Topics = topicsData,
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
        Guid? programId,
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

            var reportsQuery = _context.StudentMonthlyReports
                .Include(r => r.Class)
                .Where(r => r.StudentProfileId == studentProfileId &&
                           r.Month == targetMonth &&
                           r.Year == targetYear &&
                           r.Status == ReportStatus.Published &&
                           r.FinalContent != null);

            if (programId.HasValue)
            {
                reportsQuery = reportsQuery
                    .Where(r => r.ClassId.HasValue && r.Class != null && r.Class.ProgramId == programId.Value);
            }

            var report = await reportsQuery
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

    private static List<string> ReadStringList(JsonElement element, string primaryProperty, string fallbackProperty)
    {
        if (element.TryGetProperty(primaryProperty, out var valuesElement) ||
            element.TryGetProperty(fallbackProperty, out valuesElement))
        {
            return valuesElement.ValueKind == JsonValueKind.Array
                ? valuesElement.EnumerateArray()
                    .Select(item => item.GetString() ?? string.Empty)
                    .Where(static item => !string.IsNullOrWhiteSpace(item))
                    .ToList()
                : new List<string>();
        }

        return new List<string>();
    }
}
