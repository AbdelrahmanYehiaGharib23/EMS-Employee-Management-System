using EMS.DAL.Contracts.Repositories;
using EMS.DAL.Contracts.Repositories.Identity;
using EMS.DAL.Entities.IdentityModel;
using EMS.DAL.Persistence.Data.DbInitializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.DAL.Persistence.Repositories.Identity
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        // إضافة Token جديد
        public Task AddTokenAsync(PasswordResetTokens token)
        {
            _context.PasswordResetTokens.Add(token);
            return Task.CompletedTask;
        }

        public async Task InvalidateActiveTokensAsync(string userId)
        {
            var activeTokens = await _context.PasswordResetTokens
                .Where(x => x.UserId == userId && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsUsed = true;
            }
        }

        // الحصول على OTP صالح للتحقق
        public async Task<PasswordResetTokens?> GetValidOtpTokenAsync(string userId, string otpHash)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(x => x.UserId == userId
                                          && x.TokenHash == otpHash
                                          && !x.IsUsed
                                          && x.ExpiresAt > DateTime.UtcNow);
        }

        // الحصول على Reset Token صالح
        public async Task<PasswordResetTokens?> GetValidResetTokenAsync(string tokenHash)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash
                                          && !x.IsUsed
                                          && x.ExpiresAt > DateTime.UtcNow);
        }

        // Update Token (Used = true)
        public Task UpdateTokenAsync(PasswordResetTokens token)
        {
            _context.PasswordResetTokens.Update(token);
            return Task.CompletedTask;
        }
    }
}
