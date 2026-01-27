using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Classes.ChangeClassStatus;

public sealed class ChangeClassStatusResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
}

