using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Domain.Media;

public class MediaAsset : Entity
{
    public Guid Id { get; set; }
    public Guid UploaderId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public string? MonthTag { get; set; }
    public MediaType Type { get; set; }
    public string Url { get; set; } = null!;
    public string? Caption { get; set; }
    public Visibility Visibility { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User UploaderUser { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Class? Class { get; set; }
    public Profile? StudentProfile { get; set; }
}
