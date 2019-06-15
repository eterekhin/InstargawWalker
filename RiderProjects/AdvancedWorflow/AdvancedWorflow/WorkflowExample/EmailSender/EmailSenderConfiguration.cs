namespace AuthProject.EmailSender
{
    #nullable enable
    public class EmailSenderConfiguration
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }
        public string? TitleName { get; set; }
        public bool UseSSL { get; set; }
        public int Port { get; set; }
    }
}