namespace Kidzgo.API.Requests;

public sealed class CreateClassroomRequest
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public string? Note { get; set; }
    public string? Floor { get; set; }
    public decimal? Area { get; set; }
    public List<string>? Equipment { get; set; }
}

