using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Login;

public sealed class LoginCommandHandler(
    IDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider
) : ICommandHandler<LoginCommand, TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users.Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(
                u => u.Email.Trim().ToLower() == command.Email.Trim().ToLower(),
                cancellationToken);

        if (user is null)
            return Result.Failure<TokenResponse>(UserErrors.NotFoundByEmail);

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);
        if (!verified)
            return Result.Failure<TokenResponse>(UserErrors.WrongPassword);

        if (user.IsActive == false || user.IsDeleted == true)
            return Result.Failure<TokenResponse>(UserErrors.InActive);

        // if (!user.IsVerified)
        //     return Result.Failure<TokenResponse>(UserErrors.IsNotVerified);

        string accessToken = tokenProvider.Create(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(1)
        };
        
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
