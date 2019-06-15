using AuthProject.Identities;
using Microsoft.VisualBasic.ApplicationServices;

namespace AuthProject.WorkflowTest
{
    public class ConfirmDto
    {
        public ConfirmDto(CustomIdentityUser user)
        {
            User = user;
        }

        public CustomIdentityUser User { get; set; }
    }
}