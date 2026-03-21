using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.QuestionBank.ImportQuestionBank;

public sealed class ImportQuestionBankFromFileCommand : ICommand<ImportQuestionBankFromFileResponse>
{
    public Guid ProgramId { get; init; }
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportQuestionBankFromFileResponse
{
    public int ImportedCount { get; init; }
}
