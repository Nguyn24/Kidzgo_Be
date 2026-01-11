namespace Kidzgo.API.Requests;

public sealed class GradeExamSubmissionRequest
{
    public decimal? FinalScore { get; set; }
    public string? TeacherComment { get; set; }
    public List<GradeAnswerItemRequest>? AnswerGrades { get; set; }
}

public sealed class GradeAnswerItemRequest
{
    public Guid QuestionId { get; set; }
    public decimal? PointsAwarded { get; set; }
    public string? TeacherFeedback { get; set; }
}


