using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Authentication.ChangePassword;
using Kidzgo.Application.Authentication.ChangePin;
using Kidzgo.Application.Authentication.ForgetPassword;
using Kidzgo.Application.Authentication.Login;
using Kidzgo.Application.Authentication.ResetPassword;
using Kidzgo.Application.Authentication.VerifyUserPin;
using Kidzgo.Application.Authentication.Profiles.RequestParentPinReset;
using Kidzgo.Application.Profiles.GetProfiles;
using Kidzgo.Application.Profiles.SelectStudentProfile;
using Kidzgo.Application.Profiles.VerifyParentPin;
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
    
    [Authorize]
    [HttpGet("profiles")]
    public async Task<IResult> GetProfiles(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProfilesQuery(), cancellationToken);
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

    [Authorize]
    [HttpPost("profiles/verify-parent-pin")]
    public async Task<IResult> VerifyParentPin([FromBody] VerifyParentPinRequest request, CancellationToken cancellationToken)
    {
        var command = new VerifyParentPinCommand
        {
            ProfileId = request.ProfileId,
            Pin = request.Pin
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [Authorize]
    [HttpPost("profiles/select-student")]
    public async Task<IResult> SelectStudentProfile([FromBody] SelectStudentProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new SelectStudentProfileCommand
        {
            ProfileId = request.ProfileId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
    
    [Authorize]
    [HttpPut("change-pin")]
    public async Task<IResult> ChangeUserPin([FromBody] ChangeUserPinRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeUserPinCommand
        {
            CurrentPin = request.CurrentPin,
            NewPin = request.NewPin
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Request PIN reset for Parent profile (nếu có email flow)
    [Authorize]
    [HttpPost("profiles/request-pin-reset")]
    public async Task<IResult> RequestParentPinReset([FromBody] RequestParentPinResetRequest request, CancellationToken cancellationToken)
    {
        var command = new RequestParentPinResetCommand
        {
            ProfileId = request.ProfileId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
} 