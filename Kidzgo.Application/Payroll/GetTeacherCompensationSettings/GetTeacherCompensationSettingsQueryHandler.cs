using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Payroll.GetTeacherCompensationSettings;

public sealed class GetTeacherCompensationSettingsQueryHandler(
    IDbContext context
) : IQueryHandler<GetTeacherCompensationSettingsQuery, GetTeacherCompensationSettingsResponse>
{
    public async Task<Result<GetTeacherCompensationSettingsResponse>> Handle(
        GetTeacherCompensationSettingsQuery query,
        CancellationToken cancellationToken)
    {
        var settings = await context.TeacherCompensationSettings.FirstOrDefaultAsync(cancellationToken);

        return Result.Success(new GetTeacherCompensationSettingsResponse
        {
            StandardSessionDurationMinutes = settings?.StandardSessionDurationMinutes ?? 90,
            ForeignTeacherDefaultSessionRate = settings?.ForeignTeacherDefaultSessionRate ?? 0m,
            VietnameseTeacherDefaultSessionRate = settings?.VietnameseTeacherDefaultSessionRate ?? 0m,
            AssistantDefaultSessionRate = settings?.AssistantDefaultSessionRate ?? 0m
        });
    }
}
