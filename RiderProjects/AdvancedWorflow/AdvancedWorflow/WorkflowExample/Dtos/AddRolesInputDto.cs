using System.Collections.Generic;
using AuthProject.Identities;

namespace AuthProject.WorkflowTest
{
    public class AddRolesInputDto
    {
        public AddRolesInputDto(CustomIdentityUser user, IEnumerable<string> roles)
        {
            User = user;
            Roles = roles;
        }

        public CustomIdentityUser User { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}