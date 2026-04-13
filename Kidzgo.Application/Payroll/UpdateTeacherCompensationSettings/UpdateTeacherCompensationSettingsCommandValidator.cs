using FluentValidation;

namespace Kidzgo.Application.Payroll.UpdateTeacherCompensationSettings;

public sealed class UpdateTeacherCompensationSettingsCommandValidator
    : AbstractValidator<UpdateTeacherCompensationSettingsCommand>
{
    public UpdateTeacherCompensationSettingsCommandValidator()
    {
        RuleFor(command => command.StandardSessionDurationMinutes)
            .GreaterThan(0);

        RuleFor(command => command.ForeignTeacherDefaultSessionRate)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.VietnameseTeacherDefaultSessionRate)
            .GreaterThanOrEqualTo(0);

        RuleFor(command => command.AssistantDefaultSessionRate)
            .GreaterThanOrEqualTo(0);
    }
}
