using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthProject
{
    public class JwtTokenOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool RequireExpirationTime { get; set; }
        public bool ValidateLifetime { get; set; }
        public int ClockSkewSecond { get; set; }
        public string SecretKey { get; set; }
    }

    public class JwtOptionsConfigurationSection
    {
        public IConfigurationSection ConfigurationSection { get; set; }
    }

  
}