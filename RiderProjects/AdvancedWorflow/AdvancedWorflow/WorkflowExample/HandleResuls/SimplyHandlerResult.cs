namespace AuthProject.EmailSender
{
    public class SimplyHandlerResult
    {
        public SimplyHandlerResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        public bool Succeeded { get; set; }
    }
}