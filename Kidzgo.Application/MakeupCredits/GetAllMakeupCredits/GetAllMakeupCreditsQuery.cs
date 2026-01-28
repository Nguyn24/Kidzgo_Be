using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.GetAllMakeupCredits;

public sealed class GetAllMakeupCreditsQuery : IPageableQuery, IQuery<Page<MakeupCreditResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? StudentProfileId { get; init; }
    public MakeupCreditStatus? Status { get; init; }
    public Guid? BranchId { get; init; }
}


