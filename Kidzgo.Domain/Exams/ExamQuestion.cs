using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams;

public class ExamQuestion : Entity
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public int OrderIndex { get; set; } // Order of question in the exam
    public string QuestionText { get; set; } = null!;
    public QuestionType QuestionType { get; set; } // MULTIPLE_CHOICE, TEXT_INPUT
    public string? Options { get; set; } // JSON array for multiple choice options
    public string? CorrectAnswer { get; set; } // Correct answer (for multiple choice: option index, for text: answer text)
    public int Points { get; set; } // Points awarded for correct answer
    public string? Explanation { get; set; } // Explanation of the answer
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Exam Exam { get; set; } = null!;
}

