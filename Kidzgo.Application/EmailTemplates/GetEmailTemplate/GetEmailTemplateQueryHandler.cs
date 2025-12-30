using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.EmailTemplates.GetEmailTemplate;

public class GetEmailTemplateQueryHandler(IDbContext context) : IQueryHandler<GetEmailTemplateQuery, List<GetEmailTemplateResponse>>
{
    public async Task<Result<List<GetEmailTemplateResponse>>> Handle(GetEmailTemplateQuery request, CancellationToken cancellationToken)
    {
        List<EmailTemplate> templates = await context.EmailTemplates
            .Where(t => !t.IsDeleted)
            .ToListAsync(cancellationToken);

        var response = templates
            .Select(t => new GetEmailTemplateResponse
            {
                Id = t.Id,
                Header = t.Subject,
                Content = t.Body ?? string.Empty,
                MainContent = string.Empty
            })
            .ToList();

        return response;
    }
}