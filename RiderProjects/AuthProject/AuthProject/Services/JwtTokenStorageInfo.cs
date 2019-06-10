using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AuthProject.AuthService;
using AuthProject.Context;
using AuthProject.EmailSender;
using AuthProject.Identities;
using AuthProject.ValueTypes;
using Force;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MimeKit.Text;

#nullable enable
namespace AuthProject.Services
{
    public class AccessRefreshDto
    {
        public AccessRefreshDto(AccessToken accessToken, RefreshToken refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public AccessToken AccessToken { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }

    public class TokenEmailDto
    {
        public string? Email { get; set; }
        public string? Code { get; set; }
    }

    public class TokenEmailPasswordDto : TokenEmailDto
    {
        public string? NewPassword { get; set; }
    }

    public class ConfirmationCodeDto
    {
        public ConfirmationCodeDto(string code)
        {
            Code = code;
        }

        public ConfirmationCodeDto(IEnumerable<IdentityError> identityErrors)
        {
            IdentityErrors = identityErrors;
        }

        public string? Code { get; set; }
        public IEnumerable<IdentityError>? IdentityErrors { get; set; }
    }


    public class UserSignupHandler : IAsyncHandler<TokenEmailDto, SimplyHandlerResult>
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager<CustomIdentityUser> _userManager;

        public UserSignupHandler(
            AuthDbContext authDbContext,
            UserManager<CustomIdentityUser> userManager)
        {
            _authDbContext = authDbContext;
            _userManager = userManager;
        }

        public async Task<SimplyHandlerResult> Handle(TokenEmailDto input, CancellationToken cancellationToken)
        {
            var user = await _authDbContext.Users.FirstOrDefaultAsync(x => x.Email == input.Email, cancellationToken);
            var s = await _userManager.ConfirmEmailAsync(user, input.Code);
            return new SimplyHandlerResult(true);
        }
    }

    public class EmailConfirmationHandler : IAsyncHandler<CustomIdentityUserWithRolesDto, ConfirmationCodeDto>
    {
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly RoleManager<CustomIdentityRole> _roleManager;
        private readonly IAsyncHandler<EmailSendDto, SimplyHandlerResult> _emailSender;

        public EmailConfirmationHandler(
            UserManager<CustomIdentityUser> userManager,
            RoleManager<CustomIdentityRole> roleManager,
            EmailSenderService emailSenderService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSenderService;
        }

        public async Task<ConfirmationCodeDto> Handle(CustomIdentityUserWithRolesDto input,
            CancellationToken cancellationToken)
        {
            CustomIdentityUser user = new CustomIdentityUser();
            try
            {
                user = new CustomIdentityUser(input.UserEmail, input.UserName, input.Password);
                var createUserResult = await _userManager.CreateAsync(user);
                if (!createUserResult.Succeeded)
                {
                    return new ConfirmationCodeDto(createUserResult.Errors);
                }

                if (input.Roles != null)
                {
                    var addRolesResult = await _userManager.AddToRolesAsync(user, input.Roles.Select(x => x.Name));
                    if (!addRolesResult.Succeeded)
                    {
                        return new ConfirmationCodeDto(addRolesResult.Errors);
                    }
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var email = (Email) input.UserEmail;
                var text = GenerateMessageText(token, email);
                var subject = input.UserName;
                var emailSendDto = new EmailSendDto(email, text, subject);

                var sendEmailResult = await _emailSender.Handle(emailSendDto, cancellationToken);

                if (!sendEmailResult.Succeeded)
                {
                }

                return new ConfirmationCodeDto(text);
            }
            catch (Exception e)
            {
                await _userManager.DeleteAsync(user);
                Console.WriteLine(e);
                throw;
            }
        }

        private string GenerateMessageText(string code, string email)
        {
            var a =
                $"<a href='http://localhost:5000/api/auth/secondaryAuthentication?code={HttpUtility.UrlEncode(code)}&email={email}'>Ссылка</a>";
            var message = $"Для успешной аутентификации по ссылке епт {a}";
            return message;
        }
    }
}


//    public class JwtTokenHandler
//    {
//        private readonly AuthDbContext _context;
//        private readonly JwtAuthorizeService _jwtAuthorizeService;
//
//        public JwtTokenHandler(
//            AuthDbContext context,
//            JwtAuthorizeService jwtAuthorizeService)
//        {
//            _context = context;
//            _jwtAuthorizeService = jwtAuthorizeService;
//        }
//
//        public AccessToken CreateAndGetJwtToken(CustomIdentityUserDto identityUser)
//        {
//            return _jwtAuthorizeService.GetJwtToken(identityUser);
//        }
//
//        public async Task<RefreshToken> GenerateAndSaveRefreshToken(CustomIdentityUserDto userDto)
//        {
//            var refreshToken = (RefreshToken) Guid.NewGuid().ToString();
//            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == userDto.UserEmail);
//            user.RefreshToken = refreshToken;
//            _context.SaveChanges();
//            return refreshToken;
//        }
//
//        public async Task DisabledAndSaveAccessToken(AccessToken token, CustomIdentityUserDto dto)
//        {
//            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.UserEmail);
//            _context.DisabledAccessTokens.Add(new DisabledAccessTokenValueObject(user, token));
//            _context.SaveChanges();
//        }
//    }