using System.Text.Json.Serialization;

namespace Kidzgo.Infrastructure.AI.DTOs;

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
    public string FromDate { get; set; } = string.Empty;
    
    [JsonPropertyName("to_date")]
    public string ToDate { get; set; } = string.Empty;
}

public sealed class A6AttendanceData
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("present")]
    public int Present { get; set; }
    
    [JsonPropertyName("absent")]
    public int Absent { get; set; }
    
    [JsonPropertyName("makeup")]
    public int Makeup { get; set; }
    
    [JsonPropertyName("not_marked")]
    public int NotMarked { get; set; }
    
    [JsonPropertyName("percentage")]
    public float Percentage { get; set; }
}

public sealed class A6HomeworkData
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("completed")]
    public int Completed { get; set; }
    
    [JsonPropertyName("submitted")]
    public int Submitted { get; set; }
    
    [JsonPropertyName("pending")]
    public int Pending { get; set; }
    
    [JsonPropertyName("late")]
    public int Late { get; set; }
    
    [JsonPropertyName("missing")]
    public int Missing { get; set; }
    
    [JsonPropertyName("average")]
    public float Average { get; set; }
    
    [JsonPropertyName("completion_rate")]
    public float CompletionRate { get; set; }
}

public sealed class A6TestResult
{
    [JsonPropertyName("exam_id")]
    public string ExamId { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("score")]
    public float Score { get; set; }
    
    [JsonPropertyName("max_score")]
    public float MaxScore { get; set; }
    
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

public sealed class A6TestData
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("tests")]
    public List<A6TestResult> Tests { get; set; } = new();
}

public sealed class A6MissionData
{
    [JsonPropertyName("completed")]
    public int Completed { get; set; }
    
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("in_progress")]
    public int InProgress { get; set; }
    
    [JsonPropertyName("stars")]
    public int Stars { get; set; }
    
    [JsonPropertyName("xp")]
    public int Xp { get; set; }
    
    [JsonPropertyName("current_level")]
    public string CurrentLevel { get; set; } = "0";
    
    [JsonPropertyName("current_xp")]
    public int CurrentXp { get; set; }
}

public sealed class A6TopicsData
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
    
    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; } = new();
    
    [JsonPropertyName("lesson_contents")]
    public List<string> LessonContents { get; set; } = new();
}

public sealed class A6SessionFeedback
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public sealed class A6RecentMonthlyReport
{
    [JsonPropertyName("month")]
    public string Month { get; set; } = string.Empty;
    
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
    
    [JsonPropertyName("attendance")]
    public A6AttendanceData? Attendance { get; set; }
    
    [JsonPropertyName("homework")]
    public A6HomeworkData? Homework { get; set; }
    
    [JsonPropertyName("test")]
    public A6TestData? Test { get; set; }
    
    [JsonPropertyName("mission")]
    public A6MissionData? Mission { get; set; }
    
    [JsonPropertyName("topics")]
    public A6TopicsData? Topics { get; set; }
    
    [JsonPropertyName("session_feedbacks")]
    public List<A6SessionFeedback> SessionFeedbacks { get; set; } = new();
    
    [JsonPropertyName("recent_reports")]
    public List<A6RecentMonthlyReport> RecentReports { get; set; } = new();
    
    [JsonPropertyName("teacher_notes")]
    public string? TeacherNotes { get; set; }
    
    [JsonPropertyName("language")]
    public string Language { get; set; } = "vi";
}

public sealed class A6SkillAssessment
{
    [JsonPropertyName("phonics")]
    public string? Phonics { get; set; }
    
    [JsonPropertyName("speaking")]
    public string? Speaking { get; set; }
    
    [JsonPropertyName("listening")]
    public string? Listening { get; set; }
    
    [JsonPropertyName("writing")]
    public string? Writing { get; set; }
}

public sealed class A6ReportSections
{
    [JsonPropertyName("attendance_rate")]
    public string AttendanceRate { get; set; } = string.Empty;
    
    [JsonPropertyName("study_attitude")]
    public string StudyAttitude { get; set; } = string.Empty;
    
    [JsonPropertyName("progress_level")]
    public string ProgressLevel { get; set; } = string.Empty;
    
    [JsonPropertyName("progress_topics")]
    public List<string> ProgressTopics { get; set; } = new();
    
    [JsonPropertyName("skills")]
    public A6SkillAssessment Skills { get; set; } = new();
    
    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();
    
    [JsonPropertyName("improvements")]
    public List<string> Improvements { get; set; } = new();
    
    [JsonPropertyName("homework_completion")]
    public string HomeworkCompletion { get; set; } = string.Empty;
    
    [JsonPropertyName("parent_support")]
    public List<string> ParentSupport { get; set; } = new();
    
    [JsonPropertyName("source_summary")]
    public Dictionary<string, object> SourceSummary { get; set; } = new();
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
