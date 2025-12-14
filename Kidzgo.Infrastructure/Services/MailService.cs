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

    // public bool SendCreateUserEmail(CreateUserEmailBody emailBody, string userEmail)
    // {
    //     try
    //     {
    //         if (string.IsNullOrWhiteSpace(userEmail))
    //         {
    //             Console.WriteLine("❌ userEmail is null or empty → cannot send email.");
    //             return false;
    //         }
    //
    //         var currentYear = DateTime.Now.Year;
    //         var clientUrl = _clientSettings.ClientUrl;
    //
    //         Console.WriteLine($"📨 Preparing email to: {userEmail}");
    //
    //         string fromEmail = _mailSettings.SmtpUsername;
    //         string toEmail = userEmail;
    //
    //         string subject = emailBody.Header;
    //         string htmlBody = emailBody.Content;
    //
    //         string body = htmlBody
    //             .Replace("{{content}}", emailBody.MainContent ?? "")
    //             .Replace("{{header}}", "VERIFY ACCOUNT")
    //             .Replace("{{username}}", emailBody.User.Username ?? "User") 
    //             .Replace("{{button_link}}", clientUrl + (emailBody.VerifyEndpoint ?? ""))
    //             .Replace("{{button_text}}", emailBody.ButtonName ?? "Verify")
    //             .Replace("{{sender_title}}", "Admin")
    //             .Replace("{{year}}", currentYear.ToString());
    //
    //
    //         MailMessage mail = new MailMessage
    //         {
    //             From = new MailAddress(fromEmail),
    //             Subject = subject,
    //             Body = body,
    //             IsBodyHtml = true
    //         };
    //         mail.To.Add(toEmail);
    //
    //         SmtpClient smtp = new SmtpClient(_mailSettings.SmtpServer, _mailSettings.SmtpPort)
    //         {
    //             Credentials = new NetworkCredential(fromEmail, _mailSettings.SmtpPassword),
    //             EnableSsl = true,
    //             DeliveryMethod = SmtpDeliveryMethod.Network,
    //             UseDefaultCredentials = false
    //         };
    //
    //         smtp.Send(mail);
    //         Console.WriteLine("✅ Email Sent Successfully!");
    //         return true;
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine("❌ Error sending email: " + ex.Message);
    //         return false;
    //     }
    // }
}