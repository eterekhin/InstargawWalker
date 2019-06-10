using System;
using System.Threading.Tasks;
using AuthProject.Context;
using AuthProject.Identities;
using AuthProject.ValueTypes;
using Microsoft.EntityFrameworkCore;

namespace AuthProject.Services
{
    public class RefreshTokenHandler
    {
        private readonly AuthDbContext _context;

        public RefreshTokenHandler(AuthDbContext context)
        {
            _context = context;
        }

        
    }
}