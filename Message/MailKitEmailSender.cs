using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Massege;

public class MailKitEmailSender : IEmailSender, IAsyncDisposable,IDisposable
{
    private SmtpClient _smtpClient = new();
    private SmtpConfig _smtpConfig;
    private readonly ILogger<MailKitEmailSender> _logger;


    public MailKitEmailSender(IOptionsSnapshot<SmtpConfig> options, ILogger<MailKitEmailSender> logger)
    {
        _smtpConfig = options.Value;
        _logger = logger;

    }

    public async Task Send(string toEmail, string subject, string htmlBody)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_smtpConfig.UserName));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        //_logger.LogInformation("Сервер удачно запущен.");
        await EnsureConnectAndAuthenticate();
        await _smtpClient.SendAsync(email);

    }

    private async Task EnsureConnectAndAuthenticate()
    {
        if (!_smtpClient.IsConnected)
        {

            await _smtpClient.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port);
        }

        if (!_smtpClient.IsAuthenticated)
        {
            await _smtpClient.AuthenticateAsync(_smtpConfig.UserName, _smtpConfig.Password);
        }
    }


    public void  Dispose()
    { 
        _smtpClient.Disconnect(true);
        _smtpClient.Dispose();
    }
    
       public async  ValueTask  DisposeAsync()
    {
        if (_smtpClient.IsConnected)
        {
         await _smtpClient.DisconnectAsync(true);
        }

        _smtpClient.Dispose();
        

    }


}