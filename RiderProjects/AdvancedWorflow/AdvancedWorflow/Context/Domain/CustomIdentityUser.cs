using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AuthProject.Identities
{
    public sealed class CustomIdentityUser : IdentityUser
    {
        public CustomIdentityUser()
        {
            PasswordHashEntity = string.Empty;
        }

        public CustomIdentityUser(string email, string username, string passwordHash)
        {
            Email = email;
            UserName = username;
            PasswordHash = passwordHash;
        }

        [NotMapped]
        public string PasswordHashEntity { get; set; }

    }
}