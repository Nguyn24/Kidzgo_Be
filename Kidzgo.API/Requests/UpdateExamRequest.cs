using Kidzgo.Domain.Exams;

namespace Kidzgo.API.Requests;

public sealed class UpdateExamRequest
{
    public ExamType? ExamType { get; set; }
    public DateOnly? Date { get; set; }
    public decimal? MaxScore { get; set; }
    public string? Description { get; set; }
    
    // Thời gian thi (cho thi tại trung tâm)
    public DateTime? ScheduledStartTime { get; set; } // Thời gian bắt đầu thi (theo lịch)
    public int? TimeLimitMinutes { get; set; } // Thời gian làm bài (phút)
    public bool? AllowLateStart { get; set; } // Cho phép bắt đầu muộn (nếu học sinh đến muộn)
    public int? LateStartToleranceMinutes { get; set; } // Cho phép muộn tối đa bao nhiêu phút
    
    // Settings
    public bool? AutoSubmitOnTimeLimit { get; set; } // Tự động nộp khi hết giờ
    public bool? PreventCopyPaste { get; set; } // Prevent copy/paste (frontend)
    public bool? PreventNavigation { get; set; } // Prevent mở tab khác (frontend)
    public bool? ShowResultsImmediately { get; set; } // Hiển thị kết quả ngay sau khi nộp (cho multiple choice)
}

