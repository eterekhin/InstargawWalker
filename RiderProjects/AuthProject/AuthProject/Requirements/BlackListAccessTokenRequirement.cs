using System.Collections.Generic;
using System.Linq;
using AuthProject.Context;
using AuthProject.ValueTypes;
using Microsoft.AspNetCore.Authorization;

namespace AuthProject
{
    public class BlackListAccessTokenRequirement : IAuthorizationRequirement
    {
        private readonly AuthDbContext _dbContext;

        public BlackListAccessTokenRequirement(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<AccessToken> GetBlackListJwtTokens()
        {
#nullable disable
           return _dbContext.DisabledAccessTokens.Select(x => x.AccessToken).ToList();
#nullable enable
        }
    }
}