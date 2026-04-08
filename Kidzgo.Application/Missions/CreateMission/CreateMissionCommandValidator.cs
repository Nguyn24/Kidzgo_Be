using FluentValidation;

namespace Kidzgo.Application.Missions.CreateMission;

public sealed class CreateMissionCommandValidator : AbstractValidator<CreateMissionCommand>
{
    public CreateMissionCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Title is required");

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

