using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using AuthProject.Context;
using AuthProject.ValueTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic.ApplicationServices;

#nullable enable
namespace AuthProject.Identities
{
    public class CustomIdentityRole : IdentityRole
    {
    }

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
            PasswordHashEntity = new HashedPassword(PasswordHash);
        }

        [NotMapped]
        public string PasswordHashEntity { get; set; }

        public RefreshToken? RefreshToken { get; set; } = new RefreshToken("123");
    }

    public class CustomIdentityUserDto
    {
        public CustomIdentityUserDto(string userName, string userEmail, string password)
        {
            UserName = userName;
            UserEmail = userEmail;
            Password = password;
        }

        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Password { get; set; }
    }

    public class ForgotPasswordDto
    {
        public ForgotPasswordDto(string email)
        {
            Email = email;
        }
        public string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        public ResetPasswordDto(IEnumerable<IdentityError> identityErrors)
        {
            IdentityErrors = identityErrors;
        }

        public ResetPasswordDto()
        {
            IsSuccess = true;
        }

        public IEnumerable<IdentityError>? IdentityErrors { get; set; }
        public bool IsSuccess { get; set; }
    }


    public class CustomIdentityUserWithRolesDto : CustomIdentityUserDto
    {
        public IEnumerable<CustomIdentityRole>? Roles { get; set; }

        public CustomIdentityUserWithRolesDto(string userName, string userEmail, string password) : base(userName,
            userEmail, password)
        {
        }
    }
}