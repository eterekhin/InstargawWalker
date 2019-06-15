using AuthProject.Identities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualBasic.ApplicationServices;

namespace AuthProject.Context
{
    public class AuthDbContext : IdentityDbContext<CustomIdentityUser, CustomIdentityRole, string>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }
    }
}