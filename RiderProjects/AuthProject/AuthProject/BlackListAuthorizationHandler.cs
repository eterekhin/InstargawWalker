using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic.ApplicationServices;

namespace AuthProject
{
    public static class Requirements
    {
    }

    public class BlackListAuthorizationHandler : AuthorizationHandler<BlackListAccessTokenRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            BlackListAccessTokenRequirement requirement)
        {
            throw new System.NotImplementedException();
        }
    }
}