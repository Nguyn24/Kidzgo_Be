using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Exams;

public class ExerciseQuestion : Entity
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public int OrderIndex { get; set; } // Order of question in the exercise
    public string QuestionText { get; set; } = null!;
    public QuestionType QuestionType { get; set; } // MULTIPLE_CHOICE, TEXT_INPUT
    public string? Options { get; set; } // JSON array for multiple choice options
    public string? CorrectAnswer { get; set; } // Correct answer (for multiple choice: option index, for text: answer text)
    public int Points { get; set; } // Points awarded for correct answer
    public string? Explanation { get; set; } // Explanation of the answer

    // Navigation properties
    public Exercise Exercise { get; set; } = null!;
}

public enum QuestionType
{
    MultipleChoice, // For reading/listening
    TextInput // For writing
}

