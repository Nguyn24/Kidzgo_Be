using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans;

public class HomeworkQuestion : Entity
{
    public Guid Id { get; set; }
    public Guid HomeworkAssignmentId { get; set; }
    public int OrderIndex { get; set; }
    public string QuestionText { get; set; } = null!;
    public HomeworkQuestionType QuestionType { get; set; }
    public string? Options { get; set; } // JSON array for multiple choice options
    public string? CorrectAnswer { get; set; } // Correct answer (for multiple choice: option index, for text: answer text)
    public int Points { get; set; }
    public string? Explanation { get; set; }

    // Navigation properties
    public HomeworkAssignment HomeworkAssignment { get; set; } = null!;
}

public enum HomeworkQuestionType
{
    MultipleChoice,
    TextInput
}

