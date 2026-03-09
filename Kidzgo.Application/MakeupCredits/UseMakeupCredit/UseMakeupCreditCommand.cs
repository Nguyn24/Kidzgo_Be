using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MakeupCredits.UseMakeupCredit;

public sealed class UseMakeupCreditCommand : ICommand
{
    public Guid MakeupCreditId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid ClassId { get; set; }
    public Guid TargetSessionId { get; set; }
}



