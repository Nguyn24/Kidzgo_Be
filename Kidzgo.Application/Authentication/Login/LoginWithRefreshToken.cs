using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Login;

public class LoginWithRefreshToken (IDbContext context, 
    ITokenProvider tokenProvider) 
    : ICommandHandler<LoginWithRefreshToken.LoginByRefreshTokenCommand , TokenResponse>
{
    public async Task<Result<TokenResponse>> Handle(LoginByRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        RefreshToken? refreshToken = await context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
        {
            return Result.Failure<TokenResponse>(UserErrors.InvalidRefreshToken);

        }

        if (refreshToken.User == null || refreshToken.User.IsActive == false || refreshToken.User.IsDeleted)
        {
            return Result.Failure<TokenResponse>(UserErrors.InActive);
        }

        string accessToken = tokenProvider.Create(refreshToken.User);
        refreshToken.Token = tokenProvider.GenerateRefreshToken();
        refreshToken.Expires = DateTime.UtcNow.AddDays(1);
        
        await context.SaveChangesAsync(cancellationToken);
        return new TokenResponse()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }

    public sealed class LoginByRefreshTokenCommand : ICommand<TokenResponse>
    {
        public string RefreshToken { get; set; }
    }
}
