using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.UseMakeupCredit;

public sealed class UseMakeupCreditCommand : ICommand
{
    public Guid MakeupCreditId { get; set; }
    public Guid ClassId { get; set; }
    public Guid TargetSessionId { get; set; }
}



