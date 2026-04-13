using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Profiles.VerifyParentPinResetZaloOtp;

public sealed class VerifyParentPinResetZaloOtpCommandHandler(
    IDbContext context,
    IPasswordHasher passwordHasher
) : ICommandHandler<VerifyParentPinResetZaloOtpCommand, VerifyParentPinResetZaloOtpResponse>
{
    private const int MaxOtpAttempts = 5;

    public async Task<Result<VerifyParentPinResetZaloOtpResponse>> Handle(
        VerifyParentPinResetZaloOtpCommand command,
        CancellationToken cancellationToken)
    {
        ParentPinResetToken? token = await context.ParentPinResetTokens
            .Include(t => t.Profile)
            .SingleOrDefaultAsync(t => t.Id == command.ChallengeId, cancellationToken);

        if (token is null || token.IsUsed || string.IsNullOrWhiteSpace(token.OtpCodeHash))
        {
            return Result.Failure<VerifyParentPinResetZaloOtpResponse>(PinErrors.InvalidResetToken);
        }

        if (token.Profile.ProfileType != ProfileType.Parent || token.Profile.IsDeleted || !token.Profile.IsActive)
        {
            return Result.Failure<VerifyParentPinResetZaloOtpResponse>(ProfileErrors.Invalid);
        }

        if (!token.OtpExpiresAt.HasValue || VietnamTime.UtcNow() > token.OtpExpiresAt.Value)
        {
            return Result.Failure<VerifyParentPinResetZaloOtpResponse>(PinErrors.InvalidOtp);
        }

        if (token.OtpVerifiedAt.HasValue)
        {
            return Result.Success(new VerifyParentPinResetZaloOtpResponse
            {
                ResetToken = token.Token,
                ExpiresAt = token.ExpiresAt
            });
        }

        if (token.OtpAttemptCount >= MaxOtpAttempts)
        {
            return Result.Failure<VerifyParentPinResetZaloOtpResponse>(PinErrors.InvalidResetToken);
        }

        bool isValidOtp = passwordHasher.Verify(command.Otp, token.OtpCodeHash);
        if (!isValidOtp)
        {
            token.OtpAttemptCount += 1;
            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<VerifyParentPinResetZaloOtpResponse>(PinErrors.InvalidOtp);
        }

        token.OtpVerifiedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new VerifyParentPinResetZaloOtpResponse
        {
            ResetToken = token.Token,
            ExpiresAt = token.ExpiresAt
        });
    }
}
