// using BloodDonation.Application.Abstraction.Authentication;
// using MailKit.Net.Smtp;
// using MailKit.Security;
// using Microsoft.Extensions.Configuration;
// using MimeKit;
//
// namespace BloodDonation.Infrastructure.Authentication;
//
// public class EmailSender(IConfiguration configuration) : IEmailSender
// {
//     public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
//     {
//         var email = new MimeMessage();
//         email.From.Add(MailboxAddress.Parse(configuration["EmailSettings:From"]));
//         email.To.Add(MailboxAddress.Parse(toEmail));
//         email.Subject = subject;
//
//         var builder = new BodyBuilder { HtmlBody = htmlMessage };
//         email.Body = builder.ToMessageBody();
//
//         using var smtp = new SmtpClient();
//
//         var host = configuration["EmailSettings:SmtpHost"];
//         var port = int.Parse(configuration["EmailSettings:SmtpPort"]);
//         SecureSocketOptions socketOptions;
//
//         if (port == 465)
//             socketOptions = SecureSocketOptions.SslOnConnect;
//         else if (port == 587)
//             socketOptions = SecureSocketOptions.StartTls;
//         else
//             socketOptions = SecureSocketOptions.Auto; // hoặc tùy chỉnh theo server
//
//         await smtp.ConnectAsync(host, port, socketOptions);
//         await smtp.AuthenticateAsync(configuration["EmailSettings:SmtpUser"], configuration["EmailSettings:SmtpPass"]);
//         await smtp.SendAsync(email);
//         await smtp.DisconnectAsync(true);
//
//     }
// }