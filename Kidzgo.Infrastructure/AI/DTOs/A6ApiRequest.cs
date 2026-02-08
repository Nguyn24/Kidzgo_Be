using System.Text.Json.Serialization;

namespace Kidzgo.Infrastructure.AI.DTOs;

/// <summary>
/// DTOs for A6 API (Python FastAPI) request/response
/// Note: All properties use JsonPropertyName to match Python snake_case naming
/// </summary>
public sealed class A6StudentInfo
{
    [JsonPropertyName("student_id")]
    public string StudentId { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("age")]
    public int? Age { get; set; }
    
    [JsonPropertyName("program")]
    public string? Program { get; set; }
}

public sealed class A6ReportRange
{
    [JsonPropertyName("from_date")]
    public string FromDate { get; set; } = string.Empty; // YYYY-MM-DD
    
    [JsonPropertyName("to_date")]
    public string ToDate { get; set; } = string.Empty; // YYYY-MM-DD
}

public sealed class A6SessionFeedback
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public sealed class A6RecentMonthlyReport
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty; // YYYY-MM
    
    [JsonPropertyName("overview")]
    public string? Overview { get; set; }
    
    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();
    
    [JsonPropertyName("improvements")]
    public List<string> Improvements { get; set; } = new();
    
    [JsonPropertyName("highlights")]
    public List<string> Highlights { get; set; } = new();
    
    [JsonPropertyName("goals_next_month")]
    public List<string> GoalsNextMonth { get; set; } = new();
}

public sealed class A6MonthlyReportRequest
{
    [JsonPropertyName("student")]
    public A6StudentInfo Student { get; set; } = new();
    
    [JsonPropertyName("range")]
    public A6ReportRange Range { get; set; } = new();
    
    [JsonPropertyName("session_feedbacks")]
    public List<A6SessionFeedback> SessionFeedbacks { get; set; } = new();
    
    [JsonPropertyName("recent_reports")]
    public List<A6RecentMonthlyReport> RecentReports { get; set; } = new();
    
    [JsonPropertyName("teacher_notes")]
    public string? TeacherNotes { get; set; }
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = "vi";
}

public sealed class A6ReportSections
{
    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;
    
    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();
    
    [JsonPropertyName("improvements")]
    public List<string> Improvements { get; set; } = new();
    
    [JsonPropertyName("highlights")]
    public List<string> Highlights { get; set; } = new();
    
    [JsonPropertyName("goals_next_month")]
    public List<string> GoalsNextMonth { get; set; } = new();
    
    [JsonPropertyName("source_summary")]
    public Dictionary<string, int> SourceSummary { get; set; } = new();
}

public sealed class A6MonthlyReportResponse
{
    [JsonPropertyName("ai_used")]
    public bool AiUsed { get; set; }
    
    [JsonPropertyName("draft_text")]
    public string DraftText { get; set; } = string.Empty;
    
    [JsonPropertyName("sections")]
    public A6ReportSections Sections { get; set; } = new();
}

