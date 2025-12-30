using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Authentication.ChangePassword;
using Kidzgo.Application.Authentication.ForgetPassword;
using Kidzgo.Application.Authentication.Login;
using Kidzgo.Application.Authentication.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = Kidzgo.API.Requests.LoginRequest;

namespace Kidzgo.API.Controllers;


[Route("api/auth/")]
[ApiController]
public class AuthenticateController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthenticateController(ISender mediator)
    {
        _mediator = mediator;
    }
    
    
    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        LoginCommand command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };
        
        var  result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
    
    [HttpPost("refresh-token")]
    public async Task<IResult> LoginWithRefreshToken([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        LoginWithRefreshToken.LoginByRefreshTokenCommand command = new LoginWithRefreshToken.LoginByRefreshTokenCommand
        {
            RefreshToken = refreshToken
        };
        
        var  result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
    
    [Authorize]
    [HttpPut("change-password")]
    public async Task<IResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command, cancellationToken);

        return result.MatchOk();
    }
    
    [HttpPost("forget-password")]
    [AllowAnonymous]
    public async Task<IResult> ForgetPassword([FromBody] ForgetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ForgetPasswordCommand
        {
            Email = request.Email
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
    
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand
        {
            Token = request.Token,
            NewPassword = request.NewPassword
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
} 