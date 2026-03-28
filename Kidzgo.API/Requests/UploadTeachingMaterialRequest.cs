using Microsoft.AspNetCore.Http;

namespace Kidzgo.API.Requests;

public sealed class UploadTeachingMaterialRequest
{
    public Guid? ProgramId { get; init; }
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? LessonTitle { get; init; }
    public string? DisplayName { get; init; }
    public string? Category { get; init; }
    public IFormFile? File { get; init; }
    public List<IFormFile>? Files { get; init; }
    public List<string>? RelativePaths { get; init; }
    public IFormFile? Archive { get; init; }
}
