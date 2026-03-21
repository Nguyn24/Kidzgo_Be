using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.Homework;

public class QuestionBankItem : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string QuestionText { get; set; } = null!;
    public HomeworkQuestionType QuestionType { get; set; }
    public string? Options { get; set; } // JSON array for multiple choice options
    public string? CorrectAnswer { get; set; } // Correct answer (option index for MC, or text)
    public int Points { get; set; }
    public string? Explanation { get; set; }
    public QuestionLevel Level { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Program Program { get; set; } = null!;
}

