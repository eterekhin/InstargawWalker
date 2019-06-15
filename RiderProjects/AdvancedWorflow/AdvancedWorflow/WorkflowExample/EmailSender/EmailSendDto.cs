
namespace AuthProject.EmailSender
{
    public class EmailSendDto
    {
        public EmailSendDto()
        {
            
        }
        public EmailSendDto(string sendeeEmail, string text, string subject)
        {
            SendeeEmail = sendeeEmail;
            Text = text;
            Subject = subject;
        }

        public string Subject { get; set; }
        public string SendeeEmail { get; set; }
        public string Text { get; set; }
    }
}