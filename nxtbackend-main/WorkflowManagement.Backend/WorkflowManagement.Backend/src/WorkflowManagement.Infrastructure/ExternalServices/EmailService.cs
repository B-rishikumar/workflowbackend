// EmailService.cs
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace WorkflowManagement.Infrastructure.ExternalServices;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(new[] { to }, subject, body, isHtml, cancellationToken);
    }

    public async Task SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"];
            var fromName = smtpSettings["FromName"];
            var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogWarning("SMTP settings not configured. Email not sent.");
                return;
            }

            using var client = new SmtpClient(host, port);
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                client.Credentials = new NetworkCredential(username, password);
            }
            
            client.EnableSsl = enableSsl;

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            foreach (var email in to)
            {
                message.To.Add(email);
            }

            await client.SendMailAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", to));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", to));
            throw;
        }
    }

    public async Task SendTemplateEmailAsync(string to, string templateName, object model, CancellationToken cancellationToken = default)
    {
        // This would integrate with a templating engine like Razor or Handlebars
        // For now, just send a basic email
        await SendEmailAsync(to, $"Notification from {templateName}", model.ToString() ?? "", cancellationToken: cancellationToken);
    }
}