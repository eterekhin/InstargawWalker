using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AuthProject.Context;
using AuthProject.EmailSender;
using AuthProject.Identities;
using AuthProject.Services;
using AuthProject.ValueTypes;
using Force;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.ApplicationServices;

namespace AuthProject.AuthService
{
    public class Login : ValueType<Login>
    {
        public Login(string name) : base(name)
        {
            // validation
        }
    }

    public class Password : ValueType<Password>
    {
        public Password(string name) : base(name)
        {
            // validation
        }
    }

    public class UserAuthenticationInfo
    {
        public Login Login { get; set; }
        public Password Password { get; set; }
    }

    public class JwtAuthorizeService
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly AuthDbContext _authDbContext;
        private readonly GetClaims _getClaims;
        private readonly JwtTokenOptions jwtTokenOptions;

        public JwtAuthorizeService(
            IOptions<JwtTokenOptions> JwtTokenOptions,
            UserManager<CustomIdentityUser> userManager,
            AuthDbContext authDbContext)
        {
            jwtTokenOptions = JwtTokenOptions.Value;
            _userManager = userManager;
            _authDbContext = authDbContext;
            _getClaims = new GetClaims();
        }

        public async Task<AccessRefreshDto> GetJwtToken(CustomIdentityUserDto userAuthenticationInfo,
            CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(userAuthenticationInfo.UserEmail);
            var checkPasword = await _userManager.CheckPasswordAsync(user, userAuthenticationInfo.Password);
            if (!checkPasword)
                throw new ArgumentException();

            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var claims = _getClaims.MakeClaimsCollection(userAuthenticationInfo);


            var accessToken = (AccessToken) jwtTokenHandler.WriteToken(new JwtSecurityToken(
                claims: claims,
                issuer: jwtTokenOptions.Issuer,
                audience: jwtTokenOptions.Audience,
                expires: DateTime.Now.Add(TimeSpan.FromSeconds(30)),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenOptions.SecretKey)),
                    SecurityAlgorithms.HmacSha256)
            ));

            var refreshToken = (RefreshToken) Guid.NewGuid().ToString();


            user.RefreshToken = refreshToken;
            await _authDbContext.SaveChangesAsync(ct);

            return new AccessRefreshDto(accessToken, refreshToken);
        }
    }

    public class PrimaryRecoveryForgotPasswordHandler : IAsyncHandler<ForgotPasswordDto, SimplyHandlerResult>
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly EmailSenderService _emailSenderService;

        public PrimaryRecoveryForgotPasswordHandler(
            UserManager<CustomIdentityUser> userManager,
            EmailSenderService emailSenderService)
        {
            _userManager = userManager;
            _emailSenderService = emailSenderService;
        }

        public async Task<SimplyHandlerResult> Handle(ForgotPasswordDto input, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var sendeeEmail = (Email) input.Email;
            var subject = user.UserName;
            var text = $"Код для восстановления пароля {code}";
            var emailSenderDto = new EmailSendDto(sendeeEmail, text, subject);

            await _emailSenderService.Handle(emailSenderDto, cancellationToken);
            return new SimplyHandlerResult(true);
        }
    }

    public class SecondaryRecoveryForgotPassword : IAsyncHandler<TokenEmailPasswordDto, ResetPasswordDto>
    {
        private readonly UserManager<CustomIdentityUser> _userManager;

        public SecondaryRecoveryForgotPassword(UserManager<CustomIdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ResetPasswordDto> Handle(TokenEmailPasswordDto input, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            var verifyToken = await _userManager.ResetPasswordAsync(user, input.Code, input.NewPassword);
            return !verifyToken.Succeeded ? new ResetPasswordDto(verifyToken.Errors) : new ResetPasswordDto();
        }
    }


    internal class GetClaims
    {
        public IEnumerable<Claim> MakeClaimsCollection(CustomIdentityUserDto user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.UserEmail),
                new Claim(ClaimTypes.NameIdentifier, user.UserName)
            };
            return claims;
        }
    }
}