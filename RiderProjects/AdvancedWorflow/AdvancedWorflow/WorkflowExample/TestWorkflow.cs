using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AdvancedWorflow.Workflow.Dtos;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using AuthProject.EmailSender;
using AuthProject.Identities;
using Force;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace AuthProject.WorkflowTest
{
    public class TestWorkflow
    {
        [UsedImplicitly]
        public class CreateUserNewHandler : IAsyncHandler<CreateNewUserInputDto, AddClaimsInputDto>,
            ICanAsyncRollBack<CreateNewUserInputDto>
        {
            private readonly UserManager<CustomIdentityUser> _userManager;

            public CreateUserNewHandler(UserManager<CustomIdentityUser> userManager)
            {
                _userManager = userManager;
            }


            public async Task<AddClaimsInputDto> Handle(CreateNewUserInputDto createNewUserInput,
                CancellationToken cancellationToken)
            {
                if (await _userManager.FindByEmailAsync(createNewUserInput.Email) != null)
                {
                    throw new WorkflowException("Пользователь уже существует");
                }

                var user = new CustomIdentityUser(
                    createNewUserInput.Password,
                    createNewUserInput.UserName,
                    createNewUserInput.Password);

                await _userManager.CreateAsync(user);
                return new AddClaimsInputDto(user, createNewUserInput.Roles);
            }

            public async Task<ErrorMessage> RollBack(CreateNewUserInputDto input,
                CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByEmailAsync(input.Email);
                if (user == null)
                {
                    return new ErrorMessage("Не удалось создать пользователя");
                }

                var identityResult = await _userManager.DeleteAsync(user);

                if (!identityResult.Succeeded)
                {
                    throw new RollBackException(identityResult.Errors.Select(x => x.Description));
                }

                return await Task.FromResult(new ErrorMessage("Ошибка в CreateUserNewHandler"));
            }
        }

        [UsedImplicitly]
        public class AddClaimsHandler : IAsyncHandler<AddClaimsInputDto, AddRolesInputDto>,
            ICanAsyncRollBack<AddClaimsInputDto>
        {
            private readonly UserManager<CustomIdentityUser> _userManager;

            public AddClaimsHandler(UserManager<CustomIdentityUser> userManager)
            {
                _userManager = userManager;
            }

            private IEnumerable<Claim> GetClaims(AddClaimsInputDto user)
            {
                yield return new Claim("email", user.User.Email);
                yield return new Claim("username", user.User.UserName);
                yield return new Claim("roles", string.Join(",", user.Roles));
            }

            public async Task<AddRolesInputDto> Handle(AddClaimsInputDto input, CancellationToken cancellationToken)
            {
                var identityResult = await _userManager.AddClaimsAsync(input.User, GetClaims(input));

                if (!identityResult.Succeeded)
                {
                    throw new WorkflowException(identityResult.Errors.Select(x => x.Description));
                }

                return new AddRolesInputDto(input.User, input.Roles);
            }


            public async Task<ErrorMessage> RollBack(AddClaimsInputDto input,
                CancellationToken cancellationToken)
            {
                var claims = GetClaims(input);
                var user = input.User;
                await _userManager.RemoveClaimsAsync(user, claims);
                return new ErrorMessage("Ошибка в AddClaimsHandler");
            }
        }

        [UsedImplicitly]
        public class AddRolesForExistingUserHandler : IAsyncHandler<AddRolesInputDto, ConfirmDto>,
            ICanAsyncRollBack<AddRolesInputDto>
        {
            private readonly RoleManager<CustomIdentityRole> _roleManager;
            private readonly UserManager<CustomIdentityUser> _userManager;

            public AddRolesForExistingUserHandler(
                UserManager<CustomIdentityUser> userManager,
                RoleManager<CustomIdentityRole> roleManager)
            {
                _roleManager = roleManager;
                _userManager = userManager;
            }

            public async Task<ConfirmDto> Handle(AddRolesInputDto input, CancellationToken cancellationToken)
            {
                var checkRoles = input.Roles.Select(async x =>
                    new {roleName = x, roleExists = await _roleManager.RoleExistsAsync(x)});
                var allRolesExists = await Task.WhenAll(checkRoles);

                if (!allRolesExists.All(x => x.roleExists))
                {
                    throw new WorkflowException(allRolesExists.Where(x => x.roleExists).Select(x => x.roleName));
                }

                var identityResult = await _userManager.AddToRolesAsync(input.User, input.Roles);

                if (!identityResult.Succeeded)
                {
                    throw new WorkflowException(identityResult.Errors.Select(x => x.Description));
                }

                return new ConfirmDto(input.User);
            }

            public async Task<ErrorMessage> RollBack(AddRolesInputDto input, CancellationToken cancellationToken)
            {
                var user = input.User;
                var roles = input.Roles;
                await _userManager.RemoveFromRolesAsync(user, roles);
                return new ErrorMessage("Ошибка в AddRolesForExistingUserHandler");
            }
        }


        [UsedImplicitly]
        public class ConfirmHandler : IAsyncHandler<ConfirmDto, ConfirmInfoDto>,
            ICanAsyncRollBack<ConfirmDto>
        {
            private readonly UserManager<CustomIdentityUser> _userManager;
            private readonly EmailSenderService _emailSenderService;

            public ConfirmHandler(UserManager<CustomIdentityUser> userManager, EmailSenderService emailSenderService)
            {
                _userManager = userManager;
                _emailSenderService = emailSenderService;
            }

            public async Task<ConfirmInfoDto> Handle(ConfirmDto input, CancellationToken cancellationToken)
            {
                var resetPasswordToken = await _userManager.GenerateEmailConfirmationTokenAsync(input.User);
                var email = input.User.Email;
                var userEmail = input.User.Email;
                var emailSendDto = new EmailSendDto(email, GenerateMessageText(resetPasswordToken, email), userEmail);
                await _emailSenderService.Handle(emailSendDto, cancellationToken);
                return new ConfirmInfoDto(userEmail, "Письмо успешно отправлено");
            }

            public Task<ErrorMessage> RollBack(ConfirmDto createNewUserInput,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new ErrorMessage("ошибка в ConfirmHandler"));
            }

            private string GenerateMessageText(string code, string email)
            {
                var a =
                    $"<a href='http://localhost:5000/api/auth/secondaryAuthentication?code={HttpUtility.UrlEncode(code)}&email={email}'>Ссылка</a>";
                var message = $"Для успешной аутентификации перейдите по ссылке  {a}";
                return message;
            }
        }
    }
}