using FluentValidation;
using VietnamTime = Kidzgo.Domain.Time.VietnamTime;

namespace Kidzgo.Application.Leads;

internal static class LeadChildDobValidationExtensions
{
    private const int MinimumAllowedAge = 2;
    private const int MaximumAllowedAge = 99;

    public static IRuleBuilderOptions<T, DateOnly?> MustBeValidLeadChildDob<T>(
        this IRuleBuilder<T, DateOnly?> ruleBuilder)
    {
        return ruleBuilder
            .Must(dob => !dob.HasValue || dob.Value <= VietnamTime.TodayDateOnly())
            .WithMessage("Child date of birth cannot be in the future")
            .Must(dob => !dob.HasValue || HasAllowedAge(dob.Value))
            .WithMessage($"Child age must be between {MinimumAllowedAge} and {MaximumAllowedAge}");
    }

    public static IRuleBuilderOptions<T, DateTime?> MustBeValidLeadChildDob<T>(
        this IRuleBuilder<T, DateTime?> ruleBuilder)
    {
        return ruleBuilder
            .Must(dob => !dob.HasValue || VietnamTime.ToVietnamDateOnly(dob.Value) <= VietnamTime.TodayDateOnly())
            .WithMessage("Child date of birth cannot be in the future")
            .Must(dob => !dob.HasValue || HasAllowedAge(VietnamTime.ToVietnamDateOnly(dob.Value)))
            .WithMessage($"Child age must be between {MinimumAllowedAge} and {MaximumAllowedAge}");
    }

    private static bool HasAllowedAge(DateOnly dob)
    {
        var today = VietnamTime.TodayDateOnly();

        if (dob > today)
        {
            return false;
        }

        var age = today.Year - dob.Year;

        if (dob > today.AddYears(-age))
        {
            age--;
        }

        return age >= MinimumAllowedAge && age <= MaximumAllowedAge;
    }
}
