using Kidzgo.Domain.Classes;

namespace Kidzgo.API.Requests;

public sealed class ChangeClassStatusRequest
{
    public ClassStatus Status { get; set; }
}

