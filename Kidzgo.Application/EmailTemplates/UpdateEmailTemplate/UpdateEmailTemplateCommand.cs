using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.EmailTemplates.UpdateEmailTemplate;

public class UpdateEmailTemplateCommand : ICommand<UpdateEmailTemplateResponse>
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string Header { get; set; } = null!;
    public string MainContent { get; set; } = null!;
}