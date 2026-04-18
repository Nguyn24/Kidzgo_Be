using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPaymentSetting;

public sealed class GetEnrollmentConfirmationPaymentSettingQuery
    : IQuery<GetEnrollmentConfirmationPaymentSettingResponse>
{
    public Guid? BranchId { get; init; }
}
