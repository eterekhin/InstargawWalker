using System;
using System.Text;
using AuthProject.AuthService;
using AuthProject.Context;
using AuthProject.EmailSender;
using AuthProject.Identities;
using AuthProject.Services;
using Force;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

#nullable enable
namespace AuthProject
{
#nullable enable

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static SymmetricSecurityKey signingKey;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AuthDbContext>(x =>
                x.UseSqlServer("Server=.;Database=AuthProject;Trusted_Connection=True;MultipleActiveResultSets=true"));

            services.AddIdentity<CustomIdentityUser, CustomIdentityRole>(
                    x =>
                    {
                        x.SignIn.RequireConfirmedEmail = true;
                        x.Password.RequireDigit = false;
                        x.Password.RequireLowercase = false;
                        x.Password.RequireUppercase = false;
                        x.Password.RequiredLength = 5;
                    })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();

            // @formatter:off 
            services.AddScoped<JwtAuthorizeService>();
            services.AddScoped<SmtpClient>();
            services.AddScoped<IAsyncHandler<TokenEmailDto, SimplyHandlerResult>, UserSignupHandler>();
            services.AddScoped<IAsyncHandler<CustomIdentityUserWithRolesDto, ConfirmationCodeDto>, EmailConfirmationHandler>();
            services.AddScoped<IAsyncHandler<EmailSendDto, SimplyHandlerResult>, EmailSenderService>();
            services.AddScoped<EmailSenderService>();
            services.AddScoped<IAsyncHandler<TokenEmailPasswordDto, ResetPasswordDto>,SecondaryRecoveryForgotPassword>();
            services.AddScoped<IAsyncHandler<ForgotPasswordDto, SimplyHandlerResult>,PrimaryRecoveryForgotPasswordHandler>();
            // @formatter:on

            services.Configure<EmailSenderConfiguration>(Configuration.GetSection("EmailSenderConfiguration"));

            var jwtAppSettingOptions = Configuration.GetSection("JwtTokenOptions");
            services.Configure<JwtTokenOptions>(Configuration.GetSection("JwtTokenOptions"));
            signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions["SecretKey"]));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "issuer",
                ValidateAudience = true,
                ValidAudience = "audience",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.ClaimsIssuer = tokenValidationParameters.ValidIssuer;
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.SaveToken = true;
                });

//            services.AddMvc(config =>
//            {
//                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
//                config.Filters.Add(new AuthorizeFilter(policy));
//            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

            services.AddMvc();
            services.AddControllers()
                .AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

//            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}