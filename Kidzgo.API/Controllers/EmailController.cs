using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.EmailTemplates.GetEmailTemplate;
using Kidzgo.Application.EmailTemplates.UpdateEmailTemplate;
using Kidzgo.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/")]
[ApiController]
public class EmailController
{
    private readonly ISender _mediator;

    public EmailController(ISender mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("email/get")]
    public async Task<IResult> GetEmailTemplate(CancellationToken cancellation)
    {
        var result = await _mediator.Send(new GetEmailTemplateQuery(), cancellation);
        return result.MatchOk();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("email/update-email")]
    public async Task<IResult> UpdateProject([FromBody] UpdateEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEmailTemplateCommand
        {
            Id = request.Id,
            Content = request.Content,
            Header = request.Header,
            MainContent = request.MainContent
        };
        var result = await _mediator.Send(command, cancellationToken);

        return result.MatchOk();
    }
}