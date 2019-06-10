using System.Threading;
using System.Threading.Tasks;
using AuthProject.ValueTypes;
using Force;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

#nullable enable
namespace AuthProject.EmailSender
{
    public class EmailSendDto
    {
        public EmailSendDto(Email sendeeEmail, string text, string subject)
        {
            SendeeEmail = sendeeEmail;
            Text = text;
            Subject = subject;
        }

        public string Subject { get; set; }
        public Email SendeeEmail { get; set; }
        public string Text { get; set; }
    }

    public class EmailSenderConfiguration
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }
        public string? TitleName { get; set; }
        public bool UseSSL { get; set; }
        public int Port { get; set; }
    }

    public class EmailSenderService : IAsyncHandler<EmailSendDto, SimplyHandlerResult>
    {
        private readonly SmtpClient _smtpClient;
        private readonly EmailSenderConfiguration _emailSenderConfiguration;

        public EmailSenderService(SmtpClient smtpClient, IOptions<EmailSenderConfiguration> emailSenderConfiguration)
        {
            _smtpClient = smtpClient;
            _emailSenderConfiguration = emailSenderConfiguration.Value;
        }

        public async Task<SimplyHandlerResult> Handle(EmailSendDto input, CancellationToken cancellationToken)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_emailSenderConfiguration.TitleName,
                _emailSenderConfiguration.Email));
            emailMessage.To.Add(new MailboxAddress(_emailSenderConfiguration.TitleName, input.SendeeEmail));
            emailMessage.Subject = input.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = input.Text
            };

            await _smtpClient.ConnectAsync(
                _emailSenderConfiguration.Host, _emailSenderConfiguration.Port,
                _emailSenderConfiguration.UseSSL, cancellationToken);

            await _smtpClient.AuthenticateAsync(_emailSenderConfiguration.Email, _emailSenderConfiguration.Password,
                cancellationToken);

            await _smtpClient.SendAsync(emailMessage, cancellationToken);
            await _smtpClient.DisconnectAsync(true, cancellationToken);
            return new SimplyHandlerResult(true);
        }
    }
}