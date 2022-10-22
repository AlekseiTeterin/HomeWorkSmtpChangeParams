using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace WebApplicationMail;

public class SmtpEmailSender : IEmailSender, IAsyncDisposable
{
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly SmtpClient _client;
    private readonly SmtpConfig _smtpConfig;

    public SmtpEmailSender(IOptionsSnapshot<SmtpConfig> options, ILogger<SmtpEmailSender> logger)
    {
        _logger = logger;
        _client = new SmtpClient();
        _smtpConfig = options.Value;
    }

    public async Task SendAsync(
        string fromName,
        string to,
        string subject,
        string bodyHtml)
    {
        _logger.LogInformation("Попытка отправить письмо: {toEmail}", to);
        var message = new MimeMessage();
        
        message.From.Add(new MailboxAddress(fromName, _smtpConfig.UserName));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = bodyHtml };
        if (!_client.IsConnected)
        {
           await _client.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port, _smtpConfig.UseSsl);
        }
        if (!_client.IsAuthenticated)
        {
           await _client.AuthenticateAsync(_smtpConfig.UserName, _smtpConfig.Password);
        }
        await _client.SendAsync(message);
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisconnectAsync(true);
        _client.Dispose();
    }
}


