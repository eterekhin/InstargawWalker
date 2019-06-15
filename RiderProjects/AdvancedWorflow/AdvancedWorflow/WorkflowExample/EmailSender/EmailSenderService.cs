using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Force;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

#nullable enable
namespace AuthProject.EmailSender
{
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