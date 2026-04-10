using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.TeachingMaterials;

public class TeachingMaterial : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public int? UnitNumber { get; set; }
    public int? LessonNumber { get; set; }
    public string? LessonTitle { get; set; }
    public string? RelativePath { get; set; }
    public string DisplayName { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string StoragePath { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public string FileExtension { get; set; } = null!;
    public long FileSize { get; set; }
    public TeachingMaterialFileType FileType { get; set; }
    public TeachingMaterialCategory Category { get; set; }
    public bool IsEncrypted { get; set; }
    public string EncryptionAlgorithm { get; set; } = null!;
    public string EncryptionKeyVersion { get; set; } = null!;
    public string? PdfPreviewPath { get; private set; }
    public DateTime? PdfPreviewGeneratedAt { get; private set; }
    public long? PdfPreviewFileSize { get; private set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Program Program { get; set; } = null!;
    public User UploadedByUser { get; set; } = null!;
    public ICollection<TeachingMaterialSlide> Slides { get; set; } = new List<TeachingMaterialSlide>();
    public ICollection<TeachingMaterialViewProgress> ViewProgresses { get; set; } = new List<TeachingMaterialViewProgress>();
    public ICollection<TeachingMaterialBookmark> Bookmarks { get; set; } = new List<TeachingMaterialBookmark>();
    public ICollection<TeachingMaterialAnnotation> Annotations { get; set; } = new List<TeachingMaterialAnnotation>();

    public void SetPdfPreview(string path, long fileSize)
    {
        PdfPreviewPath = path;
        PdfPreviewFileSize = fileSize;
        PdfPreviewGeneratedAt = DateTime.UtcNow;
    }

    public void ClearPdfPreview()
    {
        PdfPreviewPath = null;
        PdfPreviewFileSize = null;
        PdfPreviewGeneratedAt = null;
    }
}
