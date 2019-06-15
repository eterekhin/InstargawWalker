namespace AuthProject.WorkflowTest
{
    public class ConfirmInfoDto
    {
        public ConfirmInfoDto()
        {
            
        }
        public ConfirmInfoDto(string email, string message)
        {
            Email = email;
            Message = message;
        }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}