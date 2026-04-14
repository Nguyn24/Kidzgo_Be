using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Kidzgo.Application.Abstraction.Authentication;
using Microsoft.Extensions.Options;
using What2Gift.Infrastructure.Shared;

namespace Kidzgo.Infrastructure.Services;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;

    public MailService(
        IOptions<MailSettings> mailSettingsOptions)
    {
        _mailSettings = mailSettingsOptions.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return;
        }

        string fromEmail = _mailSettings.SmtpUsername;
        ValidateSettings(fromEmail);

        var mail = new MimeMessage();
        mail.From.Add(MailboxAddress.Parse(fromEmail));
        mail.To.Add(MailboxAddress.Parse(toEmail));
        mail.Subject = subject;
        mail.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        var secureSocketOptions = ResolveSecureSocketOptions();
        await smtp.ConnectAsync(
            _mailSettings.SmtpServer,
            _mailSettings.SmtpPort,
            secureSocketOptions,
            cancellationToken);
        await smtp.AuthenticateAsync(fromEmail, _mailSettings.SmtpPassword, cancellationToken);
        await smtp.SendAsync(mail, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }

    private void ValidateSettings(string fromEmail)
    {
        if (string.IsNullOrWhiteSpace(_mailSettings.SmtpServer))
        {
            throw new InvalidOperationException("MailSettings:SmtpServer is required.");
        }

        if (_mailSettings.SmtpPort <= 0)
        {
            throw new InvalidOperationException("MailSettings:SmtpPort must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("MailSettings:SmtpUsername is required.");
        }

        if (string.IsNullOrWhiteSpace(_mailSettings.SmtpPassword))
        {
            throw new InvalidOperationException("MailSettings:SmtpPassword is required.");
        }
    }

    private SecureSocketOptions ResolveSecureSocketOptions()
    {
        if (Enum.TryParse<SecureSocketOptions>(_mailSettings.SecureSocketOptions, true, out var configuredOptions))
        {
            return configuredOptions;
        }

        return _mailSettings.SmtpPort switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            _ => SecureSocketOptions.Auto
        };
    }
} 
