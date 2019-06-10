using AuthProject.Identities;
using AuthProject.ValueTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.VisualBasic.ApplicationServices;

#nullable enable
namespace AuthProject.Context
{
#nullable disable
    public class DisableAccessTokensEntityConfiguration : IEntityTypeConfiguration<DisabledAccessTokenValueObject>
    {
        public void Configure(EntityTypeBuilder<DisabledAccessTokenValueObject> builder)
        {
            builder.OwnsOne(x => x.AccessToken).Property(x => x.Name).HasColumnName("DisableAccessToken");
        }
    }


    public class CustomIdentityUserEntityConfiguration : IEntityTypeConfiguration<CustomIdentityUser>
    {
        public void Configure(EntityTypeBuilder<CustomIdentityUser> builder)
        {
            builder.OwnsOne(x => x.RefreshToken).Property(x => x.Name);
        }
    }
#nullable enable

    public class AuthDbContext : IdentityDbContext<CustomIdentityUser, CustomIdentityRole, string>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<DisabledAccessTokenValueObject>? DisabledAccessTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new DisableAccessTokensEntityConfiguration());
            builder.ApplyConfiguration(new CustomIdentityUserEntityConfiguration());
            base.OnModelCreating(builder);
        }
    }

    public class DisabledAccessTokenValueObject
    {
        public int Id { get; set; }
        public CustomIdentityUser? CustomIdentityUser;

        protected DisabledAccessTokenValueObject()
        {
        }

        public DisabledAccessTokenValueObject(CustomIdentityUser? customIdentityUser, AccessToken? accessToken)
        {
            CustomIdentityUser = customIdentityUser;
            AccessToken = accessToken;
        }

        public AccessToken? AccessToken { get; set; }
    }
}