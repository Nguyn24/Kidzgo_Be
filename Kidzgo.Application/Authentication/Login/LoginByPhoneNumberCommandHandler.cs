using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Login;

public sealed class LoginByPhoneNumberCommandHandler(
    IDbContext context,
    ITokenProvider tokenProvider
) : ICommandHandler<LoginByPhoneNumberCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginByPhoneNumberCommand command, CancellationToken cancellationToken)
    {
        var phoneLookupCandidates = PhoneNumberNormalizer.GetLookupCandidates(command.PhoneNumber);

        User? user = await context.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(
                u => u.PhoneNumber != null &&
                     phoneLookupCandidates.Contains(
                         u.PhoneNumber
                             .Replace(" ", "")
                             .Replace("-", "")
                             .Replace(".", "")
                             .Replace("(", "")
                             .Replace(")", "")
                             .Replace("+", "")),
                cancellationToken);

        if (user is null)
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByPhoneNumber);

        if (user.IsActive == false || user.IsDeleted == true)
            return Result.Failure<TokenResponse>(UserErrors.InActive);

        string accessToken = tokenProvider.Create(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(1)
        };

        var now = DateTime.UtcNow;
        user.LastLoginAt = now;
        user.LastSeenAt = now;
        user.UpdatedAt = now;

        context.RefreshTokens.RemoveRange(user.RefreshTokens);
        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync(cancellationToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            Role = user.Role,
            UserId = user.Id,
        };
    }
}
