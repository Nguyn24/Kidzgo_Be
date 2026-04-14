using System.Security.Cryptography;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Profiles.RequestParentPinResetZaloOtp;

public sealed class RequestParentPinResetZaloOtpCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IZaloService zaloService,
    IClientUrlProvider clientUrlProvider
) : ICommandHandler<RequestParentPinResetZaloOtpCommand, RequestParentPinResetZaloOtpResponse>
{
    private const int OtpExpiryMinutes = 10;
    private const int ResetTokenExpiryMinutes = 60;

    public async Task<Result<RequestParentPinResetZaloOtpResponse>> Handle(
        RequestParentPinResetZaloOtpCommand command,
        CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        Profile? profile = await context.Profiles
            .SingleOrDefaultAsync(
                p => p.UserId == userId &&
                     p.ProfileType == ProfileType.Parent &&
                     !p.IsDeleted &&
                     p.IsActive,
                cancellationToken);

        if (profile is null)
        {
            return Result.Failure<RequestParentPinResetZaloOtpResponse>(ProfileErrors.Invalid);
        }

        if (string.IsNullOrWhiteSpace(profile.ZaloId))
        {
            return Result.Failure<RequestParentPinResetZaloOtpResponse>(ProfileErrors.ZaloIdNotSet);
        }

        DateTime now = VietnamTime.UtcNow();

        List<ParentPinResetToken> oldTokens = await context.ParentPinResetTokens
            .Where(t => t.ProfileId == profile.Id && t.UsedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        if (oldTokens.Count > 0)
        {
            context.ParentPinResetTokens.RemoveRange(oldTokens);
        }

        string otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        DateTime otpExpiresAt = VietnamTime.UtcNow().AddMinutes(OtpExpiryMinutes);

        var resetToken = new ParentPinResetToken
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            Token = Guid.NewGuid().ToString("N"),
            OtpCodeHash = passwordHasher.Hash(otp),
            OtpExpiresAt = otpExpiresAt,
            ExpiresAt = VietnamTime.UtcNow().AddMinutes(ResetTokenExpiryMinutes),
            OtpAttemptCount = 0
        };

        context.ParentPinResetTokens.Add(resetToken);
        await context.SaveChangesAsync(cancellationToken);

        string verifyLink =
            $"{clientUrlProvider.GetFrontendUrl()}/auth/reset-pin/verify-zalo-otp?challengeId={resetToken.Id}";
        string message =
            $"Ma OTP dat lai ma PIN KidzGo cua ban la {otp}. Ma co hieu luc trong {OtpExpiryMinutes} phut.";

        bool sent = await zaloService.SendMessageWithDeeplinkAsync(
            profile.ZaloId,
            message,
            verifyLink,
            cancellationToken);

        if (!sent)
        {
            context.ParentPinResetTokens.Remove(resetToken);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<RequestParentPinResetZaloOtpResponse>(ProfileErrors.ZaloOtpSendFailed);
        }

        return Result.Success(new RequestParentPinResetZaloOtpResponse
        {
            ChallengeId = resetToken.Id,
            OtpExpiresAt = otpExpiresAt
        });
    }
}
