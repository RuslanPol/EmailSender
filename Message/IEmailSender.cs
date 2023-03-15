namespace Massege;

public interface IEmailSender
{
    Task Send(string toEmail, string subject, string htmlBody);
}