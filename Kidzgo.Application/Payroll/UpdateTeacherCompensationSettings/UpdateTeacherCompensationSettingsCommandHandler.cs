using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Payroll.UpdateTeacherCompensationSettings;

public sealed class UpdateTeacherCompensationSettingsCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateTeacherCompensationSettingsCommand, UpdateTeacherCompensationSettingsResponse>
{
    public async Task<Result<UpdateTeacherCompensationSettingsResponse>> Handle(
        UpdateTeacherCompensationSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var settings = await context.TeacherCompensationSettings.FirstOrDefaultAsync(cancellationToken);

        if (settings is null)
        {
            settings = new TeacherCompensationSettings
            {
                Id = 1,
                StandardSessionDurationMinutes = command.StandardSessionDurationMinutes,
                ForeignTeacherDefaultSessionRate = command.ForeignTeacherDefaultSessionRate,
                VietnameseTeacherDefaultSessionRate = command.VietnameseTeacherDefaultSessionRate,
                AssistantDefaultSessionRate = command.AssistantDefaultSessionRate,
                CreatedAt = VietnamTime.UtcNow()
            };

            context.TeacherCompensationSettings.Add(settings);
        }
        else
        {
            settings.StandardSessionDurationMinutes = command.StandardSessionDurationMinutes;
            settings.ForeignTeacherDefaultSessionRate = command.ForeignTeacherDefaultSessionRate;
            settings.VietnameseTeacherDefaultSessionRate = command.VietnameseTeacherDefaultSessionRate;
            settings.AssistantDefaultSessionRate = command.AssistantDefaultSessionRate;
            settings.UpdatedAt = VietnamTime.UtcNow();
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateTeacherCompensationSettingsResponse
        {
            StandardSessionDurationMinutes = settings.StandardSessionDurationMinutes,
            ForeignTeacherDefaultSessionRate = settings.ForeignTeacherDefaultSessionRate,
            VietnameseTeacherDefaultSessionRate = settings.VietnameseTeacherDefaultSessionRate,
            AssistantDefaultSessionRate = settings.AssistantDefaultSessionRate
        });
    }
}
