using System.Net;
using System.Net.Mail;
using Kidzgo.Application.Abstraction.Authentication;
using Microsoft.Extensions.Options;
using What2Gift.Infrastructure.Shared;

namespace Kidzgo.Infrastructure.Services;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;
    private readonly ClientSettings _clientSettings;

    public MailService(
        IOptions<MailSettings> mailSettingsOptions,
        IOptions<ClientSettings> clientSettingsOptions)
    {
        _clientSettings = clientSettingsOptions.Value;
        _mailSettings = mailSettingsOptions.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return;
        }

        string fromEmail = _mailSettings.SmtpUsername;

        using var mail = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mail.To.Add(toEmail);

        using var smtp = new SmtpClient(_mailSettings.SmtpServer, _mailSettings.SmtpPort)
        {
            Credentials = new NetworkCredential(fromEmail, _mailSettings.SmtpPassword),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        await smtp.SendMailAsync(mail, cancellationToken);
    }
} 