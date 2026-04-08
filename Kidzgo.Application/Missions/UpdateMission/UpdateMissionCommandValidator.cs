using FluentValidation;

namespace Kidzgo.Application.Missions.UpdateMission;

public sealed class UpdateMissionCommandValidator : AbstractValidator<UpdateMissionCommand>
{
    public UpdateMissionCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Mission ID is required");

        RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .When(command => !string.IsNullOrWhiteSpace(command.Title));

        RuleFor(command => command.StartAt)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("StartAt cannot be in the past")
            .When(command => command.StartAt.HasValue);

        RuleFor(command => command.EndAt)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("EndAt cannot be in the past")
            .When(command => command.EndAt.HasValue)
            .GreaterThanOrEqualTo(command => command.StartAt)
            .WithMessage("EndAt must be greater than or equal to StartAt")
            .When(command => command.EndAt.HasValue && command.StartAt.HasValue);
    }
}

