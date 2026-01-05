using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Errors;

namespace Kidzgo.Application.EmailTemplates.UpdateEmailTemplate;

public class UpdateEmailTemplateCommandHandler(IDbContext context) : ICommandHandler<UpdateEmailTemplateCommand, UpdateEmailTemplateResponse>
{
    public async Task<Result<UpdateEmailTemplateResponse>> Handle(UpdateEmailTemplateCommand command, CancellationToken cancellationToken)
    {
        EmailTemplate? template = await context.EmailTemplates
            .SingleOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (template is null)
        {
            return Result.Failure<UpdateEmailTemplateResponse>(EmailTemplateErrors.NotFound);
        }

        template.Subject = command.Header;
        template.Body = command.Content;
        template.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        var response = new UpdateEmailTemplateResponse
        {
            Id = template.Id,
            Header = template.Subject,
            Content = template.Body ?? string.Empty,
            MainContent = string.Empty
        };

        return response;
    }
}