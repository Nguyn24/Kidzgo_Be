namespace Kidzgo.Application.EmailTemplates.GetEmailTemplate;

public class GetEmailTemplateResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public string Header { get; set; } = null!;
    public string MainContent { get; set; } = null!;
}