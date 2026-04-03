using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Programs;

public class ExtracurricularProgram : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public DateOnly Date { get; set; }
    public int Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public decimal Fee { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Branch Branch { get; set; } = null!;
}
