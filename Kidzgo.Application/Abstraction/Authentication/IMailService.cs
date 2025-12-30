
namespace Kidzgo.Application.Abstraction.Authentication;

public interface IMailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
} 