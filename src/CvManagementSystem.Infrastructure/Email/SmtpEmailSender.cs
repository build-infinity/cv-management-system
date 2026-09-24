using System.Net;
using System.Net.Mail;
using CvManagementSystem.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace CvManagementSystem.Infrastructure.Email;
public class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(recipient);
        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            UseDefaultCredentials = false
        };
        if (!string.IsNullOrWhiteSpace(settings.Username))
            client.Credentials = new NetworkCredential(settings.Username, settings.Password);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        await client.SendMailAsync(message, timeout.Token);
    }
}
