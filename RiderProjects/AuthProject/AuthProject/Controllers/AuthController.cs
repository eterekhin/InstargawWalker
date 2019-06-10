using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthProject.AuthService;
using AuthProject.Context;
using AuthProject.EmailSender;
using AuthProject.Identities;
using AuthProject.Services;
using AuthProject.ValueTypes;
using Force;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthProject.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<CustomIdentityUser> _userManager;
        private readonly RoleManager<CustomIdentityRole> _identityUserRole;
        private readonly SignInManager<CustomIdentityUser> _signInManager;

        public AuthController(
            AuthDbContext context,
            UserManager<CustomIdentityUser> userManager,
            RoleManager<CustomIdentityRole> identityUserRole,
            SignInManager<CustomIdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _identityUserRole = identityUserRole;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<string> PrimaryAuthentication(
            [FromBody] CustomIdentityUserDto userAuthenticationInfo,
            [FromServices] IAsyncHandler<CustomIdentityUserWithRolesDto, ConfirmationCodeDto> primarySignupHandler)
        {
            var result = await primarySignupHandler.Handle(
                new CustomIdentityUserWithRolesDto(userAuthenticationInfo.UserName, userAuthenticationInfo.UserEmail,
                    userAuthenticationInfo.Password), CancellationToken.None);
            return "уже отправлено письмо тебе сукеа";
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task SecondaryAuthentication(
            [FromServices] IAsyncHandler<TokenEmailDto, SimplyHandlerResult> secondaryAuthService,
            [FromQuery] TokenEmailDto authDto)
        {
            await secondaryAuthService.Handle(authDto, CancellationToken.None);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task ForgotPassword(
            [FromBody] ForgotPasswordDto forgotPasswordDto,
            [FromServices] IAsyncHandler<ForgotPasswordDto, SimplyHandlerResult> recoveryForgotPasswordHandler)
        {
            await recoveryForgotPasswordHandler.Handle(forgotPasswordDto, CancellationToken.None);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ResetPasswordDto> RecoveryPassword(
            [FromBody] TokenEmailPasswordDto authDto,
            [FromServices] IAsyncHandler<TokenEmailPasswordDto, ResetPasswordDto> secondaryRecoveryPasswordHandler)
        {
            return await secondaryRecoveryPasswordHandler.Handle(authDto, CancellationToken.None);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<AccessRefreshDto> GetJwtToken(
            [FromBody] CustomIdentityUserDto customIdentityUser,
            [FromServices] JwtAuthorizeService jwtAuthorizeService)
        {
            var result = await jwtAuthorizeService.GetJwtToken(customIdentityUser, CancellationToken.None);
            return result;
        }

        [HttpPost]
        // POST api/values
        [HttpGet]
        public IActionResult LoginTest()
        {
            return Ok();
        }
    }
}