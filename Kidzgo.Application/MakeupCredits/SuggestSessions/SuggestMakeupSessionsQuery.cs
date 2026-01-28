using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.SuggestSessions;

public sealed class SuggestMakeupSessionsQuery : IQuery<IEnumerable<SuggestedSessionResponse>>
{
    public Guid MakeupCreditId { get; set; }

    /// <summary>
    /// Ngày mong muốn học bù (DateOnly).
    /// </summary>
    public DateOnly MakeupDate { get; set; }

    /// <summary>
    /// Buổi trong ngày mong muốn (vd: \"morning\", \"afternoon\", \"evening\").
    /// Nếu null hoặc rỗng thì không lọc theo buổi.
    /// </summary>
    public string? TimeOfDay { get; set; }
}





