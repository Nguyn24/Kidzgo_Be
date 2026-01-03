using System.Text.Json.Serialization;
using Kidzgo.API.Extensions;

namespace Kidzgo.API.Requests;

public sealed class UpdateTuitionPlanRequest
{
    [JsonConverter(typeof(NullableGuidJsonConverter))]
    public Guid? BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public string Name { get; set; } = null!;
    public int TotalSessions { get; set; }
    public decimal TuitionAmount { get; set; }
    public decimal UnitPriceSession { get; set; }
    public string Currency { get; set; } = null!;
}

