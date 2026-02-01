using System.ComponentModel.DataAnnotations;

namespace Kidzgo.API.Requests;

public sealed class AddPlacementTestNoteRequest
{
    [Required]
    public string Note { get; set; } = null!;
}

