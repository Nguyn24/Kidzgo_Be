using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplates;

public sealed class ImportLessonPlanTemplatesFromFileCommand : ICommand<ImportLessonPlanTemplatesFromFileResponse>
{
    public Guid? ProgramId { get; init; }
    public string? Level { get; init; }
    public bool OverwriteExisting { get; init; } = true;
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportLessonPlanTemplatesFromFileResponse
{
    public int ImportedCount { get; init; }
    public IReadOnlyList<ImportedLessonPlanTemplateProgramDto> Programs { get; init; } =
        Array.Empty<ImportedLessonPlanTemplateProgramDto>();
}

public sealed record ImportedLessonPlanTemplateProgramDto
{
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public int ImportedSessions { get; init; }
}
