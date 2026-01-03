namespace Kidzgo.API.Requests;

public sealed class UpdateClassroomRequest
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public string? Note { get; set; }
}

